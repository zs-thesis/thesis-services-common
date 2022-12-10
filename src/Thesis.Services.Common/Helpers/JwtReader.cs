using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Thesis.Services.Common.Options;

namespace Thesis.Services.Common.Helpers;

public class JwtReader
{
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly ILogger<JwtReader> _logger;

    public JwtReader(IOptions<JwtOptions> jwtOptions, ILogger<JwtReader> logger)
    {
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    private bool Read(string token, out ClaimsPrincipal? claims, out DateTime validTo)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validations = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _jwtOptions.Value.GetSymmetricSecurityKey(),
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = $"*.{_jwtOptions.Value.Issuer}",
            ValidateLifetime = true
        };
        try
        {
            claims = tokenHandler.ValidateToken(token, validations, out var validatedToken);
            validTo = validatedToken.ValidTo;
            return true;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex.ToString());
        }
        claims = null;
        validTo = DateTime.MinValue;
        return false;
    }
}