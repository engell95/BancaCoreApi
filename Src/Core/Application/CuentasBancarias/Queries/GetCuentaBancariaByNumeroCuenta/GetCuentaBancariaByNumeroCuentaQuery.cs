using MediatR;

namespace BancaCore.Application.CuentasBancarias.Queries.GetCuentaBancariaByNumeroCuenta
{
    public class GetCuentaBancariaByNumeroCuentaQuery : IRequest<CuentaBancariaDto>
    {
        public string NumeroCuenta { get; set; }
    }
}
