using BancaCore.Application.CuentasBancarias.Commands.CreateCuentaBancaria;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using BancaCore.Persistence;
using EntityFrameworkCore.Testing.Moq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BancaCoreApi.Test
{
    /// <summary>
    /// Pruebas unitarias para los comandos de la entidad CuentaBancaria
    /// </summary>
    public class CuentasBancariasCommandsTest
    {
        private readonly IBancaDbContext _context;
        private readonly Mock<IMediator> _mediatorMock;

        public CuentasBancariasCommandsTest()
        {
            _context = Create.MockedDbContextFor<BancaDbContext>();
            _mediatorMock = new Mock<IMediator>();
        }

        #region CreateCuentaBancaria Command Tests

        [Fact]
        public async Task Handle_CreateCuentaBancaria_SinSaldoInicial_CuentaCreada()
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

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var command = new CreateCuentaBancariaCommand
            {
                ClienteId = cliente.Id,
                SaldoInicial = 0
            };

            var handler = new CreateCuentaBancariaCommandHandler(_context, _mediatorMock.Object);

            // Act
            var numeroCuenta = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(numeroCuenta);
            Assert.NotEmpty(numeroCuenta);

            var cuentaCreada = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta);

            Assert.NotNull(cuentaCreada);
            Assert.Equal(cliente.Id, cuentaCreada.ClienteId);
            Assert.Equal(0, cuentaCreada.Saldo);

            // Verificar que no se llamó al mediator para realizar un depósito
            _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CreateCuentaBancaria_ConSaldoInicial_CuentaCreadaYDepositoRealizado()
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

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var command = new CreateCuentaBancariaCommand
            {
                ClienteId = cliente.Id,
                SaldoInicial = 1000.00m
            };

            // Configurar el mock del mediator para simular el depósito
            _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1); // Simular que se creó una transacción con ID 1

            var handler = new CreateCuentaBancariaCommandHandler(_context, _mediatorMock.Object);

            // Act
            var numeroCuenta = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(numeroCuenta);
            Assert.NotEmpty(numeroCuenta);

            var cuentaCreada = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta);

            Assert.NotNull(cuentaCreada);
            Assert.Equal(cliente.Id, cuentaCreada.ClienteId);

            // Verificar que se llamó al mediator para realizar un depósito
            _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<int>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateCuentaBancaria_GeneraNumeroCuentaUnico()
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

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var command1 = new CreateCuentaBancariaCommand { ClienteId = cliente.Id, SaldoInicial = 0 };
            var command2 = new CreateCuentaBancariaCommand { ClienteId = cliente.Id, SaldoInicial = 0 };

            var handler = new CreateCuentaBancariaCommandHandler(_context, _mediatorMock.Object);

            // Act
            var numeroCuenta1 = await handler.Handle(command1, CancellationToken.None);
            var numeroCuenta2 = await handler.Handle(command2, CancellationToken.None);

            // Assert
            Assert.NotEqual(numeroCuenta1, numeroCuenta2);

            var cuentas = await _context.CuentasBancarias
                .Where(c => c.ClienteId == cliente.Id)
                .ToListAsync();

            Assert.Equal(2, cuentas.Count);
        }

        #endregion
    }
}