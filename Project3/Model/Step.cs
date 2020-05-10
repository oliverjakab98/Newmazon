using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newmazon.Model
{
    public class Step
    {
        public int x;
        public int y;
        public int dir; //0 jobb, 1 fel, 2 bal, 3 le
        public Step(int _x,int _y,int _dir)
        {
            x = _x;
            y = _y;
            dir = _dir;
        }
    }
}
