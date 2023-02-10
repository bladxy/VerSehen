using DocumentFormat.OpenXml.Office2010.PowerPoint;
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

        public RelayCommand NavigateToSettingsCommand { get; set; }
        public HomeViewModel(INavigationService navigation)
        {
            Navigation = navigation;
            NavigateToSettingsCommand = new RelayCommand(o => { Navigation.NavigateTo<SettingsViewModel>();
                // Starte das Snake-Spiel als separate Anwendung
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = @"C:\Users\jaeger04\source\repos\bladxy\Snake\Snake\bin\Debug\net6.0-windows\Snake.exe";
            process.Start();
                // Warte eine bestimmte Zeit, bis das Snake-Spiel gestartet wurde
            System.Threading.Thread.Sleep(3000);

            }, o => true);


        }
    }
}
