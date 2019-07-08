using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Spark.Authentication;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Spark
{
    public class SparkJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        private JwtBearerAuthenticationOptions Options;
        public SparkJwtSecurityTokenHandler()
        {
            Options = new JwtBearerAuthenticationOptions
            {
                TokenHandler = this,//new SparkJwtSecurityTokenHandler(),
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidIssuers = Settings.JwtKeyIssuers,
                    IssuerSigningKeys = Settings.JwtKeys,
                    ClockSkew = TimeSpan.Zero,
                },
                IssuerSecurityKeyProviders = Settings.JwtKeyProviders
            };
        }
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
        public ClaimsPrincipal DoValidateToken(string token, out SecurityToken validatedToken)
        {
            try
            {
                return base.ValidateToken(token, Options.TokenValidationParameters, out validatedToken);
            }
            catch (Exception e)
            {
                SparkAuthenticationEventSource.Log.TokenValidationFailure(e.Message, token);
                throw e;
            }
        }
    }
}