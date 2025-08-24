using Godot;
using System;

//Attach to Camera
public partial class MMOCamera : Camera3D
{
    [Export] float _cameraSpeed = 0.02f;
    [Export] float _cameraChangeDistanceSpeed = 1.0f;
    [Export] float _minCameraDistance = 2.0f;
    [Export] float _maxCameraDistance = 12.0f;
    [Export] float _minCameraXRotation = -30.0f;
    [Export] float _maxCameraXRotation = 80.0f;

    [Export] Node3D _playerMesh;
    Node3D _cameraPivot;

    float _pitch = 0;
    float _yaw = 0;

    public override void _Ready()
    {
        base._Ready();
        _cameraPivot = GetParent<Node3D>();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        var leftMouseDown = Input.IsMouseButtonPressed(MouseButton.Left);
        var rightMouseDown = Input.IsMouseButtonPressed(MouseButton.Right);

        if(@event is InputEventMouseMotion motion)
        {

            if(!leftMouseDown && !rightMouseDown) { return; }
            var mouseX = motion.Relative.X;
            var mouseY = motion.Relative.Y;
            _pitch += -mouseY * _cameraSpeed;
            _yaw += -mouseX * _cameraSpeed;

            _cameraPivot.Rotation = new Vector3(_pitch, _yaw, 0);


        }
        if(@event is InputEventMouseButton mouseButton)
        {
            switch (mouseButton.ButtonIndex)
            {
                case MouseButton.WheelUp: HandleCameraZoom(-1); break;
                case MouseButton.WheelDown: HandleCameraZoom(1); break;
            }
        }
        if (rightMouseDown)
        {
            var pivotForward = -_cameraPivot.GlobalTransform.Basis.Z;
            pivotForward.Y = 0;
            pivotForward = pivotForward.Normalized();
            _playerMesh.LookAt(_playerMesh.GlobalPosition + pivotForward, Vector3.Up);
        }
    }

    void HandleCameraZoom(int mouseWheelDir)
    {
        var prevCamPos = GlobalPosition;
        Position += Basis.Z * mouseWheelDir * _cameraChangeDistanceSpeed;
    }
}
