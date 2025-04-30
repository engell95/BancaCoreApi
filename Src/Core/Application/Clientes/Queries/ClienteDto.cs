using AutoMapper;
using BancaCore.Common.Mappings;
using BancaCore.Domain.Entities;
using System;
using System.Collections.Generic;

namespace BancaCore.Application.Clientes.Queries
{
    /// <summary>
    /// DTO que representa la información detallada de un cliente, incluyendo sus cuentas bancarias.
    /// </summary>
    public class ClienteDto : IMapFrom<Cliente>
    {
        /// <summary>
        /// Identificador único del cliente.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre completo del cliente.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Fecha de nacimiento del cliente.
        /// </summary>
        public DateOnly FechaNacimiento { get; set; }

        /// <summary>
        /// Sexo del cliente (por ejemplo, Masculino o Femenino).
        /// </summary>
        public string Sexo { get; set; }

        /// <summary>
        /// Ingresos mensuales o anuales del cliente.
        /// </summary>
        public decimal Ingresos { get; set; }

        /// <summary>
        /// Lista de cuentas bancarias asociadas al cliente.
        /// </summary>
        public IList<CuentasDto> CuentasBancarias { get; set; } = new List<CuentasDto>();

        /// <summary>
        /// Configura el mapeo de propiedades desde la entidad <see cref="Cliente"/>.
        /// </summary>
        /// <param name="profile">Perfil de AutoMapper donde se define el mapeo.</param>
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Cliente, ClienteDto>()
                .ForMember(d => d.CuentasBancarias, opt => opt.MapFrom(s => s.CuentasBancaria));
        }
    }

    /// <summary>
    /// DTO que representa una cuenta bancaria básica asociada a un cliente.
    /// </summary>
    public class CuentasDto : IMapFrom<CuentaBancaria>
    {
        /// <summary>
        /// Identificador único de la cuenta bancaria.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número de cuenta bancaria.
        /// </summary>
        public string NumeroCuenta { get; set; }

        /// <summary>
        /// Saldo actual de la cuenta bancaria.
        /// </summary>
        public decimal Saldo { get; set; }

        /// <summary>
        /// Configura el mapeo de propiedades desde la entidad <see cref="CuentaBancaria"/>.
        /// </summary>
        /// <param name="profile">Perfil de AutoMapper donde se define el mapeo.</param>
        public void Mapping(Profile profile)
        {
            profile.CreateMap<CuentaBancaria, CuentasDto>();
        }
    }
}
