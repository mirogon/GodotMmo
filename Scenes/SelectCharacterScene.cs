using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class SelectCharacterScene : Node3D
{
    [Export] public Node3D CharacterSlotBase = new();
    [Export] public Node3D[] CharacterSlots;
    Button _createCharButton;
    Button _deleteCharButton;
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
        _deleteCharButton = GetNode<Button>("MarginContainer/DeleteCharacterButton");
        _leftArrowButton = GetNode<Button>("MarginContainer/LeftArrowButton");
        _rightArrowButton = GetNode<Button>("MarginContainer/RightArrowButton");
        _characterNameLabel = GetNode<Label>("MarginContainer/CharacterNameLabel");

        _leftArrowButton.Pressed += _leftArrowButton_Pressed;
        _rightArrowButton.Pressed += _rightArrowButton_Pressed;
        
        _createCharButton.Pressed += _createCharButton_Pressed;
        _deleteCharButton.Pressed += _deleteCharButton_Pressed;

        _characterNameLabel.Text = "";
        if(NetworkClient.KnownCharacters.ContainsKey(0))
        {
            UpdateCharacterInfo();
            UpdateCharacterPreviews();
        }

        NetworkClient.GetCharactersUpdate();
        NetworkClient.KnownCharactersUpdate += OnKnownCharactersUpdate;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        NetworkClient.KnownCharactersUpdate -= OnKnownCharactersUpdate;
        GD.Print("Select Char Scene Exit Tree");
    }

    private void OnKnownCharactersUpdate()
    {
        CallDeferred("OnKnownCharactersUpdateDeferred");
    }

    private void OnKnownCharactersUpdateDeferred()
    {
        UpdateCharacterInfo();
        UpdateCharacterPreviews();
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
        var createNewCharScene = GD.Load<PackedScene>("res://Scenes/CreateNewCharacterScene.tscn").Instantiate() as CreateNewCharacterScene;
        createNewCharScene.Initialize(_currentCharSlotSelected);
        GetTree().Root.AddChild(createNewCharScene);
        QueueFree();
    }
    private void _deleteCharButton_Pressed()
    {
        NetworkClient.DeleteCharacter((byte)_currentCharSlotSelected);
        NetworkClient.GetCharactersUpdate();
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

    void UpdateCharacterPreviews()
    {
        foreach (var slot in CharacterSlots)
        {
            var allChildren = slot.GetChildren();
            foreach (var child in allChildren)
            {
                if (child.Name.ToString().Contains("Base")) { continue; }
                slot.RemoveChild(child);
            }
        }
        for(int i = 0; i < NetworkClient.KnownCharacters.Count; ++i)
        {
            var current = NetworkClient.KnownCharacters[i];
            ECharacterClass charClass = current.Class;
            var previewScenePath = Classes.ClassToPreviewSceneDictionary[charClass];
            var characterPreviewScene = GD.Load<PackedScene>(previewScenePath);
            var characterPreviewSceneInstance = characterPreviewScene.Instantiate();
            var charSlot = CharacterSlots[current.Slot];
            charSlot.AddChild(characterPreviewSceneInstance);
        }
    }

    void PrintCurrentSlot()
    {
        GD.Print("Current Slot: " + _currentCharSlotSelected);
    }
}
