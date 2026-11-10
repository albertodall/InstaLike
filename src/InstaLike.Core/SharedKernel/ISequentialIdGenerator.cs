namespace InstaLike.Core.SharedKernel;

public interface ISequentialIdGenerator<out TId>
{
    TId GetNextId();
}
