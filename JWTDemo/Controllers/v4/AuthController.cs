using Domain.Entities;
using Domain.Enums;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using JWTDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Domain.ViewModels.Auth;
using DataAccess.Interfaces;

namespace JWTDemo.Controllers.v4
{
    [ApiVersion(4)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IClaimsService _claimService;

        public AuthController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, TokenService tokenService, IClaimsService claimService)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
            _claimService = claimService;
        }



        /// <summary>
        /// Sign up 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Auth/Register
        ///     {
        ///       "email": "string",
        ///       "username": "string",
        ///       "password": "string",
        ///       "role": "Manager"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created user</response>
        /// <response code="400">If the user is invalid</response>
        [MapToApiVersion(4)]
        [MapToApiVersion(4)]
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Sign in
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A AuthResponse with token</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Auth/Login
        ///     {
        ///       "email": "string",
        ///       "password": "string"
        ///     }
        ///
        /// </remarks>
        /// <response code="400">If the user is invalid</response>
        /// <response code="401">User doesn't exist</response>
        [MapToApiVersion(4)]
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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


        /// <summary>
        /// Get Profile when signed in
        /// </summary>
        /// <returns>A ApplicationUser object</returns>
        /// <remarks></remarks>
        /// <response code="400">If the user is invalid</response>
        /// <response code="404">User doesn't exist</response>
        [MapToApiVersion(4)]
        [HttpGet("Profile")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            string userId = _claimService.GetCurrentUserId.ToString().ToLower();
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound("User not found.");
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
