using MobileVersion.Dtos;
using System.Collections.ObjectModel;

namespace MobileVersion.Messages
{
    public record PostUpdatedMessage(
        int PostId, 
        string UpvoteColor, 
        string DownvoteColor, 
        string FavoriteColor, 
        int Votes, 
        ObservableCollection<GetCommentsFromPost> comments, 
        bool reported,
        string reportstatus
        );
    public record PostCreatedMessage();
}