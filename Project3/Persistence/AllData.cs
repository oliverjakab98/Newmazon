using System.Collections.Generic;

namespace Newmazon.Persistence
{

    public class Goods //áruk adatait tároló osztály
    {
        public int x; //x-koordináta
        public int y; //y-koordináta
        public int[] destinations; //hova kell vinni az árut
        public Goods(int _x, int _y, int[] dests)
        {
            x = _x;
            y = _y;
            destinations = dests;
        }
    }

    public class AllData //a központnak ezekre az adatokra van szüksége a szimuláció indításához
    {
        public char[,] dataTable; //pályaelemek
        public List<Goods> goods; //áruk
        public int tableSize; //pálya mérete
        public int robotEnergy; //robotok alap energiaszintje

        public AllData(char[,] _dataTable, List<Goods> _goods, int _tableSize, int _robotEnergy)
        {
            dataTable = _dataTable;
            goods = _goods;
            tableSize = _tableSize;
            robotEnergy = _robotEnergy;
        }

        public AllData() 
        {
            
        }

    }
}
