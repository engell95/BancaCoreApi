using MediatR;

namespace BancaCore.Application.CuentasBancarias.Queries.GetCuentaBancariaById
{
    public class GetCuentaBancariaByIdQuery : IRequest<CuentaBancariaDto>
    {
        public string NumeroCuenta { get; set; }
    }
}
