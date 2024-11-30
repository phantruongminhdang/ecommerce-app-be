using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using Domain.Entities;
using Asp.Versioning;
using AutoMapper;
using DataAccess.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Domain.ViewModels.Order;
using System.Linq.Expressions;
using Domain.Enums;
using Domain.ViewModels.Product;
using Microsoft.AspNetCore.Identity;
using Serilog;
using DataAccess.Interfaces;
using DataAccess.Commons;

namespace JWTDemo.Controllers.v5
{
    [ApiVersion(5)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IClaimsService _claimService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<ChatHub> hubContext, IClaimsService claimService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
            _claimService = claimService;
            _userManager = userManager;
        }


        // GET: api/Orders
        [MapToApiVersion(5)]
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Pagination<OrderResponseDTO>>> GetOrders(int pageIndex = 0, int pageSize = 20)
        {
            var includes = new List<Expression<Func<Order, object>>>{
                x=> x.Customer.ApplicationUser,
                x => x.OrderItems,
                                    };
            var orders = await _unitOfWork.OrderRepository.GetAsyncPagination(includes: includes, pageIndex: pageIndex, pageSize: pageSize);
            var orderResponseDTOs = _mapper.Map<Pagination<OrderResponseDTO>>(orders);
            return Ok(orderResponseDTOs);
        }

        // GET: api/Orders/Customer
        [MapToApiVersion(5)]
        [HttpGet("Customer")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<Pagination<OrderResponseDTO>>> GetOrdersByCustomer(int pageIndex = 0, int pageSize = 20)
        {
            var orders = await GetOrderByCustomer(pageIndex, pageSize);
            var orderResponseDTOs = _mapper.Map<Pagination<OrderResponseDTO>>(orders);
            return Ok(orderResponseDTOs);
        }

        [MapToApiVersion(5)]
        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateStatusOrder(Guid id, OrderStatus orderStatus)
        {
            var order = await _unitOfWork.OrderRepository.GetById(id);
            if (order == null)
            {
                return NotFound("Order Not found.");
            }
            if (order.OrderStatus == OrderStatus.Delivered || order.OrderStatus == OrderStatus.DeliveryFailed)
            {
                return BadRequest("Order has been completed/failed!");
            }
            if (orderStatus == OrderStatus.Delivered)
            {
                order.DeliveryDate = DateTime.Now;
            }
            order.OrderStatus = orderStatus;
            try
            {
                _unitOfWork.OrderRepository.Modified(order);
                await _unitOfWork.Complete();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(new { msg = "Update order successafully!" });
        }

        [MapToApiVersion(5)]
        // GET: api/Orders/5
        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<OrderResponseDTO> GetOrder(Guid id)
        {
            var includes = new List<Expression<Func<Order, object>>>{
                x=> x.Customer.ApplicationUser,
                x => x.OrderItems,
                                    };
            var order = _unitOfWork.OrderRepository.GetAsync(expression: x => x.Id == id, includes: includes).Result.FirstOrDefault();

            if (order == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<OrderResponseDTO>(order));
        }

        [MapToApiVersion(5)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PostOrder(OrderRequestDTO orderRequestDTO)
        {
            if (orderRequestDTO == null) { return BadRequest(); }
            var order = _mapper.Map<Order>(orderRequestDTO);
            await _unitOfWork.OrderRepository.Add(order);
            await _unitOfWork.Complete();
            var userId = _claimService.GetCurrentUserId.ToString();
            var user = await _userManager.FindByIdAsync(userId);

            Log.Information($"Admin {user.Email} created order successfully!");
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", userId, $"Admin {user.Email} created order successfully!");
            return Ok(new { msg = "Created order successfully!" });
        }

        private async Task<Pagination<Order>> GetOrderByCustomer(int pageIndex, int pageSize)
        {
            var userId = _claimService.GetCurrentUserId;
            var customerId = _unitOfWork.CustomerRepository.GetAsync(expression: x => x.UserId == userId.ToString()).Result.Select(x => x.Id).FirstOrDefault();
            var includes = new List<Expression<Func<Order, object>>>{
                                 x => x.OrderItems,
                                 x => x.Customer.ApplicationUser
                                    };
            var orders = await _unitOfWork.OrderRepository.GetAsyncPagination(expression: x => x.CustomerId == customerId, includes: includes, pageIndex: pageIndex, pageSize: pageSize);
            //foreach (var item in order.OrderItems)
            //{
            //    var product = await _unitOfWork.ProductRepository.GetById(item.ProductId);
            //    item.Product = product;

            //}
            return orders;
        }
    }
}
