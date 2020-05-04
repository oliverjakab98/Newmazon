using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newmazon.Model
{
    public abstract class NewmazonClasses
    {
        public Int32 x { get; set; }
        public Int32 y { get; set; }

        public Int32 ID { get; set; }

        public List<int> goods = null;

        public bool otthon = true;

    }
}
