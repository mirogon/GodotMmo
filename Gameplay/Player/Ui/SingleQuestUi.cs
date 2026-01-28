using Godot;
using System;

public partial class SingleQuestUi : Control
{
    Label _nameLabel;
    Label _taskDescriptionLabel;
    public override void _Ready()
    {
        base._Ready();
        _nameLabel = FindChild("Name") as Label;
        _taskDescriptionLabel = FindChild("TaskDescription") as Label;
    }
    public void SetData(string name, string taskDescription)
    {
        if(_nameLabel == null || _taskDescriptionLabel == null)
        {
            _nameLabel = FindChild("Name") as Label;
            _taskDescriptionLabel = FindChild("TaskDescription") as Label;
        }
        _nameLabel.Text = name;
        _taskDescriptionLabel.Text = taskDescription;
    }
}
