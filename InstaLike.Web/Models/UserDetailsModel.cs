namespace InstaLike.Web.Models
{
    public class UserDetailsModel
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public byte[] ProfilePicture { get; set; }
    }
}
