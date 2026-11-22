using UnityEngine;
using System.Collections.Generic;

public static class DooghSkins
{
    public static readonly string[] Ids = { "Classic", "Mint", "Gas", "Herbs", "Persian", "Ayran", "Water", "Juice", "Milk" };
    public static readonly string[] Names = { "CLASSIC", "MINT", "SPARKLING", "PENNYROYAL & HERBS", "TRADITIONAL", "AYRAN", "WATER", "ORANGE JUICE", "MILK" };
    const string Key = "SelectedDooghSkin";
    const string OwnedPrefix = "OwnedDooghSkin.";
    static readonly Dictionary<string, int> Prices = new Dictionary<string, int>
    {
        { "Classic", 0 }, { "Mint", 100 }, { "Gas", 100 }, { "Herbs", 100 },
        { "Persian", 500 }, { "Ayran", 2000 }, { "Water", 1000 },
        { "Juice", 1000 }, { "Milk", 1500 }
    };
    public static readonly CosmeticInventory Inventory = new CosmeticInventory(Key, OwnedPrefix, "Classic", Prices);
    public static int Price(string id) => Inventory.Price(id);
    public static bool IsOwned(string id) => Inventory.IsOwned(id);
    public static string Current => Inventory.Current;
    public static event System.Action Changed
    {
        add => Inventory.Changed += value;
        remove => Inventory.Changed -= value;
    }
    public static Sprite Art(string id, string part = "") => Resources.Load<Sprite>("DooghSkins/" + id + part);
    public static void Select(string id) => Inventory.Select(id);
    public static bool TryBuyAndSelect(string id) => Inventory.TryBuyAndSelect(id);
}
