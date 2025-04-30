using BancaCore.Application.Transacciones.Queries.GetTransaccionesSummary;
using BancaCore.Common.Enumerable;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using BancaCore.Persistence;
using EntityFrameworkCore.Testing.Moq;

namespace BancaCoreApi.Test
{
    /// <summary>
    /// Pruebas unitarias para las consultas de la entidad Transaccion
    /// </summary>
    public class TransaccionesQueriesTest
    {
        private readonly IBancaDbContext _context;

        public TransaccionesQueriesTest()
        {
            _context = Create.MockedDbContextFor<BancaDbContext>();
        }

        #region GetTransaccionesSummary Query Tests

        [Fact]
        public async Task Handle_GetTransaccionesSummary_RetornaResumenCorrecto()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = 1,
                Nombre = "Juan Pérez",
                FechaNacimiento = new DateOnly(1990, 5, 15),
                Sexo = "M",
                Ingresos = 5000.00m
            };

            var cuenta = new CuentaBancaria
            {
                Id = 1,
                NumeroCuenta = "1234567890",
                ClienteId = cliente.Id,
                Saldo = 2500.00m,
                Cliente = cliente
            };

            var fechaBase = DateTime.Now.AddDays(-10);

            var transaccion1 = new Transaccion
            {
                Id = 1,
                CuentaId = cuenta.Id,
                TipoTransaccion = (int)EnumTipoTransaccion.Deposito,
                Monto = 1000.00m,
                SaldoPosterior = 1000.00m,
                Descripcion = "Depósito inicial",
                FechaTransaccion = fechaBase
            };

            var transaccion2 = new Transaccion
            {
                Id = 2,
                CuentaId = cuenta.Id,
                TipoTransaccion = (int)EnumTipoTransaccion.Deposito,
                Monto = 2000.00m,
                SaldoPosterior = 3000.00m,
                Descripcion = "Segundo depósito",
                FechaTransaccion = fechaBase.AddDays(2)
            };

            var transaccion3 = new Transaccion
            {
                Id = 3,
                CuentaId = cuenta.Id,
                TipoTransaccion = (int)EnumTipoTransaccion.Retiro,
                Monto = -500.00m,
                SaldoPosterior = 2500.00m,
                Descripcion = "Retiro",
                FechaTransaccion = fechaBase.AddDays(5)
            };

