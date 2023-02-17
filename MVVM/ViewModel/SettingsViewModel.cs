using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VerSehen.Core;
using VerSehen.Services;
using Application = System.Windows.Application;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms.Integration;
using System.Runtime.InteropServices;
using System.Windows.Forms.Integration;

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
        private readonly HomeViewModel _homeViewModel;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetParent(IntPtr hWnd);

        const int GWL_STYLE = -16;
        const int WS_VISIBLE = 0x10000000;

        public RelayCommand NavigateToHomeAndStartGameCommand { get; set; }
        public SettingsViewModel(INavigationService navigation, HomeViewModel homeViewModel)
        {
            _homeViewModel = homeViewModel;
            NavigateToHomeAndStartGameCommand = new RelayCommand(o => {
                navigation.NavigateTo<HomeViewModel>();

                Task.Run(() => ProcessStart(_homeViewModel));
            }, o => true);
        }

        private void ProcessStart(HomeViewModel _homeViewModel)
        {
            using var process = new Process();
            process.StartInfo.FileName = @"C:\Users\jaeger04\source\repos\bladxy\Snake\Snake\bin\Debug\net6.0-windows\Snake.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            IntPtr handle = IntPtr.Zero;
            while (handle == IntPtr.Zero)
            {
                process.Refresh();
                if (process.HasExited)
                {
                    // handle process exit here
                    break;
                }
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    handle = process.MainWindowHandle;
                }
                Thread.Sleep(100);
            }

            var thread = new Thread(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var width = 800; // replace with your desired width
                    var height = 600; // replace with your desired height
                    

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        System.Windows.Forms.Panel p = new System.Windows.Forms.Panel();
                        SetParent(handle, p.Handle);
                        _homeViewModel.WfHost.Child = p;
                        SetWindowLong(handle, GWL_STYLE, WS_VISIBLE);
                        MoveWindow(handle,0, 0, width, height,true);
                        SetForegroundWindow(handle);
                        
                    });

                    StartWpfApplication();
                });
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private static void StartWpfApplication()
        {
            System.Windows.Threading.Dispatcher.Run();
        }
     
        
    }
}
    


