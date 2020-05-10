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
        private List<List<Step>> paths;
        private int startingEnergy;
        public List<Goods> goods;
        private int totalEnergyUsed;
        private int totalSteps;
        private int goodsDelivered;
        private List<int> robotEnergyUsed;
        public AllData savedData;
        public int celallomasCount;

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
            savedData = new AllData();
            savedData = data;  // adatok betöltése
            int mID = 1, cID = 10001, pID = 20001, tID = 30001, rID = 40001; //mezo: 1-10000, cel: 10001-20000, polc: 20001-30000, tolto: 30001-40000, robot: 40001-50000
            totalEnergyUsed = 0;
            totalSteps = 0;
            goodsDelivered = 0;
            celallomasCount = 0;
            tableSize = data.tableSize;
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
                        case 'R':                               //Robot
                            table[i,j] = new Mezo(mID, i, j);
                            mID++;
                            robots.Add(new Robot(rID, i, j, startingEnergy, 0, null));
                            robotEnergyUsed.Add(0);
                            rID++;
                            break;
                        case 'M':                               //Mező
                            table[i,j] = new Mezo(mID, i, j);
                            mID++;
                            break;
                        case 'P':                               //Polc
                            table[i,j] = new Polc(pID, i, j);
                            pID++;
                            break;
                        case 'T':                               //Töltőállomás
                            table[i,j] = new Toltoallmoas(tID, i, j);
                            tID++;
                            break;
                        case 'F':                               //Fal
                            table[i,j] = new Fal(0, i, j);
                            break;
                        case 'C':                               //Célállomás
                            table[i,j] = new Celallomas(cID, i, j);
                            celallomasCount++;
                            cID++;
                            break;
                    }
                }
            }

            goods = new List<Goods>();                          // áruk listájának betöltése a datából
            foreach (Goods good in data.goods)
            {
                goods.Add(good);
            }

            paths = new List<List<Step>>(robots.Count);         // minden robotnak saját útvonal létrehozása
            for (int i=0;i<robots.Count;++i)
            {
                paths.Add(new List<Step>());
            }

            foreach (Goods good in data.goods)                  // polcokon lévő áruk beöltése
            {
                foreach (int i in good.destinations)
                {
                    table[good.x,good.y].goods.Add(i);
                }
            }
        }

        public void StepSimulation()                           // Szimuláció léptetése
        {
            totalSteps++;
            for (int i=0;i<robots.Count;++i)                   // Minden robotra megnézni, hogyha üres az útja, akkor új út megtervezése
            {
                if (paths[i].Count > 0)
                {
                    bool foglalt = false;
                    for (int j=0;j<robots.Count;++j)           // Megnézi, hogy a következő mezőn van-e robot,
                    {
                        if (paths[i][0].x == robots[j].x && paths[i][0].y == robots[j].y && i != j)
                        {
                            foglalt = true;
                        }    
                    }
                    if (!foglalt)                              // Ha nincs, akkor lép a robot
                    {
                        if (robots[i].x == paths[i][0].x &&    // Nem mozog a robot
                            robots[i].y == paths[i][0].y &&
                            robots[i].dir == paths[i][0].dir)
                        {
                            paths[i].RemoveAt(0);
                        }
                        else if (robots[i].dir != paths[i][0].dir)  // Ha nem jó a robot iránya, akkor fordul
                        {
                            robots[i].dir = paths[i][0].dir;
                            robots[i].energy--;
                            robotEnergyUsed[i]++;
                            totalEnergyUsed++;
                        }
                        else
                        {                                     // egyébként lép a következő mezőre
                            robots[i].x = paths[i][0].x;
                            robots[i].y = paths[i][0].y;
                            robots[i].energy--;
                            robotEnergyUsed[i]++;
                            totalEnergyUsed++;
                            paths[i].RemoveAt(0);
                        }
                    }
                    else                                     // Ha következő mező foglalt, akkor kiüríti az útvonalát, és ügyel, 
                                                             // hogyha kapott utasítást hogy vegyen fel egy polcot, akkor azon a polcon lévő termékeket visszarakja a goods listába
                    {
                        if(table[paths[i].Last().x, paths[i].Last().y].goods != null && table[paths[i].Last().x,paths[i].Last().y].goods.Count > 0)
                        {
                            int[] g1 = new int[table[paths[i].Last().x, paths[i].Last().y].goods.Count];
                            for (int j=0;j<table[paths[i].Last().x, paths[i].Last().y].goods.Count;++j)
                            {
                                g1[j] = table[paths[i].Last().x, paths[i].Last().y].goods[j];
                            }
                            goods.Add(new Goods(paths[i].Last().x, paths[i].Last().y, g1));
                        }
                        paths[i].Clear();
                    }
                }
                else
                {
                    DoStationaryThings(robots[i]);                                  // Minden egy helyben megcsinálható dolgot itt végez el (Pl robot feltöltése, vagy polc lerakása, stb...)
                    NewmazonClasses target = CalculateNextJob(robots[i]);           // A következő célpont kiszámítása
                    if (EverythingIsHome() && EverythingDelivered())                // Ha minden polc a helyén van, és nincs semmi már a polcokat, akkor vége a szimulációnak.
                    {
                        OnSimOver();
                        return;
                    }
                    CalculateRoute(robots[i], target);                              // Útvonal megtervezése (A* algoritmussal)
                }
            }
        }

        public bool EverythingIsHome()                                              // Minden polc otthon van-e?
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

        public bool EverythingDelivered()                                           // Minden polc üres-e?
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

        public void DoStationaryThings(Robot robot)                                 //pl felemeli a polcot, lerakja a polcot, stb...
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
            }

            if (table[robot.x, robot.y].ID > 30000 && table[robot.x, robot.y].ID < 40001)   // ha töltőállomáson feltölt
            {
                robot.energy = startingEnergy;
            }

            if (table[robot.x,robot.y].ID > 20000 && table[robot.x,robot.y].ID < 30001)   // ha polc helyen van nincs rajta polc, vegye fel a polcot, ha pedig van üres rajta polc, akkor rakja le
            {
                if (robot.polc == null)
                {
                    if (table[robot.x, robot.y].goods.Count != 0 && table[robot.x, robot.y].otthon)
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

        public NewmazonClasses CalculateNextJob(Robot robot)                                                             //pl helyére viszi a polcot, elmegy tölteni, stb...
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

            if (table[robot.x, robot.y].ID > 0 && table[robot.x, robot.y].ID < 10001 && robot.polc != null && robot.polc.goods.Count > 0)   // ha mezőn van és van rajta nemüres polc, menjen a megfelelő célállomáshoz
            {
                for (int i = 0; i < tableSize; ++i)
                {
                    for (int j = 0; j < tableSize; ++j)
                    {
                        if (table[i, j].ID == robot.polc.goods[0] + 10000)
                        {
                            return table[i, j];
                        }
                    }
                }
            }

            if (table[robot.x, robot.y].ID > 0 && table[robot.x, robot.y].ID < 10001 && robot.polc != null && robot.polc.goods.Count == 0)   // ha mezőn van és van rajta üres polc, vigye vissza a polcot
            {
                return table[robot.polc.x, robot.polc.y];
            }


            if (table[robot.x, robot.y].ID > 20000 && table[robot.x, robot.y].ID < 30001 && robot.polc != null)   // ha polc helyen van és van rajta polc, menjen a megfelelő célállomáshoz
            {
                for (int i = 0; i < tableSize; ++i)
                {
                    for (int j = 0; j < tableSize; ++j)
                    {
                        if (table[i, j].ID == robot.polc.goods[0] + 10000)
                        {
                            return table[i, j];
                        }
                    }
                }
            }

            if (robot.polc == null && robot.energy<=(4 + celallomasCount) * tableSize)   // (4 + celallomasCount) * tableSize alatt menjen tölteni
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
                else         // ha nincs több áru, álljon egyhelyben
                {
                    return table[robot.x, robot.y];
                }
            }

            if (table[robot.x, robot.y].ID > 0 && table[robot.x, robot.y].ID < 10001 && robot.polc == null)   // ha mezőn van és nincs rajta polc, menjen a következő árut tartalmazó polc alá
            {
                if (goods.Count > 0)
                {
                    Goods temp = goods[0];
                    goods.RemoveAt(0);
                    return table[temp.x, temp.y];
                }
                else        // ha nincs több áru, menjen a saját töltőállomására
                {
                    for (int i = 0; i < tableSize; ++i)
                    {
                        for (int j = 0; j < tableSize; ++j)
                        {
                            if (table[i, j].ID == robot.ID - 10000)
                            {
                                return table[i, j];
                            }
                        }
                    }
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
                else        // ha nincs több áru, menjen a saját töltőállomására
                {
                    for (int i = 0; i < tableSize; ++i)
                    {
                        for (int j = 0; j < tableSize; ++j)
                        {
                            if (table[i, j].ID == robot.ID - 10000)
                            {
                                return table[i, j];
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void AddStop(Robot robot,int amount)      // egy helyben állás, "amount" ideig.
        {
            for (int i=0;i<amount;++i)
            {
                paths[robot.ID - 40001].Add(new Step(robot.x, robot.y, robot.dir));
            }
        }

        public int getRobotEnergy(int i)        // robot energiahasználat
        {
            return robotEnergyUsed[i];
        }

        class Astar                             // az A* algoritmus node-jai
        {
            public int dir;                     // 0: jobbra, 1: fel, 2: balra, 3: le
            public NewmazonClasses tile;        // a tábla melyik mezője?
            public int td;                      // cél távolsága
            public Astar pi;                    // a mögötte lévőre mutat
            public int sd;                      // start távolsága
            public Astar[] neighbours;          // mező szomszédjai
            public List<int> blocked;           // blokkolt időpontok -- ebben az időben nem mehet ide a robot, mert épp foglalt a mező
            public int steps;                   // hány lépésben jutna ide a robot

            public Astar(NewmazonClasses field, NewmazonClasses target)
            {
                neighbours = new Astar[5];
                blocked = new List<int>();
                steps = 1000000;
                tile = field;

                td = Math.Abs(target.x - field.x) + Math.Abs(target.y - field.y);
                sd = 1000000; 

                pi = null;
                dir = -1;
            }
        }

        public void CalculateRoute(Robot robot, NewmazonClasses target)      // útvonaltervező függvény
        {
            List<Astar> prioQ = new List<Astar>();

            List<int>[,] blocks = new List<int>[tableSize, tableSize];       // blokkolt időpontok mátrixa (tábla szerint)

            for (int i = 0; i < tableSize; ++i)
            {
                for (int j = 0; j < tableSize; ++j)
                {
                    blocks[i, j] = new List<int>();
                }
            }

            for (int i = 0; i < robots.Count; ++i)     // minden roboton végigfut, és berakja a blokkolt időpontok mátrixába a megfelelő mezőkre, hogy mikor vannak blokkolva
            {
                if (paths[i].Count > 0)
                {
                    int steps = 0;
                    blocks[paths[i][0].x, paths[i][0].y].Add(steps - 1);
                    blocks[paths[i][0].x, paths[i][0].y].Add(steps);
                    blocks[paths[i][0].x, paths[i][0].y].Add(steps + 1);
                    steps++;
                    for (int j = 1; j < paths[i].Count; ++j)
                    {
                        if (paths[i][j].dir != paths[i][j - 1].dir)
                        {
                            blocks[paths[i][j - 1].x, paths[i][j - 1].y].Add(steps - 1);
                            blocks[paths[i][j - 1].x, paths[i][j - 1].y].Add(steps);
                            blocks[paths[i][j - 1].x, paths[i][j - 1].y].Add(steps + 1);
                            steps++;
                        }
                        blocks[paths[i][j].x, paths[i][j].y].Add(steps - 1);
                        blocks[paths[i][j].x, paths[i][j].y].Add(steps);
                        blocks[paths[i][j].x, paths[i][j].y].Add(steps+1);
                        steps++;
                    }
                }
                if (robots[i] != robot)
                {
                    blocks[robots[i].x, robots[i].y].Add(0);
                    blocks[robots[i].x, robots[i].y].Add(1);
                }
            }


            for (int i=0;i<tableSize;++i)       // megnézi, hogy melyik mezőkhöz kell rendelni Node-ot (Pl a falhoz nem kell, mert oda nem léphet robot)
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
            Astar fin = new Astar(target, target);      // targethez külön hozzá kell rendelni egy Node-ot, mert pl az lehet töltőállomás, ami alapból nem kerül bele az algoritmusba
            fin.blocked = blocks[target.x, target.y];
            prioQ.Add(fin);

            foreach (Astar a in prioQ)          // szomszédok hozzárendelése egymáshoz
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

            prioQ = prioQ.OrderBy(o => (o.sd+o.td)).ToList();        // rendezés sd+td szerint

            Astar u = prioQ[0];
            prioQ.RemoveAt(0);               // mindig a legkisebb sd+td -vel rendelkező elem kivétele

            while (u.tile != target)         // amíg meg nem találjuk a célt, addig fusson
            {
                Astar v = u.neighbours[0];   // mindig csak a szomszédok között "fut él", ezért csak azokat kell megnézni, hogy az adott mezőből könnyebben oda tudunk-e jutni
                int dirC = 0;
                int stepC = 0;
                if (v != null)               // jobb szomszéd
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
                    if (u.sd + u.td + dirC + 1 < v.sd + v.td && !v.blocked.Contains(u.steps + stepC))  // ha gyorsabban idejutunk, és nicns blokkolva ebben az időben, akkor mostmár u-ból jöjjünk ide
                    {
                        v.sd = u.sd + dirC + 1;
                        v.pi = u;
                        v.dir = 0;
                        v.steps = u.steps + stepC + 1;
                    }
                }
                v = u.neighbours[1];
                if (v != null)               // felső szomszéd
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
                    if (u.sd + u.td + dirC + 1 < v.sd + v.td && !v.blocked.Contains(u.steps + stepC))  // ha gyorsabban idejutunk, és nicns blokkolva ebben az időben, akkor mostmár u-ból jöjjünk ide
                    {
                        v.sd = u.sd + dirC + 1;
                        v.pi = u;
                        v.dir = 1;
                        v.steps = u.steps + stepC + 1;
                    }
                }
                v = u.neighbours[2];
                if (v != null)               // bal szomszéd
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
                    if (u.sd + u.td + dirC + 1 < v.sd + v.td && !v.blocked.Contains(u.steps + stepC))  // ha gyorsabban idejutunk, és nicns blokkolva ebben az időben, akkor mostmár u-ból jöjjünk ide
                    {
                        v.sd = u.sd + dirC + 1;
                        v.pi = u;
                        v.dir = 2;
                        v.steps = u.steps + stepC + 1;
                    }
                }
                v = u.neighbours[3];
                if (v != null)               // alsó szomszéd
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
                    if (u.sd + u.td + dirC + 1 < v.sd + v.td && !v.blocked.Contains(u.steps + stepC))  // ha gyorsabban idejutunk, és nicns blokkolva ebben az időben, akkor mostmár u-ból jöjjünk ide
                    {
                        v.sd = u.sd + dirC + 1;
                        v.pi = u;
                        v.dir = 3;
                        v.steps = u.steps + stepC + 1;
                    }
                }

                prioQ = prioQ.OrderBy(o => o.sd + o.td).ToList();
                                                                // itt jön egy trükk: hogyha a blokkolt elemek miatt a PrioQ-ban a legkisebb prioritású elem még inicializálatlan,
                                                                // azaz a pi még null, akkor ne vegye ki azt a Node-ot, helyette rakjon bele a queue-ba egy másik Node-ot, aminek
                                                                // ugyanaz a koordinátája, mint az u-nak. 
                                                                // és ha éppen ez is blokkolt, akkor szimplán hívjon meg egy AddStop(robot, 1)-et, ami megállítja a robotot egy tickre,
                                                                // így majd a léptető függvény elrendezi a potenciális ütközést.
                if (prioQ[0].sd > 10000)                    
                {
                    Astar newA = new Astar(u.tile, target);

                    newA.blocked = u.blocked;
                    newA.steps = u.steps + 1;
                    newA.neighbours = u.neighbours;
                    if (u.neighbours[0] != null) u.neighbours[0].neighbours[2] = newA;
                    if (u.neighbours[1] != null) u.neighbours[1].neighbours[3] = newA;
                    if (u.neighbours[2] != null) u.neighbours[2].neighbours[0] = newA;
                    if (u.neighbours[3] != null) u.neighbours[3].neighbours[1] = newA;

                    if (!newA.blocked.Contains(newA.steps))
                    {
                        newA.sd = u.sd + 1;
                        newA.dir = u.dir;
                        newA.pi = u;
                        u = newA;
                    }
                    else
                    {
                        AddStop(robot, 1);
                        return;
                    }
                }
                        // egyébként vegye ki a legkisebb prioritású elemet, DE attól még ugyanúgy rakjon bele a prioQ-ba egy másik Node-ot,
                        // u koordinátájával.
                else
                {
                    Astar newA = new Astar(u.tile, target);

                    newA.blocked = u.blocked;
                    newA.steps = u.steps + 1;
                    newA.neighbours = u.neighbours;
                    if (u.neighbours[0] != null) u.neighbours[0].neighbours[2] = newA;
                    if (u.neighbours[1] != null) u.neighbours[1].neighbours[3] = newA;
                    if (u.neighbours[2] != null) u.neighbours[2].neighbours[0] = newA;
                    if (u.neighbours[3] != null) u.neighbours[3].neighbours[1] = newA;

                    if (!newA.blocked.Contains(newA.steps))
                    {
                        newA.sd = u.sd + 1;
                        newA.dir = u.dir;
                        newA.pi = u;
                    }
                    else
                    {
                        newA.sd = 1000000;
                        newA.dir = -1;
                        newA.pi = null;
                    }
                    prioQ.Add(newA);
                    u = prioQ[0];
                    prioQ.RemoveAt(0);
                }
            }

            int endDir = u.dir;
            
            while (u.pi != null)
            {
                paths[robot.ID - 40001].Add(new Step(u.tile.x, u.tile.y, u.dir));  // ha megtaláltuk a targetet, akkor visszafelé a pi segítségével állítsuk elő az útvonalat
                u = u.pi;
            }
            paths[robot.ID - 40001].Reverse();
            if (table[target.x, target.y].ID > 30000 && table[target.x, target.y].ID < 40001)  // ha a target egy töltőállomás, akkor még álljon 4et hogy meglegyen az 5
            {
                paths[robot.ID - 40001].Add(new Step(target.x, target.y, endDir));
                paths[robot.ID - 40001].Add(new Step(target.x, target.y, endDir));
                paths[robot.ID - 40001].Add(new Step(target.x, target.y, endDir));
                paths[robot.ID - 40001].Add(new Step(target.x, target.y, endDir));
            }
            if (table[target.x, target.y].ID > 10000 && table[target.x, target.y].ID < 20001)  // ha pedig célállomás, akkor álljon még egyet.
            {
                paths[robot.ID - 40001].Add(new Step(target.x, target.y, endDir));
            }

        }
        private void OnSimOver()
        {
            if (SimOver != null)
                SimOver(this, new NewmazonEventArgs(false, 0));
        }
    }
}
