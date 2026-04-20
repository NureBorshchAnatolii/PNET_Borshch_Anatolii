namespace Domain.Model;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
 
    public ICollection<Test> Tests { get; set; } = new List<Test>();
}