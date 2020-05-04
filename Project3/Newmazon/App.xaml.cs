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
using System.IO;

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
        private int sec;
        private int ms;

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
            sec = 0;
            ms = 500;


            _model = new NewmazonModel(dataAccess);
            _model._kozpont.SimOver += new EventHandler<NewmazonEventArgs>(Model_SimOver);
            //_model.SimCreated += new EventHandler<NewmazonEventArgs>(Model_SimCreated);

            char[,] dt = { {'R', 'M', 'M', 'M', 'M' }, 
                           {'F', 'M', 'P', 'M', 'M' }, 
                           {'F', 'M', 'P', 'M', 'T' }, 
                           {'F', 'M', 'M', 'M', 'M' }, 
                           {'F', 'M', 'C', 'M', 'C' } };
            List<Goods> g = new List<Goods>();
            int[] g1 = new int[1]; g1[0] = 1;
            int[] g2 = new int[2]; g2[0] = 1; g2[1] = 2;
            g.Add(new Goods(1, 2, g1));
            g.Add(new Goods(2, 2, g2));
            int tS = 5;
            int rE = 100;
            AllData data = new AllData(dt, g, tS, rE);

            _model._kozpont.NewSimulation(data);
            

            _viewModel = new NewmazonViewModel(_model);
            _viewModel.ExitApp += new EventHandler(ViewModel_ExitApp);
            _viewModel.NewSim += new EventHandler(MenuFileNewSim_Click);
            _viewModel.ResSim += new EventHandler(MenuFileRestartSim_Click);
            _viewModel.TimeRestart += new EventHandler(ViewModel_TimeRestart);
            _viewModel.SpeedUp += new EventHandler(ViewModel_SpeedUp);
            _viewModel.SlowDown += new EventHandler(ViewModel_SlowDown);

            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.Closing += new System.ComponentModel.CancelEventHandler(View_Closing); // eseménykezelés a bezáráshoz
            _view.Show();

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, sec, ms);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void MenuFileRestartSim_Click(Object sender, EventArgs e) 
        {
            _model._kozpont.NewSimulation(_model._kozpont.savedData);
            sec = 0;
            ms = 500;
            _timer.Interval = new TimeSpan(0, 0, 0, sec, ms);
            _timer.Start();
        }

        private async void MenuFileNewSim_Click(Object sender, EventArgs e)
        {
            sec = 0;
            ms = 500;
            _timer.Interval = new TimeSpan(0, 0, 0, sec, ms);
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
            _timer.Stop();
            if (MessageBox.Show("Biztos, hogy ki akar lépni?", "NewMazon", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true; // töröljük a bezárást
            }
            _timer.Start();
        }

        private void Model_SimOver(object sender, NewmazonEventArgs e)
        {
            _timer.Stop();
            string path = AppDomain.CurrentDomain.BaseDirectory + "/latestLog.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("Lépésszám: " + _model._kozpont.TotalSteps.ToString());
                sw.WriteLine("Összes energiahasználat: " + _model._kozpont.TotalEnergyUsed.ToString());
                for(int i = 0; i < _model._kozpont.TotalRobots; i++)
                {
                    sw.WriteLine((i+1).ToString() + ". robot energiahasználata: " + _model._kozpont.getRobotEnergy(i));
                }
            }
            MessageBox.Show("Szimuláció vége!", "NewMazon", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /*private void Model_SimCreated(object sender, NewmazonEventArgs e)
        {
            _view.DataContext = null;

            _viewModel = null;
            _viewModel = new NewmazonViewModel(_model);
            _viewModel.ExitApp += new EventHandler(ViewModel_ExitApp);
            _viewModel.NewSim += new EventHandler(MenuFileNewSim_Click);

            _view.DataContext = _viewModel;
        }*/

        private void ViewModel_ExitApp(object sender, System.EventArgs e)
        {
            _view.Close(); // ablak bezárása
        }

        private void ViewModel_TimeRestart(object sender, System.EventArgs e)
        {
            _timer.Start(); // ablak bezárása
        }

        private void ViewModel_SpeedUp(object sender, System.EventArgs e)
        {
            ms -= 100;
            if(sec == 0 && ms == 0)
            {
                return;
            }
            else if(sec == 0 && ms > 0)
            {
                _timer.Interval = new TimeSpan(0, 0, 0, sec, ms);
            }
            else if(sec == 1)
            {
                sec -= 1;
                ms = 900;
                _timer.Interval = new TimeSpan(0, 0, 0, sec, ms);
            }
        }

        private void ViewModel_SlowDown(object sender, System.EventArgs e)
        {
            ms += 100;
            if(sec == 1)
            {
                return;
            }
            else if (sec == 0 && ms < 1000)
            {
                _timer.Interval = new TimeSpan(0, 0, 0, sec, ms);
            }
            else if(ms == 1000)
            {
                sec += 1;
                ms = 0;
                _timer.Interval = new TimeSpan(0, 0, 0, sec, ms);
            }
        }
    }
}
