using Godot;
using System;

public partial class PlayerUi : Control
{
    Label _levelLabel;
    Label _expLabel;
    public override void _Ready()
    {
        base._Ready();
        _levelLabel = GetNode<Label>("LevelLabel");
        _expLabel = GetNode<Label>("ExpLabel");
    }
    public void SetLevelAndExp(int level, long exp)
    {
        _levelLabel.Text = "Level: " + level;
        _expLabel.Text = "Exp: " + exp;
    }
}
