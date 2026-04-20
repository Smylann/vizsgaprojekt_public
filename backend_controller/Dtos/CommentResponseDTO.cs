namespace vizsgaController.Dtos
{
    public class CommentResponseDTO
    {
        public int CommentID { get; set; }
        public string Commentcontent { get; set; } = string.Empty;
        public UserSummaryDTO? User { get; set; }
        public DateTime Created_at { get; set; }
    }
}
