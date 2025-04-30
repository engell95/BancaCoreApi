using BancaCore.Common.Enumerable;
using BancaCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BancaCore.Persistence
{
    public class DatabaseSeeder
    {
        private readonly BancaDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(BancaDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();

                // Only seed if the database is empty
                if (!await _context.Clientes.AnyAsync())
                {
                    _logger.LogInformation("Seeding database with test data...");
                    await SeedTestDataAsync();
                    _logger.LogInformation("Database seeding completed successfully.");
                }
                else
                {
                    _logger.LogInformation("Database already contains data. Skipping seed operation.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task SeedTestDataAsync()
        {
            // Add test clients
            var clientes = new List<Cliente>
            {
                new Cliente
                {
                    Nombre = "Juan Pérez",
                    FechaNacimiento = new DateOnly(1985, 5, 15),
                    Sexo = "M",
                    Ingresos = 45000.00m
                },
                new Cliente
                {
                    Nombre = "María López",
                    FechaNacimiento = new DateOnly(1990, 8, 22),
                    Sexo = "F",
                    Ingresos = 38000.00m
                },
                new Cliente
                {
                    Nombre = "Carlos Rodríguez",
                    FechaNacimiento = new DateOnly(1978, 3, 10),
                    Sexo = "M",
                    Ingresos = 52000.00m
                }
            };

            await _context.Clientes.AddRangeAsync(clientes);
            await _context.SaveChangesAsync();

            // Add bank accounts
            var cuentas = new List<CuentaBancaria>
            {
                new CuentaBancaria
                {
                    NumeroCuenta = CuentaBancariaHelper.GenerarNumeroCuenta(),
                    ClienteId = clientes[0].Id,
                    Saldo = 15000.00m
                },
                new CuentaBancaria
                {
                    NumeroCuenta = CuentaBancariaHelper.GenerarNumeroCuenta(),
                    ClienteId = clientes[1].Id,
                    Saldo = 8500.00m
                },
                new CuentaBancaria
                {
                    NumeroCuenta = CuentaBancariaHelper.GenerarNumeroCuenta(),
                    ClienteId = clientes[2].Id,
                    Saldo = 22000.00m
                },
                new CuentaBancaria
                {
                    NumeroCuenta = CuentaBancariaHelper.GenerarNumeroCuenta(),
                    ClienteId = clientes[0].Id,
                    Saldo = 3500.00m
                }
            };

            await _context.CuentasBancarias.AddRangeAsync(cuentas);
            await _context.SaveChangesAsync();

            // Add transactions
            var transacciones = new List<Transaccion>
            {
                new Transaccion
                {
                    CuentaId = cuentas[0].Id,
                    TipoTransaccion = (int)EnumTipoTransaccion.Deposito,
                    Descripcion = "Depósito inicial",
                    Monto = 15000.00m,
                    SaldoPosterior = 15000.00m,
                    FechaTransaccion = DateTime.Now.AddDays(-30)
                },
                new Transaccion
                {
                    CuentaId = cuentas[1].Id,
                    TipoTransaccion = (int)EnumTipoTransaccion.Deposito,
                    Descripcion = "Depósito inicial",
                    Monto = 10000.00m,
                    SaldoPosterior = 10000.00m,
                    FechaTransaccion = DateTime.Now.AddDays(-25)
                },
                new Transaccion
                {
                    CuentaId = cuentas[1].Id,
                    TipoTransaccion = (int)EnumTipoTransaccion.Retiro,
                    Descripcion = "Pago de servicios",
                    Monto = 1500.00m,
                    SaldoPosterior = 8500.00m,
                    FechaTransaccion = DateTime.Now.AddDays(-10)
                },
                new Transaccion
                {
                    CuentaId = cuentas[2].Id,
                    TipoTransaccion = (int)EnumTipoTransaccion.Deposito,
                    Descripcion = "Depósito inicial",
                    Monto = 22000.00m,
                    SaldoPosterior = 22000.00m,
                    FechaTransaccion = DateTime.Now.AddDays(-15)
                },
                new Transaccion
                {
                    CuentaId = cuentas[3].Id,
                    TipoTransaccion = (int)EnumTipoTransaccion.Deposito,
                    Descripcion = "Depósito inicial",
                    Monto = 5000.00m,
                    SaldoPosterior = 5000.00m,
                    FechaTransaccion = DateTime.Now.AddDays(-20)
                },
                new Transaccion
                {
                    CuentaId = cuentas[3].Id,
                    TipoTransaccion = (int)EnumTipoTransaccion.Retiro,
                    Descripcion = "Compra en línea",
                    Monto = 1500.00m,
                    SaldoPosterior = 3500.00m,
                    FechaTransaccion = DateTime.Now.AddDays(-5)
                }
            };

            await _context.Transacciones.AddRangeAsync(transacciones);
            await _context.SaveChangesAsync();
        }
    }
}