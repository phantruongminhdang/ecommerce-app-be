using Microsoft.AspNetCore.Mvc;
using Domain;
using Domain.ViewModels.Order;
using AutoMapper;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Interfaces;
using DataAccess.Commons;
using Domain.Entities;
using System.Linq.Expressions;
using System.Drawing.Printing;

namespace JWTDemo.Controllers.v5
{
    [ApiVersion(5)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class OrderItemsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderItemsController(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }



        // GET: api/OrderItems
        [MapToApiVersion(5)]
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Pagination<OrderItemResponseDTO>>> GetOrderItems(int pageIndex = 0, int pageSize = 20)
        {
            List<Expression<Func<OrderItem, object>>> includes = new List<Expression<Func<OrderItem, object>>>{
                                 x => x.Product,
                                    };
            var orderItems = await _unitOfWork.OrderItemRepository.GetAsyncPagination(
            includes: includes,
                pageIndex: pageIndex, pageSize: pageSize);
            var orderItemsDTOs = _mapper.Map<Pagination<OrderItemResponseDTO>>(orderItems);
            return Ok(orderItemsDTOs);
        }

        // GET: api/OrderItems/5
        [MapToApiVersion(5)]
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<OrderItemResponseDTO>> GetOrderItem(Guid id)
        {
            var orderItem = await _unitOfWork.OrderItemRepository.GetById(id);

            if (orderItem == null)
            {
                return NotFound("Order Item Not Found.");
            }

            var orderItemDTO = _mapper.Map<OrderItemResponseDTO>(orderItem);

            return Ok(orderItemDTO);
        }

        // GET: api/OrderItems/Order/5
        [MapToApiVersion(5)]
        [HttpGet("Order/{orderId}")]
        [Authorize]
        public async Task<ActionResult<Pagination<OrderItemResponseDTO>>> GetOrderItemsByOrder(Guid orderId, int pageIndex = 0, int pageSize = 20)
        {
            List<Expression<Func<OrderItem, object>>> includes = new List<Expression<Func<OrderItem, object>>>{
                                 x => x.Product,
                                    };
            var orderItems = await _unitOfWork.OrderItemRepository.GetAsyncPagination(
                expression: x => x.OrderId == orderId,
                includes: includes,
                pageIndex: pageIndex, pageSize: pageSize);

            var orderItemsDTOs = _mapper.Map<Pagination<OrderItemResponseDTO>>(orderItems);

            return Ok(orderItemsDTOs);
        }

    }
}
