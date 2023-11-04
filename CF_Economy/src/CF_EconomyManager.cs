using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using static CF_Economy.API;

public class CF_EconomyManager
{
    public class EconomyDB
    {
        public Dictionary<string, CF_Currency> currenties = new Dictionary<string, CF_Currency>();
        public Dictionary<string, CF_BankAccount> accounts = new Dictionary<string, CF_BankAccount>();
    }
    public static EconomyDB database = new EconomyDB();

    public static void LoadDatabase()
    {
        // Check if file exists
        if (!File.Exists(filePath))
        {
            database.currenties["USD"] = new CF_Currency("dukes", "Dukes", "dukes");
            RegisterCurrency("duke", "Dukes", "dukes");
            StoreDatabase();
        }
        else
        {
            // Load the database from the file
            string jsonData = File.ReadAllText(filePath);
            database = JsonConvert.DeserializeObject<EconomyDB>(jsonData);
        }

        if(fileWatcher == null)
        {
            fileWatcher = new FileSystemWatcher
            {
                Path = mod.modDatabasePath,
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite
            };
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.EnableRaisingEvents = true;
        }
    }

    public static void StoreDatabase()
    {
        // Convert the database object to JSON and write it to the file
        string jsonData = JsonConvert.SerializeObject(database, Formatting.Indented);
        File.WriteAllText(filePath, jsonData);
    }

    public static void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath == filePath)
        {
            log.Out("Database file changed, reloading...");
            LoadDatabase();
        }
    }
    // Currencies

    public static void RegisterCurrency(string id, string name, string tag)
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
        database.currenties.Add(currency.Id, currency);
    }
    public static List<CF_Currency> GetAllCurrencies()
    {
        return new List<CF_Currency>(database.currenties.Values);
    }
    public static CF_Currency GetCurrency(string id)
    {
        if (database.currenties.ContainsKey(id))
            return database.currenties[id];

        return null;
    }
    public static CF_Currency GetCurrencyByTag(string tag)
    {
        foreach (var currency in database.currenties.Values)
        {
            if (currency.ItemTag == tag)
                return currency;
        }
        return null;
    }
    public static bool IsIdUsed(string id)
    {
        return database.currenties.ContainsKey(id);
    }
    public static bool IsTagUsed(string tag)
    {
        return GetCurrencyByTag(tag) != null;
    }

    // Bank Accounts

    public static CF_BankAccount GetOrCreateAccount(ClientInfo cInfo, string currencyId) => GetOrCreateAccount(cInfo.PlatformId.ReadablePlatformUserIdentifier, currencyId);
    public static CF_BankAccount GetOrCreateAccount(string steamId, string currencyId)
    {
        string accountId = $"{steamId}_{currencyId}";
        if (!database.accounts.ContainsKey(accountId))
        {
            CF_BankAccount account = new CF_BankAccount(steamId, currencyId);
            database.accounts[accountId] = account;
        }
        return database.accounts[accountId];
    }
    public static int GetBalance(ClientInfo cInfo, string currencyId) => GetBalance(cInfo.PlatformId.ReadablePlatformUserIdentifier, currencyId);
    public static int GetBalance(string steamId, string currencyId)
    {
        CF_BankAccount account = GetOrCreateAccount(steamId, currencyId);
        return account.Balance;
    }
    public static bool Deposit(ClientInfo cInfo, string currencyId, int amount) => Deposit(cInfo.PlatformId.ReadablePlatformUserIdentifier, currencyId, amount);
    public static bool Deposit(string steamId, string currencyId, int amount)
    {
        CF_BankAccount account = GetOrCreateAccount(steamId, currencyId);
        account.Balance += amount;

        return true;
    }
    public static bool Withdraw(ClientInfo cInfo, string currencyId, int amount) => Withdraw(cInfo.PlatformId.ReadablePlatformUserIdentifier, currencyId, amount);
    public static bool Withdraw(string steamId, string currencyId, int amount)
    {
        CF_BankAccount account = GetOrCreateAccount(steamId, currencyId);
        int fee = (int)(amount * database.currenties[currencyId].WithdrawalFeePercentage);
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