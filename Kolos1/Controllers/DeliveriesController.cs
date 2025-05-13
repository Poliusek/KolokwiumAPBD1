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

            return Ok(response.Result);
        }

        [HttpPost]
        public IActionResult InsertDelivery([FromBody] DeliveryInputDto input)
        {
            if (_deliveryService.DeliveryExists(input.DeliveryId).Result)
                return Conflict("Delivery Exists");

            var result = _deliveryService.CreateDelivery(input).Result;
            return result switch
            {
                -1 => NotFound("Customer not found"),
                -2 => NotFound("Driver not found"),
                -3 => NotFound("One or more products not found"),
                -4 => BadRequest("Could not create delivery"),
                -5 => BadRequest("Could not associate delivery with products"),
                1 => Created("api/deliveries",
                    new
                    {
                        Id = input.DeliveryId,
                        input.CustomerId,
                        input.LicenceNumber,
                        Products = input.Products.Select(x => x.Name)
                    }),
                _ => BadRequest("Could not create delivery")
            };
        }
    }
}
