using Godot;
using System;
using System.Collections.Generic;

public partial class HoverUi : Panel
{
    Label _nameLabel;
    Label _statsLabel;
    public override void _Ready()
    {
        base._Ready();
        _nameLabel = GetNode("VBoxContainer/NameLabel") as Label;
        _statsLabel = GetNode("VBoxContainer/StatsLabel") as Label;
    }
    public void SetData(string name, List<EquipmentBonus> stats)
    {
        _nameLabel = GetNode("VBoxContainer/NameLabel") as Label;
        _statsLabel = GetNode("VBoxContainer/StatsLabel") as Label;

        _nameLabel.Text = name;
        _statsLabel.Text = "";
        for(int i = 0; i < stats.Count; ++i)
        {
            var current = stats[i];
            var statText = EquipmentBonus.EquipmentBonusText(current);
            _statsLabel.Text += statText;
            if(i < stats.Count - 1)
            {
                _statsLabel.Text += "\n";
            }
        }

    }
}
