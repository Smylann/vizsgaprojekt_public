using CommunityToolkit.Mvvm.ComponentModel;
using MobileVersion.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace MobileVersion.Model
{
    public partial class BrightnessAdjustment : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OverlayOpacity))]
        private double _brightnessValue;

        private DispatcherTimer _resetTimer;

        private int _hundredCount;
        private bool _hasCountedCurrentHundred;

        public double OverlayOpacity => BrightnessValue / 100.0;
        public BrightnessAdjustment()
        {
            _resetTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(7)
            };
            _resetTimer.Tick += (s, e) =>
            {
                _hundredCount = 0;
                _hasCountedCurrentHundred = false;
                _resetTimer.Stop();
            };
        }

        partial void OnBrightnessValueChanged(double value)
        {
            if (value >= 80 && !_hasCountedCurrentHundred)
            {
                if (_hundredCount ==0) _resetTimer?.Start();
                _hundredCount++;
                _hasCountedCurrentHundred = true;

                if (_hundredCount >= 5)
                {
                    _resetTimer?.Stop();
                    ShowSite();
                    _hundredCount = 0;
                    
                    // Defer the reset so the Slider's drag event doesn't overwrite it
                    Dispatcher.UIThread.Post(async () =>
                    {
                        await Task.Delay(300); 
                        BrightnessValue = 0;
                    });
                }
            }
            else if (value < 40)
            {
                _hasCountedCurrentHundred = false;
            }
        }

        private void ShowSite()
        {
            var uri = new Uri("https://cat-bounce.com/");

            // Try to get the Avalonia Launcher based on the platform paradigm
            TopLevel? topLevel = null;

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                topLevel = desktop.MainWindow;
            }
            else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            {
                topLevel = TopLevel.GetTopLevel(singleView.MainView);
            }

            // Launch the URI if a valid TopLevel is found
            if (topLevel != null)
            {
                _ = topLevel.Launcher.LaunchUriAsync(uri);
            }
            else
            {
                // Fallback for Desktop if Avalonia mapping fails
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = uri.ToString(),
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to open URL: {ex.Message}");
                }
            }
        }
    }
}
