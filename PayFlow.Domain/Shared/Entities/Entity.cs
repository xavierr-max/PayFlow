namespace PayFlow.Domain.Shared.Entities;

/// <summary>
/// Base class for all entities.
/// Provides identity and equality handling.
/// </summary>
public abstract class Entity(Guid id)
{
    protected Entity() : this(Guid.CreateVersion7())
    {
    }

    /// <summary>
    /// Unique identifier of the entity.
    /// </summary>
    private Guid Id { get; } = id;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}