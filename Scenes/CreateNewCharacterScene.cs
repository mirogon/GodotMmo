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
    const float SLOT_ROTATION_TIME = 0.5f;

    Quaternion _targetRotation;

    public override void _Ready()
    {
        base._Ready();

        _targetRotation = ClassesSlotBase.Quaternion;

        _createCharButton = GetNode<Button>("MarginContainer/CreateButton");
        _leftArrowButton = GetNode<Button>("MarginContainer/LeftArrowButton");
        _rightArrowButton = GetNode<Button>("MarginContainer/RightArrowButton");
        _nameInput = GetNode<TextEdit>("MarginContainer/NameInput");

        _leftArrowButton.Pressed += _leftArrowButton_Pressed;
        _rightArrowButton.Pressed += _rightArrowButton_Pressed;
        _createCharButton.Pressed += _createCharButton_Pressed;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Quaternion newRot = ClassesSlotBase.Quaternion.Slerp(_targetRotation, (float)delta * 10.0f);
        ClassesSlotBase.Quaternion = newRot.Normalized();
    }

    private void _createCharButton_Pressed()
    {
        ECharacterClass chosenClass = (ECharacterClass)_currentCharSlotSelected;
        string name = _nameInput.Text;

        NetworkClient.CreateNewCharacter((byte)_currentCharSlotSelected, name, chosenClass);

        var selectCharSceneInstance = GD.Load<PackedScene>("res://Scenes/SelectCharacterScene.tscn").Instantiate();
        GetTree().Root.AddChild(selectCharSceneInstance);
        QueueFree();
    }

    private void _leftArrowButton_Pressed()
    {
        var baseRot = _targetRotation.Normalized();
        _targetRotation = (baseRot * new Quaternion(Vector3.Up, Mathf.DegToRad(90))).Normalized();

        _currentCharSlotSelected++;
        if(_currentCharSlotSelected == MAX_SLOTS)
        {
            _currentCharSlotSelected = 0;
        }
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
        PrintCurrentSlot();
    }

    void PrintCurrentSlot()
    {
        GD.Print("Current Slot: " + _currentCharSlotSelected);
    }


}
