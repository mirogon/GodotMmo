using Godot;
using System;
using System.Collections.Generic;

public partial class InventorySystem : Panel
{
    public static int TILE_WIDTH = 5;
    public static int TILE_HEIGHT = 8;
    public static int TILE_PIXEL_SIZE = 40;
    [Export] public PackedScene InventoryTileScene;

    public Dictionary<Guid, MongoInventoryItem> Items = new();
    public List<MongoItemContainerTile> Tiles = new();

    Dictionary<Guid, Control> InventoryUiItems = new();

    //Moving
    Guid _currentlyMoving = Guid.Empty;
    public override void _Ready()
    {
        base._Ready();
        for(int y = 0; y < TILE_HEIGHT; y++)
        {
            for (int x = 0; x < TILE_WIDTH; x++)
            {
                var tileInstance = InventoryTileScene.Instantiate() as Control;
                AddChild(tileInstance);
                tileInstance.Position = new Vector2(x * TILE_PIXEL_SIZE, y * TILE_PIXEL_SIZE);
            }
        }
    }
    public override void _Process(double delta)
    {
        var localMousePos = GetLocalMousePosition();
        if (Input.IsActionJustPressed("MouseLeft"))
        {
            var mouseTilePos = Utility.LocalMousePosToTilePos(localMousePos, TILE_PIXEL_SIZE);
            var tileIndex = Utility.GridXAndYPosToIndex(mouseTilePos.Item1, mouseTilePos.Item2, TILE_WIDTH);

            if (tileIndex >= Tiles.Count) { return; }

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
                var itemSize = ItemInfo.ItemTypeToTileSizeDict[item.ItemType];
                var invItem = InventoryUiItems[_currentlyMoving];

                var tilePos = Utility.LocalMousePosToTilePos(GetLocalMousePosition(), TILE_PIXEL_SIZE);
                if(ItemCanFit(tilePos.x, tilePos.y, itemSize.X, itemSize.Y))
                {
                    invItem.Position = new Vector2(tilePos.x * TILE_PIXEL_SIZE, tilePos.y * TILE_PIXEL_SIZE);

                    NetworkClient.MoveItem(_currentlyMoving, tilePos.x, tilePos.y);

                    _currentlyMoving = Guid.Empty;
                }
                else
                {
                    invItem.Position = new Vector2(Items[_currentlyMoving].TilePosTopLeftX * TILE_PIXEL_SIZE, Items[_currentlyMoving].TilePosTopLeftY * TILE_PIXEL_SIZE);
                    _currentlyMoving = Guid.Empty;
                }
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

            var invScene = ItemManager.ItemTypeToInventoryItemScene[current.ItemType];
            var sceneInstance = invScene.Instantiate() as Control;
            AddChild(sceneInstance);
            sceneInstance.Position = new Vector2(current.TilePosTopLeftX * TILE_PIXEL_SIZE, current.TilePosTopLeftY * TILE_PIXEL_SIZE);
            InventoryUiItems.Add(current.Id, sceneInstance);
        }
    }

    void OccupyTiles(MongoInventoryItem item)
    {
        int tileWidth = ItemInfo.ItemTypeToTileSizeDict[item.ItemType].X;
        int tileHeight = ItemInfo.ItemTypeToTileSizeDict[item.ItemType].Y;
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
}
