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

            _model = new NewmazonModel();

            _viewModel = new NewmazonViewModel(_model);

            _viewModel.ExitApp += new EventHandler(ViewModel_ExitApp);

            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.Closing += new System.ComponentModel.CancelEventHandler(View_Closing); // eseménykezelés a bezáráshoz
            _view.Show();
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
