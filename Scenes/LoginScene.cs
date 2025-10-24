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
        NetworkClient.KnownCharactersUpdate += OnKnownCharacterUpdate;
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
            NetworkClient.StartClient();
        }
    }
    void OnKnownCharacterUpdate()
    {
        CallDeferred("OnKnownCharactersUpdateMainThread");
    }

    void OnKnownCharactersUpdateMainThread()
    {
        GD.Print("LoginScene Known Char Update");
        var selectCharScene = GameManager.SelectCharacterScene.Instantiate();
        GetTree().Root.AddChild(selectCharScene);
        QueueFree();
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
