namespace Tutorial9.Services;
using Tutorial9.Model.DTOs;
 
public interface IDbService
{
    Task<int> AddProductToWarehouse(ProductDto product);
    Task<int> AddProductToWarehouseProcedure(ProductDto product);
}