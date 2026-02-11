using Godot;
using System;

public partial class PlayerUi : Control
{
    CharacterStats _characterStatsUi;

    Label _levelLabel;
    Label _expLabel;
    public override void _Ready()
    {
        base._Ready();
        _levelLabel = GetNode<Label>("LevelLabel");
        _expLabel = GetNode<Label>("ExpLabel");

        _characterStatsUi = GetNode("CharacterStatsUi") as CharacterStats;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsActionJustPressed("ToggleCharacterStats"))
        {
            _characterStatsUi.Visible = !_characterStatsUi.Visible;
        }
    }

    public void SetLevelAndExp(int level, long exp)
    {
        _levelLabel.Text = "Level: " + level;
        _expLabel.Text = "Exp: " + exp;
    }
}
