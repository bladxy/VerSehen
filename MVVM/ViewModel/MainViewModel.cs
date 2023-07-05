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
       public HomeViewModel? homeview = GetHomeViewHandle();

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

        public void SetFocusToWinFormsApp()
        {

            SetForegroundWindow(this.homeview.formHandle);

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
            HomeViewModel? homeview = GetHomeViewHandle();
            Navigation = navigationService;
            NavigateToHomeCommand = new RelayCommand(o => { Navigation.NavigateTo<HomeViewModel>(); }, o => true);
            NavigateToSettingsCommand = new RelayCommand(o => { Navigation.NavigateTo<SettingsViewModel>(); }, o => true);
            StartKi = new RelayCommand(o =>
            {
                Navigation.NavigateTo<HomeViewModel>();
                SetFocusToWinFormsApp();
                //snakeAI.Start(homeview.formHandle);
            }, o => true);
        }
    }
}
