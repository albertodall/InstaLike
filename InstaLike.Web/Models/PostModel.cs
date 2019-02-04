using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaLike.Web.Models
{
    public class PostModel
    {
        public int IDPost { get; set; }
        public string AuthorNickName { get; set; }
        public byte[] AuthorProfilePicture { get; set; }
        public DateTimeOffset PostDate { get; set; }
        public byte[] Picture { get; set; }
        public string Comment { get; set; }
        public string[] UserLikes { get; set; }
        public CommentModel[] Commenti { get; set; }
    }
}
