using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

class Program
{
    private static string SecretKey = "seriouslyneverleavethissittinginyourcode";
    private static readonly SymmetricSecurityKey _signingKey = 
        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));


    // https://goo.gl/rP4635
    private static HttpClient httpClient = new HttpClient();
    
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        
        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);        

        // Create some claims for the token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "AppUser_Bob"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, 
                      ToUnixEpochDate(DateTime.Now).ToString(), 
                    ClaimValueTypes.Integer64), 
            new Claim("icanhazcheeseburger", "true"),       
        };


        // Create a JWT bearer token...
              // Create the JWT security token and encode it.
        var jwt = new JwtSecurityToken(
          issuer: "issuer",
          audience: "audience",
          claims: claims,
          notBefore: DateTime.UtcNow,
          expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(20)),
          signingCredentials: creds);

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        Console.WriteLine("Encoded JWT to be used for bearer auth: " + encodedJwt);

        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", encodedJwt);
        var result = httpClient.GetAsync("http://localhost:5000/api/secured").Result;
        Console.WriteLine(result.StatusCode);
        Console.WriteLine(result.Content.ReadAsStringAsync().Result);   

        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", encodedJwt);
        var policyResult = httpClient.GetAsync("http://localhost:5000/api/secured/policy").Result;
        Console.WriteLine(policyResult.StatusCode);
        Console.WriteLine(policyResult.Content.ReadAsStringAsync().Result); 
    }

     private static long ToUnixEpochDate(DateTime date)
      => (long)Math.Round((date.ToUniversalTime() - 
                           new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                          .TotalSeconds);
}
