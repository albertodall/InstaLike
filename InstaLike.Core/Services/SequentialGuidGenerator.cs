namespace InstaLike.Core.Services
{
    /// <summary>
    /// Generates Sequential GUIDs.
    /// </summary>
    public class SequentialGuidGenerator : ISequentialIdGenerator<SequentialGuid>
    {
        public SequentialGuid GetNextId() => SequentialGuid.NewGuid();
    }
}
