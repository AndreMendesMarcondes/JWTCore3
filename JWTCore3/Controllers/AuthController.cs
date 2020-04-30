
using JWTCore3.Models;
using JWTCore3.Util;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWTCore3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;

        public AuthController(SignInManager<IdentityUser> signInManager,
                              UserManager<IdentityUser> userManager,
                              IOptions<AppSettings> appSettings)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(RegisterCheckInUser registerCheckInUser)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(e => e.Errors));

            var user = new IdentityUser
            {
                UserName = registerCheckInUser.Email,
                Email = registerCheckInUser.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, registerCheckInUser.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            await _signInManager.SignInAsync(user, false);

            return StatusCode(201, await GerarJwt(registerCheckInUser.Email));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(CheckInUserLogin checkInUserLogin)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(e => e.Errors));

            var result = await _signInManager.PasswordSignInAsync(checkInUserLogin.Email, checkInUserLogin.Password, true, false);

            if (result.Succeeded) return Ok(await GerarJwt(checkInUserLogin.Email));

            return BadRequest("Usuário ou senha inválidos");
        }

        private async Task<string> GerarJwt(string email)
        {
            var user = await _userManager.FindByNameAsync(email);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Emitter,
                Audience = _appSettings.ValidAt,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpirationHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}