namespace InstaLike.Core.Domain
{
    public interface IEntity<out TId>
    {
        TId ID { get; }
        bool IsTransient();
    }
}
