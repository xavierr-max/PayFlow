using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Sales.Entities;

/// <summary>
/// Product category used to organize inventory.
/// </summary>
public class Category : Entity
{
    protected Category() { }

    public Category(string name, string color)
    {
        Validate(name, color);

        Name = name.Trim();
        Color = color.Trim();
    }

    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;

    public void Update(string name, string color)
    {
        Validate(name, color);

        Name = name.Trim();
        Color = color.Trim();
    }

    private static void Validate(string name, string color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentException("Color is required");
    }
}
