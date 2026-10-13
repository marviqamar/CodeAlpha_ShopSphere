using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.DTOs.Order;
using ShopSphere.Application.Interfaces;

namespace ShopSphere.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize] // every endpoint here requires a logged-in user at minimum
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST: api/order
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(PlaceOrderRequest request)
        {
            var order = await _orderService.PlaceOrderAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        // GET: api/order/mine
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyOrders()
        {
            var orders = await _orderService.GetMyOrdersAsync(GetUserId());
            return Ok(orders);
        }

        // GET: api/order/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(GetUserId(), id);
            return Ok(order);
        }

        // GET: api/order?pageNumber=1&pageSize=10   (Admin only — all orders, all users)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var orders = await _orderService.GetAllAsync(pageNumber, pageSize);
            return Ok(orders);
        }

        // PUT: api/order/5/status   (Admin only)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
        {
            var order = await _orderService.UpdateStatusAsync(id, request);
            return Ok(order);
        }

        // PUT: api/order/5/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _orderService.CancelOrderAsync(GetUserId(), id);
            return Ok(order);
        }
    }
}