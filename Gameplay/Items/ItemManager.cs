using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ItemManager
{
    public static Dictionary<ItemType, PackedScene> ItemMeshScenes = new Dictionary<ItemType, PackedScene>()
    {
        {ItemType.Sword, ResourceLoader.Load<PackedScene>("Gameplay/Items/Sword.tscn") }
    };

    public static Dictionary<ItemType, PackedScene> ItemTypeToInventoryItemScene = new Dictionary<ItemType, PackedScene>()
    {
        {ItemType.Sword, ResourceLoader.Load<PackedScene>("Gameplay/Items/InventorySword.tscn")}
    };
}
