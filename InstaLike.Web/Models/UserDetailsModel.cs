namespace InstaLike.Web.Models
{
    public class UserDetailsModel
    {
        public int UserID { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Bio { get; set; }
        public byte[] ProfilePicture { get; set; }
    }
}
