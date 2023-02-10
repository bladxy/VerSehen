using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerSehen.Core;
using VerSehen.Services;

namespace VerSehen.MVVM.ViewModel
{
    public class SettingsViewModel : Core.ViewModel
    {
        private INavigationService navigation;
        public INavigationService Navigation
        {
            get { return navigation; }
            set
            {
                navigation = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand NavigateToHomeCommand { get; set; }
        public SettingsViewModel(INavigationService navigation)
        {
            Navigation = navigation;
            NavigateToHomeCommand = new RelayCommand(o => { Navigation.NavigateTo<HomeViewModel>(); }, o => true);
        }
    }
}
