namespace InstaLike.Core.Services
{
    public interface ISequentialIdGenerator<out TId>
    {
        TId GetNextId();
    }
}
