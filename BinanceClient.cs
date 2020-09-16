using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

enum RequestType
{
    GET,
    POST,
    DELETE
}

public class BinanceClient
{
	static readonly string BaseURL = "https://api.binance.com/api/v3/";
    private HttpClient client;
    private bool IgnoreFirstRequest = true; // the very first request's timing is higher to establish connection
    private Random rnd = new Random();
    private HttpStatusCode lastResult;   

    public BinanceClient()
	{
        HttpClientHandler httpClientHandler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
        };
        client = new HttpClient(httpClientHandler);
        client.Timeout = TimeSpan.FromMinutes(10);
        if (BinanceSpot.settings.KeysSet)
        {
            client.DefaultRequestHeaders.Add("X-MBX-APIKEY", BinanceSpot.settings.ApiKey);
        }
    }

    private string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[rnd.Next(s.Length)]).ToArray());
    }

    //Timestamp for signature
    private static string GetTimestamp()
    {
        long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        return milliseconds.ToString();
    }

    private string DoRestRequest(string endpoint, string args = "",
        RequestType httpMethod = RequestType.GET,
        bool DoSigned = false)  
    {
        long ElapsedTime = 0;
        string reqTypeStr = "";

        string responseString;
        Stopwatch sw = new Stopwatch();
        try
        {
            if (DoSigned)
            {
                if (!string.IsNullOrEmpty(args)) { args += "&"; }
                args += "recvWindow=6000&timestamp=" + GetTimestamp();
                var signature = args.CreateSignature(BinanceSpot.settings.ApiSecret);
                args += "&signature=" + signature;
            }
            if (!string.IsNullOrEmpty(args)) { args = "?" + args; }

            sw.Start();

            HttpResponseMessage response;
            switch (httpMethod)
            {
                case RequestType.POST:
                    reqTypeStr = "POST ";
                    response = client.PostAsync(BaseURL + endpoint + args, null).Result;
                    break;
                case RequestType.DELETE:
                    reqTypeStr = "DELETE ";
                    response = client.DeleteAsync(BaseURL + endpoint + args).Result;
                    break;
                default:
                    reqTypeStr = "GET ";
                    response = client.GetAsync(BaseURL + endpoint + args).Result;
                    break;
            }
            lastResult = response.StatusCode;
            responseString = response.Content.ReadAsStringAsync().Result;

            sw.Stop();
            ElapsedTime = sw.ElapsedMilliseconds;

            IEnumerable<string> values;
            if (response.Headers.TryGetValues("X-MBX-USED-WEIGHT", out values))
            {
                BinanceSpot.used_weight = Convert.ToInt32(values.First());
            }
        }
        catch (Exception e)
        {
            ElapsedTime = 0;
            responseString = "";
            lastResult = HttpStatusCode.NoContent;
            Logger.ConsoleOut($"Error making request {endpoint} : {e.Message}");
        }
        if (!IgnoreFirstRequest)
            Logger.CheckRequestTiming(ElapsedTime, reqTypeStr + endpoint + args);
        IgnoreFirstRequest = false;
        return responseString;
    }

    public void UpdateExchangeInfo()
    {
        var rawResult = DoRestRequest("exchangeInfo");
        if (lastResult == HttpStatusCode.OK)
            BinanceSpot.updateSymbols(rawResult);
        UpdatePrices();
    }

    public void UpdatePrices()
    {
        var rawResult = DoRestRequest("ticker/bookTicker");
        if (lastResult == HttpStatusCode.OK)
            BinanceSpot.updatePrices(rawResult);
    }
    public void GetBalances()
    {
        string rawData = DoRestRequest("account", "", 
            RequestType.GET, true);
    }
    public void GetKLines(Spot.Symbol sym)
    {
        string rawData = DoRestRequest("klines", $"symbol={sym.symbol}&interval=5m");
    }
    public void GetOrderBook(Spot.Symbol sym)
    {
        string rawData = DoRestRequest("depth", $"symbol={sym.symbol}&limit=100");
    }

    public Spot.BinanceOrder PlaceBuyOrder(string symbol, double quantity, double price, string type = "LIMIT")
    {
        string ClientOrderID = "odn_" + RandomString(16);
        string rawData = DoRestRequest("order",
            $"symbol={symbol}&side=BUY&type={type}&quantity={Math.Round(quantity,8)}&price={Math.Round(price,8)}&timeInForce=GTC&newClientOrderId={ClientOrderID}",
            RequestType.POST, true);

//        Logger.ConsoleOut("Order: " + rawData);

        if (!string.IsNullOrEmpty(rawData) & (lastResult == HttpStatusCode.OK))
        {
            Spot.BinanceOrder order = JsonConvert.DeserializeObject<Spot.BinanceOrder>(rawData);
            order.CreateTime = DateTime.UtcNow;
            return order;
        }
        else
        {
//            Logger.ConsoleOut("req: " + $"symbol={symbol}&side=BUY&type={type}&quantity={Math.Round(quantity, 8)}&price={Math.Round(price, 8)}&timeInForce=GTC&newClientOrderId={ClientOrderID}");
            CancelOrder(symbol, ClientOrderID);
            return null;
        }
    }

    public Spot.BinanceOrder PlaceMinBuyOrder(Spot.Symbol sym)
    {
        double price = sym.avgPrice / 2;  // we are lucky enough to buy that cheap

        price = Math.Round((price - sym.priceFilter.minPrice) / sym.priceFilter.tickSize);
        price = price * sym.priceFilter.tickSize + sym.priceFilter.minPrice;

        double quantity = Math.Max(sym.lotFilter.minQty, sym.notionalFilter.minNotional / price);
        quantity = Math.Round((quantity - sym.lotFilter.minQty) / sym.lotFilter.stepSize) + 1;
        quantity = quantity * sym.lotFilter.stepSize + sym.lotFilter.minQty;

        return PlaceBuyOrder(sym.symbol, quantity, price);

    }

    public void CancelOrder (Spot.BinanceOrder order) 
    {
        CancelOrder(order.Symbol, order.ClientOrderId);
    }

    public void CancelOrder(string symbol, string orderID)
    {
        string rawData = DoRestRequest("order",
            $"symbol={symbol}&origClientOrderId={orderID}", RequestType.DELETE, true);
    }

    public void CheckOrder(Spot.BinanceOrder order)
    {
        string rawData = DoRestRequest("order",
            $"symbol={order.Symbol}&orderId={order.OrderId}", RequestType.GET, true);
        if (rawData.Contains("does not exist")) 
        {
            long timeDelta = Convert.ToInt64((DateTime.UtcNow - order.CreateTime).TotalMilliseconds);

            Logger.ReportODN(timeDelta, $"The actually existing order (Market:{order.Symbol} id: {order.OrderId}, clientID: {order.ClientOrderId}) " + 
                $"\n  created {order.CreateTime.ToLongTimeString()} \n  was reported as does not exist since {timeDelta} ms after creation!");
        }

    }


    public void TestSequence()
    {
        GetBalances();
        UpdatePrices();
        Spot.Symbol sym = BinanceSpot.RandomSymbol();
        GetKLines(sym);
        GetOrderBook(sym);

    }


}
