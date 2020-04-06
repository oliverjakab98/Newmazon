﻿using Newmazon.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace Newmazon.ViewModel
{
    public class NewmazonViewModel : ViewModelBase
    {

        #region Fields
        private NewmazonModel _model;
        #endregion

        #region Properties
        /// <summary>
        /// Játékmező gyűjtemény lekérdezése.
        /// </summary>
        public ObservableCollection<NewmazonField> Fields { get; set; }

        /// <summary>
        /// Kilépés parancs lekérdezése.
        /// </summary>
        public DelegateCommand ExitCommand { get; private set; }
        public DelegateCommand NewsimCommand { get; private set; }

        public DelegateCommand UpdateTable { get; private set; }

        public int Size1 { get; private set; }
        public int Size2 { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// App való kilépés eseménye.
        /// </summary>
        public event EventHandler ExitApp;
        public event EventHandler NewSim;
        #endregion

        #region Constructors
        public NewmazonViewModel(NewmazonModel model)
        {
            _model = model;

            Size1 = _model._kozpont.tableSize;
            Size2 = _model._kozpont.tableSize;

            OnPropertyChanged("Size1");
            OnPropertyChanged("Size2");


            ExitCommand = new DelegateCommand(param => OnExitApp());
            NewsimCommand = new DelegateCommand(param => OnNewsim());
            model.SimAdvanced += new EventHandler<NewmazonEventArgs>(Model_SimAdvanced);

            Fields = new ObservableCollection<NewmazonField>();

            for (Int32 i = 0; i < _model._kozpont.tableSize; i++) // inicializáljuk a mezőket  
            {
                for (Int32 j = 0; j < _model._kozpont.tableSize; j++)
                {
                    Fields.Add(new NewmazonField
                    {
                        Identity = 'M',
                        X = i,
                        Y = j,
                        Number = i * _model._kozpont.tableSize + j, // a gomb sorszáma, amelyet felhasználunk az azonosításhoz
                        //StepCommand = new DelegateCommand(param => StepGame(Convert.ToInt32(param)))

                        // ha egy mezőre léptek, akkor jelezzük a léptetést, változtatjuk a lépésszámot
                    }) ;
                }
            }
            RefreshTable();
        }



        public void RefreshTable()
        {
            foreach (NewmazonField field in Fields)
            {
                if (_model._kozpont.table[field.X, field.Y].ID == 0)
                {
                    field.Identity = 'F';
                }
                if (_model._kozpont.table[field.X,field.Y].ID > 0 && _model._kozpont.table[field.X,field.Y].ID < 10001)
                {
                    field.Identity = 'M';
                }
                else if (_model._kozpont.table[field.X,field.Y].ID > 10000 && _model._kozpont.table[field.X,field.Y].ID < 20001)
                {
                    field.Identity = 'C';
                }
                else if (_model._kozpont.table[field.X,field.Y].ID > 20000 && _model._kozpont.table[field.X,field.Y].ID < 30001)
                {
                    Polc polc = (Polc)_model._kozpont.table[field.X,field.Y];
                    if (polc.otthon == true) { field.Identity = 'P'; }
                    else { field.Identity = 'M'; }
                }
                else if (_model._kozpont.table[field.X,field.Y].ID > 30000 && _model._kozpont.table[field.X,field.Y].ID < 40001)
                {
                    field.Identity = 'T';
                }
                
            }


            foreach (Robot robot in _model._kozpont.robots) 
            {
                int x = robot.x;
                int y = robot.y;

                NewmazonField field = Fields[x * _model._kozpont.tableSize + y];

                if (robot.polc != null) { field.Identity = 'V'; Debug.WriteLine("V"); Debug.WriteLine(field.X); Debug.WriteLine(field.Y); }
                else if (robot.polc == null && field.Identity == 'P') { field.Identity = 'A'; Debug.WriteLine("A"); Debug.WriteLine(field.X); Debug.WriteLine(field.Y); }
                else { field.Identity = 'R'; Debug.WriteLine("R"); Debug.WriteLine(field.X); Debug.WriteLine(field.Y); }
            }

        }

        private void Model_SimAdvanced(object sender, NewmazonEventArgs e)
        {
            RefreshTable();
        }

        private void OnExitApp()
        {
            if (ExitApp != null)
                ExitApp(this, EventArgs.Empty);
        }
        private void OnNewsim()
        {
            if (NewSim != null)
                NewSim(this, EventArgs.Empty);
        }
        #endregion
    }
}
