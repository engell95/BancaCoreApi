using AutoMapper;
using BancaCore.Application.Common.Enumerable;
using BancaCore.Common.Mappings;
using BancaCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BancaCore.Application.CuentasBancarias.Queries
{
    public class CuentaBancariaDto : IMapFrom<CuentaBancaria>
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Saldo { get; set; }
        public ClienteSimpleDto Cliente { get; set; }
        public IList<TransaccionDtoSimple> Transacciones { get; set; } = new List<TransaccionDtoSimple>();
        public void Mapping(Profile profile)
        {
            profile.CreateMap<CuentaBancaria, CuentaBancariaDto>()
                .ForMember(d => d.Cliente, opt => opt.MapFrom(s => new ClienteSimpleDto
                {
                    Id = s.Cliente.Id,
                    Nombre = s.Cliente.Nombre
                }))
                .ForMember(d => d.Transacciones, opt => opt.MapFrom(s => s.Transacciones.Select(t => new TransaccionDtoSimple
                {
                    Id = t.Id,
                    Monto = t.Monto,
                    FechaTransaccion = t.FechaTransaccion,
                    Descripcion = t.Descripcion,
                    TipoTransaccion = ((EnumTipoTransaccion)t.TipoTransaccion).ToString()
                })));
        }
    }

    public class ClienteSimpleDto 
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class TransaccionDtoSimple
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string TipoTransaccion { get; set; }
        public string Descripcion { get; set; }
    }

}
