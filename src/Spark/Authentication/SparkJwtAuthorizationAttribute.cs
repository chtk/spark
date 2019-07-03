using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Text;
using System.Security.Claims;
using System.Net.Http.Headers;

namespace Spark.Authentication
{
    public class SparkJwtAuthorizationAttribute : AuthorizeAttribute
    {
        private readonly SparkJwtSecurityTokenHandler tokenHandler = new SparkJwtSecurityTokenHandler();
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            try
            {
                string sToken = actionContext.Request.Headers.Authorization.Parameter;
                ClaimsPrincipal res = tokenHandler.DoValidateToken(sToken, out SecurityToken outValue);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}