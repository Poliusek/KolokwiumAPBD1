using System.Data.Common;
using Kolos1.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Kolos1.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IConfiguration _configuration;
    public DeliveryService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<DeliveryDto> GetDelivery(int id)
    {
        string command = "SELECT date, customer_id, driver_id FROM Delivery where delivery_id = @DeliveryId";
        
        DeliveryDto deliveryReturnDto = new DeliveryDto();
        int customerid = 0;
        int driverid = 0;
        
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            cmd.Parameters.AddWithValue("@DeliveryId", id);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    deliveryReturnDto.Date = reader.GetDateTime(0);
                    customerid = reader.GetInt32(1);
                    driverid = reader.GetInt32(2);
                }
            }
        }
        
        command = "Select first_name, last_name, date_of_birth from Customer where customer_id = @CustomerId";
        
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            cmd.Parameters.AddWithValue("@CustomerId", customerid);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    deliveryReturnDto.Customer = new CustomerDto()
                    {
                        FirstName = reader.GetString(0),
                        LastName = reader.GetString(1),
                        DateOfBirth = reader.GetDateTime(2),
                    };
                }
            }
        }
        
        command = "Select first_name, last_name,licence_number from Driver where driver_id = @DriverId";
        
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            cmd.Parameters.AddWithValue("@DriverId", driverid);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    deliveryReturnDto.Driver = new DriverDto()
                    {
                        FirstName = reader.GetString(0),
                        LastName = reader.GetString(1),
                        LicenceNumber = reader.GetString(2)
                    };
                }
            }
        }
        
        command = "Select p.name, p.price, pd.amount from Product_Delivery pd join Product p on p.product_id = pd.product_id where pd.delivery_id = @DeliveryId";
        
        List<ProductDto> productDtos = new List<ProductDto>();
        
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            cmd.Parameters.AddWithValue("@DeliveryId", id);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    productDtos.Add(new ProductDto()
                    {
                        Name = reader.GetString(0),
                        Price = reader.GetDecimal(1),
                        Amount = reader.GetInt32(2)
                    });
                }
            }
        }
        
        deliveryReturnDto.Products = productDtos;

        return deliveryReturnDto;
    }

    public async Task<bool> DeliveryExists(int id)
    {
        string command = "SELECT 1 FROM Delivery WHERE delivery_id = @DeliveryId";
        
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@DeliveryId", id);

            conn.Open();
            var delivery = cmd.ExecuteScalar();
            if (delivery is null)
                return false;
            return true;
        }
    }

    public async Task<int> CreateDelivery(DeliveryInputDto input)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand("SELECT 1 FROM Customer WHERE customer_id = @CustomerId");
        
        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.AddWithValue("@CustomerId", input.CustomerId);
            var customer = await command.ExecuteScalarAsync();
            if (customer is null)
                return -1;
            
            command.CommandText = "Select 1 from Driver where licence_number = @LicenceNumber";
            command.Parameters.Clear();
            
            command.Parameters.AddWithValue("@LicenceNumber", input.LicenceNumber);

            var driver = await command.ExecuteScalarAsync();
            if (driver is null)
                return -2;
            
            command.CommandText = "Select name from Product";
            command.Parameters.Clear();
            
            var count = 0;
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    if (input.Products.Select(x => x.Name).Contains(reader.GetString(0)))
                        count++;
                
            }
            if (count != input.Products.Count)
                return -3;
            
            command.CommandText = "Insert into Delivery (delivery_id, customer_id, driver_id,date) values (@DeliveryId, @CustomerId, (SELECT driver_id FROM Driver WHERE licence_number = @LicenceNumber), @Date)";
            command.Parameters.Clear();
            
            command.Parameters.AddWithValue("@DeliveryId", input.DeliveryId);
            command.Parameters.AddWithValue("@CustomerId", input.CustomerId);
            command.Parameters.AddWithValue("@LicenceNumber", input.LicenceNumber);
            command.Parameters.AddWithValue("@Date", DateTime.Now);

            var row = await command.ExecuteScalarAsync();
            if (row is not null)
                return -4;
            
            foreach (var product in input.Products)
            {
                command.CommandText = "Insert into Product_Delivery (delivery_id, product_id, amount) values (@DeliveryId, (Select product_id from Product where name = @ProductName), @Amount)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@DeliveryId", input.DeliveryId);
                command.Parameters.AddWithValue("@ProductName", product.Name);
                command.Parameters.AddWithValue("@Amount", product.Amount);

                row = await command.ExecuteScalarAsync();
                if (row is not null)
                    return -5;
            }

            await transaction.CommitAsync();
            return 1;
            
        }
        catch (Exception _)
        {
            Console.WriteLine("Rollback");
            await transaction.RollbackAsync();
            throw;
        }
    }
}