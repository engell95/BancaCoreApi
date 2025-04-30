using AutoMapper;
using BancaCore.Application.Clientes.Queries;
using BancaCore.Application.Clientes.Queries.GetAllClientes;
using BancaCore.Application.Clientes.Queries.GetClienteById;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using BancaCore.Persistence;
using EntityFrameworkCore.Testing.Moq;

namespace BancaCoreApi.Test
{
    /// <summary>
    /// Pruebas unitarias para las consultas de la entidad Cliente
    /// </summary>
    public class ClientesQueriesTest
    {
        private readonly IBancaDbContext _context;
        private readonly IMapper _mapper;

        public ClientesQueriesTest()
        {
            _context = Create.MockedDbContextFor<BancaDbContext>();

            // Configuración de AutoMapper
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Cliente, ClienteDto>()
                    .ForMember(d => d.CuentasBancarias, opt => opt.MapFrom(s => s.CuentasBancaria));
                cfg.CreateMap<CuentaBancaria, CuentasDto>();
            });
            _mapper = configuration.CreateMapper();
        }

        #region GetAllClientes Query Tests

        [Fact]
        public async Task Handle_GetAllClientes_RetornaListaClientes()
        {
            // Arrange
            var clientes = new List<Cliente>
            {
                new Cliente
                {
                    Nombre = "Juan Pérez",
                    FechaNacimiento = new DateOnly(1990, 5, 15),
                    Sexo = "M",
                    Ingresos = 5000.00m
                },
                new Cliente
                {
                    Nombre = "María López",
                    FechaNacimiento = new DateOnly(1985, 3, 20),
                    Sexo = "F",
                    Ingresos = 6500.00m
                },
                new Cliente
                {
                    Nombre = "Carlos Rodríguez",
                    FechaNacimiento = new DateOnly(1978, 8, 25),
                    Sexo = "M",
                    Ingresos = 7800.00m
                }
            };

            _context.Clientes.AddRange(clientes);
            await _context.SaveChangesAsync();

            var query = new GetAllClientesQuery();
            var handler = new GetAllClientesQueryHandler(_context, _mapper);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, c => c.Nombre == "Juan Pérez");
            Assert.Contains(result, c => c.Nombre == "María López");
            Assert.Contains(result, c => c.Nombre == "Carlos Rodríguez");
        }

        [Fact]
        public async Task Handle_GetAllClientes_SinClientes_RetornaListaVacia()
        {
            // Arrange
            var query = new GetAllClientesQuery();
            var handler = new GetAllClientesQueryHandler(_context, _mapper);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_GetAllClientes_OrdenadosPorIdDescendente()
        {
            // Arrange
            var cliente1 = new Cliente
            {
                Nombre = "Juan Pérez",
                FechaNacimiento = new DateOnly(1990, 5, 15),
                Sexo = "M",
                Ingresos = 5000.00m
            };

            var cliente2 = new Cliente
            {
                Nombre = "María López",
                FechaNacimiento = new DateOnly(1985, 3, 20),
                Sexo = "F",
                Ingresos = 6500.00m
            };

            var cliente3 = new Cliente
            {
                Nombre = "Carlos Rodríguez",
                FechaNacimiento = new DateOnly(1978, 8, 25),
                Sexo = "M",
                Ingresos = 7800.00m
            };

            _context.Clientes.Add(cliente1);
            await _context.SaveChangesAsync();
            _context.Clientes.Add(cliente2);
            await _context.SaveChangesAsync();
            _context.Clientes.Add(cliente3);
            await _context.SaveChangesAsync();

            var query = new GetAllClientesQuery();
            var handler = new GetAllClientesQueryHandler(_context, _mapper);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            
            // Verificar que estén ordenados por Id descendente
            Assert.Equal(cliente3.Id, result[0].Id);
            Assert.Equal(cliente2.Id, result[1].Id);
            Assert.Equal(cliente1.Id, result[2].Id);
        }

        [Fact]
        public async Task Handle_GetAllClientes_ClientesConCuentas_IncluirCuentas()
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

            var cuenta1 = new CuentaBancaria
            {
                NumeroCuenta = "1111222233",
                ClienteId = cliente.Id,
                Saldo = 2500.00m,
                Cliente = cliente
            };

            var cuenta2 = new CuentaBancaria
            {
                NumeroCuenta = "4444555566",
                ClienteId = cliente.Id,
                Saldo = 3800.00m,
                Cliente = cliente
            };

            cliente.CuentasBancaria.Add(cuenta1);
            cliente.CuentasBancaria.Add(cuenta2);
            _context.CuentasBancarias.Add(cuenta1);
            _context.CuentasBancarias.Add(cuenta2);
            await _context.SaveChangesAsync();

            var query = new GetAllClientesQuery();
            var handler = new GetAllClientesQueryHandler(_context, _mapper);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            
            var clienteDto = result.First();
            Assert.Equal(cliente.Id, clienteDto.Id);
            Assert.Equal(cliente.Nombre, clienteDto.Nombre);
            Assert.Equal(2, clienteDto.CuentasBancarias.Count);
            Assert.Contains(clienteDto.CuentasBancarias, c => c.NumeroCuenta == "1111222233");
            Assert.Contains(clienteDto.CuentasBancarias, c => c.NumeroCuenta == "4444555566");
        }

        #endregion

        #region GetClienteById Query Tests

        [Fact]
        public async Task Handle_GetClienteById_RetornaClienteExistente()
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

            var query = new GetClienteByIdQuery { Id = cliente.Id };
            var handler = new GetClienteByIdQueryHandler(_context, _mapper);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cliente.Id, result.Id);
            Assert.Equal(cliente.Nombre, result.Nombre);
            Assert.Equal(cliente.FechaNacimiento, result.FechaNacimiento);
            Assert.Equal(cliente.Sexo, result.Sexo);
            Assert.Equal(cliente.Ingresos, result.Ingresos);
        }

        #endregion
    }
}