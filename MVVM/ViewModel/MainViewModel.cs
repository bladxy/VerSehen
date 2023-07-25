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
using System.Windows.Input;

namespace VerSehen.MVVM.ViewModel
{
    class MainViewModel : Core.ViewModel
    {
        private SnakeAI snakeAI = new SnakeAI();
        public ICommand StopKiCommand { get; }
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

        public static HomeViewModel? GetHomeViewHandle()
        {
            // Find the ContentPresenter in the MainWindow
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            ContentControl contentControl = (ContentControl)mainWindow.FindName("ContentControl");
            var homeview = contentControl.Content as HomeViewModel;
            return homeview;
        }

        public MainViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;
            NavigateToHomeCommand = new RelayCommand(o => { Navigation.NavigateTo<HomeViewModel>(); }, o => true);
            NavigateToSettingsCommand = new RelayCommand(o => { Navigation.NavigateTo<SettingsViewModel>(); }, o => true);
            StartKi = new RelayCommand(async o =>
            {
                Navigation.NavigateTo<HomeViewModel>();
                await Task.Delay(1000); // Wait for 1 second
                var homeview = GetHomeViewHandle();
                SetFocusToWinFormsApp(homeview);
                snakeAI.Start(homeview.formHandle);
            }, o => true);
            StopKiCommand = new RelayCommand(o =>
            {
                snakeAI.SaveQTable("C:\\Users\\jaeger04\\Desktop\\SnakeKi\\Ki.Txt");
                snakeAI.Stop();
            });
        }
    }
}
