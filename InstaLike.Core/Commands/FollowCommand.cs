namespace InstaLike.Core.Commands
{
    public class FollowCommand : ICommand
    {
        public int FollowerID { get; }
        public string FollowedNickname { get; }

        public FollowCommand(int followerID, string followedNickname)
        {
            FollowerID = followerID;
            FollowedNickname = followedNickname;
        }
    }
}
