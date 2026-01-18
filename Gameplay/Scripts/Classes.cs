using Godot;
using Godot.Collections;
using System;

public partial class Classes : Node
{
    public static Dictionary<ECharacterClass, string> ClassToPreviewSceneDictionary = new()
    {
        {ECharacterClass.Unknown, "res://Gameplay/CharacterScreen/Preview/WarriorPreview.tscn"},
        {ECharacterClass.Warrior, "res://Gameplay/CharacterScreen/Preview/WarriorPreview.tscn"},
        {ECharacterClass.Ninja, "res://Gameplay/CharacterScreen/Preview/WarriorPreview.tscn"},
        {ECharacterClass.Sura, "res://Gameplay/CharacterScreen/Preview/WarriorPreview.tscn"},
        {ECharacterClass.Shaman, "res://Gameplay/CharacterScreen/Preview/WarriorPreview.tscn"},
    };
    public static Dictionary<EEnemyType, string> EnemyTypeToEnemySceneDictionary = new()
    {
        {EEnemyType.TestEnemy, "res://Gameplay/Enemies/TestEnemy.tscn"},
        {EEnemyType.Rat, "res://Gameplay/Enemies/Rat/Rat.tscn"},
    };

}
