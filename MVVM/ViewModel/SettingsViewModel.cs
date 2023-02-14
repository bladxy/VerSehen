using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VerSehen.Core;
using VerSehen.MVVM.View;
using VerSehen.Services;
using Application = System.Windows.Application;
using System.Windows.Interop;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows.Media;
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

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        const int GWL_STYLE = -16;
        const int WS_VISIBLE = 0x10000000;

        public RelayCommand NavigateToHomeAndStartGameCommand { get; set; }
        public SettingsViewModel(INavigationService navigation)
        {
            NavigateToHomeAndStartGameCommand = new RelayCommand(o => {
                navigation.NavigateTo<HomeViewModel>();
                Task.Run(() => ProcessStart());
            }, o => true);
        }

        private void ProcessStart()
        {
            using var process = new Process();
            process.StartInfo.FileName = @"C:\Users\jaeger04\source\repos\bladxy\Snake\Snake\bin\Debug\net6.0-windows\Snake.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            var handle = IntPtr.Zero;
            while (handle == IntPtr.Zero)
            {
                process.Refresh();
                handle = process.MainWindowHandle;
                Thread.Sleep(100);
            }
            
            var thread = new Thread(() =>
            {
                // Erstelle das WPF-Host-Element und das Windows-Formular
                var host = new System.Windows.Forms.Integration.ElementHost();
                var homeview = new HomeView();
                var wfHost = homeview.wfHost;
                var parent = VisualTreeHelper.GetParent(wfHost);
                if (parent != null)
                {
                    if (parent is WindowsFormsHost host1)
                    {
                        host1.Child = null;
                    }
                    else if (parent is Panel parentPanel)
                    {
                        parentPanel.Children.Remove(wfHost);
                    }
                }

                host.Child = wfHost;

                var gameWindow = new System.Windows.Window();
                gameWindow.Content = host;
                gameWindow.Show();

                // Warte, bis das Fenster vollständig angezeigt wurde
                while (!gameWindow.IsLoaded)
                {
                    Thread.Sleep(100);
                }

                // Entferne das WindowsFormsHost-Element aus der HomeView
                var homeView = new HomeView();
                var wfHostInHomeView = homeView.wfHost;
                if (wfHostInHomeView.Parent is WindowsFormsHost parentHost)
                {
                    parentHost.Child = null;
                }

                // Übergabe des Delegaten an den Dispatcher
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SetParent(handle, new WindowInteropHelper(Application.Current.MainWindow).Handle);
                    SetWindowLong(handle, GWL_STYLE, WS_VISIBLE);

                    var isWindowLoadedAndVisible = new Func<bool>(() =>
                    {
                        return gameWindow.Dispatcher.Invoke(() => gameWindow.IsLoaded && gameWindow.IsVisible);
                    });

                    while (!isWindowLoadedAndVisible())
                    {
                        Thread.Sleep(10);
                    }

                    MoveWindow(handle, 0, 0, (int)gameWindow.Width, (int)gameWindow.Height, true);
                });

                // Starte die WPF-Anwendung
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }



    }
}

