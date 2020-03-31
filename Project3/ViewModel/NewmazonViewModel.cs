using Newmazon.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newmazon.ViewModel
{
    public class NewmazonViewModel : ViewModelBase
    {

        #region Fields
        #endregion

        #region Properties
        /// <summary>
        /// Játékmező gyűjtemény lekérdezése.
        /// </summary>
        public ObservableCollection<NewmazonField> Fields { get; set; }

        #endregion

        #region Events
        #endregion

        #region Constructors
        public NewmazonViewModel(NewmazonModel model)
        {
            Fields = new ObservableCollection<NewmazonField>();

            for (Int32 i = 0; i < 10; i++) // inicializáljuk a mezőket  
            {
                for (Int32 j = 0; j < 10; j++)
                {
                    Fields.Add(new NewmazonField
                    {
                        
                        X = i,
                        Y = j,
                        Number = i * 10 + j, // a gomb sorszáma, amelyet felhasználunk az azonosításhoz
                        //StepCommand = new DelegateCommand(param => StepGame(Convert.ToInt32(param)))

                        // ha egy mezőre léptek, akkor jelezzük a léptetést, változtatjuk a lépésszámot
                    });
                }
            }
        }
        #endregion
    }
}
