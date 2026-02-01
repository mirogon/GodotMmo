using Godot;
using System;

public partial class NpcTitle : Control
{
    Label _label;
    public override void _Ready()
    {
        base._Ready();
        _label = GetNode<Label>("Label");
    }
    public void Init(string name)
    {
        _label = GetNode<Label>("Label");
        _label.Text = name;
    }
}
