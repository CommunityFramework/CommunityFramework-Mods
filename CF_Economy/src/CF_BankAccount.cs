
using System.Collections.Generic;
using static CF_Economy.API;

public class CF_BankAccount
{
    public string steamId { get; }
    public string currencyId { get; }
    private int _balance = 0; 
    public int Balance
    {
        get { return _balance; }
        set { _balance = value; }
    }
    public CF_BankAccount(string steamId, string currencyId)
    {
        this.steamId = steamId;
        this.currencyId = currencyId;
    }

}