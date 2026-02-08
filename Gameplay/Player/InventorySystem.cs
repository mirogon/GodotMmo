using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class InventorySystem : Panel
{
    public static int TILE_WIDTH = 5;
    public static int TILE_HEIGHT = 8;
    public static int TILE_PIXEL_SIZE = 40;
    public PackedScene InventoryTileScene = ResourceLoader.Load<PackedScene>("res://Gameplay/Items/InventoryTile.tscn");

    public Dictionary<Guid, MongoInventoryItem> Items = new();
    public List<MongoItemContainerTile> Tiles = new();

    Dictionary<Guid, Control> InventoryUiItems = new();

    public Dictionary<EquipmentSlot, MongoInventoryItem> EquippedItems = new();

    public Action<EquipmentSlot, Node3D> AttachEquipment;

    //Moving
    Guid _currentlyMoving = Guid.Empty; 
    bool _currentlyShown = false;

    //EQ
    Panel _equipmentPanel;

    Dictionary<EquipmentSlot, TextureRect> _equipmentSlots = new();

    public override void _Ready()
    {
        base._Ready();
        _equipmentPanel = GetNode("EquipmentPanel") as Panel;

        _equipmentSlots.Add(EquipmentSlot.Weapon, GetNode("EquipmentPanel/WeaponSlot") as TextureRect);
        _equipmentSlots.Add(EquipmentSlot.Helmet, GetNode("EquipmentPanel/HelmetSlot") as TextureRect);
        _equipmentSlots.Add(EquipmentSlot.BodyArmor, GetNode("EquipmentPanel/BodyArmorSlot") as TextureRect);
        _equipmentSlots.Add(EquipmentSlot.Shoes, GetNode("EquipmentPanel/ShoesSlot") as TextureRect);

        for (int y = 0; y < TILE_HEIGHT; y++)
        {
            for (int x = 0; x < TILE_WIDTH; x++)
            {
                var tileInstance = InventoryTileScene.Instantiate() as Control;
                AddChild(tileInstance);
                tileInstance.Position = new Vector2(x * TILE_PIXEL_SIZE, y * TILE_PIXEL_SIZE);
            }
        }

        var values = Enum.GetValues(typeof(EquipmentSlot));
        foreach (var v in values)
        {
            EquippedItems.Add((EquipmentSlot)v, new MongoInventoryItem());
        }

        Visible = false;
    }
    public void ToggleInventory()
    {
        Visible = !Visible;
    }
    public override void _Process(double delta)
    {
        var localMousePos = GetLocalMousePosition();
        bool mouseIsInInv = Utility.MouseIsInControl(this);
        if (Input.IsActionJustPressed("MouseLeft") && mouseIsInInv)
        {
            var mouseTilePos = Utility.LocalMousePosToTilePos(localMousePos, TILE_PIXEL_SIZE);
            var tileIndex = Utility.GridXAndYPosToIndex(mouseTilePos.Item1, mouseTilePos.Item2, TILE_WIDTH);

            if (tileIndex >= Tiles.Count || tileIndex < 0) { return; }

            var itemId = Tiles[tileIndex].OccupiedBy;
            if (!Items.ContainsKey(itemId)) { return; }
            var item = Items[itemId];
            GD.Print("Clicked at inv tile " + mouseTilePos.Item1 + ":" + mouseTilePos.Item2 + " " + item.Id);

            _currentlyMoving = itemId;
        }
        if (Input.IsMouseButtonPressed(MouseButton.Left) && _currentlyMoving != Guid.Empty)
        {
            var currentlyMoving = InventoryUiItems[_currentlyMoving];
            currentlyMoving.Position = localMousePos;
        }
        if (Input.IsActionJustReleased("MouseLeft"))
        {
            if (_currentlyMoving != Guid.Empty)
            {
                var item = Items[_currentlyMoving];
                var itemSize = ItemInfo.ItemTypeToItemInfo[item.ItemType].TileSize;
                var invItem = InventoryUiItems[_currentlyMoving];

                var tilePos = Utility.LocalMousePosToTilePos(localMousePos, TILE_PIXEL_SIZE);
                if (!mouseIsInInv)
                {
                    NetworkClient.ThrowAwayItem(_currentlyMoving);
                    _currentlyMoving = Guid.Empty;
                }
                else if(ItemCanFit(tilePos.x, tilePos.y, itemSize.X, itemSize.Y))
                {
                    invItem.Position = new Vector2(tilePos.x * TILE_PIXEL_SIZE, tilePos.y * TILE_PIXEL_SIZE);

                    NetworkClient.MoveItem(_currentlyMoving, tilePos.x, tilePos.y);

                    _currentlyMoving = Guid.Empty;
                }
                else if(Utility.MouseIsInControl(this))
                {
                    invItem.Position = new Vector2(Items[_currentlyMoving].TilePosTopLeftX * TILE_PIXEL_SIZE, Items[_currentlyMoving].TilePosTopLeftY * TILE_PIXEL_SIZE);
                    _currentlyMoving = Guid.Empty;
                }
            }
        }
        if (Input.IsActionJustPressed("MouseRight") && mouseIsInInv)
        {
            var mouseTilePos = Utility.LocalMousePosToTilePos(localMousePos, TILE_PIXEL_SIZE);
            var tileIndex = Utility.GridXAndYPosToIndex(mouseTilePos.Item1, mouseTilePos.Item2, TILE_WIDTH);

            if (tileIndex >= Tiles.Count || tileIndex < 0) { return; }

            var itemId = Tiles[tileIndex].OccupiedBy;
            if (!Items.ContainsKey(itemId)) { return; }
            var item = Items[itemId];
            GD.Print("Clicked at inv tile " + mouseTilePos.Item1 + ":" + mouseTilePos.Item2 + " " + item.Id);
        }
        HandleEquipmentUi();
    }

    void HandleEquipmentUi()
    {
        if (Input.IsActionJustPressed("MouseLeft"))
        {
            if (Utility.MouseIsInControl(_equipmentPanel))
            {
                GD.Print("Mouse is in EQ PANEL");
            }
            else
            {
                GD.Print("Mouse is NOT in EQ PANEL");
            }

            if (Utility.MouseIsInControl(_equipmentSlots[EquipmentSlot.Weapon]))
            {
                GD.Print("Mouse is in Weapon Slot");
            }
            if (Utility.MouseIsInControl(_equipmentSlots[EquipmentSlot.Helmet]))
            {
                GD.Print("Mouse is in Helmet Slot");
            }
            if (Utility.MouseIsInControl(_equipmentSlots[EquipmentSlot.BodyArmor]))
            {
                GD.Print("Mouse is in Body Armor Slot");
            }
        }
    }

    public void HandleInventoryUpdate(List<MongoInventoryItem> items)
    {
        Items.Clear();
        Tiles.Clear();

        foreach(var item in InventoryUiItems.Values)
        {
            item.QueueFree();
        }
        InventoryUiItems.Clear();

        for(int i = 0; i < TILE_WIDTH*TILE_HEIGHT; ++i)
        {
            Tiles.Add(new MongoItemContainerTile(false, Guid.Empty));
        }

        for(int i = 0; i < items.Count; ++i)
        {
            var current = items[i];
            Items.Add(current.Id, current);
            OccupyTiles(current);

            var invScene = ResourceLoader.Load<PackedScene>(ItemInfo.ItemTypeToScenePath(current.ItemType, ItemInfo.SceneType.InventoryScene));
            var sceneInstance = invScene.Instantiate() as Control;
            AddChild(sceneInstance);
            sceneInstance.Position = new Vector2(current.TilePosTopLeftX * TILE_PIXEL_SIZE, current.TilePosTopLeftY * TILE_PIXEL_SIZE);
            InventoryUiItems.Add(current.Id, sceneInstance);
        }
    }

    void OccupyTiles(MongoInventoryItem item)
    {
        int tileWidth = ItemInfo.ItemTypeToItemInfo[item.ItemType].TileSize.X;
        int tileHeight = ItemInfo.ItemTypeToItemInfo[item.ItemType].TileSize.Y;
        for (int i = 0; i < tileWidth; i++)
        {
            for (int j = 0; j < tileHeight; j++)
            {
                int index = Utility.GridXAndYPosToIndex(i + item.TilePosTopLeftX, j + item.TilePosTopLeftY, TILE_WIDTH);
                Tiles[index].IsOccupied = true;
                Tiles[index].OccupiedBy = item.Id;
            }
        }
    }

    bool ItemCanFit(int topLeftX, int topLeftY, int itemWidth, int itemHeight)
    {
        if (itemWidth > TILE_WIDTH - topLeftX || itemHeight > TILE_HEIGHT - topLeftY)
        {
            return false;
        }

        for (int i = 0; i < itemWidth; i++)
        {
            for (int j = 0; j < itemHeight; j++)
            {
                int index = Utility.GridXAndYPosToIndex(i + topLeftX, j + topLeftY, TILE_WIDTH);
                if (Tiles[index].IsOccupied)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool MouseIsBlockedByUi()
    {
        return Utility.MouseIsInControl(this) || _currentlyMoving != Guid.Empty;
    }

    public void HandleEquipmentSystemUpdate(Dictionary<EquipmentSlot, MongoInventoryItem> update)
    {
        EquippedItems = update;
    }
}
