using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newmazon.Persistence;

namespace Newmazon.Model
{
    public class Kozpont
    {
        private int tableSize;
        private List<List<NewmazonClasses>> table;
        private List<Robot> robots;
        private List<List<Step>> paths;
        private int startingEnergy;
        private List<Goods> goods;

        #region Constructors

        public Kozpont()
        {
            
        }

        public void NewSimulation(AllData data)
        {
            int mID = 1, cID = 10001, pID = 20001, tID = 30001, rID = 40001; //mezo: 1-10000, cel: 10001-20000, polc: 20001-30000, tolto: 30001-40000, robot: 40001-50000
            table = new List<List<NewmazonClasses>>();
            tableSize = data.tableSize;
            startingEnergy = data.robotEnergy;
            for (int i = 0; i < tableSize; ++i)
            {
                for (int j = 0; j < tableSize; ++j)
                {
                    switch (data.dataTable[i, j])
                    {
                        case 'R':
                            table[i][j] = new Mezo(mID, i, j);
                            mID++;
                            robots.Add(new Robot(rID, i, j, startingEnergy, 0, null));
                            rID++;
                            break;
                        case 'M':
                            table[i][j] = new Mezo(mID, i, j);
                            mID++;
                            break;
                        case 'P':
                            table[i][j] = new Polc(pID, i, j);
                            pID++;
                            break;
                        case 'T':
                            table[i][j] = new Toltoallmoas(tID, i, j);
                            tID++;
                            break;
                        case 'F':
                            table[i][j] = new Fal(0, i, j);
                            break;
                        case 'C':
                            table[i][j] = new Celallomas(cID, i, j);
                            cID++;
                            break;

                    }
                }
            }
            foreach (Goods good in data.goods)
            {
                foreach (int i in good.destinations)
                {
                    table[good.x][good.y].goods.Add(i);
                }
            }
            goods = data.goods;
        }

        public void StepSimulation()
        {
            for (int i=0;i<robots.Count;++i)
            {
                if (paths[i].Count > 0)
                {
                    if (robots[i].x != paths[i][0].x ||
                        robots[i].y != paths[i][0].y ||
                        robots[i].dir != paths[i][0].dir)
                    {
                        robots[i].energy--;
                        robots[i].x = paths[i][0].x;
                        robots[i].y = paths[i][0].y;
                        robots[i].dir = paths[i][0].dir;
                    }
                    paths[i].RemoveAt(0);

                }
                else
                {
                    robots[i].x = paths[i][0].x;
                    robots[i].y = paths[i][0].y;
                    robots[i].dir = paths[i][0].dir;
                    DoStationaryThings(robots[i]);
                    CalculateNextJob(robots[i]);
                }
            }
        }

        public void DoStationaryThings(Robot robot)   //pl felemeli a polcot, lerakja a polcot, stb...
        {
            if (table[robot.x][robot.y].ID > 10000 && table[robot.x][robot.y].ID < 20001)   //célállomáson lerakja a megfelelő árukat
            {
                List<int> toBeRemoved = new List<int>();
                foreach(int good in robot.polc.goods)
                {
                    if (good == table[robot.x][robot.y].ID - 10000)
                    {
                        toBeRemoved.Add(good);
                    }
                }
                foreach(int goodToBeRemoved in toBeRemoved)
                {
                    robot.polc.goods.Remove(goodToBeRemoved);
                }
            }

            if (table[robot.x][robot.y].ID > 20000 && table[robot.x][robot.y].ID < 30001)   // ha polc helyen van nincs rajta polc, vegye fel a polcot, ha pedig van rajta polc, akkor rakja le
            { 
                if (robot.polc == null)
                {
                    robot.polc = (Polc)table[robot.x][robot.y];
                }
                else
                {
                    robot.polc = null;
                }
            }

        }

        public NewmazonClasses CalculateNextJob(Robot robot)   //pl helyére viszi a polcot, elmegy tölteni, stb...
        {
            if (table[robot.x][robot.y].ID > 10000 && table[robot.x][robot.y].ID < 20001 && robot.polc.goods.Count == 0)   // vigye helyére a polcot ha célállomáson van és már nincs áru
            {
                return table[robot.polc.x][robot.polc.y];
            }

            if (table[robot.x][robot.y].ID > 10000 && table[robot.x][robot.y].ID < 20001 && robot.polc.goods.Count != 0)   // ha célállomáson van és van még áru a polcon, menjen másik célállomásra
            {
                for (int i = 0; i < tableSize; ++i)
                {
                    for (int j = 0; j < tableSize; ++j)
                    {
                        if (table[i][j].ID == robot.polc.goods[0] + 10000)
                        {
                            return table[i][j];
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
                        if (table[i][j].ID == robot.ID-10000)
                        {
                            return table[i][j];
                        }
                    }
                }
            }

            if (table[robot.x][robot.y].ID > 30000 && table[robot.x][robot.y].ID < 40001)   // ha töltőállomáson van, menjen a következő árut tartalmazó polc alá
            {
                return table[goods[0].x][goods[0].y];
            }

            if (table[robot.x][robot.y].ID > 20000 && table[robot.x][robot.y].ID < 30001 && robot.polc == null)   // ha polc helyen van nincs rajta polc, menjen a következő árut tartalmazó polc alá 
            {
                return table[goods[0].x][goods[0].y];
            }

            if (table[robot.x][robot.y].ID > 20000 && table[robot.x][robot.y].ID < 30001 && robot.polc != null)   // ha polc helyen van és van rajta polc, menjen a megfelelő célállomáshoz
            {
                for (int i = 0; i < tableSize; ++i)
                {
                    for (int j = 0; j < tableSize; ++j)
                    {
                        if (table[i][j].ID == robot.polc.goods[0] + 10000)
                        {
                            return table[i][j];
                        }
                    }
                }
            }

            return null;
        }


        #endregion
    }
}
