
using System.Collections.Generic;
using static CF_Economy.API;

public class CF_BankAccount
{
    private static object accountLock = new object();
    private const float WithdrawalFeePercentage = 0.01f; // 1% for example
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
        lock (accountLock)
        {
            var account = GetOrCreateAccount(eosId, currencyId);
            account.Balance += amount;
        }
    }
    public static bool Withdraw(string eosId, int currencyId, int amount)
    {
        lock (accountLock)
        {
            var account = GetOrCreateAccount(eosId, currencyId);
            int fee = (int)(amount * WithdrawalFeePercentage);
            int totalDeduction = amount + fee;

            // Check if the account has enough to cover both the withdrawal and the fee.
            if (account.Balance >= totalDeduction)
            {
                account.Balance -= totalDeduction;
                return true;
            }
            else
            {
                // If they don't have enough to cover the total, but they can cover the fee, 
                // then we'll try to give them as much as we can after deducting the fee.
                if (account.Balance > fee)
                {
                    int maxPossibleWithdrawal = account.Balance - fee;
                    account.Balance -= (maxPossibleWithdrawal + fee);
                    log.Out($"Insufficient funds for full withdrawal. Withdrawn {maxPossibleWithdrawal} with a fee of {fee}.");
                    return true;
                }
                else
                {
                    // If they can't even cover the fee, then no withdrawal happens.
                    log.Out($"Insufficient funds for withdrawal and fee. No amount withdrawn.");
                    return false;
                }
            }
        }
    }

}