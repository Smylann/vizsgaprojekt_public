using Avalonia;
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
using System.Text;
using System.Threading.Tasks;

namespace MobileVersion.ViewModels
{
    public partial class OwnCommentsVM : ViewModelBase
    {
        private readonly UserVM _userprofile;
        private readonly consoleClientModel _model;
        public string Username { get; } //you need this as a constructor parameter, because we get the value of this from the uservm's username
        [ObservableProperty] private int _displayLimit = 10;
        public List<PostVM> _fullComments;
        public ObservableCollection<PostVM>? OwnComms { get; }=new();

        public OwnCommentsVM(consoleClientModel model, UserVM user, string username, IEnumerable<PostsFromOwnComment> comments)
        {
            _model = model;
            _userprofile = user;
            Username = username;
            _fullComments = comments.Select(p => new PostVM(_model, p, null, this, null, null, null)).ToList();
            UpdateVisibleComments();
        }
        [RelayCommand]
        public void LoadMore()
        {
            if (DisplayLimit < _fullComments.Count) //if we only have 9, nothing gets added
            {
                DisplayLimit += 10;
                UpdateVisibleComments();
            }
        }
        private void UpdateVisibleComments()
        {
            var toAdd = _fullComments.Skip(OwnComms.Count).Take(DisplayLimit - OwnComms.Count); //we skip the already loaded ones, and add 10 more

            foreach (var comms in toAdd)
            {
                OwnComms.Add(comms);
            }
        }
        [RelayCommand]
        private void Close() => WeakReferenceMessenger.Default.Send(new NavigateToUserMessage(_userprofile.User));

    }
}
