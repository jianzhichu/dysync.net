using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace dy.net.utils
{
    /// <summary>
    /// JWT 生成与密钥管理。
    /// 优先使用配置 Jwt:Key；未配置时，在数据库目录持久化生成 jwt.key，
    /// 确保应用或容器重启后旧 Token 仍能继续使用到正常过期时间。
    /// </summary>
    public sealed class JwtTokenService
    {
        private const int MinimumKeyBytes = 32;
        private const int DefaultExpireDays = 7;
        private readonly byte[] _keyBytes;

        public JwtTokenService(IConfiguration configuration, string dbPath)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            ExpireDays = configuration.GetValue<int?>("Jwt:ExpireDays") ?? DefaultExpireDays;
            if (ExpireDays <= 0 || ExpireDays > 365)
            {
                throw new InvalidOperationException("Jwt:ExpireDays 必须在 1 到 365 之间。");
            }

            Issuer = configuration["Jwt:Issuer"]?.Trim();
            if (string.IsNullOrWhiteSpace(Issuer))
            {
                Issuer = "dysync.net";
            }

            Audience = configuration["Jwt:Audience"]?.Trim();
            if (string.IsNullOrWhiteSpace(Audience))
            {
                Audience = "dysync.web";
            }

            var configuredKey = configuration["Jwt:Key"]?.Trim();
            var key = string.IsNullOrWhiteSpace(configuredKey)
                ? GetOrCreatePersistentKey(dbPath)
                : configuredKey;

            _keyBytes = Encoding.UTF8.GetBytes(key);
            if (_keyBytes.Length < MinimumKeyBytes)
            {
                throw new InvalidOperationException(
                    $"JWT 签名密钥至少需要 {MinimumKeyBytes} 个 UTF-8 字节，请设置 Jwt:Key 或删除无效的 jwt.key 后重启。");
            }
        }

        public int ExpireDays { get; }

        public string Issuer { get; }

        public string Audience { get; }

        public long ExpireMilliseconds => checked((long)TimeSpan.FromDays(ExpireDays).TotalMilliseconds);

        public TokenValidationParameters CreateValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_keyBytes),
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(60)
            };
        }

        public string GenerateToken(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("用户名不能为空。", nameof(username));
            }

            var now = DateTime.UtcNow;
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(_keyBytes),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddDays(ExpireDays),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GetOrCreatePersistentKey(string dbPath)
        {
            var keyDirectory = string.IsNullOrWhiteSpace(dbPath)
                ? Path.Combine(AppContext.BaseDirectory, "db")
                : Path.Combine(Path.GetFullPath(dbPath), "db");

            Directory.CreateDirectory(keyDirectory);

            var keyPath = Path.Combine(keyDirectory, "jwt.key");
            if (File.Exists(keyPath))
            {
                var existingKey = File.ReadAllText(keyPath, Encoding.UTF8).Trim();
                if (!string.IsNullOrWhiteSpace(existingKey))
                {
                    return existingKey;
                }
            }

            var generatedKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tempPath = keyPath + ".tmp-" + Guid.NewGuid().ToString("N");

            try
            {
                File.WriteAllText(tempPath, generatedKey, new UTF8Encoding(false));

                try
                {
                    File.Move(tempPath, keyPath, overwrite: false);
                }
                catch (IOException) when (File.Exists(keyPath))
                {
                    // 极端情况下有另一个启动实例先创建了密钥，使用已落盘的密钥。
                }

                if (File.Exists(keyPath))
                {
                    var persistedKey = File.ReadAllText(keyPath, Encoding.UTF8).Trim();
                    if (!string.IsNullOrWhiteSpace(persistedKey))
                    {
                        Log.Information("JWT 签名密钥已从持久化文件加载：{KeyPath}", keyPath);
                        return persistedKey;
                    }
                }

                throw new InvalidOperationException("JWT 签名密钥持久化失败：" + keyPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
    }
}
