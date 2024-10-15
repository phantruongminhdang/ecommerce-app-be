using Asp.Versioning;
using AutoMapper;
using DataAccess.Hubs;
using DataAccess.Interfaces;
using Domain.Entities;
using Domain.ViewModels.Cart;
using Domain.ViewModels.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Linq.Expressions;

namespace JWTDemo.Controllers.v5
{
    [ApiVersion(5)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IClaimsService _claimService;

        public CartsController(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<ChatHub> hubContext, IClaimsService claimService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
            _claimService = claimService;
        }

        // get: api/Carts
        [MapToApiVersion(5)]
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<CartResponseDTO>> GetCart()
        {
            var cart =await GetCartAsync();
            
            var cartResponseDTO = _mapper.Map<CartResponseDTO>(cart);

            if (cart == null)
            {
                return NotFound("Cart Not Found");
            }

            return Ok(cartResponseDTO);
        }

        // POST: api/Carts/AddToCart
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [MapToApiVersion(5)]
        [HttpPost("AddToCart")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<CartResponseDTO>> AddToCart(CartItemRequestDTO cartItemRequestDTO)
        {
            var cart = await GetCartAsync();
            if(cart == null)
            {
                return NotFound("Cart Not found.");
            }
            var product = await _unitOfWork.ProductRepository.GetById(cartItemRequestDTO.ProductId);
            if (product == null)
            {
                return NotFound("Product Not found.");
            }
            var cartItem = _mapper.Map<CartItem>(cartItemRequestDTO);
            cartItem.CartId = cart.Id;
            var itemInCart = cart.CartItems.FirstOrDefault(i => i.ProductId == cartItem.ProductId);

            if (itemInCart == null)
            {
                cart.CartItems.Add(cartItem);
            }
            else
            {
                itemInCart.Quantity += cartItem.Quantity;
            }
            if(product.Quantity < cartItem.Quantity)
            {
                return BadRequest("Product is not enough!");
            }
            product.Quantity -= cartItem.Quantity;
            try
            {
                _unitOfWork.BeginTransaction();
                _unitOfWork.ProductRepository.Modified(product);
                _unitOfWork.CartRepository.Modified(cart);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }

            return CreatedAtAction(nameof(GetCart), new { id = cart.Id }, _mapper.Map<CartResponseDTO>(cart));
        }

        // POST: api/Carts/RemoveToCart
        [MapToApiVersion(5)]
        [HttpPost("RemoveToCart")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<CartResponseDTO>> RemoveToCart(Guid cartItemId)
        {
            var cart = await GetCartAsync();
            if (cart == null)
            {
                return NotFound("Cart Not found.");
            }
            var cartItem = await _unitOfWork.CartItemRepository.GetById(cartItemId);
            if (cartItem == null)
            {
                return NotFound("Cart Item Not found.");
            }
            var product = await _unitOfWork.ProductRepository.GetById(cartItem.ProductId);
            if (product == null)
            {
                return NotFound("Product Not found.");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                product.Quantity += cartItem.Quantity;
                _unitOfWork.ProductRepository.Modified(product);
                _unitOfWork.CartItemRepository.Remove(cartItem);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }

            return CreatedAtAction(nameof(GetCart), new { id = cart.Id }, _mapper.Map<CartResponseDTO>(cart));
        }

        [MapToApiVersion(5)]
        [HttpPost("Checkout")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<CartResponseDTO>> Checkout(CheckoutRequest checkoutRequest)
        {
            var cart = await GetCartAsync();

            var cartResponseDTO = _mapper.Map<CartResponseDTO>(cart);

            if (cart == null)
            {
                return NotFound("Cart Not Found");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                OrderRequestDTO orderRequestDTO = new OrderRequestDTO()
                {
                    CustomerId = cartResponseDTO.CustomerId,
                    Address = checkoutRequest.Address,
                    TotalPrice = cartResponseDTO.TotalPrice,
                    Note = checkoutRequest.Note,
                };
                var order = _mapper.Map<Order>(orderRequestDTO);
                await _unitOfWork.OrderRepository.Add(order);
                await _unitOfWork.Complete();
                foreach (var cartItem in cart.CartItems)
                {
                    OrderItemRequestDTO orderItemRequestDTO = new OrderItemRequestDTO()
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                    };
                    var orderItem = _mapper.Map<OrderItem>(orderItemRequestDTO);
                    await _unitOfWork.OrderItemRepository.Add(orderItem);
                    await _unitOfWork.Complete();
                }
                ClearCart(cart);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex) {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
            return Ok("Checkout successfully!");
        }

        private async Task<Cart> GetCartAsync()
        {
            var userId = _claimService.GetCurrentUserId;
            var customerId = _unitOfWork.CustomerRepository.GetAsync(expression: x => x.UserId == userId.ToString()).Result.Select(x => x.Id).FirstOrDefault();
            var includes = new List<Expression<Func<Cart, object>>>{
                                 x => x.CartItems,
                                    };
            var cart = _unitOfWork.CartRepository.GetAsync(expression: x => x.CustomerId == customerId, includes: includes).Result.FirstOrDefault();
            foreach (var item in cart.CartItems)
            {
                var product = await _unitOfWork.ProductRepository.GetById(item.ProductId);
                item.Product = product;

            }
            return cart;
        }

        private void ClearCart(Cart cart)
        {
            cart.CartItems.Clear();
            _unitOfWork.CartRepository.Modified(cart);
        }
    }
}
