using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newmazon.Persistence;
using System.Diagnostics;

namespace Newmazon.Model
{
    public class Kozpont
    {
        public int tableSize;
        //public List<List<NewmazonClasses>> table;
        public NewmazonClasses[,] table;
        public List<Robot> robots;
        private List<Stack<Step>> paths;
        private int startingEnergy;
        private List<Goods> goods;

        public event EventHandler<NewmazonEventArgs> SimOver;

        #region Constructors

        public Kozpont()
        {

        }

        #endregion

        public void NewSimulation(AllData data)
        {
            int mID = 1, cID = 10001, pID = 20001, tID = 30001, rID = 40001;
            //mezo: 1-10000, cel: 10001-20000, polc: 20001-30000, tolto: 30001-40000, robot: 40001-50000
            tableSize = data.tableSize;
            //table = new List<List<NewmazonClasses>>(tableSize);
            table = new NewmazonClasses[tableSize, tableSize];
            startingEnergy = data.robotEnergy;
            robots = new List<Robot>();
            for (int i = 0; i < tableSize; ++i)
            {
                for (int j = 0; j < tableSize; ++j)
                {
                    switch (data.dataTable[i, j])
                    {
                        case 'R':
                            table[i,j] = new Mezo(mID, i, j);
                            mID++;
                            robots.Add(new Robot(rID, i, j, startingEnergy, 0, null));
                            rID++;
                            break;
                        case 'M':
                            table[i,j] = new Mezo(mID, i, j);
                            mID++;
                            break;
                        case 'P':
                            table[i,j] = new Polc(pID, i, j);
                            pID++;
                            break;
                        case 'T':
                            table[i,j] = new Toltoallmoas(tID, i, j);
                            tID++;
                            break;
                        case 'F':
                            table[i,j] = new Fal(0, i, j);
                            break;
                        case 'C':
                            table[i,j] = new Celallomas(cID, i, j);
                            cID++;
                            break;

                    }
                }
            }

            paths = new List<Stack<Step>>(robots.Count);
            for (int i=0;i<robots.Count;++i)
            {
                paths.Add(new Stack<Step>());
            }

            foreach (Goods good in data.goods)
            {
                foreach (int i in good.destinations)
                {
                    table[good.x,good.y].goods.Add(i);
                }
            }
            goods = data.goods;

        }

        public void StepSimulation()
        {
            Debug.WriteLine("tick");
            for (int i=0;i<robots.Count;++i)
            {
                if (robots[i].stop > 0)
                    robots[i].stop--;
                else
                {
                    if (paths[i].Count > 0)
                    {
                        if (robots[i].x == paths[i].First().x &&
                            robots[i].y == paths[i].First().y &&
                            robots[i].dir == paths[i].First().dir)
                        {

                        }
                        else if (robots[i].dir != paths[i].First().dir)
                        { 
                            robots[i].dir = paths[i].First().dir;
                            robots[i].energy--;
                        }
                        else
                        {
                            robots[i].x = paths[i].First().x;
                            robots[i].y = paths[i].First().y;
                            robots[i].energy--;
                        }
                        paths[i].Pop();

                    }
                    else
                    {
                        DoStationaryThings(robots[i]);
                        NewmazonClasses target = CalculateNextJob(robots[i]);
                        CalculateRoute(robots[i], target);
                    }
                }
            }
        }

        public void DoStationaryThings(Robot robot)   //pl felemeli a polcot, lerakja a polcot, stb...
        {
            if (table[robot.x,robot.y].ID > 10000 && table[robot.x,robot.y].ID < 20001)   //célállomáson lerakja a megfelelő árukat
            {
                List<int> toBeRemoved = new List<int>();
                foreach(int good in robot.polc.goods)
                {
                    if (good == table[robot.x,robot.y].ID - 10000)
                    {
                        toBeRemoved.Add(good);
                    }
                }
                foreach(int goodToBeRemoved in toBeRemoved)
                {
                    robot.polc.goods.Remove(goodToBeRemoved);
                }
            }

            if (table[robot.x,robot.y].ID > 20000 && table[robot.x,robot.y].ID < 30001)   // ha polc helyen van nincs rajta polc, vegye fel a polcot, ha pedig van rajta polc, akkor rakja le
            { 
                if (robot.polc == null)
                {
                    robot.polc = (Polc)table[robot.x,robot.y];
                }
                else
                {
                    robot.polc = null;
                }
            }

        }

