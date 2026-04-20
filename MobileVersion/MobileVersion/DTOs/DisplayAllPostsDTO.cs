using System;
using System.ComponentModel.DataAnnotations;

namespace MobileVersion.Dtos
{
    public class DisplayAllPostsDTO
    {
        public int PostID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created_at { get; set; }
        public int Votes { get; set; }
        public string? ImagePath { get; set; }
    }
}