            cuenta.Transacciones.Add(transaccion1);
            cuenta.Transacciones.Add(transaccion2);
            cuenta.Transacciones.Add(transaccion3);

            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta);
            _context.Transacciones.Add(transaccion1);
            _context.Transacciones.Add(transaccion2);
            _context.Transacciones.Add(transaccion3);
            await _context.SaveChangesAsync();

            var query = new GetTransaccionesSummaryQuery { NumeroCuenta = cuenta.NumeroCuenta };
            var handler = new GetTransaccionesSummaryQueryHandler(_context);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cuenta.NumeroCuenta, result.NumeroCuenta);
            Assert.Equal(cuenta.Saldo, result.SaldoFinal);
            Assert.Equal(3, result.Transacciones.Count);

            // Verificar que las transacciones estén ordenadas por fecha
            Assert.Equal(transaccion1.Id, result.Transacciones[0].Id);
            Assert.Equal(transaccion2.Id, result.Transacciones[1].Id);
            Assert.Equal(transaccion3.Id, result.Transacciones[2].Id);

            // Verificar los detalles de la primera transacción
            var primeraTransaccion = result.Transacciones[0];
            Assert.Equal("Deposito", primeraTransaccion.TipoTransaccion);
            Assert.Equal(1000.00m, primeraTransaccion.Monto);
            Assert.Equal(1000.00m, primeraTransaccion.SaldoPosterior);
            Assert.Equal("Depósito inicial", primeraTransaccion.Descripcion);
            Assert.Equal(fechaBase, primeraTransaccion.FechaTransaccion);

            // Verificar los detalles de la última transacción
            var ultimaTransaccion = result.Transacciones[2];
            Assert.Equal("Retiro", ultimaTransaccion.TipoTransaccion);
            Assert.Equal(-500.00m, ultimaTransaccion.Monto);
            Assert.Equal(2500.00m, ultimaTransaccion.SaldoPosterior);
            Assert.Equal("Retiro", ultimaTransaccion.Descripcion);
        }

        [Fact]
        public async Task Handle_GetTransaccionesSummary_CuentaSinTransacciones_RetornaListaVacia()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = 1,
                Nombre = "María López",
                FechaNacimiento = new DateOnly(1985, 3, 20),
                Sexo = "F",
                Ingresos = 6500.00m
            };

            var cuenta = new CuentaBancaria
            {
                Id = 1,
                NumeroCuenta = "0987654321",
                ClienteId = cliente.Id,
                Saldo = 0.00m,
                Cliente = cliente
            };

            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            var query = new GetTransaccionesSummaryQuery { NumeroCuenta = cuenta.NumeroCuenta };
            var handler = new GetTransaccionesSummaryQueryHandler(_context);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cuenta.NumeroCuenta, result.NumeroCuenta);
            Assert.Equal(0.00m, result.SaldoFinal);
            Assert.Empty(result.Transacciones);
        }

        [Fact]
        public async Task Handle_GetTransaccionesSummary_MultiplesTransacciones_OrdenadosPorFecha()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = 1,
                Nombre = "Carlos Rodríguez",
                FechaNacimiento = new DateOnly(1978, 8, 25),
                Sexo = "M",
                Ingresos = 7800.00m
            };

            var cuenta = new CuentaBancaria
            {
                Id = 1,
                NumeroCuenta = "1122334455",
                ClienteId = cliente.Id,
                Saldo = 3500.00m,
                Cliente = cliente
            };

            // Crear transacciones con fechas desordenadas para verificar el ordenamiento
            var transaccion1 = new Transaccion
            {
                Id = 1,
                CuentaId = cuenta.Id,
                TipoTransaccion = (int)EnumTipoTransaccion.Deposito,
                Monto = 1000.00m,
                SaldoPosterior = 1000.00m,
                Descripcion = "Depósito inicial",
                FechaTransaccion = DateTime.Now.AddDays(-5)
            };

            var transaccion2 = new Transaccion
            {
                Id = 2,
                CuentaId = cuenta.Id,
                TipoTransaccion = (int)EnumTipoTransaccion.Deposito,
                Monto = 3000.00m,
                SaldoPosterior = 4000.00m,
                Descripcion = "Segundo depósito",
                FechaTransaccion = DateTime.Now.AddDays(-1) // Más reciente que transaccion3
            };

            var transaccion3 = new Transaccion
            {
                Id = 3,
                CuentaId = cuenta.Id,
                TipoTransaccion = (int)EnumTipoTransaccion.Retiro,
                Monto = -500.00m,
                SaldoPosterior = 3500.00m,
                Descripcion = "Retiro",
                FechaTransaccion = DateTime.Now.AddDays(-3) // Entre transaccion1 y transaccion2
            };

            cuenta.Transacciones.Add(transaccion1);
            cuenta.Transacciones.Add(transaccion2);
            cuenta.Transacciones.Add(transaccion3);

            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta);
            _context.Transacciones.Add(transaccion1);
            _context.Transacciones.Add(transaccion2);
            _context.Transacciones.Add(transaccion3);
            await _context.SaveChangesAsync();

            var query = new GetTransaccionesSummaryQuery { NumeroCuenta = cuenta.NumeroCuenta };
            var handler = new GetTransaccionesSummaryQueryHandler(_context);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Transacciones.Count);

            // Verificar que las transacciones estén ordenadas por fecha
            Assert.Equal(transaccion1.Id, result.Transacciones[0].Id);
            Assert.Equal(transaccion3.Id, result.Transacciones[1].Id);
            Assert.Equal(transaccion2.Id, result.Transacciones[2].Id);

            // Verificar que las fechas estén en orden ascendente
            Assert.True(result.Transacciones[0].FechaTransaccion < result.Transacciones[1].FechaTransaccion);
            Assert.True(result.Transacciones[1].FechaTransaccion < result.Transacciones[2].FechaTransaccion);
        }

        #endregion
    }
}