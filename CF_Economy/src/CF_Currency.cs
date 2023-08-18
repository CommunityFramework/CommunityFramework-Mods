using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CF_Economy.API;

public class CF_Currency
{
    private static object currencyLock = new object();
    public int Id { get; }
    public string Name { get; }
    public string Tag { get; }

    public CF_Currency()
    {
    }
    public CF_Currency(int id, string name, string tag)
    {
        Id = id;
        Name = name;
        Tag = tag;
    }
    public static void Initialize()
    {
        RegisterCurrency(1, "Dukes", "dukes");
    }
    public static void RegisterCurrency(int id, string name, string tag)
    {
        lock (currencyLock)
        {
            if (IsIdUsed(id))
            {
                log.Out($"Currency with id {id} already exists!");
                return;
            }

            if (IsTagUsed(tag))
            {
                log.Out($"Currency with tag {tag} already exists!");
                return;
            }

            var currency = new CF_Currency(id, name, tag);
            allCurrencies.Add(currency.Id, currency);
        }
    }
    public static List<CF_Currency> GetAllCurrencies()
    {
        return new List<CF_Currency>(allCurrencies.Values);
    }
    public static CF_Currency GetCurrency(int id)
    {
        if (allCurrencies.ContainsKey(id))
            return allCurrencies[id];

        return null;
    }
    public static CF_Currency GetCurrencyByTag(string tag)
    {
        foreach (var currency in allCurrencies.Values)
        {
            if (currency.Tag == tag)
                return currency;
        }
        return null;
    }
    public static bool IsIdUsed(int id)
    {
        return allCurrencies.ContainsKey(id);
    }
    public static bool IsTagUsed(string tag)
    {
        return GetCurrencyByTag(tag) != null;
    }
}
