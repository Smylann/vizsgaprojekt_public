namespace vizsgaController.Dtos
{
    public class PostsFromOwnComment : DisplayAllPostsDTO
    {
        public List<OwnComments> OwnComments { get; set; }
    }
}
