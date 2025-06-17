namespace GestaoCompras.Web.Utils;

public class AppSettings
{
    public static string GetAppSettings(string envName, string key)
    {
        Dictionary<string, string> appSettings = [];
        var value = string.Empty;

        appSettings.Clear();

        appSettings.Add($"{envName}:APIEndpoint", (envName == "Production" ? "https://backend.mesaderei.com.br:30497/api/" : "https://localhost:30497/api/"));
        appSettings.Add($"{envName}:AppCredential:AppId", (envName == "Production" ? "aa7f018a-3fe4-49aa-b9be-ce9dc6a7a406" : "59f01c7b-450c-4000-89f6-e91f0a1e28cb"));
        appSettings.Add($"{envName}:AppCredential:AppSecret", (envName == "Production" ? "07bcb6f9-3898-4a99-acc8-5bcaafad3c81" : "881e2c2b-4b18-4cec-b4e1-fc098680dd9f"));

        if (appSettings.TryGetValue($"{envName}:{key}", out string outValue))
            value = outValue;

        return value;
    }

}
