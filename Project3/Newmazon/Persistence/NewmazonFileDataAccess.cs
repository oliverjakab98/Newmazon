using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newmazon.Persistence
{
    public class NewmazonFileDataAccess
    {
        private String _simDirectory;

        public NewmazonFileDataAccess(String simDirectory)
        {
            _simDirectory = simDirectory;
        }

		/*
		public async Task<AllData> LoadAsync(String name)
		{

		}
		*/

		public async Task<ICollection<Simulations>> ListAsync()
		{
			try
			{
				return Directory.GetFiles(_simDirectory, "*.stl")
					.Select(path => new Simulations
					{
						Name = Path.GetFileNameWithoutExtension(path),
					})
					.ToList();
			}
			catch
			{
				throw new NewmazonDataExcception();
			}
		}
	}
}
