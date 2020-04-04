﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newmazon.Model
{
    public class Polc : NewmazonClasses
    {
        public Boolean otthon;
        public Polc(Int32 ID, Int32 x, Int32 y)
        {
            this.ID = ID;
            this.x = x;
            this.y = y;
            otthon = true;
            goods = new List<int>();

        }

        public Polc(Int32 ID, Int32 x, Int32 y, List<int> goods)
        {
            this.ID = ID;
            this.x = x;
            this.y = y;
            otthon = true;
            this.goods = goods;
        }

    }
}
