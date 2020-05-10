using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Newmazon.Persistence
{
    public class NewmazonFileDataAccess : IPersistence
    {
        private String _simDirectory; //a mappa, ahol a szimulációkat tároljuk

        public NewmazonFileDataAccess(String simDirectory) //a konstruktornak adjuk át a mappát
        {
            _simDirectory = simDirectory;
        }
		
		public async Task<AllData> LoadAsync(String name) //a játék betöltésére szolgáló függvény, a fájlnevet kapja paraméterül
		{
            String path = Path.Combine(_simDirectory, name); //összekombinálja az elérési utat és a fájlnevet
            Debug.WriteLine(path);
			try
			{
				using (StreamReader reader = new StreamReader(path)) //megnyitja a path útvonalon található fájlt olvasásra
				{
					String line = await reader.ReadLineAsync(); //új sor
					String[] fileData = line.Split(' '); //whitespace az elválasztó karakter az adatok között
					int tableSize = int.Parse(fileData[0]); //1. sorban a szimuláció mérete található
					char[,] table = new char[tableSize,tableSize];

					for (int i = 0; i < tableSize; i++) //ezután beolvasunk minden pályaelemet (robotok, polcok, célállomások, töltőállomások, falak, üres mezők)
					{
						line = await reader.ReadLineAsync();
						fileData = line.Split(' ');

						for (int j = 0; j < tableSize; j++)
						{
							table[i,j] = fileData[j].ToCharArray()[0];
						}
					}

					line = await reader.ReadLineAsync();
					fileData = line.Split(' ');
					int robotEnergy = int.Parse(fileData[0]); //robotok energiája
					int goodsCount = int.Parse(fileData[1]); //kiszállítandó áru mennyisége

					List<Goods> goods = new List<Goods>();
					int goodX, goodY, destCount;
					int[] destinations;

					for(int i = 0; i < goodsCount; i++) //végül beolvassuk, hogy hol vannak a kiszállítandó áruk
					{
						line = await reader.ReadLineAsync();
						fileData = line.Split(' ');
						goodX = int.Parse(fileData[0]);
						goodY = int.Parse(fileData[1]);
						destCount = int.Parse(fileData[2]);
						destinations = new int[destCount];

						for(int j = 0; j < destCount; j++)
						{
							destinations[j] = int.Parse(fileData[j+3]);
						}
						goods.Add(new Goods(goodX, goodY, destinations));
					}
					AllData data = new AllData(table, goods, tableSize, robotEnergy); //a beolvasott adatokat elhelyezzük az AllData típusú változóba, a központ innen fogja kiolvasni őket
					return data;
				}
			}
			catch
			{
				throw new NewmazonDataException();
			}
		}
	}
}
