using Microsoft.IdentityModel.Tokens;
using Spark.Authentication;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Spark
{
    public class SparkJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        protected override void ValidateLifetime(DateTime? notBefore, DateTime? expires, JwtSecurityToken jwtToken, TokenValidationParameters validationParameters)
        {
            // The token handler for .net framework (non-core) seemingly doesn't
            // respect the "iat" header value. We'll use it here to calculate
            if ((expires == null) && jwtToken.Payload.ContainsKey("iat"))
            {
                expires = DateTimeOffset.FromUnixTimeSeconds(
                        long.Parse(jwtToken.Payload["iat"].ToString())).DateTime
                    .AddMinutes(
                        Settings.JwtCerts.ContainsKey(jwtToken.Issuer)
                        ? Settings.JwtCerts[jwtToken.Issuer].TokenTtl.GetValueOrDefault(5)
                        : 5);
            }
            base.ValidateLifetime(notBefore, expires, jwtToken, validationParameters);
        }
        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            try
            {
                return base.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (Exception e)
            {
                SparkAuthenticationEventSource.Log.TokenValidationFailure(e.Message, token);
                throw e;
            }
        }
    }
}