        public NewmazonClasses CalculateNextJob(Robot robot)   //pl helyére viszi a polcot, elmegy tölteni, stb...
        {
            if (table[robot.x,robot.y].ID > 10000 && table[robot.x,robot.y].ID < 20001 && robot.polc.goods.Count == 0)   // vigye helyére a polcot ha célállomáson van és már nincs áru
            {
                return table[robot.polc.x,robot.polc.y];
            }

            if (table[robot.x,robot.y].ID > 10000 && table[robot.x,robot.y].ID < 20001 && robot.polc.goods.Count != 0)   // ha célállomáson van és van még áru a polcon, menjen másik célállomásra
            {
                for (int i = 0; i < tableSize; ++i)
                {
                    for (int j = 0; j < tableSize; ++j)
                    {
                        if (table[i,j].ID == robot.polc.goods[0] + 10000)
                        {
                            return table[i,j];
                        }
                    }
                }
            }

            if (robot.energy<=startingEnergy*0.2)   // 20% alatt menjen tölteni
            {
                for (int i=0;i<tableSize;++i)
                {
                    for (int j=0;j<tableSize;++j)
                    {
                        if (table[i,j].ID == robot.ID-10000)
                        {
                            return table[i,j];
                        }
                    }
                }
            }

            if (table[robot.x,robot.y].ID > 30000 && table[robot.x,robot.y].ID < 40001)   // ha töltőállomáson van, menjen a következő árut tartalmazó polc alá
            {
                return table[goods[0].x,goods[0].y];
            }

            if (table[robot.x, robot.y].ID > 0 && table[robot.x, robot.y].ID < 10001)   // ha mezőn van, menjen a következő árut tartalmazó polc alá
            {
                return table[goods[0].x, goods[0].y];
            }


            if (table[robot.x,robot.y].ID > 20000 && table[robot.x,robot.y].ID < 30001 && robot.polc == null)   // ha polc helyen van nincs rajta polc, menjen a következő árut tartalmazó polc alá 
            {
                return table[goods[0].x,goods[0].y];
            }

            if (table[robot.x,robot.y].ID > 20000 && table[robot.x,robot.y].ID < 30001 && robot.polc != null)   // ha polc helyen van és van rajta polc, menjen a megfelelő célállomáshoz
            {
                for (int i = 0; i < tableSize; ++i)
                {
                    for (int j = 0; j < tableSize; ++j)
                    {
                        if (table[i,j].ID == robot.polc.goods[0] + 10000)
                        {
                            return table[i,j];
                        }
                    }
                }
            }

            return null;
        }

        class Astar
        {
            public int dir; // 0: jobbra, 1: fel, 2: balra, 3: le
            public NewmazonClasses tile;
            public int td;   //target distance
            public Astar pi;   //mindenki tudja, hogy mi az a pí
            public int sd;   //start distance
            public int fd;   //td+sd
            public Astar[] neighbours;

            public Astar(NewmazonClasses field, NewmazonClasses target)
            {
                neighbours = new Astar[4];
                tile = field;
                td = (int)Math.Sqrt((target.x - field.x) * (target.x - field.x) + (target.y - field.y) * (target.y - field.y));
                sd = 1000000;   //1millió == infinite ? Hmmm...
                pi = null;
                dir = -1;
            }
        }

