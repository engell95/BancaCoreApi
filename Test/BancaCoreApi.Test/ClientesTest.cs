using AutoMapper;
using BancaCore.Application.Clientes.Queries;
using BancaCore.Application.Clientes.Queries.GetClienteById;
using BancaCore.Common.Exceptions;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using BancaCore.Persistence;
using EntityFrameworkCore.Testing.Moq;

namespace BancaCoreApi.Test
{
    /// <summary>
    /// Pruebas unitarias para la entidad Cliente y sus operaciones relacionadas
    /// </summary>
    public class ClientesTest
    {
        private readonly IBancaDbContext _context;
        private readonly IMapper _mapper;
        private readonly GetClienteByIdQueryHandler _handler;

        public ClientesTest()
        {
            _context = Create.MockedDbContextFor<BancaDbContext>();

            // Configuración real de AutoMapper
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Cliente, ClienteDto>()
                    .ForMember(d => d.CuentasBancarias, opt => opt.MapFrom(s => s.CuentasBancaria));
                cfg.CreateMap<CuentaBancaria, CuentasDto>();
            });
            _mapper = configuration.CreateMapper();

            _handler = new GetClienteByIdQueryHandler(_context, _mapper);
        }

        #region Pruebas de Entidad Cliente

        [Fact]
        public void Cliente_CreacionConPropiedadesValidas_CreacionExitosa()
        {
            // Arrange & Act
            var fechaNacimiento = new DateOnly(1990, 5, 15);
            var cliente = new Cliente
            {
                Id = 1,
                Nombre = "Juan Pérez",
                FechaNacimiento = fechaNacimiento,
                Sexo = "M",
                Ingresos = 5000.00m
            };

            // Assert
            Assert.Equal(1, cliente.Id);
            Assert.Equal("Juan Pérez", cliente.Nombre);
            Assert.Equal(fechaNacimiento, cliente.FechaNacimiento);
            Assert.Equal("M", cliente.Sexo);
            Assert.Equal(5000.00m, cliente.Ingresos);
            Assert.Empty(cliente.CuentasBancaria);
        }

        [Fact]
        public void Cliente_RelacionConCuentasBancarias_RelacionCorrecta()
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

            var cuenta1 = new CuentaBancaria
            {
                Id = 1,
                NumeroCuenta = "1234567890",
                ClienteId = 1,
                Saldo = 1000.00m,
                Cliente = cliente
            };

            var cuenta2 = new CuentaBancaria
            {
                Id = 2,
                NumeroCuenta = "0987654321",
                ClienteId = 1,
                Saldo = 2500.00m,
                Cliente = cliente
            };

            // Act
            cliente.CuentasBancaria.Add(cuenta1);
            cliente.CuentasBancaria.Add(cuenta2);

            // Assert
            Assert.Equal(2, cliente.CuentasBancaria.Count);
            Assert.Contains(cuenta1, cliente.CuentasBancaria);
            Assert.Contains(cuenta2, cliente.CuentasBancaria);
            Assert.All(cliente.CuentasBancaria, cuenta => Assert.Equal(cliente.Id, cuenta.ClienteId));
        }

        #endregion

        #region Pruebas de CRUD para Cliente

        [Fact]
        public async Task CrearCliente_DatosValidos_ClienteCreado()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nombre = "Carlos Rodríguez",
                FechaNacimiento = new DateOnly(1978, 8, 25),
                Sexo = "M",
                Ingresos = 7800.00m
            };

            // Act
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            // Assert
            Assert.NotEqual(0, cliente.Id); // El ID debería ser asignado por el contexto
            var clienteGuardado = await _context.Clientes.FindAsync(cliente.Id);
            Assert.NotNull(clienteGuardado);
            Assert.Equal("Carlos Rodríguez", clienteGuardado.Nombre);
        }

        [Fact]
        public async Task ActualizarCliente_DatosValidos_ClienteActualizado()
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

            // Act
            cliente.Nombre = "Ana María Martínez";
            cliente.Ingresos = 5000.00m;
            await _context.SaveChangesAsync();

            // Assert
            var clienteActualizado = await _context.Clientes.FindAsync(cliente.Id);
            Assert.Equal("Ana María Martínez", clienteActualizado.Nombre);
            Assert.Equal(5000.00m, clienteActualizado.Ingresos);
        }

        [Fact]
        public async Task EliminarCliente_ClienteExistente_ClienteEliminado()
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
            var clienteId = cliente.Id;

            // Act
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            // Assert
            var clienteEliminado = await _context.Clientes.FindAsync(clienteId);
            Assert.Null(clienteEliminado);
        }

        #endregion

        #region Pruebas de Consulta GetClienteById

        [Fact]
        public async Task Handle_ClienteExiste_RetornaClienteDto()
        {
            // Arrange
            var clienteId = 1;
            var fechaNacimiento = new DateOnly(1990, 5, 15);
            var cliente = new Cliente
            {
                Id = clienteId,
                Nombre = "Juan Pérez",
                FechaNacimiento = fechaNacimiento,
                Sexo = "M",
                Ingresos = 5000.00m
            };

            // Agregar datos al contexto simulado
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var query = new GetClienteByIdQuery { Id = clienteId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(clienteId, result.Id);
            Assert.Equal("Juan Pérez", result.Nombre);
            Assert.Equal(fechaNacimiento, result.FechaNacimiento);
            Assert.Equal("M", result.Sexo);
            Assert.Equal(5000.00m, result.Ingresos);
        }

        [Fact]
        public async Task Handle_ClienteConCuentas_RetornaClienteDtoConCuentas()
        {
            // Arrange
            var clienteId = 2;
            var cliente = new Cliente
            {
                Id = clienteId,
                Nombre = "María López",
                FechaNacimiento = new DateOnly(1985, 3, 20),
                Sexo = "F",
                Ingresos = 6500.00m
            };

            var cuenta1 = new CuentaBancaria
            {
                Id = 1,
                NumeroCuenta = "1234567890",
                ClienteId = clienteId,
                Saldo = 1000.00m,
                Cliente = cliente
            };

            var cuenta2 = new CuentaBancaria
            {
                Id = 2,
                NumeroCuenta = "0987654321",
                ClienteId = clienteId,
                Saldo = 2500.00m,
                Cliente = cliente
            };

            cliente.CuentasBancaria.Add(cuenta1);
            cliente.CuentasBancaria.Add(cuenta2);

            // Agregar datos al contexto simulado
            _context.Clientes.Add(cliente);
            _context.CuentasBancarias.Add(cuenta1);
            _context.CuentasBancarias.Add(cuenta2);
            await _context.SaveChangesAsync();

            var query = new GetClienteByIdQuery { Id = clienteId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(clienteId, result.Id);
            Assert.Equal("María López", result.Nombre);
            Assert.Equal(2, result.CuentasBancarias.Count);
            Assert.Contains(result.CuentasBancarias, c => c.NumeroCuenta == "1234567890");
            Assert.Contains(result.CuentasBancarias, c => c.NumeroCuenta == "0987654321");
        }

        [Fact]
        public async Task Handle_ClienteNoExiste_LanzaNotFoundException()
        {
            // Arrange
            var clienteId = 999; // ID que no existe
            var query = new GetClienteByIdQuery { Id = clienteId };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }

        #endregion
    }
}