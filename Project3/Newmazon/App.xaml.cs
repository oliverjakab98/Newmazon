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
            _model = new NewmazonModel();

            _viewModel = new NewmazonViewModel(_model);

            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.Show();
        }
    }
}
