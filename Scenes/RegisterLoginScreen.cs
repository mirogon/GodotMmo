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
