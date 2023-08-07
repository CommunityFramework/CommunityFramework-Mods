
using System.Collections.Generic;
using static CF_Economy.API;

public class CF_BankAccount
{
    public static Dictionary<string, CF_BankAccount> accounts = new Dictionary<string, CF_BankAccount>();
    public string eosId { get; }
    public int currencyId { get; }
    private int _balance = 0; 
    public int Balance
    {
        get { return _balance; }
        private set { _balance = value; }
    }

    public CF_BankAccount(string _eosId, int currencyId)
    {
        eosId = _eosId;
        this.currencyId = currencyId;
    }

    public static CF_BankAccount GetOrCreateAccount(string eosId, int currencyId)
    {
        var accountId = $"{eosId}_{currencyId}";
        if (!accounts.ContainsKey(accountId))
        {
            var account = new CF_BankAccount(eosId, currencyId);
            accounts[accountId] = account;
        }
        return accounts[accountId];
    }
    public static int GetBalance(string eosId, int currencyId)
    {
        var account = GetOrCreateAccount(eosId, currencyId);
        return account.Balance;
    }
    public static void Deposit(string eosId, int currencyId, int amount)
    {
        if (amount < 0)
        {
            log.Out("Cannot deposit a negative amount.");
            return;
        }
        var account = GetOrCreateAccount(eosId, currencyId);
        account.Balance += amount;
    }

    public static bool Withdraw(string eosId, int currencyId, int amount)
    {
        var account = GetOrCreateAccount(eosId, currencyId);
        if (account.Balance >= amount)
        {
            account.Balance -= amount;
            return true;
        }
        return false;
    }
}