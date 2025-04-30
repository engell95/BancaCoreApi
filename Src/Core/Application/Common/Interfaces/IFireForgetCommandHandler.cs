using System;
using System.Threading.Tasks;

namespace BancaCore.Common.Interfaces
{
    public interface IFireForgetCommandHandler
    {
        void Execute(Func<IBancaDbContext, Task> databaseWork);
    }
}
