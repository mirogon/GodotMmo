using Godot;
using Godot.Collections;
using System;

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
        QuestInfo info = QuestInfo.QuestTypeToQuestInfoDict[quest.Id];

        if (_questInstances.ContainsKey(quest.Id))
        {
            var scene = _questInstances[quest.Id];
            scene.SetData(info.Name, info.TaskDescriptions[0]);
        }
        else
        {
            var sceneInstance = _singleQuestUiScene.Instantiate() as SingleQuestUi;
            sceneInstance.SetData(info.Name, info.TaskDescriptions[0]);
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
}
