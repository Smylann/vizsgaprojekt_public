using Avalonia;
using Avalonia.Android;
using Avalonia.Media;
using Android.App;
using Android.Content.PM;

namespace MobileVersion.Android;

[Activity(
    Label = "MobileVersion.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .With(new FontManagerOptions
            {
                // "sans-serif" is the standard Android system alias that includes emoji fallbacks
                DefaultFamilyName = "sans-serif"
            });
    }
}
