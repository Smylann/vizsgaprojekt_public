namespace vizsgaController.Dtos
{
    public class PostFeedDTO
    {
        public int PostID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Created_at { get; set; }
        public int Votes { get; set; }
        public string? ImagePath { get; set; }
        public UserSummaryDTO? User { get; set; }
        public CategorySummaryDTO? Category { get; set; }
        public List<CommentRefDTO> Comments { get; set; } = new();
    }

    public class UserSummaryDTO
    {
        public string Username { get; set; } = string.Empty;
    }

    public class CategorySummaryDTO
    {
        public string Categoryname { get; set; } = string.Empty;
    }

    public class CommentRefDTO
    {
        public int CommentID { get; set; }
    }
}
