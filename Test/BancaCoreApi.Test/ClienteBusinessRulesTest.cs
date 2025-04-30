using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using BancaCore.Persistence;
using EntityFrameworkCore.Testing.Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BancaCoreApi.Test
{
    /// <summary>
    /// Pruebas unitarias para validar las reglas de negocio relacionadas con la entidad Cliente
    /// </summary>
    public class ClienteBusinessRulesTest
    {
        private readonly IBancaDbContext _context;

        public ClienteBusinessRulesTest()
        {
            // Configuración del contexto simulado
            _context = Create.MockedDbContextFor<BancaDbContext>();
        }

        [Fact]
        public async Task Cliente_CalcularEdad_EdadCorrecta()
        {
            // Arrange
            var fechaNacimiento = new DateOnly(1990, 5, 15);
            var cliente = new Cliente
            {
                Nombre = "Eduardo Mendoza",
                FechaNacimiento = fechaNacimiento,
                Sexo = "M",
                Ingresos = 6800.00m
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            // Act
            var edad = CalcularEdad(fechaNacimiento);

            // Assert
            var edadEsperada = DateTime.Now.Year - 1990;
            if (DateTime.Now.Month < 5 || (DateTime.Now.Month == 5 && DateTime.Now.Day < 15))
            {
                edadEsperada--;
            }
            Assert.Equal(edadEsperada, edad);
        }

        [Fact]
        public async Task Cliente_VerificarLimiteCredito_LimiteCreditoCorrecto()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nombre = "Patricia Vega",
                FechaNacimiento = new DateOnly(1983, 9, 22),
                Sexo = "F",
                Ingresos = 7500.00m
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            // Act
            var limiteCredito = CalcularLimiteCredito(cliente.Ingresos);

            // Assert
            // Suponiendo que el límite de crédito es 5 veces los ingresos
            Assert.Equal(37500.00m, limiteCredito);
        }

        [Fact]
        public async Task Cliente_VerificarSegmentoCliente_SegmentoCorrecto()
        {
            // Arrange
            var clienteA = new Cliente
            {
                Nombre = "Ricardo Gómez",
                FechaNacimiento = new DateOnly(1970, 3, 10),
                Sexo = "M",
                Ingresos = 12000.00m
            };

            var clienteB = new Cliente
            {
                Nombre = "Lucía Martínez",
                FechaNacimiento = new DateOnly(1992, 7, 18),
                Sexo = "F",
                Ingresos = 4500.00m
            };

            var clienteC = new Cliente
            {
                Nombre = "Fernando Díaz",
                FechaNacimiento = new DateOnly(1985, 11, 30),
                Sexo = "M",
                Ingresos = 2800.00m
            };

            _context.Clientes.Add(clienteA);
            _context.Clientes.Add(clienteB);
            _context.Clientes.Add(clienteC);
            await _context.SaveChangesAsync();

            // Act
            var segmentoA = DeterminarSegmentoCliente(clienteA.Ingresos);
            var segmentoB = DeterminarSegmentoCliente(clienteB.Ingresos);
            var segmentoC = DeterminarSegmentoCliente(clienteC.Ingresos);

            // Assert
            Assert.Equal("Premium", segmentoA);
            Assert.Equal("Estándar", segmentoB);
            Assert.Equal("Básico", segmentoC);
        }

        [Fact]
        public async Task Cliente_VerificarSaldoTotalCuentas_SaldoCorrecto()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nombre = "Gabriela Flores",
                FechaNacimiento = new DateOnly(1988, 6, 14),
                Sexo = "F",
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

            var cuenta3 = new CuentaBancaria
            {
                NumeroCuenta = "7777888899",
                ClienteId = cliente.Id,
                Saldo = 1200.00m,
                Cliente = cliente
            };

            cliente.CuentasBancaria.Add(cuenta1);
            cliente.CuentasBancaria.Add(cuenta2);
            cliente.CuentasBancaria.Add(cuenta3);
            _context.CuentasBancarias.Add(cuenta1);
            _context.CuentasBancarias.Add(cuenta2);
            _context.CuentasBancarias.Add(cuenta3);
            await _context.SaveChangesAsync();

            // Act
            var saldoTotal = CalcularSaldoTotalCuentas(cliente);

            // Assert
            Assert.Equal(7500.00m, saldoTotal);
        }

        [Fact]
        public async Task Cliente_VerificarElegibilidadCredito_ElegibilidadCorrecta()
        {
            // Arrange
            var clienteElegible = new Cliente
            {
                Nombre = "Javier Ruiz",
                FechaNacimiento = new DateOnly(1980, 2, 25),
                Sexo = "M",
                Ingresos = 8500.00m
            };

            var clienteNoElegible = new Cliente
            {
                Nombre = "Carmen Ortiz",
                FechaNacimiento = new DateOnly(1995, 8, 12),
                Sexo = "F",
                Ingresos = 1800.00m
            };

            _context.Clientes.Add(clienteElegible);
            _context.Clientes.Add(clienteNoElegible);
            await _context.SaveChangesAsync();

            // Act
            var elegibilidadA = VerificarElegibilidadCredito(clienteElegible);
            var elegibilidadB = VerificarElegibilidadCredito(clienteNoElegible);

            // Assert
            Assert.True(elegibilidadA);
            Assert.False(elegibilidadB);
        }

        #region Métodos auxiliares para simular lógica de negocio

        private int CalcularEdad(DateOnly fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;
            
            // Ajustar la edad si aún no ha llegado el cumpleaños este año
            if (new DateOnly(hoy.Year, fechaNacimiento.Month, fechaNacimiento.Day) > DateOnly.FromDateTime(hoy))
            {
                edad--;
            }
            
            return edad;
        }

        private decimal CalcularLimiteCredito(decimal ingresos)
        {
            // Simulación: el límite de crédito es 5 veces los ingresos mensuales
            return ingresos * 5;
        }

        private string DeterminarSegmentoCliente(decimal ingresos)
        {
            if (ingresos >= 10000)
                return "Premium";
            else if (ingresos >= 3000)
                return "Estándar";
            else
                return "Básico";
        }

        private decimal CalcularSaldoTotalCuentas(Cliente cliente)
        {
            return cliente.CuentasBancaria.Sum(c => c.Saldo);
        }

        private bool VerificarElegibilidadCredito(Cliente cliente)
        {
            // Simulación: un cliente es elegible para crédito si sus ingresos son mayores a 3000
            return cliente.Ingresos > 3000;
        }

        #endregion
    }
}