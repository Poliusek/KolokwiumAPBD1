using Kolos1.Models.DTOs;

namespace Kolos1.Services;

public interface IDeliveryService
{
    Task<DeliveryDto> GetDelivery(int id);
    Task<bool> DeliveryExists(int id);
    Task<int> CreateDelivery(DeliveryInputDto input);
}