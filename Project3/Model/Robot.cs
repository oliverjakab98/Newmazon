using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newmazon.Model
{
    public class Robot : NewmazonClasses
    {
        public Int32 energy { get; set; }
        public Int32 dir { get; set; }
        public Polc polc { get; set; }
        public int stop { get; set; }

        public Robot(Int32 ID, Int32 x, Int32 y, Int32 energy, Int32 dir, Polc polc)
        {
            this.ID = ID;
            this.x = x;
            this.y = y;
            this.energy = energy;
            this.dir = dir;
            this.polc = polc;
            stop = 0;
        }

        public void PickUp(Polc polc, Int32 x, Int32 y)
        {
            //TODO
        }

        public void Deliver(Celallomas celallomas, Int32 x, Int32 y)
        {
            //TODO
        }

        public void Charge(Toltoallmoas toltoallmoas, Int32 x, Int32 y)
        {
            //TODO
        }
        public List<NewmazonClasses> PathSearch(NewmazonClasses destination)
        {
            //TODO
            return null;
        }



    }
}
