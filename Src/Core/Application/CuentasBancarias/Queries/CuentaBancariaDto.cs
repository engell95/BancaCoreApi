using AutoMapper;
using BancaCore.Common.Enumerable;
using BancaCore.Common.Mappings;
using BancaCore.Domain.Entities;
using System;
using System.Linq;

namespace BancaCore.Application.CuentasBancarias.Queries
{
    /// <summary>
    /// DTO que representa una cuenta bancaria con información del cliente y la última transacción realizada.
    /// </summary>
    public class CuentaBancariaDto : IMapFrom<CuentaBancaria>
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
        /// Información básica del cliente propietario de la cuenta.
        /// </summary>
        public ClienteSimpleDto Cliente { get; set; }

        /// <summary>
        /// Última transacción realizada en la cuenta bancaria.
        /// </summary>
        public TransaccionDtoSimple UltimaTransaccion { get; set; }

        /// <summary>
        /// Configura el mapeo de propiedades desde la entidad <see cref="CuentaBancaria"/>.
        /// </summary>
        /// <param name="profile">Perfil de AutoMapper donde se define el mapeo.</param>
        public void Mapping(Profile profile)
        {
            profile.CreateMap<CuentaBancaria, CuentaBancariaDto>()
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
                        TipoTransaccion = ((EnumTipoTransaccion)t.TipoTransaccion).ToString()
                    })
                    .FirstOrDefault()
                ));
        }
    }

    /// <summary>
    /// DTO simplificado que representa la información básica de un cliente.
    /// </summary>
    public class ClienteSimpleDto
    {
        /// <summary>
        /// Identificador único del cliente.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del cliente.
        /// </summary>
        public string Nombre { get; set; }
    }

    /// <summary>
    /// DTO simplificado que representa los datos esenciales de una transacción.
    /// </summary>
    public class TransaccionDtoSimple
    {
        /// <summary>
        /// Identificador único de la transacción.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Monto de la transacción.
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó la transacción.
        /// </summary>
        public DateTime FechaTransaccion { get; set; }

        /// <summary>
        /// Tipo de transacción (Depósito o Retiro).
        /// </summary>
        public string TipoTransaccion { get; set; }

        /// <summary>
        /// Descripción asociada a la transacción.
        /// </summary>
        public string Descripcion { get; set; }
    }
}
