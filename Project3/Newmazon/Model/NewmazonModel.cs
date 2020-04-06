using Newmazon.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Newmazon.Model
{
    public class NewmazonModel
    {
        #region Fields
        public Kozpont _kozpont;
        public IPersistence _dataAccess;

        #endregion

        #region Properties
        #endregion

        #region Events
        public event EventHandler<NewmazonEventArgs> SimAdvanced;
        public event EventHandler<NewmazonEventArgs> SimCreated;
        #endregion

        #region Constructors
        public NewmazonModel(IPersistence dataAccess)
        {
            _dataAccess = dataAccess;
            _kozpont = new Kozpont();
        }

        #endregion

        #region Private Events
        #endregion

        #region Public game methods

        public void NewSimulation()
        {
            //_kozpont = new Kozpont();
        }
        #endregion

        #region Private game methods

        public async Task LoadGameAsync(String path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            AllData data = await _dataAccess.LoadAsync(path);

            _kozpont.NewSimulation(data);
            OnSimCreated();
        }

        public void StepSimulation()
        {
            _kozpont.StepSimulation();
            OnSimAdvanced();
        }

        private void OnSimAdvanced()
        {
            if (SimAdvanced != null)
                SimAdvanced(this, new NewmazonEventArgs(false, 0));
        }

        private void OnSimCreated()
        {
            if (SimCreated != null)
                SimCreated(this, new NewmazonEventArgs(false, 0));
        }
        #endregion

    }
}
