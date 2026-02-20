using Godot;
using System;

public partial class Npc : GameEntity
{
    protected NpcTitle _npcTitle;
    public NPCType NpcType{ get; private set; }
    public override void _Ready()
    {
        base._Ready();
        _npcTitle = FindChild("NpcTitle") as NpcTitle;
    }

    public void Init(NPCType npcType)
    {
        _npcTitle = FindChild("NpcTitle") as NpcTitle;
        NpcType = npcType;

        var info = NpcInfo.NpcTypeToNpcInfoDict[npcType];

        _npcTitle.Init(info.Name, info.AvailableQuests.Count > 0);
    }
}
