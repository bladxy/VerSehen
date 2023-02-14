using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VerSehen.Core;
using VerSehen.MVVM.View;
using VerSehen.MVVM.ViewModel;

namespace VerSehen.Services
{
    public interface INavigationService
    {
        ViewModel CurrentView { get; }
        void NavigateTo<T>() where T : ViewModel;
        
    }

    public class NavigationService : ObservableObjects, INavigationService
    {
        public ViewModel _currentView;
        private readonly Func<Type, ViewModel> viewModelFactory;

        public ViewModel CurrentView
        {
            get => _currentView; 
            private set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }


        public NavigationService(Func<Type, ViewModel> viewModelFactory)
        {
            this.viewModelFactory = viewModelFactory;
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModel
        {
            ViewModel viewmodel = viewModelFactory(typeof(TViewModel));
            CurrentView = viewmodel;
        }
    }
}
