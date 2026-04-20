using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MobileVersion.ViewModels;

namespace MobileVersion;

public partial class DislikedPostsPage : UserControl
{
    public DislikedPostsPage()
    {
        InitializeComponent();
    }
    private void ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer sv && DataContext is DislikedPostsVM vm)
        {
            // Trigger when user is within 50px of the bottom
            if (sv.Offset.Y >= sv.Extent.Height - sv.Viewport.Height - 50)
            {
                vm.LoadMoreCommand.Execute(null);
            }
        }
    }
}