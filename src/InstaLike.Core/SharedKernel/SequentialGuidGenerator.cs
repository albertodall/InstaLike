namespace InstaLike.Core.SharedKernel;

/// <summary>
/// Generates Sequential GUIDs.
/// </summary>
public class SequentialGuidGenerator : ISequentialIdGenerator<SequentialGuid>
{
    public SequentialGuid GetNextId() => SequentialGuid.NewGuid();
}
