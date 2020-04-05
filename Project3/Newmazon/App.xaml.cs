using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Newmazon.View;
using Newmazon.ViewModel;
using Newmazon.Model;
using Newmazon.Persistence;
using Microsoft.Win32;
using System.Diagnostics;

namespace Newmazon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private MainWindow _view; //Az alkalmazás főablaka
        private NewmazonViewModel _viewModel;
        private NewmazonModel _model;
        private DispatcherTimer _timer;

        #endregion

        #region Constructors
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }
        #endregion

        private void App_Startup(object sender, StartupEventArgs e)
        {
            IPersistence dataAccess;
            dataAccess = new NewmazonFileDataAccess(AppDomain.CurrentDomain.BaseDirectory);

            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.Closing += new System.ComponentModel.CancelEventHandler(View_Closing); // eseménykezelés a bezáráshoz
            _view.Show();

            _model = new NewmazonModel(dataAccess);

            char[,] dt = { {'R', 'M', 'M', 'M', 'M' }, 
                           {'F', 'M', 'P', 'M', 'M' }, 
                           {'F', 'M', 'P', 'M', 'T' }, 
                           {'F', 'M', 'M', 'M', 'M' }, 
                           {'F', 'M', 'C', 'M', 'C' } };
            List<Goods> g = new List<Goods>();
            int[] g1 = new int[1]; g1[0] = 1;
            int[] g2 = new int[2]; g2[0] = 1; g2[0] = 2;
            g.Add(new Goods(1, 2, g1));
            g.Add(new Goods(1, 2, g2));
            int tS = 5;
            int rE = 100;
            AllData data = new AllData(dt, g, tS, rE);

            _model._kozpont.NewSimulation(data);

            MenuFileNewSim_Click(null, null); // ha Mégse-t clickelsz error
            _viewModel = new NewmazonViewModel(_model);
            /*
            _model.StartSimulationForViewmodel();
            */
            _viewModel.ExitApp += new EventHandler(ViewModel_ExitApp);

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private async void MenuFileNewSim_Click(Object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Simulation files (*.sim)|*.sim";
            if (openFileDialog.ShowDialog() == true) // ha kiválasztottunk egy fájlt
            {
                try
                {
                    // játék betöltése
                    await _model.LoadGameAsync(openFileDialog.FileName);
                }
                catch (NewmazonDataException)
                {
                    MessageBox.Show("Játék betöltése sikertelen!" + Environment.NewLine + "Hibás az elérési út, vagy a fájlformátum.", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void Timer_Tick(Object sender, EventArgs e)
        {
            _model.StepSimulation();
        }

        private void View_Closing(object sender, CancelEventArgs e)
        {

            if (MessageBox.Show("Biztos, hogy ki akar lépni?", "NewMazon", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true; // töröljük a bezárást
            }
        }

        private void ViewModel_ExitApp(object sender, System.EventArgs e)
        {
            _view.Close(); // ablak bezárása
        }
    }
}
