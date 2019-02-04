namespace InstaLike.Web.Models
{
    public class UserProfileModel
    {
        public string Nickname { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Bio { get; set; }
        public int NumberOfPosts { get; set; }
        public int NumberOfFollowers { get; set; }
        public int NumberOfFollows { get; set; }
        public bool IsCurrentUserProfile { get; set; }
        public bool Following { get; set; }
        public byte[] ProfilePicture { get; set; }
        public PostThumbnailModel[] RecentPosts { get; set; }
    }
}
