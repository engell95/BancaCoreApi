using BancaCore.Application.CuentasBancarias.Commands.CreateCuentaBancaria;
using BancaCore.Application.CuentasBancarias.Queries;
using BancaCore.Application.CuentasBancarias.Queries.GetCuentaBancariaByNumeroCuenta;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BancaCore.WebApi.Controllers
{
    public class CuentasBancariasController : BaseController
    {

        /// <summary>
        /// Obtiene los detalles de una cuenta bancaria específica por su número de cuenta.
        /// </summary>
        /// <param name="numeroCuenta">El número único de la cuenta bancaria.</param>
        /// <returns>Un objeto <see cref="CuentaBancariaDto"/> con la información de la cuenta bancaria solicitada.</returns>
        [HttpGet("{numeroCuenta}")]
        [ProducesResponseType(typeof(CuentaBancariaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CuentaBancariaDto>> GetByNumeroCuenta(string numeroCuenta)
        {
            return await Mediator.Send(new GetCuentaBancariaByNumeroCuentaQuery { NumeroCuenta = numeroCuenta });
        }

        /// <summary>
        /// Crea una nueva cuenta con un deposito inicial.
        /// </summary>
        /// <param name="command">Datos de la cuenta a crear, incluyendo toda la información necesaria para la creación.</param>
        /// <returns>El identificador único de la cuenta recién creado.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Create(CreateCuentaBancariaCommand command)
        {
            var numeroCuenta = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetByNumeroCuenta), new { numeroCuenta }, numeroCuenta);
        }


    }
}