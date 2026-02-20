using Godot;
using System;

public partial class BlacksmithUpgradeUi : Panel
{
    public Action<Guid> AttemptUpgradeButtonPressed;
    Button _attemptUpgradeButton;
    TextureRect _itemFrame;

    Button _closeButton;

    public Guid ItemToUpgrade;

    bool _initialized = false;

    public override void _Ready()
    {
        base._Ready();
        if (!_initialized)
        {
            Initialize();
        }
    }
    void Initialize()
    {
        _attemptUpgradeButton = GetNode("AttemptUpgradeButton") as Button;
        _itemFrame = GetNode("ItemFrame") as TextureRect;

        _closeButton = GetNode("CloseButton") as Button;
        _closeButton.Pressed += OnCloseButtonPressed;

        _attemptUpgradeButton.Pressed += OnAttemptUpgradeButtonPressed;

        _initialized = true;
    }

    void OnCloseButtonPressed()
    {
        Visible = false;
        QueueFree();
    }

    void OnAttemptUpgradeButtonPressed()
    {
        AttemptUpgradeButtonPressed?.Invoke(ItemToUpgrade);
        NetworkClient.AttemptUpgrade(ItemToUpgrade);
    }

    public void Initialize(MongoInventoryItem itemToUpgrade)
    {
        if (!_initialized)
        {
            Initialize();
        }
        ItemToUpgrade = itemToUpgrade.Id;
        if(itemToUpgrade.Id != Guid.Empty)
        {
            Visible = true;
            var invItemScene = ResourceLoader.Load<PackedScene>(ItemInfo.ItemTypeToScenePath(itemToUpgrade.ItemType, ItemInfo.SceneType.InventoryScene));
            var invItemInstance = invItemScene.Instantiate() as Control;
            _itemFrame.AddChild(invItemInstance);
            invItemInstance.Position = Vector2.Zero;

        }
    }
}
