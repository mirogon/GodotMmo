using Godot;
using Godot.Collections;
using System;

public partial class Classes : Node
{
    public static Dictionary<ECharacterClass, string> ClassToPreviewSceneDictionary = new()
    {
        {ECharacterClass.Unknown, "res://Scenes/Classes/Preview/Warrior.tscn"},
        {ECharacterClass.Warrior, "res://Scenes/Classes/Preview/Warrior.tscn"},
        {ECharacterClass.Ninja, "res://Scenes/Classes/Preview/Warrior.tscn"},
        {ECharacterClass.Sura, "res://Scenes/Classes/Preview/Warrior.tscn"},
        {ECharacterClass.Shaman, "res://Scenes/Classes/Preview/Warrior.tscn"},
    };
}
