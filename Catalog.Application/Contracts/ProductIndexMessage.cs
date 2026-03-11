namespace Catalog.Application.Contracts;

public class ProductIndexMessage
{
    public Guid ProductId { get; set; }
    public string Action { get; set; } = string.Empty; 
}