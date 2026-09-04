using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace SOLTIUS_Web_API_Add_On.Authentication
{
    public class RefreshTokenStore
    {
        private readonly ConcurrentDictionary<string, RefreshTokenEntry> _tokens
            = new ConcurrentDictionary<string, RefreshTokenEntry>();

        private readonly int _expiryMinutes;

        public RefreshTokenStore(int expiryMinutes = 10080) // default 7 days
        {
            _expiryMinutes = expiryMinutes;
        }

        public string Create(string clientId)
        {
            string token = GenerateRandomToken();
            _tokens[token] = new RefreshTokenEntry
            {
                ClientId = clientId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_expiryMinutes)
            };
            return token;
        }

        public string ValidateAndConsume(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;

            if (!_tokens.TryRemove(refreshToken, out var entry))
                return null;

            if (DateTime.UtcNow > entry.ExpiresAt)
                return null;

            return entry.ClientId;
        }

        private static string GenerateRandomToken()
        {
            byte[] bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private class RefreshTokenEntry
        {
            public string ClientId { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
