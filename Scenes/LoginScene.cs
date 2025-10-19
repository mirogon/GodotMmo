using Godot;
using System;

public partial class LoginScene : Panel
{
    [Export] PackedScene RegisterScene;
    [Export] LineEdit _loginEmailEdit;
    [Export] LineEdit _loginPasswordEdit;
    [Export] Button _loginButton;
    [Export] Button _createNewAccountButton;

    public override void _Ready()
    {
        base._Ready();
        _loginButton.Pressed += OnLoginPressed;
        _createNewAccountButton.Pressed += OnCreateNewAccountPressed;

        LoginClient.LoginUpdate += OnLoginUpdate;
    }

    void OnLoginUpdate(bool result, string message)
    {
        if (!result)
        {
            var newInfoWindow = GameManager.InfoWindowScreen.Instantiate();
            InfoWindow iw = newInfoWindow as InfoWindow;
            iw.DescriptionLabel.Text = "Login failed: " + message;
            AddChild(newInfoWindow);
        }
        else
        {
            var mainSceneInstance = GameManager.GameScene.Instantiate();
            GetTree().Root.AddChild(mainSceneInstance);
            QueueFree();
        }
    }

    void OnLoginPressed()
    {
        LoginClient.Login(_loginEmailEdit.Text, _loginPasswordEdit.Text);
    }
    void OnCreateNewAccountPressed()
    {
        var registerSceneInstance = RegisterScene.Instantiate();
        AddChild(registerSceneInstance);
    }
}
