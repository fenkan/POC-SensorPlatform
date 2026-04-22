using System.Security.Cryptography;
using System.Text;

namespace SensorPlatform.Application.Security
{
    public static class HmacHelper
    {
        public static string CreateSignature(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hash);
        }
    }
}
