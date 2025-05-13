namespace Kolos1.Models.DTOs;

public class CustomerDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class DriverDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string LicenceNumber { get; set; }
}