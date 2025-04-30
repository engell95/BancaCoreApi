using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using BancaCore.Common.Interfaces;

namespace BancaCore.Persistence
{
    public class FireForgetCommandHandler : IFireForgetCommandHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public FireForgetCommandHandler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        public void Execute(Func<IBancaDbContext, Task> databaseWork)
        {
            // Fire off the task, but don't await the result
            Task.Run(async () =>
            {
                // Exceptions must be caught
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IBancaDbContext>();
                    await databaseWork(repository);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

    }
}
