using Godot;
using System;

public partial class SingleQuestUi : Control
{
    public Action CompletedQuest;
    Label _nameLabel;
    Label _taskDescriptionLabel;
    Button _completeButton;

    public override void _Ready()
    {
        base._Ready();
        _nameLabel = FindChild("Name") as Label;
        _taskDescriptionLabel = FindChild("TaskDescription") as Label;
        if(_completeButton == null) { 
            _completeButton = FindChild("CompleteButton") as Button;
            _completeButton.Pressed += OnCompletedButtonPressed;

        }
    }

    void OnCompletedButtonPressed()
    {
        CompletedQuest?.Invoke();
    }

    public void SetData(string name, string taskDescription, bool finished)
    {
        if(_nameLabel == null || _taskDescriptionLabel == null || _completeButton == null)
        {
            _nameLabel = FindChild("Name") as Label;
            _taskDescriptionLabel = FindChild("TaskDescription") as Label;
            _completeButton = FindChild("CompleteButton") as Button;
            _completeButton.Pressed += OnCompletedButtonPressed;
        }
        _nameLabel.Text = name;
        _taskDescriptionLabel.Text = taskDescription;

        _completeButton.Visible = finished;
    }
}
