namespace Kolos1.Models.DTOs;

public class DeliveryDto
{
    public DateTime Date { get; set; }
    public CustomerDto Customer { get; set; }
    public DriverDto Driver { get; set; }
    public List<ProductDto> Products { get; set; }
}

public class DeliveryInputDto
{
    public int DeliveryId { get; set; }
    public int CustomerId { get; set; }
    public string LicenceNumber { get; set; }
    public List<ProductDto> Products { get; set; }
}