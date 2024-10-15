using Domain.Entities;
using Domain;
using JWTDemo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Domain.ViewModels.Product;
using Serilog;
using System.Linq.Expressions;
using Domain.ViewModels.User;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Domain.Enums;
using DataAccess.Interfaces;
using DataAccess.Commons;
using System.Drawing.Printing;

namespace JWTDemo.Controllers.v5
{
    [ApiVersion(5)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly IClaimsService _claimService;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, TokenService tokenService, IClaimsService claimService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _tokenService = tokenService;
            _claimService = claimService;
        }

        // GET: api/Users
        /// <summary>
        /// Get a list of ApplicationUser
        /// </summary>
        /// <returns>A list of ProductDTO</returns>
        [MapToApiVersion(5)]
        [HttpGet]
        [Produces("application/json")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Pagination<UserResponseDTO>>> GetUsers(int pageIndex = 0, int pageSize = 20)
        {
            var users = await _userManager.Users.AsNoTracking().OrderBy(x => x.Role).ThenByDescending(x => x.Email).ToListAsync(); 
            List<UserResponseDTO> userResponseDTOs = new List<UserResponseDTO>();
            foreach (var user in users)
            {
                var isLockout = await _userManager.IsLockedOutAsync(user);
                var userResponseDTO = _mapper.Map<UserResponseDTO>(user);
                userResponseDTO.IsLockout = isLockout;
                userResponseDTOs.Add(userResponseDTO);
            }

            var itemCount = userResponseDTOs.Count();
            var items = userResponseDTOs.Skip(pageIndex * pageSize)
                                    .Take(pageSize)
                                    .ToList();
            var result = new Pagination<UserResponseDTO>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItemsCount = itemCount,
                Items = items,
            };
            Log.Information("Result: {@userResponseDTOs}", result);
            return Ok(result);
        }

        [MapToApiVersion(5)]
        [HttpGet("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetUserId(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User Not found.");
                }
                var result = _mapper.Map<UserResponseDTO>(user);

                var isLockout = await _userManager.IsLockedOutAsync(user);
                result.IsLockout = isLockout;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [MapToApiVersion(5)]
        [HttpPatch("Lockout")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> LockOutAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToLower());
                if (user == null)
                    return NotFound("User Not found.");
                var isLockout = await _userManager.IsLockedOutAsync(user);
                if (!isLockout)
                {
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                    return Ok(new { msg = "Lockout successfully!" });
                }
                else
                {
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    return Ok(new {msg = "Unlock successfully!" });

                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [MapToApiVersion(5)]
        [Authorize(Roles = "Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UserRequestUpdateDTO userRequestDTO)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound("User Not found.");
                }
                else
                {
                    var temp = await _userManager.Users
                        .Where(x => !x.Id.ToLower().Equals(user.Id.ToLower()) && x.UserName.ToLower().Equals(userRequestDTO.Username.ToLower())).FirstOrDefaultAsync();
                    if (temp != null)
                        throw new Exception("UserName has been existed!");
                    try
                    {
                        user.UserName = userRequestDTO.Username;
                        user.NormalizedUserName = userRequestDTO.Username.ToUpper();
                        user.Fullname = userRequestDTO.Fullname;
                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                            return Ok(new { msg = "Update User information successfully!" });
                        else
                        {
                            return BadRequest(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [MapToApiVersion(5)]
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRequestCreateDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user != null)
            {
                return BadRequest("Email has been existed!");
            }
            else
            {
                var temp = await _userManager.Users.Where(x => x.UserName.ToLower().Equals(model.Email.ToLower())).FirstOrDefaultAsync();
                if (temp != null)
                    return BadRequest("Username has been existed!");
                try
                {
                    ApplicationUser newUser = new ApplicationUser()
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        Fullname = model.Fullname,
                        Role = (Role) Enum.Parse(typeof(Role), model.Role),
                    };
                    var result = await _userManager.CreateAsync(newUser, "NewAccount1!");
                    if (result.Succeeded)
                    {
                        var tempUser = await _userManager.FindByIdAsync(newUser.Id);
                        if (tempUser != null)
                        {
                            try
                            {
                                Customer customer = new Customer { UserId = tempUser.Id };
                                await _unitOfWork.CustomerRepository.Add(customer);
                                await _unitOfWork.Complete();
                                var cusResult = await _userManager.AddToRoleAsync(tempUser, model.Role);
                                if (!cusResult.Succeeded)
                                {
                                    throw new Exception("Add role failed.");
                                }
                                return Ok(new { msg = "Create user successfully!" });
                            }
                            catch (Exception ex)
                            {
                                await _userManager.DeleteAsync(tempUser);
                                return BadRequest("Error occurs during create user processing: " + ex.Message);
                            }
                        }
                    }
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
    }
}
