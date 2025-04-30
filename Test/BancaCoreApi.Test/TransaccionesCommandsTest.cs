using BancaCore.Application.Transacciones.Commands.RealizarDeposito;
using BancaCore.Application.Transacciones.Commands.RealizarRetiro;
using BancaCore.Common.Exceptions;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using BancaCore.Persistence;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.EntityFrameworkCore;

namespace BancaCoreApi.Test
{
    /// <summary>
    /// Pruebas unitarias para los comandos de la entidad Transaccion
    /// </summary>
    public class TransaccionesCommandsTest
    {
        private readonly IBancaDbContext _context;

        public TransaccionesCommandsTest()
        {
            _context = Create.MockedDbContextFor<BancaDbContext>();
        }

        #region RealizarDeposito Command Tests

        [Fact]
        public async Task Handle_RealizarDeposito_DepositoExitoso()
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
                Saldo = 1000.00m,
                Cliente = cliente
            };

            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            var command = new RealizarDepositoCommand
            {
                NumeroCuenta = cuenta.NumeroCuenta,
                Monto = 500.00m,
                Descripcion = "Depósito de prueba"
            };

            var handler = new RealizarDepositoCommandHandler(_context);

            // Act
            var transaccionId = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(0, transaccionId);

            var cuentaActualizada = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.NumeroCuenta == cuenta.NumeroCuenta);
            Assert.Equal(1500.00m, cuentaActualizada.Saldo);

            var transaccion = await _context.Transacciones
                .FirstOrDefaultAsync(t => t.Id == transaccionId);
            Assert.NotNull(transaccion);
            Assert.Equal(cuenta.Id, transaccion.CuentaId);
            Assert.Equal(500.00m, transaccion.Monto);
            Assert.Equal("Depósito de prueba", transaccion.Descripcion);
            Assert.Equal(1500.00m, transaccion.SaldoPosterior);
            Assert.Equal(1, transaccion.TipoTransaccion); // Depósito
        }

        [Fact]
        public async Task Handle_RealizarDeposito_SinDescripcion_UsaDescripcionPredeterminada()
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
                Saldo = 2000.00m,
                Cliente = cliente
            };

            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            var command = new RealizarDepositoCommand
            {
                NumeroCuenta = cuenta.NumeroCuenta,
                Monto = 800.00m,
                Descripcion = null // Sin descripción
            };

            var handler = new RealizarDepositoCommandHandler(_context);

            // Act
            var transaccionId = await handler.Handle(command, CancellationToken.None);

            // Assert
            var transaccion = await _context.Transacciones
                .FirstOrDefaultAsync(t => t.Id == transaccionId);
            Assert.NotNull(transaccion);
            Assert.Equal("Depósito en cuenta", transaccion.Descripcion);
        }

        #endregion

        #region RealizarRetiro Command Tests

        [Fact]
        public async Task Handle_RealizarRetiro_RetiroExitoso()
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
                Saldo = 3000.00m,
                Cliente = cliente
            };

            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            var command = new RealizarRetiroCommand
            {
                NumeroCuenta = cuenta.NumeroCuenta,
                Monto = 1000.00m,
                Descripcion = "Retiro de prueba"
            };

            var handler = new RealizarRetiroCommandHandler(_context);

            // Act
            var transaccionId = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(0, transaccionId);

            var cuentaActualizada = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.NumeroCuenta == cuenta.NumeroCuenta);
            Assert.Equal(2000.00m, cuentaActualizada.Saldo);

            var transaccion = await _context.Transacciones
                .FirstOrDefaultAsync(t => t.Id == transaccionId);
            Assert.NotNull(transaccion);
            Assert.Equal(cuenta.Id, transaccion.CuentaId);
            Assert.Equal(-1000.00m, transaccion.Monto); // Monto negativo para retiros
            Assert.Equal("Retiro de prueba", transaccion.Descripcion);
            Assert.Equal(2000.00m, transaccion.SaldoPosterior);
            Assert.Equal(2, transaccion.TipoTransaccion); // Retiro
        }

        [Fact]
        public async Task Handle_RealizarRetiro_SaldoInsuficiente_LanzaValidationException()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = 1,
                Nombre = "Ana Martínez",
                FechaNacimiento = new DateOnly(1992, 4, 10),
                Sexo = "F",
                Ingresos = 4500.00m
            };

            var cuenta = new CuentaBancaria
            {
                Id = 1,
                NumeroCuenta = "5566778899",
                ClienteId = cliente.Id,
                Saldo = 500.00m,
                Cliente = cliente
            };

            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            var command = new RealizarRetiroCommand
            {
                NumeroCuenta = cuenta.NumeroCuenta,
                Monto = 1000.00m, // Monto mayor al saldo
                Descripcion = "Retiro que excede el saldo"
            };

            var handler = new RealizarRetiroCommandHandler(_context);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));

            // Verificar que el saldo no cambió
            var cuentaActualizada = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.NumeroCuenta == cuenta.NumeroCuenta);
            Assert.Equal(500.00m, cuentaActualizada.Saldo);

            // Verificar que no se creó ninguna transacción
            var transacciones = await _context.Transacciones
                .Where(t => t.CuentaId == cuenta.Id)
                .ToListAsync();
            Assert.Empty(transacciones);
        }

        [Fact]
        public async Task Handle_RealizarRetiro_SinDescripcion_UsaDescripcionPredeterminada()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = 1,
                Nombre = "Pedro Gómez",
                FechaNacimiento = new DateOnly(1980, 12, 5),
                Sexo = "M",
                Ingresos = 6200.00m
            };

            var cuenta = new CuentaBancaria
            {
                Id = 1,
                NumeroCuenta = "9988776655",
                ClienteId = cliente.Id,
                Saldo = 2500.00m,
                Cliente = cliente
            };

            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            var command = new RealizarRetiroCommand
            {
                NumeroCuenta = cuenta.NumeroCuenta,
                Monto = 500.00m,
                Descripcion = null // Sin descripción
            };

            var handler = new RealizarRetiroCommandHandler(_context);

            // Act
            var transaccionId = await handler.Handle(command, CancellationToken.None);

            // Assert
            var transaccion = await _context.Transacciones
                .FirstOrDefaultAsync(t => t.Id == transaccionId);
            Assert.NotNull(transaccion);
            Assert.Equal("Retiro de cuenta", transaccion.Descripcion);
        }

        #endregion
    }
}