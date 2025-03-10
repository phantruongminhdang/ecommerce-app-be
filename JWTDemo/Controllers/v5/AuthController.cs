﻿using Domain.Entities;
using Domain.Enums;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using JWTDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Domain.ViewModels.Auth;
using DataAccess.Interfaces;
using System.Security.Claims;
using Domain.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace JWTDemo.Controllers.v5
{
    [ApiVersion(5)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly IClaimsService _claimService;

        public AuthController(IUnitOfWork unitOfWork, ApplicationDbContext context, UserManager<ApplicationUser> userManager, TokenService tokenService, IClaimsService claimService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _userManager = userManager;
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
        [MapToApiVersion(5)]
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
                new ApplicationUser { UserName = request.Username, Fullname = request.Fullname, Email = request.Email, Role = request.Role },
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
                        try
                        {
                            Customer customer = new Customer { UserId = temp.Id };
                            _unitOfWork.BeginTransaction();
                            await _unitOfWork.CustomerRepository.Add(customer);
                            var cart = new Cart()
                            {
                                CustomerId = customer.Id,
                            };
                            await _unitOfWork.CartRepository.Add(cart);
                            await _unitOfWork.CommitTransactionAsync();
                        }
                        catch (Exception)
                        {
                            _unitOfWork.RollbackTransaction();
                            await _userManager.DeleteAsync(temp);
                            throw new Exception("Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại!");
                        }
                        //return CreatedAtAction(nameof(Register), new { email = request.Email, role = request.Role }, request);
                        return Ok(new { msg = "Signup successfully!" });
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
        [MapToApiVersion(5)]
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
        [MapToApiVersion(5)]
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

        [MapToApiVersion(5)]
        [Authorize]
        [HttpPut("Profile")]
        public async Task<IActionResult> ChangeProfile([FromForm] UserRequestUpdateDTO userRequestUpdateDTO)
        {
            string userId = _claimService.GetCurrentUserId.ToString().ToLower();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User Not Found!");
            }
            else
            {
                var temp = await _userManager.Users.Where(x => !x.Id.ToLower().Equals(user.Id.ToLower()) && x.UserName.ToLower().Equals(userRequestUpdateDTO.Username.ToLower())).FirstOrDefaultAsync();
                if (temp != null)
                    throw new Exception("Tên đăng nhập này đã được sử dụng!");
                try
                {
                    user.UserName = userRequestUpdateDTO.Username;
                    user.NormalizedUserName = userRequestUpdateDTO.Username.ToUpper();
                    user.Fullname = userRequestUpdateDTO.Fullname;
                    user.PhoneNumber = userRequestUpdateDTO.PhoneNumber;
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                        return Ok("Thay đổi thông tin cá nhân thành công!");

                    var errors = new List<string>();
                    errors.AddRange(result.Errors.Select(x => x.Description));
                    return BadRequest(errors);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [MapToApiVersion(5)]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            string userId = _claimService.GetCurrentUserId.ToString().ToLower();
            var user = await _userManager.FindByIdAsync(userId); 
            if (user == null)
            {
                throw new Exception("Không tìm thấy tài khoản bạn yêu cầu!");
            }
            try
            {
                if (!model.NewPassword.Equals(model.ConfirmPassword))
                {
                    return BadRequest("Mật khẩu với và mật khẩu xác nhận không khớp!");
                }
                
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (result.Succeeded)
                {
                    // Mật khẩu đã được đặt lại thành công
                    return Ok(new { msg = "Mật khẩu đã được đặt lại thành công! Vui lòng tiến hành đăng nhập!" });
                }
                else
                {
                    // Đặt lại mật khẩu không thành công
                    return BadRequest("Không thể đặt lại mật khẩu!");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
