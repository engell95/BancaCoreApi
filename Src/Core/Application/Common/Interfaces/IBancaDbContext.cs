using BancaCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace BancaCore.Common.Interfaces
{
    public interface IBancaDbContext
    {
        #region DBSets
        DbSet<Cliente> Clientes { get; set; }
        DbSet<CuentaBancaria> CuentasBancarias { get; set; }
        DbSet<Transaccion> Transacciones { get; set; }
        #endregion

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}