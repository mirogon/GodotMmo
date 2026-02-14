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
    public static Vector3 PositionToVector3(M1Vector3 pos)
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

    public static Godot.Collections.Dictionary Camera3DRaycast(Camera3D camera) 
    {
        Vector2 mousePos = camera.GetViewport().GetMousePosition();
        Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
        Vector3 rayDir = camera.ProjectRayNormal(mousePos);
        Vector3 rayEnd = rayOrigin + rayDir * 250f;

        var queryParams = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
        queryParams.CollideWithAreas = true;
        queryParams.CollideWithBodies = true;

        var spaceState = camera.GetWorld3D().DirectSpaceState;

        /*
           position: Vector2 # point in world space for collision
           normal: Vector2 # normal in world space for collision
           collider: Object # Object collided or null (if unassociated)
           collider_id: ObjectID # Object it collided against
           rid: RID # RID it collided against
           shape: int # shape index of collider
           metadata: Variant() # metadata of collider
        */
        var result = spaceState.IntersectRay(queryParams);
        return result;
    }

    public static bool MouseIsInControl(Control c)
    {
        var localMousePos = c.GetLocalMousePosition();
        if(localMousePos.X < 0 || localMousePos.X > c.Size.X)
        {
            return false;
        }
        if(localMousePos.Y < 0 || localMousePos.Y > c.Size.Y)
        {
            return false;
        }
        return true;
    }
    public static float GetFloorHeight(float x, float z, World3D world)
    {
        Vector3 rayOrigin = new Vector3(x, 1000, z);
        Vector3 rayDir = Vector3.Down;
        Vector3 rayEnd = rayOrigin + rayDir * 2000;

        var queryParams = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd, 0b1);
        queryParams.CollideWithAreas = true;
        queryParams.CollideWithBodies = true;

        var spaceState = world.DirectSpaceState;

        /*
           position: Vector2 # point in world space for collision
           normal: Vector2 # normal in world space for collision
           collider: Object # Object collided or null (if unassociated)
           collider_id: ObjectID # Object it collided against
           rid: RID # RID it collided against
           shape: int # shape index of collider
           metadata: Variant() # metadata of collider
        */
        var result = spaceState.IntersectRay(queryParams);
        if (!result.ContainsKey("collider")) { return 0; }
        if((Object)result["collider"] == null) { return 0; }
        return ((Vector3)result["position"]).Y;

    }
}
