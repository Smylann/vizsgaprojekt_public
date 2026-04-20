using System;
using System.ComponentModel.DataAnnotations;

namespace MobileVersion.Dtos
{
    public class PostDTO
    {
        public int userID { get; set; }
        public string categoryname { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public DateTime created_at { get; set; }
        public string? imagePath { get; set; }
        
        public string? ImageBase64 { get; set; }
        public string? FileExtension { get; set; }

    }
}
