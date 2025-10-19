using Godot;
using System;

public partial class CreateNewCharacterScene : Node3D
{
    [Export] public Node3D ClassesSlotBase = new();
    Button _createCharButton;
    Button _leftArrowButton;
    Button _rightArrowButton;
    TextEdit _nameInput;

    int _currentCharSlotSelected = 0;
    const int MAX_SLOTS = 4;

    public override void _Ready()
    {
        base._Ready();
        _createCharButton = GetNode<Button>("MarginContainer/CreateButton");
        _leftArrowButton = GetNode<Button>("MarginContainer/LeftArrowButton");
        _rightArrowButton = GetNode<Button>("MarginContainer/RightArrowButton");
        _nameInput = GetNode<TextEdit>("MarginContainer/NameInput");

        _leftArrowButton.Pressed += _leftArrowButton_Pressed;
        _rightArrowButton.Pressed += _rightArrowButton_Pressed;
        _createCharButton.Pressed += _createCharButton_Pressed;
    }

    private void _createCharButton_Pressed()
    {
        ECharacterClass chosenClass = (ECharacterClass)_currentCharSlotSelected;
        string name = _nameInput.Text;

        var selectCharSceneInstance = GD.Load<PackedScene>("res://Scenes/SelectCharacterScene.tscn").Instantiate();
        GetTree().Root.AddChild(selectCharSceneInstance);
        QueueFree();
    }

    private void _rightArrowButton_Pressed()
    {
        var radians = Godot.Mathf.DegToRad(-90);
        ClassesSlotBase.Rotate(Vector3.Up, radians);
        _currentCharSlotSelected++;
        if(_currentCharSlotSelected == MAX_SLOTS)
        {
            _currentCharSlotSelected = 0;
        }
    }

    private void _leftArrowButton_Pressed()
    {
        var radians = Godot.Mathf.DegToRad(90);
        ClassesSlotBase.Rotate(Vector3.Up, radians);
        _currentCharSlotSelected++;
        if(_currentCharSlotSelected == MAX_SLOTS)
        {
            _currentCharSlotSelected = 0;
        }
    }

}
