namespace WebApi.Models;

public class Store
{
    public string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool Active { get; set; }
    public List<Product> Products { get; set; } = [];
}