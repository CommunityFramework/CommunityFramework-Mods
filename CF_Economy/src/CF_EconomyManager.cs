using System;
using System.Collections.Generic;
using UnityEngine;
using static CF_Economy.API;

public class CF_EconomyManager
{
    public static bool DepositToAccount(string eosId, int currencyId, int amount)
    {
        try
        {
            CF_BankAccount.Deposit(eosId, currencyId, amount);
            return true;
        }
        catch (Exception ex)
        {
            log.Out($"Error depositing to account: {ex.Message}");
            return false;
        }
    }
}