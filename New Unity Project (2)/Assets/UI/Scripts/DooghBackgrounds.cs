using System.Collections.Generic;
using UnityEngine;

public static class DooghBackgrounds
{
    public static readonly string[] Ids = { "Classic", "Oak-Daylight", "Copper-Loft", "Sage-Studio", "Turquoise", "Courtyard", "Teahouse", "NorthernKitchen" };
    public static readonly string[] Names = { "CLASSIC", "OAK DAYLIGHT", "COPPER LOFT", "SAGE STUDIO", "TURQUOISE", "COURTYARD", "TEAHOUSE", "NORTHERN KITCHEN" };
    public static readonly CosmeticInventory Inventory = new CosmeticInventory("SelectedDooghBackground", "OwnedDooghBackground.", "Classic",
        new Dictionary<string, int> {
            { "Oak-Daylight", 200 }, { "Copper-Loft", 200 }, { "Sage-Studio", 200 },
            { "Turquoise", 1000 }, { "Courtyard", 1000 }, { "Teahouse", 1000 }, { "NorthernKitchen", 1000 }, { "Classic", 0 }
        });
    public static Sprite Preview(string id) => Resources.Load<Sprite>("DooghBackgrounds/Previews/" + id);
    public static Sprite SelectedArt() => Resources.Load<Sprite>("DooghBackgrounds/" + Inventory.Current);
}
