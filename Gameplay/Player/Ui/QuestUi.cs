using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class QuestUi : Control
{
    PackedScene _singleQuestUiScene = ResourceLoader.Load<PackedScene>("res://Gameplay/Player/Ui/SingleQuestUi.tscn");
    Control _scrollContainer;

    Dictionary<QuestId, SingleQuestUi> _questInstances = new();
    public override void _Ready()
    {
        base._Ready();
        _scrollContainer = FindChild("ScrollContainerChild") as Control;
    }
    public void AddOrUpdateQuest(QuestData quest)
    {
        Quest info = Quest.QuestIdToQuest(quest.Id, quest.ProgressData.ToList());

        if (_questInstances.ContainsKey(quest.Id))
        {
            var scene = _questInstances[quest.Id];
            scene.SetData(info.GetName(), info.GetTaskDescriptions()[0], quest.Finished);
        }
        else
        {
            var sceneInstance = _singleQuestUiScene.Instantiate() as SingleQuestUi;
            sceneInstance.SetData(info.GetName(), info.GetTaskDescriptions()[0], quest.Finished);
            sceneInstance.CompletedQuest += () => { OnCompletedQuest(sceneInstance, quest.Id); };
            _scrollContainer.AddChild(sceneInstance);

            if (!_questInstances.ContainsKey(quest.Id))
            {
                _questInstances.Add(quest.Id, sceneInstance);
            }
            else
            {
                _questInstances[quest.Id] = sceneInstance;
            }
        }
    }

    void OnCompletedQuest(SingleQuestUi completedInstance, QuestId id)
    {
        _scrollContainer.RemoveChild(completedInstance);
        completedInstance.QueueFree();
        NetworkClient.CompleteQuest(id);
    }
}
