using System;
using System.Collections.Generic;
using System.Text;

namespace Spot
{
    public class RateLimit
    {
        public string rateLimitType { get; set; }
        public string interval { get; set; }
        public int intervalNum { get; set; }
        public int limit { get; set; }
    }

    public class Filter
    {
        public string filterType { get; set; }
        public double minPrice { get; set; }
        public double maxPrice { get; set; }
        public double tickSize { get; set; }
        public double multiplierUp { get; set; }
        public double multiplierDown { get; set; }
        public int? avgPriceMins { get; set; }
        public double minQty { get; set; }
        public double maxQty { get; set; }
        public double stepSize { get; set; }
        public double minNotional { get; set; }
        public bool? applyToMarket { get; set; }
        public int? limit { get; set; }
        public int? maxNumAlgoOrders { get; set; }
        public int? maxNumOrders { get; set; }
    }

    public class Symbol
    {
        public string symbol { get; set; }
        public string status { get; set; }
        public string baseAsset { get; set; }
        public int baseAssetPrecision { get; set; }
        public string quoteAsset { get; set; }
        public int quotePrecision { get; set; }
        public int quoteAssetPrecision { get; set; }
        public int baseCommissionPrecision { get; set; }
        public int quoteCommissionPrecision { get; set; }
        public List<string> orderTypes { get; set; }
        public bool icebergAllowed { get; set; }
        public bool ocoAllowed { get; set; }
        public bool quoteOrderQtyMarketAllowed { get; set; }
        public bool isSpotTradingAllowed { get; set; }
        public bool isMarginTradingAllowed { get; set; }
        public List<Filter> filters { get; set; }
        public List<string> permissions { get; set; }
        public double avgPrice { get; set; }
        public Filter lotFilter;
        public Filter priceFilter;
        public Filter notionalFilter;


    }

    public class Root
    {
        public string timezone { get; set; }
        public long serverTime { get; set; }
        public List<RateLimit> rateLimits { get; set; }
        public List<object> exchangeFilters { get; set; }
        public List<Symbol> symbols { get; set; }
    }

    public class bookTicker
    {
        public string symbol { get; set; }
        public double bidPrice { get; set; }
        public double bidQty { get; set; }
        public double askPrice { get; set; }
        public double askQty { get; set; }
        public ulong TIme { get; set; }

    }

    public class BinanceOrder
    {
        public string Symbol { get; set; } = "";
        public long OrderId { get; set; }
        public string ClientOrderId { get; set; } = "";
        public long OrderListId { get; set; }
        public string origClientOrderId { get; set; } = "";
        public decimal Price { get; set; }
        public decimal origQty { get; set; }
        public decimal executedQty { get; set; }
        public decimal cummulativeQuoteQty { get; set; }
        public decimal origQuoteOrderQty { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Side { get; set; }
        public DateTime CreateTime { get; set; }
        public long Time { get; set; }
        public long UpdateTime { get; set; }
    }

}
