using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CF_Economy.API;

public class CF_Currency
{
    public string Id { get; }
    public string Name { get; }
    public string ItemTag { get; }

    public float WithdrawalFeePercentage;

    public CF_Currency()
    {
    }
    public CF_Currency(string id, string name, string itemTag, float withdrawalFeePercentage = 0.01f)
    {
        Id = id;
        Name = name;
        ItemTag = itemTag;
        WithdrawalFeePercentage = withdrawalFeePercentage;
    }
}
