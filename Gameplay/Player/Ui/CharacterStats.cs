using Godot;
using System;

public partial class CharacterStats : Panel
{
    Label _vitLabel;
    Button _vitButton;

    Label _intLabel;
    Button _intButton;

    Label _strLabel;
    Button _strButton;

    Label _dexLabel;
    Button _dexButton;

    Label _availablePointsLabel;

    Button _closeButton;

    public override void _Ready()
    {
        base._Ready();

        _vitLabel = GetNode("Panel/Stats/Vitality/Label") as Label;
        _vitButton = GetNode("Panel/Stats/Vitality/Button") as Button;
        _vitButton.Pressed += OnVitButtonPressed;

        _intLabel = GetNode("Panel/Stats/Intelligence/Label") as Label;
        _intButton = GetNode("Panel/Stats/Intelligence/Button") as Button;
        _intButton.Pressed += OnIntButtonPressed;

        _strLabel = GetNode("Panel/Stats/Strength/Label") as Label;
        _strButton = GetNode("Panel/Stats/Strength/Button") as Button;
        _strButton.Pressed += OnStrButtonPressed;

        _dexLabel = GetNode("Panel/Stats/Dexterity/Label") as Label;
        _dexButton = GetNode("Panel/Stats/Dexterity/Button") as Button;
        _dexButton.Pressed += OnDexButtonPressed;

        _availablePointsLabel = GetNode("Panel/Stats/AvailablePointsLabel") as Label;

        _closeButton = FindChild("CloseButton") as Button;
        _closeButton.Pressed += () => { Visible = false; };
    }

    public void Update(int vit, int int_, int str, int dex, short availablePoints)
    {
        _vitLabel.Text = "Vitality: " + vit.ToString();
        _intLabel.Text = "Intelligence: " + int_.ToString();
        _strLabel.Text = "Strength: " + str.ToString();
        _dexLabel.Text = "Dexterity: " + dex.ToString();
        _availablePointsLabel.Text = "Available Points: " + availablePoints.ToString();
    }
    public void Update(short availablePoints)
    {
        _availablePointsLabel.Text = "Available Points: " + availablePoints.ToString();
    }
    void OnVitButtonPressed()
    {
        NetworkClient.SendIncreaseStat(StatType.Vitality);
    }
    void OnIntButtonPressed()
    {
        NetworkClient.SendIncreaseStat(StatType.Intelligence);
    }
    void OnStrButtonPressed()
    {
        NetworkClient.SendIncreaseStat(StatType.Strength);
    }
    void OnDexButtonPressed()
    {
        NetworkClient.SendIncreaseStat(StatType.Dexterity);
    }
}
