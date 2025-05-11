using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model.DTOs;
using Tutorial9.Services;

namespace Tutorial9.Controllers;
[Route("api/[controller]")]
[ApiController]
public class WarehouseController: ControllerBase
{
    private readonly IDbService _dbService;
    public WarehouseController(IDbService dbService)
    {
        _dbService = dbService;
    }
    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse(ProductDto productDto)
    {
        if (productDto == null)
            return BadRequest("Product cannot be null");

        try
        {
            var id = await _dbService.AddProductToWarehouse(productDto);
            return Ok(id);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("Procedure")]
    public async Task<IActionResult> AddProductToWarehouseProcedure(ProductDto productDto)
    {
        if(productDto == null)
            return BadRequest("Product cannot be null");
        try
        {
            var result = await _dbService.AddProductToWarehouseProcedure(productDto);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}