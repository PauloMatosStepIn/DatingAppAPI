using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Interfaces;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace api.Services
{
  public class TokenService : ITokenService
  {
    public readonly SymmetricSecurityKey _key;

    // public TokenService(IConfiguration config)
    private readonly UserManager<AppUser> _userManager;

    public TokenService(IConfiguration config, UserManager<AppUser> userManager)
    {
      _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
      _userManager = userManager;
    }

    public UserManager<AppUser> UserManager { get; }

    public async Task<string> CreateToken(AppUser user)
    {
      var claims = new List<Claim>
      {
        new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
      };

      var roles = await _userManager.GetRolesAsync(user);

      // Insert Roles in Token
      claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

      var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(7),
        SigningCredentials = creds
      };

      var tokenhandler = new JwtSecurityTokenHandler();

      var token = tokenhandler.CreateToken(tokenDescriptor);

      return tokenhandler.WriteToken(token);
    }
  }
}