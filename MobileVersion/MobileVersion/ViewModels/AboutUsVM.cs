using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MobileVersion.Dtos;
using MobileVersion.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileVersion.ViewModels
{
    public partial class AboutUsVM : ViewModelBase
    {
        public AboutUsVM()
        {
        }

        [RelayCommand]
        private void Close() => WeakReferenceMessenger.Default.Send(new GoBackMessage());
    }
}
