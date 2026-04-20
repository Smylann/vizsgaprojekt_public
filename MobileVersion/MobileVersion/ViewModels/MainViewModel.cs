using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MobileVersion.Dtos;
using MobileVersion.Messages;
using MobileVersion.Model;
using MobileVersion.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace MobileVersion.ViewModels;

public partial class MainViewModel : ViewModelBase,
    IRecipient<GoBackMessage>,
    IRecipient<NavigateToPostMessage>,
    IRecipient<NavigateToUserMessage>,
    IRecipient<NavigateToOwnPostsMessage>,
    IRecipient<NavigateToOwnCommentsMessage>,
    IRecipient<NavigateToLikedPostsMessage>,
    IRecipient<NavigateToDislikedPostsMessage>,
    IRecipient<NavigateToFavoritesMessage>,
    IRecipient<NavigateToSettingsMessage>, 
    IRecipient<NavigateToAboutUsMessage>,
    IRecipient<NavigateToLoginMessage>,
    IRecipient<NavigateToCreatePostMessage>,
    IRecipient<UserLoggedInMessage>,
    IRecipient<UserLoggedOutMessage>,
    IRecipient<PostCreatedMessage>
{
    private readonly consoleClientModel _model;
    private readonly Filtering _filtering;

    public MainViewModel(consoleClientModel model)
    {
        _model = model;
        _filtering = new Filtering(model);
        WeakReferenceMessenger.Default.RegisterAll(this);
        _brightness = new();
        _isLoggedIn = false;
        _allPosts = new();
        _allUsers = new();
        _filteredPosts = new();
        _filteredUsers = new();
        _searchText = "";
        _sort = "";
        SortBy = new();
        Categories = new();
        _ = LoadDefault();
    }

    /***************************
    *                         *
    *                         *
    *      DEFAULT LOAD       *
    *                         *
    *                         *
    ***************************/

    [ObservableProperty]
    private BrightnessAdjustment _brightness;
    [ObservableProperty] private bool _isLoggedIn;

    private List<DisplayAllPostsDTO> _allPosts;
    private List<DisplayAllUserDTO> _allUsers;

    [ObservableProperty] private ObservableCollection<PostVM> _filteredPosts;
    [ObservableProperty] private ObservableCollection<UserVM> _filteredUsers;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private string _sort;
    [ObservableProperty] private CategoryDTO? _selectedCategory;
    [ObservableProperty] private bool _noPostsFound;
    [ObservableProperty] private bool _noUsersFound;
    [ObservableProperty] private int _postsDisplayLimit = 10;
    [ObservableProperty] private int _usersDisplayLimit = 10;

    public ObservableCollection<string> SortBy { get; }
    public ObservableCollection<CategoryDTO> Categories { get; }

    // --- Property change hooks ---
    partial void OnSearchTextChanged(string value) => _ = RunFilterAsync();
    partial void OnSortChanged(string value) => _ = RunFilterAsync();
    partial void OnSelectedCategoryChanged(CategoryDTO? value) => _ = RunFilterAsync();

    // Fix: replaced removed RelayCommand method groups with message-sending lambdas
    private async Task RunFilterAsync()
    {
        await _filtering.Filter(
            _allPosts,
            _allUsers,
            SearchText,
            SelectedCategory,
            Sort,
            FilteredPosts,
            FilteredUsers,
            PostsDisplayLimit, 
            UsersDisplayLimit); 

        NoPostsFound = FilteredPosts.Count == 0;
        NoUsersFound = FilteredUsers.Count == 0;
    }



    // --- Load ---
    public async Task LoadDefault()
    {
        try
        {
            _allPosts = await _model.getallposts() ?? new();
            _allUsers = await _model.getallusers() ?? new();

            Categories.Clear();
            var loadcats = await _model.getallcats();
            loadcats?.ForEach(r => Categories.Add(r));
            
            SortBy.Clear();
            new List<string> { "Newest", "Oldest", "Most Liked", "Most Disliked" } .ForEach(SortBy.Add);

            SelectedCategory = Categories.FirstOrDefault(c => c.categoryname == "All") ?? Categories.FirstOrDefault();
            Sort = "Most Liked";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading default: {ex.Message}");
        }
    }
    [RelayCommand]
    public void LoadMorePosts()
    {
        if (PostsDisplayLimit < _allPosts.Count)
        {
            PostsDisplayLimit += 5;
            _ = RunFilterAsync();
        }
    }

    [RelayCommand]
    public void LoadMoreUsers()
    {
        if (UsersDisplayLimit < _allUsers.Count)
        {
            UsersDisplayLimit += 5;
            _ = RunFilterAsync();
        }
    }
    [RelayCommand]
    public async Task Refresh()
    {
        PostsDisplayLimit = 10;
        UsersDisplayLimit = 10;
        _allPosts = await _model.getallposts() ?? new();
        _allUsers = await _model.getallusers() ?? new();
        Sort = "Newest";
        await RunFilterAsync();
    }
    /***************************
     *                         *
     *                         *
     *    NAVIGATION LOGIC     *
     *                         *
     *                         *
     ***************************/

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    public void Receive(GoBackMessage message) => CurrentPage = null;
    public void Receive(NavigateToAboutUsMessage message) => CurrentPage = new AboutUsVM();
    [RelayCommand] private void AboutUs() => WeakReferenceMessenger.Default.Send(new NavigateToAboutUsMessage());
    public void Receive(NavigateToUserMessage message) => CurrentPage = new UserVM(message.User, _model);
    public void Receive(NavigateToPostMessage message) => CurrentPage = message.PostVM;
    public void Receive(NavigateToOwnPostsMessage message) => CurrentPage = message.ownpostsvm;
    public void Receive(NavigateToOwnCommentsMessage message) => CurrentPage = message.owncommentsvm;
    public void Receive(NavigateToLikedPostsMessage message) => CurrentPage = message.likedpostsvm;
    public void Receive(NavigateToDislikedPostsMessage message) => CurrentPage = message.dislikedpostsvm;
    public void Receive(NavigateToFavoritesMessage message) => CurrentPage = message.favoritepostsvm;
    public void Receive(NavigateToSettingsMessage message) => CurrentPage = message.settingsvm;
    public void Receive(NavigateToLoginMessage message) => CurrentPage = message.loginvm;
    [RelayCommand]
    private void ToLogin()
    {
        if (_model.CurrentUser != null)
        {
            // User is logged in, show their profile
            CurrentPage = new UserVM(_model.CurrentUser, _model);
        }
        else
        {
            // User is not logged in, show login page
            WeakReferenceMessenger.Default.Send(new NavigateToLoginMessage(new LoginVM(_model)));
        }
    }
    public void Receive(NavigateToCreatePostMessage message) => CurrentPage = message.createvm;
    public void Receive(PostCreatedMessage message) => LoadDefault();
    [RelayCommand]
    private void ToCreatePost()
    {
        if (_model.CurrentUser != null)
        {
            CurrentPage = new CreatePostVM(_model);
        }
    }
    // Handle the login message
    public void Receive(UserLoggedInMessage message)
    {
        IsLoggedIn = true;
        _filtering.ClearCache(); // Force new PostVM instances so InitializeColorsAsync runs with CurrentUser set
        _ = LoadDefault();
    }
    public void Receive(UserLoggedOutMessage message)
    {
        IsLoggedIn = false;
        _filtering.ClearCache(); // Force new PostVM instances so InitializeColorsAsync runs with CurrentUser set
        _ = LoadDefault();
    }

    
}
