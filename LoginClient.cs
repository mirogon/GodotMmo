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
    public static Action<bool, string> RegisterUpdate;
    public static Action<bool, string> LoginUpdate;

    static System.Net.Http.HttpClient _httpClient = new();
    public static async Task SignUp(string email, string name, string password)
    {
        SignupRequest request = new SignupRequest(email, name, "", password);
        string json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync("https://localhost:7285/signup", content);

        var responseContentStr = GetResponseContentStringFromResponse(response);
        var reqResponse = ResponseContentStringToRequestResponse(responseContentStr);

        if (reqResponse.Success)
        {
            RegisterUpdate?.Invoke(true, "");
        }
        else
        {
            RegisterUpdate?.Invoke(false, reqResponse.Message);
        }
        GD.Print("Sign up succeeded: " + reqResponse.Success);
        return;
    }
    public static async Task Login(string email, string password)
    {
        GD.Print("Trying to log in...");
        LoginRequest request = new (email, password);
        try
        {
            string json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("https://localhost:7285/login", content);

            var responseContentStr = GetResponseContentStringFromResponse(response);
            var reqResponse = ResponseContentStringToRequestResponse(responseContentStr);

            if (reqResponse.Success)
            {
                GD.Print("Login succeeded: ");
                LoginUpdate?.Invoke(true, "");
            }
            else
            {
                GD.Print("Login Failed: " + reqResponse.Message);
                LoginUpdate?.Invoke(false, reqResponse.Message);
            }
        }
        catch (Exception ex)
        {
            GD.Print("Login Failed: " + ex.Message);
            LoginUpdate?.Invoke(false, ex.Message);
        }

        return;
    }

    public static string GetResponseContentStringFromResponse(HttpResponseMessage responseMessage)
    {
        var stream = responseMessage.Content.ReadAsStream();
        var len = stream.Length;
        byte[] responseContentBytes = new byte[len];
        stream.Read(responseContentBytes);
        string responseContentStr = Encoding.UTF8.GetString(responseContentBytes);
        return responseContentStr;
    }
    public static RequestResponse ResponseContentStringToRequestResponse(string contentStr)
    {
        JsonSerializerOptions opt = new();
        opt.PropertyNameCaseInsensitive = true;
        return JsonSerializer.Deserialize<RequestResponse>(contentStr, opt);
    }
}
