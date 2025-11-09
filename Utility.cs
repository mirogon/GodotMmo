using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Utility
{
    public static float EulerRotationValueNormalized(float value)
    {
        float yNormalized = value % 360f;
        if (yNormalized < 0) yNormalized += 360f;
        return yNormalized;
    }
    public static Vector3 PositionToVector3(Position pos)
    {
        return new Vector3(pos.X, 0, pos.Z);
    }
    public static int GridXAndYPosToIndex(int x, int y, int gridTileWidth)
    {
        return y * gridTileWidth + x;
    }
    public static (int x,int y) LocalMousePosToTilePos(Vector2 mousePos, int tilePixelSize)
    {
        int xTile = (int)(mousePos.X / tilePixelSize);
        if(mousePos.X % tilePixelSize > 0)
        {
            //++xTile;
        }
        int yTile = (int)(mousePos.Y / tilePixelSize);
        if(mousePos.Y % tilePixelSize > 0)
        {
            //++yTile;
        }
        return (xTile, yTile);
    }
}
