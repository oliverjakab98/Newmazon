using Newmazon.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newmazon.Model
{
    public class NewmazonModel
    {
        #region Fields
        public Kozpont _kozpont;

        #endregion

        #region Properties
        #endregion

        #region Events
        #endregion

        #region Constructors
        public NewmazonModel(IPersistence dataAccess)
        {
            AllData data = dataAccess.LoadAsync();
            _kozpont = new Kozpont();
        }
            
        #endregion

        #region Private Events
        #endregion

        #region Public game methods

        public void NewSimulation() //majd parameterkent .txt file vagy idk
        {

        }
        #endregion

        #region Private game methods
        #endregion
    }
}
