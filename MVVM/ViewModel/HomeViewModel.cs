using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerSehen.Core;
using VerSehen.Services;
using System.Diagnostics;

namespace VerSehen.MVVM.ViewModel
{
    public class HomeViewModel : Core.ViewModel
    {
        private INavigationService navigation;
        public INavigationService Navigation { 
            get { return navigation; }
            set
            { 
                navigation = value;
                OnPropertyChanged();
            }
        }
        //Navigation.NavigateTo<SettingsViewModel>();
        public RelayCommand NavigateToSettingsCommand { get; set; }
        public HomeViewModel(INavigationService navigation)
        {
            Navigation = navigation;
            NavigateToSettingsCommand = new RelayCommand(o => {
                Navigation.NavigateTo<SettingsViewModel>();

            }, o => true);


        }
    }
}
