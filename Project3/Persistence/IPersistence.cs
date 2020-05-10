using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newmazon.Persistence
{
    public interface IPersistence
    {
        Task<AllData> LoadAsync(String name);

        Task<ICollection<Simulations>> ListAsync();
    }
}
