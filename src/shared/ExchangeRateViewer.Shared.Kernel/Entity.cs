using System.Diagnostics.Contracts;

namespace ExchangeRateViewer.Shared.Kernel;

public class Entity<TId> where TId : notnull
{
    private readonly Lazy<int> _requestedHashCode;

    public virtual TId Id { get; private set; }

#pragma warning disable CS8618, CS9264 // EF Core constructor
    protected Entity() { }
#pragma warning restore CS8618, CS9264

    protected Entity(TId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
        _requestedHashCode = new Lazy<int>(() =>
            !IsTransient() ? checked(Id.GetHashCode() ^ 31) : base.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> entity)
        {
            return false;
        }

        return Equals(entity);
    }

    [Pure]
    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        if (IsTransient() || other.IsTransient())
        {
            return false;
        }

        return other.Id.Equals(Id);
    }

    [Pure]
    protected virtual bool IsTransient()
    {
        return Id.Equals(default);
    }


    [Pure]
    public override int GetHashCode()
    {
        return _requestedHashCode.Value;
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
        {
            return true;
        }
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
        {
            return false;
        }
        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
