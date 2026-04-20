using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MobileVersion.Dtos;
using MobileVersion.Messages;
using MobileVersion.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MobileVersion.ViewModels
{
    // 1. Add IRecipient
    public partial class PostVM : ViewModelBase, IRecipient<PostUpdatedMessage>
    {
        private readonly consoleClientModel _model;
        private readonly OwnPostsVM? _ownposts;
        private readonly OwnCommentsVM? _owncomms;
        private readonly DislikedPostsVM? _dposts;
        private readonly LikedPostsVM? _lposts;
        private readonly FavoritePostsVM? _fposts;

        [ObservableProperty] private DisplayAllPostsDTO _post;
        [ObservableProperty] private int _displayLimit = 10;
        private List<GetCommentsFromPost> _comments;
        [ObservableProperty] private ObservableCollection<GetCommentsFromPost>? _displayComments = new();
        public List<OwnComments> UserOwnComments => (Post as PostsFromOwnComment)?.OwnComments ?? new List<OwnComments>();
         

        [ObservableProperty] private int _votes;
        [ObservableProperty] private string _upvotecolor;
        [ObservableProperty] private string _downvotecolor;
        [ObservableProperty] private string _favorited;
        [ObservableProperty] private string _commentText;
        [ObservableProperty] private string _reportReason;
        [ObservableProperty] private string _repStatus;
        [ObservableProperty] private string _voteStatus;
        [ObservableProperty] private bool _isCommentingOpen;
        [ObservableProperty] private bool _isReportOpen;

        public bool IsLoggedIn => _model.CurrentUser != null;

        [ObservableProperty] private bool _notReported;
        [ObservableProperty] private string? _imageUrl;

        public PostVM(consoleClientModel model, 
            DisplayAllPostsDTO post, 
            OwnPostsVM? ownposts = null, 
            OwnCommentsVM? comms = null, 
            DislikedPostsVM? dposts = null, 
            LikedPostsVM? lposts = null, 
            FavoritePostsVM? fposts = null)
        {
            _model = model;
            _post = post;
            _ownposts = ownposts;
            _owncomms = comms;
            _dposts = dposts;
            _lposts = lposts;
            _fposts = fposts;
            _comments = new();
            _votes = post.Votes;
            _upvotecolor = "Transparent";
            _downvotecolor = "Transparent";
            _favorited = "Transparent";
            _isCommentingOpen = false;
            _isReportOpen = false;
            _notReported = true;
            CommentText = string.Empty;
            ReportReason = string.Empty;
            RepStatus = string.Empty;
            VoteStatus = string.Empty;
            ImageUrl = !string.IsNullOrEmpty(post.ImagePath) 
                ? $"{App.GetBackendUrl()}{post.ImagePath}"  
                : null;
            _ = InitializeComments(); //loads comments on new post open
            _ = InitializeColors(); //loads interaction colors on new post open
            _ = InitializeReport(); //loads report status on new post open
            _ = InitializeVotes();
            
            WeakReferenceMessenger.Default.Register(this);
        }


        // Sends NavigateToPostMessage → caught by MainViewModel.Receive(NavigateToPostMessage)
        [RelayCommand]
        private void Select()
        {
            DisplayLimit = 10;  
            DisplayComments?.Clear();  
            _ = InitializeComments();
            WeakReferenceMessenger.Default.Send(new NavigateToPostMessage(this));
        }

        // Sends GoBackMessage → caught by MainViewModel.Receive(GoBackMessage)
        [RelayCommand]
        private void Close()
        {
            if (_ownposts != null) WeakReferenceMessenger.Default.Send(new NavigateToOwnPostsMessage(_ownposts)); 
            else if (_owncomms != null) WeakReferenceMessenger.Default.Send(new NavigateToOwnCommentsMessage(_owncomms));
            else if (_dposts != null) WeakReferenceMessenger.Default.Send(new NavigateToDislikedPostsMessage(_dposts));
            else if (_lposts != null) WeakReferenceMessenger.Default.Send(new NavigateToLikedPostsMessage(_lposts));
            else if (_fposts != null) WeakReferenceMessenger.Default.Send(new NavigateToFavoritesMessage(_fposts));
            else WeakReferenceMessenger.Default.Send(new GoBackMessage());
        }

        [RelayCommand] private void OpenCommenting() => IsCommentingOpen = !IsCommentingOpen;
        [RelayCommand] private void OpenReporting() => IsReportOpen = !IsReportOpen;

        [RelayCommand]
        private async Task UpVote()
        {
            if (_model.CurrentUser == null) return;
            await _model.votePost(new VoteDTO { postId = Post.PostID, userId = _model.CurrentUser.UserID, isUpvote = true });

            if (Upvotecolor == "Green")
            {
                Upvotecolor = "Transparent";
                Votes--; // Removing an upvote
                _lposts?.LikedPosts.Remove(this);
            }
            else
            {
                if (Downvotecolor == "Red") Votes++; // Removing the downvote penalty

                Upvotecolor = "Green";
                Downvotecolor = "Transparent";
                Votes++; // Adding the upvote
                _dposts?.DislikedPosts.Remove(this);
            }
            NotifySync();
            InitializeVotes();
        }

        [RelayCommand]
        private async Task DownVote()
        {
            if (_model.CurrentUser == null) return;
            await _model.votePost(new VoteDTO { postId = Post.PostID, userId = _model.CurrentUser.UserID, isUpvote = false });

            if (Downvotecolor == "Red")
            {
                Downvotecolor = "Transparent";
                Votes++; // Removing a dislike
                _dposts?.DislikedPosts.Remove(this);
            }
            else
            {
                if (Upvotecolor == "Green") Votes--; // Removing the downvote penalty

                Downvotecolor = "Red";
                Upvotecolor = "Transparent";
                Votes--; // Adding the dislike
                _lposts?.LikedPosts.Remove(this);
            }
            NotifySync();
            InitializeVotes();
        }

        [RelayCommand]
        private async Task Favoriting()
        {
            if (_model.CurrentUser == null) return;
            await _model.favouritePosts(new FavouritePostDTO { postId = Post.PostID, userId = _model.CurrentUser.UserID });

            if (Favorited == "Yellow")
            {
                Favorited = "Transparent";
                _fposts?.FavoritePosts.Remove(this);
            }
            else
            {
                Favorited = "Yellow";
            }
            NotifySync();
        }
        [RelayCommand]
        private async Task DeleteOwnPost()
        {
            if (_model.CurrentUser == null) return;
            try
            {
                await _model.deleteOwnPost(new DeleteOwnPostDTO { postid = Post.PostID, userId = _model.CurrentUser.UserID });
                _ownposts?.OwnPosts.Remove(this);
            }
            catch { }

        }
        [RelayCommand]
        private async Task Commenting()
        {
            if (_model.CurrentUser == null) return;
            try
            {
                await _model.comment(new CommentDTO { userID = _model.CurrentUser.UserID, postID = Post.PostID, commentcontent = CommentText });
                var comment = new GetCommentsFromPost { commentcontent = CommentText, userID = _model.CurrentUser.UserID, username = _model.CurrentUser.Username, commentcreated_at = DateTime.Now };
                _comments.Add(comment);
                DisplayComments.Add(comment);
                IsCommentingOpen = false;
                CommentText = string.Empty;
            }
            catch { }

        }
        
        
        [RelayCommand]
        private async Task Report()
        {
            if (_model.CurrentUser != null && NotReported)
            {
                try
                {
                    await _model.createReport(new ReportDTO { userID = _model.CurrentUser.UserID, postID = Post.PostID, reportreason = ReportReason, reportcreated_at = DateTime.Now });
                    IsReportOpen = false;
                    ReportReason = string.Empty;
                    NotReported = false;
                    RepStatus = "Pending";
                    NotifySync();
                }
                catch { }
            }
        }

        //refetch datas when opening a new instances of the same post
        private async Task InitializeColors()
        {
            if (_model.CurrentUser == null) return;

            var liked = await _model.likedposts(_model.CurrentUser.UserID);
            if (liked.Any(x => x.PostID == Post.PostID)) Upvotecolor = "Green";

            var disliked = await _model.dislikedposts(_model.CurrentUser.UserID);
            if (disliked.Any(x => x.PostID == Post.PostID)) Downvotecolor = "Red";

            var favs = await _model.favorites(_model.CurrentUser.UserID);
            if (favs.Any(x => x.PostID == Post.PostID)) Favorited = "Yellow";
        }
        private async Task InitializeComments()
        {
            try
            {
                _comments.Clear();
                _comments = await _model.fetchcomments(Post.PostID);
                DisplayComments?.Clear();
                UpdateVisibleComments();
            }
            catch { }
        }
        [RelayCommand]
        public void LoadMore()
        {
            if (DisplayLimit < _comments.Count) //if we only have 9, nothing gets added
            {
                DisplayLimit += 10;
                UpdateVisibleComments();
            }
        }
        private void UpdateVisibleComments()
        {
            DisplayComments?.Clear();
            foreach (var comment in _comments.Take(DisplayLimit))
            {
                DisplayComments?.Add(comment);
            }
        }
        private async Task InitializeReport()
        {
            if (_model.CurrentUser == null) return;

            List<OwnReports> reps = await _model.reports(_model.CurrentUser.UserID);
            OwnReports? existingReport = reps.FirstOrDefault(x => x.PostID == Post.PostID);

            if (existingReport != null)
            {
                NotReported = false;
                RepStatus = existingReport.ReportStatus;
            }
        }
        private async Task InitializeVotes()
        {
            if (Votes < 0)
            {
                VoteStatus = "💀";
            }
            else if (Votes > 1000)
            {
                VoteStatus = "🚀";
            }
            else
            {
                VoteStatus = "🌟";
            }
        }


        /***************************
        *                         *
        *                         *
        *     Real-Time changes   * 
        *        (optional)       *
        *                         *
        *                         *
        ***************************/

        // Helper to notify other instances of interaction
        private void NotifySync()
        {
            WeakReferenceMessenger.Default.Send(new PostUpdatedMessage(
                Post.PostID,
                Upvotecolor,
                Downvotecolor,
                Favorited,
                Votes,
                DisplayComments,
                NotReported,
                RepStatus));
        }

        // Handles NotifySync()
        public void Receive(PostUpdatedMessage message)
        {
            if (message.PostId == Post.PostID)
            {
                Upvotecolor = message.UpvoteColor;
                Downvotecolor = message.DownvoteColor;
                Favorited = message.FavoriteColor;
                Votes = message.Votes;
                DisplayComments = message.comments;
                NotReported = message.reported;
                RepStatus = message.reportstatus;
            }
        }
        
    }
}
