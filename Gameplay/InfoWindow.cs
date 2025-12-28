using Godot;
using System;

public partial class InfoWindow : Control
{
    [Export] public Button OkButton;
    [Export] public Label DescriptionLabel;
    public override void _Ready()
    {
        base._Ready();
        OkButton.Pressed += OnOkButtonPressed;

    }

    public void Init(string description)
    {
        DescriptionLabel.Text = description;
    }

    void OnOkButtonPressed()
    {
        QueueFree();
    }
}
