using Godot;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Xml;



public class MapAnalyzer
{
    public static List<SpawnArea> GetSpawnAreasOnMap(Node sceneRootNode)
    {
        List<SpawnArea> areas = new();

        var spawnAreasRoot = sceneRootNode.FindChild("SpawnAreas") as Node3D;
        var allChildren = spawnAreasRoot.GetChildren();

        for(int i= 0; i < allChildren.Count; ++i)
        {
            var current = allChildren[i] as SpawnArea;
            if(current == null) { continue; }
            areas.Add(current);
        }

        return areas;
    }

    public static string CreateSpawnAreasJson(List<SpawnArea> areas)
    {
        List<SpawnAreaData> datas = new List<SpawnAreaData>();
        for(int i = 0; i < areas.Count; ++i)
        {
            var current = areas[i];
            var data = current.ToSpawnAreaData();

            datas.Add(data);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string jsonString = JsonSerializer.Serialize(datas, options);
        return jsonString;
    }
}
