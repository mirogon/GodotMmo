using Godot;
using System;

public partial class NpcTitle : Control
{
    Label _label;
    Label _exclamationMark;
    public override void _Ready()
    {
        base._Ready();
        _label = GetNode<Label>("Label");
        _exclamationMark = GetNode<Label>("ExclamationMark");
    }
    public void Init(string name, bool hasQuest)
    {
        _label = GetNode<Label>("Label");
        _exclamationMark = GetNode<Label>("ExclamationMark");
        _label.Text = name;
        _exclamationMark.Visible = hasQuest;
    }
}
