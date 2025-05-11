using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Model.DTOs;

namespace Tutorial9.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    public async Task<int> AddProductToWarehouse(ProductDto product)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand("", connection);
        
        await connection.OpenAsync();
        
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText = @"SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            int count  =(int) await command.ExecuteScalarAsync();
            if (count <= 0)
                throw new Exception($"Product {product.IdProduct} does not exist");
    
            command.Parameters.Clear();
            command.CommandText = @"SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse""";
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            count  =(int) await command.ExecuteScalarAsync();
            if (count <= 0)
                throw new Exception($"Warehouse {product.IdWarehouse} does not exist");
            
            command.CommandText = @"SELECT IdOrder FROM [Order] WHERE [Order].IdProduct = @IdProduct AND [Order].Amount = @Amount AND [Order].CreatedAt < @CreatedAt";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            var idOrder = await command.ExecuteScalarAsync();
            if (idOrder == null)
                throw new Exception($"Order for product id: {product.IdProduct} does not exist");


            command.CommandText = @"SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            count =(int) await command.ExecuteScalarAsync();
            if (count > 0)
                throw new Exception($"Order for product {product.IdProduct} has been already fulfilled");
            
                    
            command.CommandText = @"UPDATE [Order]SET [Order].FulfilledAt = @FulfilledAtWHERE [Order].IdOrder = @IdOrder";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
            await command.ExecuteNonQueryAsync();     
            
            command.CommandText = @"SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            decimal price = (decimal)await command.ExecuteScalarAsync();

            
            command.CommandText = @"INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt) SELECT CAST(SCOPE_IDENTITY() as int);";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@Price", price*product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);        
            int result = (int)await command.ExecuteScalarAsync();        
                    
            
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
       
    }

    public async Task<int> AddProductToWarehouseProcedure(ProductDto product)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand("AddProductToWarehouse")
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Connection = connection;
        command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", product.Amount);
        command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
        await connection.OpenAsync();
        int result = (int)await command.ExecuteScalarAsync();
        return result;
    }
}