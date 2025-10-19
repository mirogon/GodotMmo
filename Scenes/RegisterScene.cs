using Godot;
using System;

public partial class RegisterScene : Node
{
    [Export] LineEdit _signupEmailEdit;
    [Export] LineEdit _signupNameEdit;
    [Export] LineEdit _signupPasswordEdit;
    [Export] Button _submitRegistrationButton;
    [Export] Button _closeButton;

    public override void _Ready()
    {
        base._Ready();
        LoginClient.RegisterUpdate += OnRegisterUpdate;
        _submitRegistrationButton.Pressed += OnSubmitPressed;
        _closeButton.Pressed += OnCloseButtonPressed;
    }

    private void OnCloseButtonPressed()
    {
        QueueFree();
    }

    void OnSubmitPressed()
    {
        LoginClient.SignUp(_signupEmailEdit.Text, _signupNameEdit.Text, _signupPasswordEdit.Text);
    }
    void OnRegisterUpdate(bool result, string message)
    {
        GD.Print("Register update");
        var newInfoWindow = GameManager.InfoWindowScreen.Instantiate();
        InfoWindow iw = newInfoWindow as InfoWindow;
        iw.DescriptionLabel.Text = "Registration failed: " + message;
        if (result)
        {
            iw.DescriptionLabel.Text = "Registration successful!";
        }
        AddChild(newInfoWindow);
    }
}
