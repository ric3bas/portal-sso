using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Portal.Domain.Base
{
    public static class JwtBase
    {
        public static string GenerateToken(
            string refreshToken,
            string login,
            string nome,
            string email,
            int expireMinutes,
            string parceiroId,
            string key,
            string issuer,
            string audience,
            string perfil,
            string[] escopos)
        {
            var claims = new[]
            {
                new Claim("refresh-token", refreshToken),
                new Claim("nome", nome),
                new Claim("usuario", login),
                new Claim("email", email),
                new Claim("tenantId", parceiroId),
                new Claim("perfil", perfil),
                new Claim("escopos", JsonSerializer.Serialize(escopos), JsonClaimValueTypes.JsonArray)
            };
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
