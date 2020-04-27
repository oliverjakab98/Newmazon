using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newmazon.Persistence;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace Newmazon.Model
{
    public class Kozpont
    {
        public int tableSize;
        //public List<List<NewmazonClasses>> table;
        public NewmazonClasses[,] table;
        public List<Robot> robots;
        private List<List<Step>> paths;
        private int startingEnergy;
        public List<Goods> goods;
        private int totalEnergyUsed;
        private int totalSteps;
        private int goodsDelivered;
        private List<int> robotEnergyUsed;

        public int TotalEnergyUsed { get { return totalEnergyUsed; } }
        public int TotalSteps { get { return totalSteps; } }
        public int GoodsDelivered { get { return goodsDelivered; } }
        public int TotalRobots { get { return robots.Count; } }

        public event EventHandler<NewmazonEventArgs> SimOver;

        #region Constructors

        public Kozpont()
        {

        }

        #endregion

        public void NewSimulation(AllData data)
        {
            int mID = 1, cID = 10001, pID = 20001, tID = 30001, rID = 40001;
            totalEnergyUsed = 0;
            totalSteps = 0;
            goodsDelivered = 0;
            //mezo: 1-10000, cel: 10001-20000, polc: 20001-30000, tolto: 30001-40000, robot: 40001-50000
            tableSize = data.tableSize;
            //table = new List<List<NewmazonClasses>>(tableSize);
            table = new NewmazonClasses[tableSize, tableSize];
            startingEnergy = data.robotEnergy;
            robots = new List<Robot>();
            robotEnergyUsed = new List<int>();

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
                            robotEnergyUsed.Add(0);
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

            goods = data.goods;

            paths = new List<List<Step>>(robots.Count);
            for (int i=0;i<robots.Count;++i)
            {
                paths.Add(new List<Step>());
            }

            foreach (Goods good in data.goods)
            {
                foreach (int i in good.destinations)
                {
                    table[good.x,good.y].goods.Add(i);
                }
            }
        }

        public void StepSimulation()
        {
            totalSteps++;
            for (int i=0;i<robots.Count;++i)
            {
                if (robots[i].stop > 0)
                    robots[i].stop--;
                else
                {
                    if (paths[i].Count > 0)
                    {
                        if (robots[i].x == paths[i][0].x &&
                            robots[i].y == paths[i][0].y &&
                            robots[i].dir == paths[i][0].dir)
                        {
                            paths[i].RemoveAt(0);
                        }
                        else if (robots[i].dir != paths[i][0].dir)
                        {
                            robots[i].dir = paths[i][0].dir;
                            robots[i].energy--;
                            robotEnergyUsed[i]++;
                            totalEnergyUsed++;
                        }
                        else
                        {
                            robots[i].x = paths[i][0].x;
                            robots[i].y = paths[i][0].y;
                            robots[i].energy--;
                            robotEnergyUsed[i]++;
                            totalEnergyUsed++;
                            paths[i].RemoveAt(0);
                        }

                    }
                    else
                    {
                        DoStationaryThings(robots[i]);
                        NewmazonClasses target = CalculateNextJob(robots[i]);
                        if (EverythingIsHome() && EverythingDelivered())
                        {
                            OnSimOver();
                            return;
                        }
                        CalculateRoute(robots[i], target);
                    }
                }
            }
        }

        public bool EverythingIsHome()
        {
            bool mindenOtthon = true;
            foreach (Robot robot in robots)
            {
                if (robot.polc != null)
                {
                    mindenOtthon = false;
                }
            }
            return mindenOtthon;
        }

        public bool EverythingDelivered()
        {
            bool mindenKezbesitve = true;
            for (int i=0;i<tableSize;++i)
            {
                for (int j=0;j<tableSize;++j)
                {
                    if (table[i,j].ID > 20000 && table[i, j].ID < 30001)
                    {
                        if (table[i,j].goods.Count != 0) mindenKezbesitve = false;
                    }
                }
            }
            return mindenKezbesitve;
        }

        public void DoStationaryThings(Robot robot)   //pl felemeli a polcot, lerakja a polcot, stb...
        {
            if (table[robot.x,robot.y].ID > 10000 && table[robot.x,robot.y].ID < 20001)   //célállomáson lerakja a megfelelő árukat 2 tick alatt
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
                    for (int i=0;i<robot.polc.goods.Count;++i)
                    {
                        if (robot.polc.goods[i] == goodToBeRemoved)
                        {
                            goodsDelivered++;
                            robot.polc.goods.RemoveAt(i);
                            i--;
                        }
                    }
                }
                //robot.stop+=2;
            }

            if (table[robot.x, robot.y].ID > 30000 && table[robot.x, robot.y].ID < 40001)   // ha töltőállomáson van, várjon 5 ticket hogy feltöltsön
            {
                robot.energy = startingEnergy;
            }

            if (table[robot.x,robot.y].ID > 20000 && table[robot.x,robot.y].ID < 30001)   // ha polc helyen van nincs rajta polc, vegye fel a polcot, ha pedig van üres rajta polc, akkor rakja le
            {
                if (robot.polc == null)
                {
                    if (table[robot.x, robot.y].goods.Count != 0)
                    {
                        robot.polc = (Polc)table[robot.x, robot.y];
                        robot.polc.otthon = false;
                    }
                }
                else if (robot.polc.goods.Count == 0)
                {
                    robot.polc.otthon = true;
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

            if (robot.polc == null && robot.energy<=6*tableSize)   // 20% alatt menjen tölteni
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
                if (goods.Count > 0)
                {
                    Goods temp = goods[0];
                    goods.RemoveAt(0);
                    return table[temp.x, temp.y];
                }
                else
                {
                    return table[robot.x, robot.y];
                }
            }

            if (table[robot.x, robot.y].ID > 0 && table[robot.x, robot.y].ID < 10001)   // ha mezőn van, menjen a következő árut tartalmazó polc alá
            {
                if (goods.Count > 0)
                {
                    Goods temp = goods[0];
                    goods.RemoveAt(0);
                    return table[temp.x, temp.y];
                }
                else
                {
                    return table[robot.x, robot.y];
                }
            }

            if (table[robot.x,robot.y].ID > 20000 && table[robot.x,robot.y].ID < 30001 && robot.polc == null)   // ha polc helyen van nincs rajta polc, menjen a következő árut tartalmazó polc alá 
            {
                if (goods.Count > 0)
                {
                    Goods temp = goods[0];
                    goods.RemoveAt(0);
                    return table[temp.x, temp.y];
                }
                else
                {
                    return table[robot.x, robot.y];
                }
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

        public void AddStop(Robot robot,int amount)
        {
            for (int i=0;i<amount;++i)
            {
                paths[robot.ID - 40001].Add(new Step(robot.x, robot.y, robot.dir));
            }
        }

        public int getRobotEnergy(int i)
        {
            return robotEnergyUsed[i];
        }

        class Astar
        {
            public int dir; // 0: jobbra, 1: fel, 2: balra, 3: le
            public NewmazonClasses tile;
            public int td;   //target distance
            public Astar pi;   //mindenki tudja, hogy mi az a pí
            public int sd;   //start distance
            public Astar[] neighbours;
            public List<int> blocked;
            public int steps;

            public Astar(NewmazonClasses field, NewmazonClasses target)
            {
                neighbours = new Astar[5];
                blocked = new List<int>();
                steps = 1000000;
                tile = field;

                td = Math.Abs(target.x - field.x) + Math.Abs(target.y - field.y);
                //td = (int)Math.Sqrt((target.x - field.x) * (target.x - field.x) + (target.y - field.y) * (target.y - field.y));
                sd = 1000000; 

                pi = null;
                dir = -1;
            }
        }

        public void CalculateRoute(Robot robot, NewmazonClasses target)
        {
            List<Astar> prioQ = new List<Astar>();

            List<int>[,] blocks = new List<int>[tableSize, tableSize];

            for (int i = 0; i < tableSize; ++i)
            {
                for (int j = 0; j < tableSize; ++j)
                {
                    blocks[i, j] = new List<int>();
                }
            }

            for (int i = 0; i < robots.Count; ++i)
            {
                if (paths[i].Count > 0)
                {
                    int steps = 0;
                    blocks[paths[i][0].x, paths[i][0].y].Add(steps);
                    blocks[paths[i][0].x, paths[i][0].y].Add(steps + 1);
                    steps++;
                    for (int j = 1; j < paths[i].Count; ++j)
                    {
                        if (paths[i][j].dir != paths[i][j - 1].dir)
                        {
                            blocks[paths[i][j - 1].x, paths[i][j - 1].y].Add(steps);
                            blocks[paths[i][j - 1].x, paths[i][j - 1].y].Add(steps + 1);
                            steps++;
                        }
                        blocks[paths[i][j].x, paths[i][j].y].Add(steps);
                        blocks[paths[i][j].x, paths[i][j].y].Add(steps+1);
                        steps++;
                    }
                }
            }


            for (int i=0;i<tableSize;++i)
            {
                for (int j=0;j<tableSize;++j)
                {
                    int ID = table[i,j].ID;
                    if (((0 < ID && ID < 10001) || (20000 < ID && ID < 30001 && robot.polc == null)) && table[robot.x,robot.y].ID != ID)
                    {
                        Astar a = new Astar(table[i,j], target);
                        a.blocked = blocks[i, j];
                        prioQ.Add(a);

                    }
                    else if (table[robot.x,robot.y].ID == ID)
                    {
                        Astar a = new Astar(table[i,j], target);
                        a.blocked = blocks[i, j];
                        a.sd = 0;
                        a.dir = robot.dir;
                        a.steps = 0;
                        prioQ.Add(a);
                    }
                }
            }
            Astar fin = new Astar(target, target);
            fin.blocked = blocks[target.x, target.y];
            prioQ.Add(fin);

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
                a.neighbours[4] = a;
            }

            prioQ = prioQ.OrderBy(o => o.sd+o.td).ToList();

            Astar u = prioQ[0];
            prioQ.RemoveAt(0);

            while (u.tile != target && prioQ.Count>0)
            {
                Astar v = u.neighbours[0];
                int dirC = 0;
                int stepC = 0;
                if (v != null)
                {
                    switch (u.dir)
                    {
                        case 0:
                            dirC = 0;
                            stepC = 0;
                            break;
                        case 1:
                            dirC = 1;
                            stepC = 1;
                            break;
                        case 2:
                            dirC = 2;
                            stepC = 1;
                            break;
                        case 3:
                            dirC = 1;
                            stepC = 1;
                            break;
                    }
                    if (u.sd + u.td + dirC + 1 < v.sd + v.td && !v.blocked.Contains(u.steps + stepC - 1) && !v.blocked.Contains(u.steps + stepC) && !v.blocked.Contains(u.steps + stepC + 1) && !v.blocked.Contains(u.steps + stepC + 2) && !v.blocked.Contains(u.steps + stepC + 3))
                    {
                        v.sd = u.sd + dirC + 1;
                        v.pi = u;
                        v.dir = 0;
                        v.steps = u.steps + stepC + 1;
                    }
                }
                v = u.neighbours[1];
                if (v != null)
                {
                    switch (u.dir)
                    {
                        case 0:
                            dirC = 1;
                            stepC = 1;
                            break;
                        case 1:
                            dirC = 0;
                            stepC = 0;
                            break;
                        case 2:
                            dirC = 1;
                            stepC = 1;
                            break;
                        case 3:
                            dirC = 2;
                            stepC = 1;
                            break;
                    }
                    if (u.sd + u.td + dirC + 1 < v.sd + v.td && !v.blocked.Contains(u.steps + stepC - 1) && !v.blocked.Contains(u.steps + stepC) && !v.blocked.Contains(u.steps + stepC + 1) && !v.blocked.Contains(u.steps + stepC + 2) && !v.blocked.Contains(u.steps + stepC + 3))
                    {
                        v.sd = u.sd + dirC + 1;
                        v.pi = u;
                        v.dir = 1;
                        v.steps = u.steps + stepC + 1;
                    }
                }
                v = u.neighbours[2];
                if (v != null)
                {
                    switch (u.dir)
                    {
                        case 0:
                            dirC = 2;
                            stepC = 1;
                            break;
                        case 1:
                            dirC = 1;
                            stepC = 1;
                            break;
                        case 2:
                            dirC = 0;
                            stepC = 0;
                            break;
                        case 3:
                            dirC = 1;
                            stepC = 1;
                            break;
                    }
                    if (u.sd + u.td + dirC + 1 < v.sd + v.td && !v.blocked.Contains(u.steps + stepC - 1) && !v.blocked.Contains(u.steps + stepC) && !v.blocked.Contains(u.steps + stepC + 1) && !v.blocked.Contains(u.steps + stepC + 2) && !v.blocked.Contains(u.steps + stepC + 3))
                    {
                        v.sd = u.sd + dirC + 1;
                        v.pi = u;
                        v.dir = 2;
                        v.steps = u.steps + stepC + 1;
                    }
                }
                v = u.neighbours[3];
                if (v != null)
                {
                    switch (u.dir)
                    {
                        case 0:
                            dirC = 1;
                            stepC = 1;
                            break;
                        case 1:
                            dirC = 2;
                            stepC = 1;
                            break;
                        case 2:
                            dirC = 1;
                            stepC = 1;
                            break;
                        case 3:
                            dirC = 0;
                            stepC = 0;
                            break;
                    }
                    if (u.sd + u.td + dirC + 1 < v.sd + v.td && !v.blocked.Contains(u.steps + stepC - 1) && !v.blocked.Contains(u.steps + stepC) && !v.blocked.Contains(u.steps + stepC + 1) && !v.blocked.Contains(u.steps + stepC + 2) && !v.blocked.Contains(u.steps + stepC + 3))
                    {
                        v.sd = u.sd + dirC + 1;
                        v.pi = u;
                        v.dir = 3;
                        v.steps = u.steps + stepC + 1;
                    }
                }

                prioQ = prioQ.OrderBy(o => o.sd + o.td).ToList();
                u = prioQ[0];
                if (u.sd > 10000)
                {
                    AddStop(robot, 1);
                    return;
                }
                prioQ.RemoveAt(0);

                /*Astar newA = new Astar(u.tile, target);

                newA.blocked = u.blocked;
                newA.steps = u.steps + 1;
                newA.dir = u.dir;
                newA.neighbours = u.neighbours;
                if (u.neighbours[0] != null) newA.neighbours[0].neighbours[2] = newA;
                if (u.neighbours[1] != null) newA.neighbours[1].neighbours[3] = newA;
                if (u.neighbours[2] != null) newA.neighbours[2].neighbours[0] = newA;
                if (u.neighbours[3] != null) newA.neighbours[3].neighbours[1] = newA;
                newA.neighbours[4] = newA;

                newA.sd = u.sd + 1;

                prioQ.Add(newA);*/
            }

            int endDir = u.dir;

            if (table[target.x, target.y].ID > 10000 && table[target.x, target.y].ID < 20001)
            {
                Astar w1 = new Astar(u.tile, target);
                w1.blocked = u.blocked;
                w1.steps = u.steps + 1;

                Astar w2 = new Astar(u.tile, target);
                w2.blocked = w1.blocked;
                w2.steps = w1.steps + 1;

                if (!w1.blocked.Contains(w1.steps - 1) && !w1.blocked.Contains(w1.steps) && !w1.blocked.Contains(w1.steps + 1) && !w1.blocked.Contains(w1.steps + 2) && !w1.blocked.Contains(w1.steps + 3) &&
                    !w2.blocked.Contains(w2.steps - 1) && !w2.blocked.Contains(w2.steps) && !w2.blocked.Contains(w2.steps + 1) && !w2.blocked.Contains(w2.steps + 2) && !w2.blocked.Contains(w2.steps + 3))
                {
                    while (u.pi != null)
                    {
                        paths[robot.ID - 40001].Add(new Step(u.tile.x, u.tile.y, u.dir));
                        u = u.pi;
                    }
                    paths[robot.ID - 40001].Reverse();
                }
                else
                {
                    AddStop(robot, 1);
                    return;
                }
            }
            else
            {
                while (u.pi != null)
                {
                    paths[robot.ID - 40001].Add(new Step(u.tile.x, u.tile.y, u.dir));
                    u = u.pi;
                }
                paths[robot.ID - 40001].Reverse();
                if (table[target.x, target.y].ID > 30000 && table[target.x, target.y].ID < 40001)
                {
                    paths[robot.ID - 40001].Add(new Step(target.x, target.y, endDir));
                    paths[robot.ID - 40001].Add(new Step(target.x, target.y, endDir));
                    paths[robot.ID - 40001].Add(new Step(target.x, target.y, endDir));
                    paths[robot.ID - 40001].Add(new Step(target.x, target.y, endDir));
                }
            }

        }
        private void OnSimOver()
        {
            if (SimOver != null)
                SimOver(this, new NewmazonEventArgs(false, 0));
        }
    }
}
