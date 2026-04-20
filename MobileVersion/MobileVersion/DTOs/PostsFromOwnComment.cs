using System.Collections.Generic;

namespace MobileVersion.Dtos
{
    public class PostsFromOwnComment : DisplayAllPostsDTO
    {
        public List<OwnComments> OwnComments { get; set; }
    }
}
