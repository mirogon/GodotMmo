using Godot;
using System;

public partial class SpawnArea : Node3D
{
    public Guid Id = Guid.NewGuid();
    [Export] public EEnemyType EnemyTypeToSpawn;
    [Export] public int MaxEnemiesAtOnce;
    public (float x, float z) GetSize()
    {
        var cs = GetNode("Area3D").GetNode("CollisionShape3D") as CollisionShape3D;
        BoxShape3D boxShape = cs.Shape as BoxShape3D;
        var size = boxShape.Size * cs.GlobalTransform.Basis.Scale.Abs();

        return (size.X, size.Z);
    }

    public SpawnAreaData ToSpawnAreaData()
    {
        var size = GetSize();
        return new SpawnAreaData(Id, EnemyTypeToSpawn, MaxEnemiesAtOnce, Position.X, Position.Z, size.x, size.z);
    }
}
