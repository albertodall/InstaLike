namespace InstaLike.Web.Models
{
    #nullable disable

    public record UserProfileModel
    {
        public int UserID { get; set; }
        public string Nickname { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Bio { get; set; }
        public int NumberOfPosts { get; set; }
        public int NumberOfFollowers { get; set; }
        public int NumberOfFollows { get; set; }
        public bool IsCurrentUserProfile { get; set; }
        public bool Following { get; set; }
        public byte[] ProfilePictureBytes { get; set; }
        public PostThumbnailModel[] RecentPosts { get; set; }
    }
}
