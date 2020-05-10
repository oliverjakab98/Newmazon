using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newmazon.ViewModel
{
    public class NewmazonField : ViewModelBase
    {
        private char _identity;
        private string _content;

        /// <summary>
        /// Label tartalma lekérdezése vagy beállítása.
        /// </summary>
        public string Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// Label háttere lekérdezése vagy beállítása
        /// </summary>
        public char Identity
        {
            get { return _identity; }
            set
            {
                if (_identity != value)
                {
                    _identity = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vízszintes koordináta lekérdezése, vagy beállítása.
        /// </summary>
        public Int32 X { get; set; }

        /// <summary>
        /// Függőleges koordináta lekérdezése, vagy beállítása.
        /// </summary>
        public Int32 Y { get; set; }

        /// <summary>
        /// Sorszám lekérdezése.
        /// </summary>
        public Int32 Number { get; set; }

    }
}
