using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class SelectCharacterScene : Node3D
{
    [Export] public Node3D CharacterSlotBase = new();
    Button _createCharButton;
    Button _leftArrowButton;
    Button _rightArrowButton;

    int _currentCharSlotSelected = 0;
    const int MAX_SLOTS = 4;

    public override void _Ready()
    {
        base._Ready();
        _createCharButton = GetNode<Button>("MarginContainer/CreateButton");
        _leftArrowButton = GetNode<Button>("MarginContainer/LeftArrowButton");
        _rightArrowButton = GetNode<Button>("MarginContainer/RightArrowButton");

        _leftArrowButton.Pressed += _leftArrowButton_Pressed;
        _rightArrowButton.Pressed += _rightArrowButton_Pressed;
        _createCharButton.Pressed += _createCharButton_Pressed;
    }

    private void _createCharButton_Pressed()
    {
        var createNewCharScene = GD.Load<PackedScene>("res://Scenes/CreateNewCharacterScene.tscn").Instantiate();
        GetTree().Root.AddChild(createNewCharScene);
        QueueFree();
    }

    private void _leftArrowButton_Pressed()
    {
        var radians = Godot.Mathf.DegToRad(90);
        CharacterSlotBase.Rotate(Vector3.Up, radians);
        _currentCharSlotSelected--;
        if(_currentCharSlotSelected < 0)
        {
            _currentCharSlotSelected = MAX_SLOTS - 1;
        }
        PrintCurrentSlot();
    }
    private void _rightArrowButton_Pressed()
    {
        var radians = Godot.Mathf.DegToRad(-90);
        CharacterSlotBase.Rotate(Vector3.Up, radians);
        _currentCharSlotSelected++;
        if(_currentCharSlotSelected == MAX_SLOTS)
        {
            _currentCharSlotSelected = 0;
        }
        PrintCurrentSlot();
    }

    void PrintCurrentSlot()
    {
        GD.Print("Current Slot: " + _currentCharSlotSelected);
    }
}
