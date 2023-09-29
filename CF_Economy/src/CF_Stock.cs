using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CF_Stock
{
    public string StockSymbol { get; } // E.g., BTC for Bitcoin, USD for U.S. Dollar, etc.
    public string StockName { get; } // E.g., Bitcoin, U.S. Dollar, etc.
    public int CurrencyId { get; } // Id of the currency required to buy this stock.
    public double CurrentValue { get; set; } // Current value of the stock.
    public double SellFeePercentage { get; } // E.g., 0.02 for 2% fee.

    public CF_Stock(string stockSymbol, string stockName, int currencyId, double sellFeePercentage)
    {
        StockSymbol = stockSymbol;
        StockName = stockName;
        CurrencyId = currencyId;
        SellFeePercentage = sellFeePercentage;
    }

    // Other related methods and logic can be added here.
}

