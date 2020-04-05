using System.Collections.Generic;

namespace Newmazon.Persistence
{

    public class Goods
    {
        public int x;
        public int y;
        public int[] destinations;
        public Goods(int _x, int _y, int[] dests)
        {
            x = _x;
            y = _y;
            destinations = dests;
        }
    }

    public class AllData
    {
        public char[,] dataTable;
        public List<Goods> goods;
        public int tableSize;
        public int robotEnergy;

        public AllData(char[,] _dataTable, List<Goods> _goods, int _tableSize, int _robotEnergy)
        {
            dataTable = _dataTable;
            goods = _goods;
            tableSize = _tableSize;
            robotEnergy = _robotEnergy;
        }

    }
}
