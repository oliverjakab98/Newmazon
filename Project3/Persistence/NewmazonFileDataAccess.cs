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
        private String _simDirectory;

        public NewmazonFileDataAccess(String simDirectory)
        {
            _simDirectory = simDirectory;
        }

		
		public async Task<AllData> LoadAsync(String name)
		{
            String path = Path.Combine(_simDirectory, name);
            Debug.WriteLine(path);
			try
			{
				using (StreamReader reader = new StreamReader(path))
				{
					String line = await reader.ReadLineAsync();
					String[] fileData = line.Split(' ');
					int tableSize = int.Parse(fileData[0]);
					char[,] table = new char[tableSize,tableSize];

					for (int i = 0; i < tableSize; i++)
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
					int robotEnergy = int.Parse(fileData[0]);
					int goodsCount = int.Parse(fileData[1]);

					List<Goods> goods = new List<Goods>();
					int goodX, goodY, destCount;
					int[] destinations;

					for(int i = 0; i < goodsCount; i++)
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
					AllData data = new AllData(table, goods, tableSize, robotEnergy);
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
