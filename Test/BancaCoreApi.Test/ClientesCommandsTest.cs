using BancaCore.Application.Clientes.Commands.CreateCliente;
using BancaCore.Application.Clientes.Commands.UpdateCliente;
using BancaCore.Common.Exceptions;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using BancaCore.Persistence;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.EntityFrameworkCore;

namespace BancaCoreApi.Test
{
    /// <summary>
    /// Pruebas unitarias para los comandos de la entidad Cliente
    /// </summary>
    public class ClientesCommandsTest
    {
        private readonly IBancaDbContext _context;

        public ClientesCommandsTest()
        {
            _context = Create.MockedDbContextFor<BancaDbContext>();
        }

        #region CreateCliente Command Tests

        [Fact]
        public async Task Handle_CreateCliente_ClienteCreado()
        {
            // Arrange
            var command = new CreateClienteCommand
            {
                Nombre = "Juan Pérez",
                FechaNacimiento = new DateOnly(1990, 5, 15),
                Sexo = "M",
                Ingresos = 5000.00m
            };

            var handler = new CreateClienteCommandHandler(_context);

            // Act
            var clienteId = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(0, clienteId);

            var clienteCreado = await _context.Clientes.FindAsync(clienteId);
            Assert.NotNull(clienteCreado);
            Assert.Equal(command.Nombre, clienteCreado.Nombre);
            Assert.Equal(command.FechaNacimiento, clienteCreado.FechaNacimiento);
            Assert.Equal(command.Sexo, clienteCreado.Sexo);
            Assert.Equal(command.Ingresos, clienteCreado.Ingresos);
            Assert.Empty(clienteCreado.CuentasBancaria);
        }

        [Fact]
        public async Task Handle_CreateCliente_MultiplesClientes_IdsUnicos()
        {
            // Arrange
            var command1 = new CreateClienteCommand
            {
                Nombre = "María López",
                FechaNacimiento = new DateOnly(1985, 3, 20),
                Sexo = "F",
                Ingresos = 6500.00m
            };

            var command2 = new CreateClienteCommand
            {
                Nombre = "Carlos Rodríguez",
                FechaNacimiento = new DateOnly(1978, 8, 25),
                Sexo = "M",
                Ingresos = 7800.00m
            };

            var handler = new CreateClienteCommandHandler(_context);

            // Act
            var clienteId1 = await handler.Handle(command1, CancellationToken.None);
            var clienteId2 = await handler.Handle(command2, CancellationToken.None);

            // Assert
            Assert.NotEqual(0, clienteId1);
            Assert.NotEqual(0, clienteId2);
            Assert.NotEqual(clienteId1, clienteId2);

            var clientes = await _context.Clientes.ToListAsync();
            Assert.Equal(2, clientes.Count);
        }

        #endregion

        #region UpdateCliente Command Tests

        [Fact]
        public async Task Handle_UpdateCliente_ClienteActualizado()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nombre = "Ana Martínez",
                FechaNacimiento = new DateOnly(1992, 4, 10),
                Sexo = "F",
                Ingresos = 4500.00m
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var command = new UpdateClienteCommand
            {
                Id = cliente.Id,
                Nombre = "Ana María Martínez",
                FechaNacimiento = new DateOnly(1992, 4, 10), // Misma fecha
                Sexo = "F", // Mismo sexo
                Ingresos = 5200.00m // Ingresos actualizados
            };

            var handler = new UpdateClienteCommandHandler(_context);

            // Act
            var clienteId = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(cliente.Id, clienteId);

            var clienteActualizado = await _context.Clientes.FindAsync(clienteId);
            Assert.NotNull(clienteActualizado);
            Assert.Equal(command.Nombre, clienteActualizado.Nombre);
            Assert.Equal(command.FechaNacimiento, clienteActualizado.FechaNacimiento);
            Assert.Equal(command.Sexo, clienteActualizado.Sexo);
            Assert.Equal(command.Ingresos, clienteActualizado.Ingresos);
        }

        [Fact]
        public async Task Handle_UpdateCliente_ClienteNoExiste_LanzaNotFoundException()
        {
            // Arrange
            var clienteIdInexistente = 999;
            var command = new UpdateClienteCommand
            {
                Id = clienteIdInexistente,
                Nombre = "Cliente Inexistente",
                FechaNacimiento = new DateOnly(1980, 1, 1),
                Sexo = "M",
                Ingresos = 3000.00m
            };

            var handler = new UpdateClienteCommandHandler(_context);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UpdateCliente_ActualizacionCompleta()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nombre = "Pedro Gómez",
                FechaNacimiento = new DateOnly(1980, 12, 5),
                Sexo = "M",
                Ingresos = 6200.00m
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var command = new UpdateClienteCommand
            {
                Id = cliente.Id,
                Nombre = "Pedro Antonio Gómez",
                FechaNacimiento = new DateOnly(1981, 1, 15), // Fecha actualizada
                Sexo = "M",
                Ingresos = 7500.00m // Ingresos actualizados
            };

            var handler = new UpdateClienteCommandHandler(_context);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var clienteActualizado = await _context.Clientes.FindAsync(cliente.Id);
            Assert.Equal("Pedro Antonio Gómez", clienteActualizado.Nombre);
            Assert.Equal(new DateOnly(1981, 1, 15), clienteActualizado.FechaNacimiento);
            Assert.Equal("M", clienteActualizado.Sexo);
            Assert.Equal(7500.00m, clienteActualizado.Ingresos);
        }

        #endregion
    }
}