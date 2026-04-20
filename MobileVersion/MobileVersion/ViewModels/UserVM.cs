using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MobileVersion.Dtos;
using MobileVersion.Messages;
using MobileVersion.Model;
using MobileVersion.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MobileVersion.ViewModels
{
    public partial class UserVM : ViewModelBase
    {
        private readonly consoleClientModel _model;

        public UserVM(DisplayAllUserDTO user, consoleClientModel model)
        {
            _user = user;
            _model = model;
        }

        [ObservableProperty] private DisplayAllUserDTO _user;

        [RelayCommand]
        private void Close() => WeakReferenceMessenger.Default.Send(new GoBackMessage());

        [RelayCommand]
        private void Select() => WeakReferenceMessenger.Default.Send(new NavigateToUserMessage(User));
        [RelayCommand]
        private void ToSettings() 
        {
            if(_model.CurrentUser == User)
            {
                WeakReferenceMessenger.Default.Send(new NavigateToSettingsMessage(new SettingsVM(this, _model)));
            }
            
        }     

        [RelayCommand]
        private async Task ShowOwnPosts()
        {
            try
            {
                var posts = await _model.ownposts(User.UserID);
                WeakReferenceMessenger.Default.Send(new NavigateToOwnPostsMessage(new OwnPostsVM(_model,this, User.Username, posts ?? new())));
            }
            catch { }
        }
        [RelayCommand]
        private async Task ShowOwnComments()
        {
            try
            {
                var comments = await _model.postsfromowncomment(User.UserID);
                WeakReferenceMessenger.Default.Send(new NavigateToOwnCommentsMessage(new OwnCommentsVM(_model, this, User.Username, comments ?? new())));
            }
            catch { }
        }
        [RelayCommand]
        private async Task ShowLikedPosts()
        {
            try
            {
                var lposts = await _model.likedposts(User.UserID);
                WeakReferenceMessenger.Default.Send(new NavigateToLikedPostsMessage(new LikedPostsVM(_model,this, User.Username, lposts ?? new())));
            }
            catch { }
        }
        [RelayCommand]
        private async Task ShowDislikedPosts()
        {
            try
            {
                var dposts = await _model.dislikedposts(User.UserID);
                WeakReferenceMessenger.Default.Send(new NavigateToDislikedPostsMessage(new DislikedPostsVM(_model, this, User.Username, dposts ?? new())));
            }
            catch { }
        }
        [RelayCommand]
        private async Task ShowFavoritePosts()
        {
            try
            {
                var favs = await _model.favorites(User.UserID);
                WeakReferenceMessenger.Default.Send(new NavigateToFavoritesMessage(new FavoritePostsVM(_model, this, User.Username, favs ?? new())));
            }
            catch { }
        }
        
    }
}
