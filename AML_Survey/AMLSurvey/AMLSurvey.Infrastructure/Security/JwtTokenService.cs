using AMLSurvey.Core.DTOS.Auth;
using AMLSurvey.Core.Interfaces;
using AMLSurvey.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static AMLSurvey.Infrastructure.Security.JwtTokenService;

namespace AMLSurvey.Infrastructure.Security
{
    public class JwtTokenService : IJwtTokenService
    {
        //ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions
        //header=>  نوع التوكن وطريقة التشفير  ("alg": "HS256" , "typ": "JWT")
        //Payload => Claims or info ( sub: معرّف المستخدم ,,name: اسم المستخدم ,, وقت إصدار التوكن (Issued At))
        //Signature => توقيع التوكن باستخدام مفتاح سري HMACSHA256( base64UrlEncode(header ,Payload ) , secret)
        private readonly JwtOptions _options;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public (string token, int expiresIn) GenerateToken(TokenGenerationRequest TG_Request)
        {
            //payload
            Claim[] claims = [
                new(JwtRegisteredClaimNames.Sub, TG_Request.UserId),
                new(JwtRegisteredClaimNames.Email, TG_Request.Email!),
                new(JwtRegisteredClaimNames.GivenName, TG_Request.FirstName),
                new(JwtRegisteredClaimNames.FamilyName, TG_Request.LastName),
                new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
                new(nameof(TG_Request.Roles), JsonSerializer.Serialize(TG_Request.Roles), JsonClaimValueTypes.JsonArray),
                new(nameof(TG_Request.Permissions), JsonSerializer.Serialize(TG_Request.Permissions), JsonClaimValueTypes.JsonArray)
            ];
            //Signature
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var singingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
                signingCredentials: singingCredentials
            );

            return (token: new JwtSecurityTokenHandler().WriteToken(token), expiresIn: _options.ExpiryMinutes * 60);
        }

        // if the token is valid, return the user ID (sub claim); otherwise, return null
        public string? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKey = symmetricSecurityKey,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    /*if (ValidateAudience,ValidateIssuer) true
                     ValidIssuer = _options.Issuer, // ✅ إضافة
                     ValidAudience = _options.Audience, // ✅ إضافة
                     ValidateLifetime = true, // ✅ إضافة*/
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                return jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            }
            catch (SecurityTokenException ex)
            {
                // ✅ تسجيل الأخطاء الأمنية مهم جداً
                // _logger.LogWarning("Invalid token validation attempt: {Error}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                // ✅ تسجيل الأخطاء العامة
                // _logger.LogError(ex, "Unexpected error during token validation");
                return null;
            }
        }
    }
}
