using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Thesis.Services.Common.Options;

/// <summary>
/// Настройки JWT-токена
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Издатель токена
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Потребитель токена
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Ключ шифрования
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Время жизни токена
    /// </summary>
    public int AccessTokenLifetime { get; set; }
    
    /// <summary>
    /// Время жизни refresh-токена
    /// </summary>
    public int RefreshTokenLifetime { get; set; }
    
    /// <summary>
    /// Получить симметричный ключ шифрования
    /// </summary>
    public SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.ASCII.GetBytes(Key));
}