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

    Label _characterNameLabel;

    int _currentCharSlotSelected = 0;
    const int MAX_SLOTS = 4;

    Quaternion _targetRotation;

    public override void _Ready()
    {
        base._Ready();

        _targetRotation = CharacterSlotBase.Quaternion;

        _createCharButton = GetNode<Button>("MarginContainer/CreateButton");
        _leftArrowButton = GetNode<Button>("MarginContainer/LeftArrowButton");
        _rightArrowButton = GetNode<Button>("MarginContainer/RightArrowButton");
        _characterNameLabel = GetNode<Label>("MarginContainer/CharacterNameLabel");

        _leftArrowButton.Pressed += _leftArrowButton_Pressed;
        _rightArrowButton.Pressed += _rightArrowButton_Pressed;
        _createCharButton.Pressed += _createCharButton_Pressed;

        _characterNameLabel.Text = "";
        if(NetworkClient.KnownCharacters.ContainsKey(0))
        {
            _characterNameLabel.Text = NetworkClient.KnownCharacters[0].Name;
            _createCharButton.Text = "Play";
        }

    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        Quaternion newRot = CharacterSlotBase.Quaternion.Slerp(_targetRotation, (float)delta * 10.0f);
        CharacterSlotBase.Quaternion = newRot.Normalized();
    }

    private void _createCharButton_Pressed()
    {
        if (NetworkClient.KnownCharacters.ContainsKey(_currentCharSlotSelected)) { return; }
        var createNewCharScene = GD.Load<PackedScene>("res://Scenes/CreateNewCharacterScene.tscn").Instantiate();
        GetTree().Root.AddChild(createNewCharScene);
        QueueFree();
    }

    private void _leftArrowButton_Pressed()
    {
        var baseRot = _targetRotation.Normalized();
        _targetRotation = (baseRot * new Quaternion(Vector3.Up, Mathf.DegToRad(90))).Normalized();

        _currentCharSlotSelected--;
        if(_currentCharSlotSelected < 0)
        {
            _currentCharSlotSelected = MAX_SLOTS - 1;
        }
        UpdateCharacterInfo();
        PrintCurrentSlot();
    }
    private void _rightArrowButton_Pressed()
    {
        var baseRot = _targetRotation.Normalized();
        _targetRotation = (baseRot * new Quaternion(Vector3.Up, Mathf.DegToRad(-90))).Normalized();

        _currentCharSlotSelected++;
        if(_currentCharSlotSelected == MAX_SLOTS)
        {
            _currentCharSlotSelected = 0;
        }
        UpdateCharacterInfo();
        PrintCurrentSlot();
    }

    void UpdateCharacterInfo()
    {
        _characterNameLabel.Text = "";
        _createCharButton.Text = "Create";

        if (!NetworkClient.KnownCharacters.ContainsKey(_currentCharSlotSelected)) { return; }
        var character = NetworkClient.KnownCharacters[_currentCharSlotSelected];
        _characterNameLabel.Text = character.Name;
        _createCharButton.Text = "Play";
    }

    void PrintCurrentSlot()
    {
        GD.Print("Current Slot: " + _currentCharSlotSelected);
    }
}
