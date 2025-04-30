using BancaCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BancaCore.Common.Interfaces
{
    public interface IBancaDbContext
    {
        #region DBSets
        DbSet<Clientes> Clientes { get; set; }
        DbSet<CuentasBancarias> CuentasBancarias { get; set; }
        DbSet<Transacciones> Transacciones { get; set; }

        #endregion

    }
}
