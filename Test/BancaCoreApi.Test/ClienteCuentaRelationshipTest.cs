using AutoMapper;
using BancaCore.Application.Clientes.Queries;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using BancaCore.Persistence;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BancaCoreApi.Test
{
    /// <summary>
    /// Pruebas unitarias para validar la relación entre Cliente y CuentaBancaria
    /// </summary>
    public class ClienteCuentaRelationshipTest
    {
        private readonly IBancaDbContext _context;
        private readonly IMapper _mapper;

        public ClienteCuentaRelationshipTest()
        {
            // Configuración del contexto simulado
            _context = Create.MockedDbContextFor<BancaDbContext>();

            // Configuración real de AutoMapper
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Cliente, ClienteDto>()
                    .ForMember(d => d.CuentasBancarias, opt => opt.MapFrom(s => s.CuentasBancaria));
                cfg.CreateMap<CuentaBancaria, CuentasDto>();
            });
            _mapper = configuration.CreateMapper();
        }

        [Fact]
        public async Task Cliente_AgregarCuentaBancaria_RelacionCorrecta()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nombre = "Roberto Sánchez",
                FechaNacimiento = new DateOnly(1982, 7, 12),
                Sexo = "M",
                Ingresos = 8500.00m
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            // Act
            var cuenta = new CuentaBancaria
            {
                NumeroCuenta = "1122334455",
                ClienteId = cliente.Id,
                Saldo = 3000.00m,
                Cliente = cliente
            };

            cliente.CuentasBancaria.Add(cuenta);
            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            // Assert
            var clienteConCuenta = await _context.Clientes
                .Include(c => c.CuentasBancaria)
                .FirstOrDefaultAsync(c => c.Id == cliente.Id);

            Assert.NotNull(clienteConCuenta);
            Assert.Single(clienteConCuenta.CuentasBancaria);
            Assert.Equal("1122334455", clienteConCuenta.CuentasBancaria.First().NumeroCuenta);
            Assert.Equal(cliente.Id, clienteConCuenta.CuentasBancaria.First().ClienteId);
        }

        [Fact]
        public async Task Cliente_EliminarCuentaBancaria_CuentaEliminada()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nombre = "Laura Fernández",
                FechaNacimiento = new DateOnly(1975, 9, 28),
                Sexo = "F",
                Ingresos = 7200.00m
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var cuenta1 = new CuentaBancaria
            {
                NumeroCuenta = "9988776655",
                ClienteId = cliente.Id,
                Saldo = 1500.00m,
                Cliente = cliente
            };

            var cuenta2 = new CuentaBancaria
            {
                NumeroCuenta = "5544332211",
                ClienteId = cliente.Id,
                Saldo = 2800.00m,
                Cliente = cliente
            };

            cliente.CuentasBancaria.Add(cuenta1);
            cliente.CuentasBancaria.Add(cuenta2);
            _context.CuentasBancarias.Add(cuenta1);
            _context.CuentasBancarias.Add(cuenta2);
            await _context.SaveChangesAsync();

            // Act
            cliente.CuentasBancaria.Remove(cuenta1);
            _context.CuentasBancarias.Remove(cuenta1);
            await _context.SaveChangesAsync();

            // Assert
            var clienteActualizado = await _context.Clientes
                .Include(c => c.CuentasBancaria)
                .FirstOrDefaultAsync(c => c.Id == cliente.Id);

            Assert.NotNull(clienteActualizado);
            Assert.Single(clienteActualizado.CuentasBancaria);
            Assert.Equal("5544332211", clienteActualizado.CuentasBancaria.First().NumeroCuenta);
        }

        [Fact]
        public async Task Cliente_EliminarClienteConCuentas_CascadaCorrecta()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nombre = "Miguel Torres",
                FechaNacimiento = new DateOnly(1988, 3, 15),
                Sexo = "M",
                Ingresos = 6300.00m
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var cuenta = new CuentaBancaria
            {
                NumeroCuenta = "1357924680",
                ClienteId = cliente.Id,
                Saldo = 4200.00m,
                Cliente = cliente
            };

            cliente.CuentasBancaria.Add(cuenta);
            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            var clienteId = cliente.Id;
            var cuentaId = cuenta.Id;

            // Act
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            // Assert
            var clienteEliminado = await _context.Clientes.FindAsync(clienteId);
            var cuentaEliminada = await _context.CuentasBancarias.FindAsync(cuentaId);

            Assert.Null(clienteEliminado);
            Assert.Null(cuentaEliminada); // La cuenta debería eliminarse en cascada
        }

        [Fact]
        public async Task Cliente_MapeoAClienteDto_MapeoCorrectoConCuentas()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nombre = "Sofía Ramírez",
                FechaNacimiento = new DateOnly(1995, 11, 8),
                Sexo = "F",
                Ingresos = 5800.00m
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var cuenta1 = new CuentaBancaria
            {
                NumeroCuenta = "2468013579",
                ClienteId = cliente.Id,
                Saldo = 1800.00m,
                Cliente = cliente
            };

            var cuenta2 = new CuentaBancaria
            {
                NumeroCuenta = "9753102468",
                ClienteId = cliente.Id,
                Saldo = 3500.00m,
                Cliente = cliente
            };

            cliente.CuentasBancaria.Add(cuenta1);
            cliente.CuentasBancaria.Add(cuenta2);
            _context.CuentasBancarias.Add(cuenta1);
            _context.CuentasBancarias.Add(cuenta2);
            await _context.SaveChangesAsync();

            // Act
            var clienteDto = _mapper.Map<ClienteDto>(cliente);

            // Assert
            Assert.NotNull(clienteDto);
            Assert.Equal(cliente.Id, clienteDto.Id);
            Assert.Equal(cliente.Nombre, clienteDto.Nombre);
            Assert.Equal(cliente.FechaNacimiento, clienteDto.FechaNacimiento);
            Assert.Equal(cliente.Sexo, clienteDto.Sexo);
            Assert.Equal(cliente.Ingresos, clienteDto.Ingresos);
            Assert.Equal(2, clienteDto.CuentasBancarias.Count);
            
            // Verificar que las cuentas se mapearon correctamente
            Assert.Contains(clienteDto.CuentasBancarias, c => c.NumeroCuenta == "2468013579" && c.Saldo == 1800.00m);
            Assert.Contains(clienteDto.CuentasBancarias, c => c.NumeroCuenta == "9753102468" && c.Saldo == 3500.00m);
        }
    }
}