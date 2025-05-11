using System.ComponentModel.DataAnnotations;

namespace Tutorial9.Model.DTOs;

public class ProductDto
{
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    [Range(1, int.MaxValue,  ErrorMessage = "Amount must be more than 0")]
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}