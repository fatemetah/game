using System;
using System.Collections.Generic;
using UnityEngine;

// Ownership and selection for one collection, paid for from the shared wallet.
public sealed class CosmeticInventory
{
    readonly Dictionary<string, int> prices;
    readonly string selectionKey, ownedPrefix, starter;
    public event Action Changed;

    public CosmeticInventory(string selectionKey, string ownedPrefix, string starter, Dictionary<string, int> prices)
    {
        this.selectionKey = selectionKey;
        this.ownedPrefix = ownedPrefix;
        this.starter = starter;
        this.prices = prices;
    }
    public int Price(string id) => prices[id];
    public bool IsOwned(string id)
    {
        return id == starter || (id != null && prices.ContainsKey(id) && PlayerPrefs.GetInt(ownedPrefix + id) == 1);
    }
    

    public string Current
    {
        get
        {
            string id = PlayerPrefs.GetString(selectionKey, starter);
            return IsOwned(id) ? id : starter;
        }
    }

    public void Select(string id)
    {
        if (!IsOwned(id) || Current == id) return;
        SaveSelection(id);
    }

    public bool TryBuyAndSelect(string id)
    {
        if (id == null || !prices.TryGetValue(id, out int price)) return false;
        if (IsOwned(id))
        {
            Select(id);
            return true;
        }
        int balance = Coin_Counter.WalletBalance;
        if (balance < price) return false;
        // Commit the balance, permanent unlock and equipped item in one save.
        Coin_Counter.WalletBalance = balance - price;
        PlayerPrefs.SetInt(ownedPrefix + id, 1);
        SaveSelection(id);
        return true;
    }

    void SaveSelection(string id)
    {
        PlayerPrefs.SetString(selectionKey, id);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
