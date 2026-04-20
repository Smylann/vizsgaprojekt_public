using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using MobileVersion.ViewModels;

namespace MobileVersion.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.CurrentPage))
                {
                    UpdateBlur(vm.CurrentPage != null);

                    if (vm.CurrentPage != null)
                        SideMenu.IsVisible = false;
                }
            };
        }
    }

    private void UpdateBlur(bool isPageOpen)
    {
        MainContent.Effect = new BlurEffect { Radius = isPageOpen ? 12 : 0 };
    }

    private void OpenMenu(object? sender, RoutedEventArgs e)
    {
        SideMenu.IsVisible = true;
    }

    private void CloseMenu(object? sender, RoutedEventArgs e)
    {
        SideMenu.IsVisible = false;
    }

    private void PostsScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer sv && DataContext is MainViewModel vm)
        {
            // Trigger load when 50px from bottom
            if (sv.Offset.Y >= sv.Extent.Height - sv.Viewport.Height - 50)
            {
                vm.LoadMorePostsCommand.Execute(null);
            }
        }
    }

    private void UsersScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer sv && DataContext is MainViewModel vm)
        {
            if (sv.Offset.Y >= sv.Extent.Height - sv.Viewport.Height - 50)
            {
                vm.LoadMoreUsersCommand.Execute(null);
            }
        }
    }

    private void Refresh_OnClick(object? sender, RoutedEventArgs e)
    {
        // Reset both post and user scrollers to the top
        PostsScroller.Offset = new Vector(0, 0);
        UsersScroller.Offset = new Vector(0, 0);
    }
}