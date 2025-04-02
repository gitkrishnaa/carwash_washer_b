using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _secretKey = "Your_Secret_Key_Here";
    private readonly IConfiguration _configuration;
    public JwtMiddleware(RequestDelegate next,IConfiguration configuration)
    {
        _next = next;
         _secretKey = configuration["JwtSettings:SecretKey"];

    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

// if internal
       if(context.Request.Path.Value.StartsWith("/internal")){
         await _next(context);
               return;
       }
    //    Console.WriteLine(context.Request.Path.Value.StartsWith("/internal"));
       
        if (!string.IsNullOrEmpty(token))
        {
            var claims = ValidateToken(token);
            if (claims != null)
            {
                context.Items["User"] = claims; // Store user claims
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid Tokens");
                return;
            }
        }
        else if (!context.Request.Path.Value.StartsWith("/api/auth")) // Allow public routes
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Token is missing");
            return;
        }
        else if (context.Request.Path.Value.StartsWith("/internal")) // Allow public routes
        {
               await _next(context);
               return;
        }
        //  else if (!context.Request.Path.Value.StartsWith("/api/secure")) // Allow public routes
        // {
        //     context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //     await context.Response.WriteAsync("Unauthorized: Token is missing");
        //     return;
        // }

        await _next(context);
    }

    private JwtSecurityToken ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
            };

            tokenHandler.ValidateToken(token, validations, out SecurityToken validatedToken);
            return (JwtSecurityToken)validatedToken;
        }
        catch
        {
            return null;
        }
    }
}
