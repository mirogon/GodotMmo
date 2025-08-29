using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class LoginClient
{
    static System.Net.Http.HttpClient _httpClient = new();
    public static async Task SignUp(string email, string name, string password)
    {
        SignupRequest request = new SignupRequest(email, name, "", password);
        string json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync("https://localhost:7285/signup", content);

        if (response.IsSuccessStatusCode)
        {
        }
        GD.Print("Sign up succeeded: " + response.IsSuccessStatusCode);

        return;
    }
    public static async Task Login(string email, string password)
    {
        LoginRequest request = new (email, password);
        string json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync("https://localhost:7285/login", content);
        var responseStr = response.Content.ToString();
        var reqResponse = JsonSerializer.Deserialize<RequestResponse>(responseStr);

        if (reqResponse.Success)
        {
            GD.Print("Login succeeded: ");
        }
        else
        {
            GD.Print("Login Failed: " + reqResponse.Message);
        }
        return;
    }

}
