using Godot;
using System;

public partial class Npc : GameEntity
{
    NpcTitle _npcTitle;
    public override void _Ready()
    {
        base._Ready();
        _npcTitle = FindChild("NpcTitle") as NpcTitle;
    }

    public void Init(NPCType npcType)
    {
        _npcTitle = FindChild("NpcTitle") as NpcTitle;

        var info = NpcInfo.NpcTypeToNpcInfoDict[npcType];

        _npcTitle.Init(info.Name);
    }
}
