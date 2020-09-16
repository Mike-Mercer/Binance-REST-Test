using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

public static class BinanceSpot
{
	const int MaxWeight = 1200;
	public static List<Spot.Symbol> usdtSymbols;
	public static int used_weight = 0;
	public static BinanceSettings settings = BinanceSettings.LoadSettings();
	private static Random rnd;

	static BinanceSpot()
	{
		rnd = new Random();
	}

	public static bool CheckUsedWeight()
	{
		return (used_weight < MaxWeight * 0.9);
	}

	public static void updateSymbols(string rawData)
	{
		Spot.Root spotRoot =
                    JsonConvert.DeserializeObject<Spot.Root>(rawData);

		foreach (var s in spotRoot.symbols) 
		{
			s.lotFilter = s.filters.Find(x => x.filterType == "LOT_SIZE");
			s.priceFilter = s.filters.Find(x => x.filterType == "PRICE_FILTER");
			s.notionalFilter = s.filters.Find(x => x.filterType == "MIN_NOTIONAL");
		}

		usdtSymbols =
            spotRoot.symbols.Where(sy => sy.quoteAsset == "USDT").ToList();

		Logger.ConsoleOut($" {usdtSymbols.Count} symbols updated");

	}

	public static void updatePrices(string rawData)
	{
		List<Spot.bookTicker> sList =
					JsonConvert.DeserializeObject<List<Spot.bookTicker>>(rawData);

		foreach (var t in sList) 
		{
			var sym = usdtSymbols.Find(x => x.symbol == t.symbol);
			if (sym != null) { sym.avgPrice = (t.bidPrice + t.askPrice) / 2; }	
		}
	}

	public static Spot.Symbol RandomSymbol()
	{
		Spot.Symbol s;
		do
		{
			int r = rnd.Next(usdtSymbols.Count);
			s = usdtSymbols[r];
		} while (s.avgPrice == 0);
		return s;
	}


}

#region Helpers
public static class BinanceHelpers
{
	private static readonly Encoding SignatureEncoding = Encoding.UTF8;

	public static string CreateSignature(this string message, string secret)
	{

		byte[] keyBytes = SignatureEncoding.GetBytes(secret);
		byte[] messageBytes = SignatureEncoding.GetBytes(message);
		HMACSHA256 hmacsha256 = new HMACSHA256(keyBytes);

		byte[] bytes = hmacsha256.ComputeHash(messageBytes);

		return BitConverter.ToString(bytes).Replace("-", "").ToLower();
	}
}

#endregion
