using Godot;
using System;

public partial class RegisterLoginScreen : Panel
{
    [Export] LineEdit _signupEmailEdit;
    [Export] LineEdit _signupNameEdit;
    [Export] LineEdit _signupPasswordEdit;
    [Export] Button _submitRegistrationButton;

    [Export] LineEdit _loginEmailEdit;
    [Export] LineEdit _loginPasswordEdit;
    [Export] Button _loginButton;

    public override void _Ready()
    {
        base._Ready();
        _submitRegistrationButton.Pressed += OnSubmitPressed;
        _loginButton.Pressed += OnLoginPressed;

        LoginClient.LoginUpdate += OnLoginUpdate;
        LoginClient.RegisterUpdate += OnRegisterUpdate;
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

    void OnSubmitPressed()
    {
        LoginClient.SignUp(_signupEmailEdit.Text, _signupNameEdit.Text, _signupPasswordEdit.Text);
    }

    void OnLoginPressed()
    {
        LoginClient.Login(_loginEmailEdit.Text, _loginPasswordEdit.Text);
    }
}
