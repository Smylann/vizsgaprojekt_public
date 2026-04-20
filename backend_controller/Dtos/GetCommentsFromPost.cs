namespace vizsgaController.Dtos
{
    public class GetCommentsFromPost
    {
        public int userID { get; set; }
        public string username { get; set; }
        public int commentid { get; set; }
        public string commentcontent { get; set; }
        public DateTime commentcreated_at { get; set; }
    }
}
