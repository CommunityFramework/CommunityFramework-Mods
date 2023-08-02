using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Project7
{
    public class CF_CurrencyManager
    {
        private static Dictionary<int, Currency> currencies = new Dictionary<int, Currency>();

        private static void LoadCurrencies()
        {
            currencies.Clear();
        }
    }

    public class BankAccount
    {
        public string eosId { get; }
        public int currencyId { get; }
        public int balance { get; set; } = 0;

        public BankAccount(string _eosId, int currencyId)
        {
            eosId = _eosId;
            this.currencyId = currencyId;
        }
    }

    public class Currency
    {
        public string Name { get; }
        public string Tag { get; }

        public Currency(string name, string tag)
        {
            Name = name;
            Tag = tag;
        }
    }
}
