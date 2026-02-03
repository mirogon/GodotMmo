using Godot;
using System;
using System.Collections.Generic;

public partial class NpcUi : Control
{
    Button _closeButton;
    Label _titleLabel;
    Label _descriptionLabel;
    List<QuestId> _availableQuests = new();
    Container _questsScrollContainer;

    PackedScene _availableQuestEntryScene = ResourceLoader.Load<PackedScene>("res://Gameplay/Npc/NpcUi/AvailableQuestEntry.tscn");

    public override void _Ready()
    {
        base._Ready();
        _closeButton = FindChild("CloseButton") as Button;
        _closeButton.Pressed += OnCloseButtonPressed;
        _titleLabel = FindChild("Title") as Label;
        _descriptionLabel = FindChild("Description") as Label;
        _questsScrollContainer = FindChild("ScrollLayout") as Container;
    }

    public void Init(string title, string description, List<QuestId> availableQuests)
    {
        _closeButton = FindChild("CloseButton") as Button;
        _titleLabel = FindChild("Title") as Label;
        _descriptionLabel = FindChild("Description") as Label;
        _questsScrollContainer = FindChild("ScrollLayout") as Container;

        _titleLabel.Text = title;
        _descriptionLabel.Text = description;
        _availableQuests = availableQuests;

        for(int i = 0; i < _availableQuests.Count; ++i)
        {
            var questEntryInstance = _availableQuestEntryScene.Instantiate() as AvailableQuestEntry;
            Quest q = Quest.QuestIdToQuest(_availableQuests[i], new());
            questEntryInstance.Init(_availableQuests[i], q.GetName());
            _questsScrollContainer.AddChild(questEntryInstance);
            questEntryInstance.AcceptedQuest += OnQuestAccepted;
        }
    }

    void OnQuestAccepted(QuestId id)
    {
        GD.Print("QUEST ACCEPTED");
        NetworkClient.AcceptQuest(id);
    }

    void OnCloseButtonPressed()
    {
        QueueFree();
    }
}
