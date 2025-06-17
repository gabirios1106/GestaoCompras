using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GestaoCompras.Utils.Converters;

public static class CustomConverter
{
    public static IEnumerable<Claim> ParseClaimsFromJWT(string token)
    {
        var claims = new List<Claim>();
        var payload = token.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        keyValuePairs.TryGetValue("role", out object roles);

        if (roles != null)
        {
            if (roles.ToString().Trim().StartsWith("["))
            {
                var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                foreach (var parsedRole in parsedRoles)
                {
                    var claim = new Claim(ClaimTypes.Role, parsedRole);
                    claims.Add(claim);
                }
            }
            else
            {
                var claim = new Claim(ClaimTypes.Role, roles.ToString());
                claims.Add(claim);
            }

            keyValuePairs.Remove(ClaimTypes.Role);
        }

        var claimsSelected = keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        claims.AddRange(claimsSelected);

        return claims;
    }

    public static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }

    public static string RemoveDiacritics(string accentedString)
    {
        if (!string.IsNullOrEmpty(accentedString))
        {
            var decomposed = accentedString.Normalize(NormalizationForm.FormD);
            var filtered = decomposed.Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
            var newString = new String(filtered.ToArray());

            return newString;
        }

        return string.Empty;
    }

    public static string GetEnumDescription(Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0)
            return attributes[0].Description;
        else
            return value.ToString();
    }

    public static string GetWeekDescription(DateTime initialDate, DateTime endDate, DateTimeFormatInfo dateTimeFormatInfo)
    {
        return $"{initialDate.ToString("dd/MM/yyyy", dateTimeFormatInfo)} - {endDate.ToString("dd/MM/yyyy", dateTimeFormatInfo)}";
    }
}
