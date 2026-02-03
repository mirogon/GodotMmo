using Godot;
using System;

public partial class AvailableQuestEntry : MarginContainer
{
    public Action<QuestId> AcceptedQuest;
    Label _questName;
    Button _acceptButton;
    QuestId _questId;
    public override void _Ready()
    {
        base._Ready();
        _questName = FindChild("QuestName") as Label;
        _acceptButton = FindChild("AcceptButton") as Button;
        _acceptButton.Pressed += OnAcceptButtonPressed;
    }

    void OnAcceptButtonPressed()
    {
        AcceptedQuest?.Invoke(_questId);
    }

    public void Init(QuestId questId, string questName)
    {
        _questName = FindChild("QuestName") as Label;
        _acceptButton = FindChild("AcceptButton") as Button;

        _questId = questId;
        _questName.Text = questName;
    }
}
