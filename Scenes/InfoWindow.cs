using Godot;
using System;

public partial class InfoWindow : Control
{
    [Export] Button _okButton;
    [Export] Label __descriptionLabel;
    public override void _Ready()
    {
        base._Ready();
        _okButton.Pressed += OnOkButtonPressed;

    }

    public void Init(string description)
    {
        __descriptionLabel.Text = description;
    }

    void OnOkButtonPressed()
    {
        QueueFree();
    }
}
