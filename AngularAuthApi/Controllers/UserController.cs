using AngularAuthApi.Context;
using AngularAuthApi.Helper;
using AngularAuthApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AngularAuthApi.Models.Dto;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace AngularAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        public UserController(AppDbContext appDbContext)
        {
            _authContext= appDbContext;
        }
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userobj)
        {
            if (userobj == null)
                return BadRequest();

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Username == userobj.Username );

            if (user == null)
                return NotFound(new { Message = "User Not Found!" });

            if(!PasswordHasher.VerifyPassword(userobj.Password , user.Password))
            {
                return BadRequest(new { Message ="Password is Incorrect"});
            }

            user.Token = CreateJwt (user);
            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiredTime = DateTime.Now.AddDays(5); 
            await _authContext.SaveChangesAsync();

            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
            
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userobj)
        {
            if (userobj == null)
                return BadRequest();
            //Check Username
            if (await CheckUserNameExistAsync(userobj.Username))
                return BadRequest(new { Message = " Username Alredy Exist!" });

            //Check Email
            if (await CheckEmailExistAsync(userobj.Email))
                return BadRequest(new { Message = "Eamil Alredy Exist" });

            //Check Password Strength
            var passMessage = CheckPasswordStrength(userobj.Password);
            if(!string.IsNullOrEmpty(passMessage))
                return BadRequest(new {Message =passMessage.ToString()});

            userobj.Password =PasswordHasher.HashePassword(userobj.Password); //Help us to hash password.
            userobj.Role ="User";
            userobj.Token= "";
            await _authContext.Users.AddAsync(userobj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {    
                Message = "User Register!"
            });
        }

        private async Task<bool> CheckUserNameExistAsync(string username)
        {
            return await _authContext.Users.AnyAsync(x => x.Username == username);
        }
        private async Task<bool> CheckEmailExistAsync(string email) 
            =>  await _authContext.Users.AnyAsync(x => x.Email == email);

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if(password.Length < 8 )
              sb.Append("Minimum Password Length Should Be 8"+ Environment.NewLine);
             if(!(Regex.IsMatch(password,"[a-z]")&& Regex.IsMatch(password,"[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password Should Be Alphanumeric" + Environment.NewLine);
            if (!Regex.IsMatch(password, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                sb.Append("Password Should Contain Special Character" + Environment.NewLine);
            return sb.ToString();
        }
        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysecreat.....");//key
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role , user.Role),
                new Claim(ClaimTypes.Name,$"{user.Username}")   //{user.FirstName} {user.LastName}
            });  //Payload(Identity)
                                                                                                       //Using this Algorithm for creating Token.
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject= identity,
                Expires= DateTime.Now.AddSeconds(30),
                SigningCredentials = credentials
            };
            var token =jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
        
        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken =Convert.ToBase64String(tokenBytes);
            var tokenInUser = _authContext.Users
                .Any(a=> a.RefreshToken== refreshToken);
            if(tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshToken;
        }
         
        private ClaimsPrincipal GetPrincipleFromExpirdToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("veryverysecreat.....");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience =false,
                ValidateIssuer =false,
                ValidateIssuerSigningKey= true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime=false
             };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principle = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken= securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null ||!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("This Is Invalid Token");
            }
                return principle;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUser()
        {
            return Ok(await _authContext.Users.ToListAsync());
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
                return BadRequest("Inavalid Client Request");
            string accessaToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            var principal = GetPrincipleFromExpirdToken(accessaToken);
            var username = principal.Identity.Name;
            var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiredTime <= DateTime.Now)
                return BadRequest("Inavalid Request");
            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _authContext.SaveChangesAsync();
            return Ok(new TokenApiDto()
            { 
                AccessToken = newAccessToken,
                RefreshToken = refreshToken,
            });
        }
    }
}
