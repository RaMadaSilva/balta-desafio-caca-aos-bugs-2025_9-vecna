using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BugStore.Application.Shared; 

public static class KeyRole
{
    public static string HashFilters(object filters)
    {
        var json = JsonSerializer.Serialize(filters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes).Substring(0, 16).ToLowerInvariant();
    }

}
