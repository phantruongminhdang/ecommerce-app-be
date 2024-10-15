using Domain.Entities;
using Domain.Enums;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using JWTDemo.Services;
using Domain.ViewModels.Auth;

namespace JWTDemo.Controllers.v3
{
    [ApiVersion(3)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, TokenService tokenService, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
        }

        [MapToApiVersion(3)]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByEmailAsync(request.Email);
            var result = await _userManager.CreateAsync(
                new ApplicationUser { UserName = request.Username, Email = request.Email, Role = request.Role },
                request.Password!
            );

            if (result.Succeeded)
            {
                if (user == null)
                {
                    var temp = await _userManager.FindByEmailAsync(request.Email);
                    var addRoleResult = await _userManager.AddToRoleAsync(temp, request.Role.ToString());
                    if (addRoleResult.Succeeded)
                    {
                        return CreatedAtAction(nameof(Register), new { email = request.Email, role = request.Role }, request);
                    }
                    else
                    {
                        await _userManager.DeleteAsync(temp);
                        throw new Exception("Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại!");
                    }
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        [MapToApiVersion(3)]
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var managedUser = await _userManager.FindByEmailAsync(request.Email!);
            if (managedUser == null)
            {
                return BadRequest("Login information not found");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password!);
            if (!isPasswordValid)
            {
                return BadRequest("Password is invalid");
            }

            var userInDb = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (userInDb is null)
            {
                return Unauthorized();
            }

            var accessToken = _tokenService.CreateToken(userInDb);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                Username = userInDb.UserName,
                Email = userInDb.Email,
                Role = await _userManager.GetRolesAsync(userInDb),
                Token = accessToken,
            });
        }
    }
}
