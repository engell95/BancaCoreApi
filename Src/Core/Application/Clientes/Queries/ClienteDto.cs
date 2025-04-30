using AutoMapper;
using BancaCore.Common.Mappings;
using BancaCore.Domain.Entities;
using System;
using System.Collections.Generic;

namespace BancaCore.Application.Clientes.Queries
{
    public class ClienteDto : IMapFrom<Cliente>
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public decimal Ingresos { get; set; }
        public IList<CuentasDto> CuentasBancarias { get; set; } = new List<CuentasDto>();
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Cliente, ClienteDto>()
                .ForMember(d => d.CuentasBancarias, opt => opt.MapFrom(s => s.CuentasBancaria));
        }
    }

    public class CuentasDto : IMapFrom<CuentaBancaria>
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Saldo { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CuentaBancaria, CuentasDto>();
        }
    }
}