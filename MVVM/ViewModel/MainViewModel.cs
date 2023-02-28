using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using VerSehen.Core;
using VerSehen.Services;

namespace VerSehen.MVVM.ViewModel
{
    class MainViewModel : Core.ViewModel
    {
        private INavigationService _navigation;
        public INavigationService Navigation
        {
            get { return _navigation; }
            set
            { 
                _navigation = value;
                OnPropertyChanged();
            }
        }
        public RelayCommand NavigateToHomeCommand { get; set; }
        public RelayCommand NavigateToSettingsCommand { get; set; }
        public RelayCommand StartKi { get; set; }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(IntPtr hWnd);

        public void SetFocusToWinFormsApp()
        {
            // Find the ContentPresenter in the MainWindow
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            ContentControl contentControl = (ContentControl)mainWindow.FindName("ContentControl");
            var homeview = contentControl.Content as HomeViewModel;
            
            // Find the Windows Forms host panel
            var hostPanel = homeview.WfHost.Child;

            // Set focus to the Windows Forms application
            SetForegroundWindow(hostPanel.Handle);
            SetFocus(hostPanel.Handle);
        }

        public MainViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;
            NavigateToHomeCommand = new RelayCommand(o => { Navigation.NavigateTo<HomeViewModel>(); }, o => true);
            NavigateToSettingsCommand = new RelayCommand(o => { Navigation.NavigateTo<SettingsViewModel>(); }, o => true);
            StartKi = new RelayCommand(o =>
            {
                Navigation.NavigateTo<HomeViewModel>();
                SetFocusToWinFormsApp();
                KiLogics.StartKi();
            }, o => true);
        }
    }
}
