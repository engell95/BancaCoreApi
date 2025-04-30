using AutoMapper;
using BancaCore.Application.CuentasBancarias.Queries;
using BancaCore.Application.CuentasBancarias.Queries.GetCuentaBancariaByNumeroCuenta;
using BancaCore.Common.Exceptions;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using BancaCore.Persistence;
using EntityFrameworkCore.Testing.Moq;

namespace BancaCoreApi.Test
{
    /// <summary>
    /// Pruebas unitarias para las consultas de la entidad CuentaBancaria
    /// </summary>
    public class CuentasBancariasQueriesTest
    {
        private readonly IBancaDbContext _context;
        private readonly IMapper _mapper;
        private readonly GetCuentaBancariaByNumeroCuentaQueryHandler _handler;

        public CuentasBancariasQueriesTest()
        {
            _context = Create.MockedDbContextFor<BancaDbContext>();

            // Configuración de AutoMapper
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CuentaBancaria, CuentaBancariaDto>()
                    .ForMember(d => d.Cliente, opt => opt.MapFrom(s => new ClienteSimpleDto
                    {
                        Id = s.Cliente.Id,
                        Nombre = s.Cliente.Nombre
                    }))
                    .ForMember(d => d.UltimaTransaccion, opt => opt.MapFrom(s => s.Transacciones
                        .OrderByDescending(t => t.FechaTransaccion)
                        .Select(t => new TransaccionDtoSimple
                        {
                            Id = t.Id,
                            Monto = t.Monto,
                            FechaTransaccion = t.FechaTransaccion,
                            Descripcion = t.Descripcion,
                            TipoTransaccion = t.TipoTransaccion.ToString()
                        })
                        .FirstOrDefault()
                    ));
            });
            _mapper = configuration.CreateMapper();

            _handler = new GetCuentaBancariaByNumeroCuentaQueryHandler(_context, _mapper);
        }

        #region GetCuentaBancariaByNumeroCuenta Query Tests

        [Fact]
        public async Task Handle_CuentaExiste_RetornaCuentaBancariaDto()
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

            var numeroCuenta = "1234567890";
            var cuenta = new CuentaBancaria
            {
                Id = 1,
                NumeroCuenta = numeroCuenta,
                ClienteId = cliente.Id,
                Saldo = 1000.00m,
                Cliente = cliente
            };

            var transaccion = new Transaccion
            {
                Id = 1,
                CuentaId = cuenta.Id,
                TipoTransaccion = 1, // Depósito
                Monto = 1000.00m,
                SaldoPosterior = 1000.00m,
                Descripcion = "Depósito inicial",
                FechaTransaccion = DateTime.Now.AddDays(-1)
            };

            cuenta.Transacciones.Add(transaccion);

            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta);
            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            var query = new GetCuentaBancariaByNumeroCuentaQuery { NumeroCuenta = numeroCuenta };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(numeroCuenta, result.NumeroCuenta);
            Assert.Equal(1000.00m, result.Saldo);
            Assert.NotNull(result.Cliente);
            Assert.Equal(cliente.Id, result.Cliente.Id);
            Assert.Equal(cliente.Nombre, result.Cliente.Nombre);
            Assert.NotNull(result.UltimaTransaccion);
            Assert.Equal(transaccion.Id, result.UltimaTransaccion.Id);
            Assert.Equal(transaccion.Monto, result.UltimaTransaccion.Monto);
        }

        [Fact]
        public async Task Handle_CuentaConMultiplesTransacciones_RetornaUltimaTransaccion()
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

            var numeroCuenta = "0987654321";
            var cuenta = new CuentaBancaria
            {
                Id = 1,
                NumeroCuenta = numeroCuenta,
                ClienteId = cliente.Id,
                Saldo = 1500.00m,
                Cliente = cliente
            };

            var transaccion1 = new Transaccion
            {
                Id = 1,
                CuentaId = cuenta.Id,
                TipoTransaccion = 1, // Depósito
                Monto = 1000.00m,
                SaldoPosterior = 1000.00m,
                Descripcion = "Depósito inicial",
                FechaTransaccion = DateTime.Now.AddDays(-3)
            };

            var transaccion2 = new Transaccion
            {
                Id = 2,
                CuentaId = cuenta.Id,
                TipoTransaccion = 1, // Depósito
                Monto = 500.00m,
                SaldoPosterior = 1500.00m,
                Descripcion = "Segundo depósito",
                FechaTransaccion = DateTime.Now.AddDays(-1)
            };

            cuenta.Transacciones.Add(transaccion1);
            cuenta.Transacciones.Add(transaccion2);

            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta);
            _context.Transacciones.Add(transaccion1);
            _context.Transacciones.Add(transaccion2);
            await _context.SaveChangesAsync();

            var query = new GetCuentaBancariaByNumeroCuentaQuery { NumeroCuenta = numeroCuenta };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.UltimaTransaccion);
            Assert.Equal(transaccion2.Id, result.UltimaTransaccion.Id);
            Assert.Equal("Segundo depósito", result.UltimaTransaccion.Descripcion);
        }

        [Fact]
        public async Task Handle_CuentaNoExiste_LanzaNotFoundException()
        {
            // Arrange
            var numeroCuentaInexistente = "9999999999";
            var query = new GetCuentaBancariaByNumeroCuentaQuery { NumeroCuenta = numeroCuentaInexistente };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }

        #endregion
    }
}