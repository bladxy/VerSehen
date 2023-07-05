using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VerSehen.Core;
using VerSehen.MVVM.View;
using VerSehen.Services;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using static System.Windows.Forms.RibbonHelpers.WinApi;
using System.Drawing;

namespace VerSehen.MVVM.ViewModel
{
    class MainViewModel : Core.ViewModel
    {
        private SnakeAI snakeAI = new SnakeAI();

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

        public void SetFocusToWinFormsApp(HomeViewModel? homeview)
        {

            SetForegroundWindow(homeview.formHandle);

        }

        private static HomeViewModel? GetHomeViewHandle()
        {
            // Find the ContentPresenter in the MainWindow
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            ContentControl contentControl = (ContentControl)mainWindow.FindName("ContentControl");
            var homeview = contentControl.Content as HomeViewModel;
            return homeview;
        }

        public MainViewModel(INavigationService navigationService)
        {
            var homeview = GetHomeViewHandle();
            Navigation = navigationService;
            NavigateToHomeCommand = new RelayCommand(o => { Navigation.NavigateTo<HomeViewModel>(); }, o => true);
            NavigateToSettingsCommand = new RelayCommand(o => { Navigation.NavigateTo<SettingsViewModel>(); }, o => true);
            StartKi = new RelayCommand(async o =>
            {
                Navigation.NavigateTo<HomeViewModel>();
                await Task.Delay(1000); // Wait for 1 second
                SetFocusToWinFormsApp(homeview);
                snakeAI.Start(homeview.formHandle);
            }, o => true);
        }
    }
}
