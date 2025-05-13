using Kolos1.Models.DTOs;
using Kolos1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kolos1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;
        public DeliveriesController(IDeliveryService deliveryService)
        {
            this._deliveryService = deliveryService;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (!_deliveryService.DeliveryExists(id).Result)
                return NotFound("Delivery not found");
            
            var response = _deliveryService.GetDelivery(id);
            if (response == null || response.Result == null)
                return NotFound("Delivery not found");
            
            return Ok(response.Result);
        }

        [HttpPost]
        public IActionResult InsertDelivery([FromBody] DeliveryInputDto input)
        {
            if (_deliveryService.DeliveryExists(input.DeliveryId).Result)
                return Conflict("Delivery Exists");

            var result = _deliveryService.CreateDelivery(input).Result;
            if (result == -1)
                return NotFound("Customer not found");
            if (result == -2)
                return NotFound("Driver not found");
            if (result == -3)
                return NotFound("One or more products not found");
            if (result == -4)
                return BadRequest("Could not create delivery");
            if (result == -5)
                return BadRequest("Could not associate delivery with products");
            if (result == 1)
                return Created("api/deliveries", new
                {
                    Id = input.DeliveryId,
                    input.CustomerId,
                    input.LicenceNumber,
                    Products = input.Products.Select(x=>x.Name)
                });
            return BadRequest("Could not create delivery");
        }
    }
}