        public void CalculateRoute(Robot robot, NewmazonClasses target)
        {
            List<Astar> prioQ = new List<Astar>();

            for (int i=0;i<tableSize;++i)
            {
                for (int j=0;j<tableSize;++j)
                {
                    int ID = table[i,j].ID;
                    if (((0 < ID && ID < 10001) || (20000 < ID && ID < 30001 && robot.polc == null)) && table[robot.x,robot.y].ID != ID)
                    {
                        Astar a = new Astar(table[i,j], target);
                        prioQ.Add(a);

                    }
                    else if (table[robot.x,robot.y].ID == ID)
                    {
                        Astar a = new Astar(table[i,j], target);
                        a.sd = 0;
                        a.dir = robot.dir;
                        prioQ.Add(a);
                    }
                }
            }

            foreach (Astar a in prioQ)
            {
                foreach (Astar b in prioQ)
                {
                    if (a.tile.x == b.tile.x && a.tile.y == b.tile.y-1)
                    {
                        a.neighbours[0] = b;
                    }
                    if (a.tile.x == b.tile.x && a.tile.y == b.tile.y+1)
                    {
                        a.neighbours[2] = b;
                    }
                    if (a.tile.x == b.tile.x+1 && a.tile.y == b.tile.y)
                    {
                        a.neighbours[1] = b;
                    }
                    if (a.tile.x == b.tile.x-1 && a.tile.y == b.tile.y)
                    {
                        a.neighbours[3] = b;
                    }
                }
            }

            prioQ = prioQ.OrderBy(o => o.sd+o.td).ToList();

            Astar u = prioQ[0];
            prioQ.RemoveAt(0);

            while (u.tile != target)
            {
                Astar v = u.neighbours[0];
                int dirC = 0;
                if (v != null)
                {
                    switch (u.dir)
                    {
                        case 0:
                            dirC = 0;
                            break;
                        case 1:
                            dirC = 1;
                            break;
                        case 2:
                            dirC = 2;
                            break;
                        case 3:
                            dirC = 1;
                            break;
                    }
                    if (u.sd + u.td + dirC < v.sd + v.td)
                    {
                        v.sd = u.sd + dirC;
                        v.pi = u;
                        v.dir = 0;
                    }
                }
                v = u.neighbours[1];
                if (v != null)
                {
                    switch (u.dir)
                    {
                        case 0:
                            dirC = 1;
                            break;
                        case 1:
                            dirC = 0;
                            break;
                        case 2:
                            dirC = 1;
                            break;
                        case 3:
                            dirC = 2;
                            break;
                    }
                    if (u.sd + u.td + dirC < v.sd + v.td)
                    {
                        v.sd = u.sd + dirC;
                        v.pi = u;
                        v.dir = 1;
                    }
                }
                v = u.neighbours[2];
                if (v != null)
                {
                    switch (u.dir)
                    {
                        case 0:
                            dirC = 2;
                            break;
                        case 1:
                            dirC = 1;
                            break;
                        case 2:
                            dirC = 0;
                            break;
                        case 3:
                            dirC = 1;
                            break;
                    }
                    if (u.sd + u.td + dirC < v.sd + v.td)
                    {
                        v.sd = u.sd + dirC;
                        v.pi = u;
                        v.dir = 2;
                    }
                }
                v = u.neighbours[3];
                if (v != null)
                {
                    switch (u.dir)
                    {
                        case 0:
                            dirC = 1;
                            break;
                        case 1:
                            dirC = 2;
                            break;
                        case 2:
                            dirC = 1;
                            break;
                        case 3:
                            dirC = 0;
                            break;
                    }
                    if (u.sd + u.td + dirC < v.sd + v.td)
                    {
                        v.sd = u.sd + dirC;
                        v.pi = u;
                        v.dir = 3;
                    }
                }
                prioQ = prioQ.OrderBy(o => o.sd + o.td).ToList();

                u = prioQ[0];
                prioQ.RemoveAt(0);
            }

            while (u.pi != null)
            {
                paths[robot.ID-40001].Push(new Step(u.tile.x, u.tile.y, u.dir));
                u = u.pi;
            }

        }

        private void OnGameOver()
        {
            if (SimOver != null)
                SimOver(this, new NewmazonEventArgs(false, 0));
        }
    }
}
