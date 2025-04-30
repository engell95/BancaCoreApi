using BancaCore.Application.Transacciones.Commands.RealizarDeposito;
using BancaCore.Application.Transacciones.Commands.RealizarRetiro;
using BancaCore.Application.Transacciones.Queries.GetTransaccionesSummary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BancaCore.WebApi.Controllers
{
    public class TransaccionesController : BaseController
    {
        /// <summary>
        /// Realiza un depósito en una cuenta existente.
        /// </summary>
        /// <param name="command">Comando que contiene el número de cuenta, el monto del depósito y una descripción.</param>
        /// <returns>El identificador de la transacción generada.</returns>
        [HttpPost("Deposito")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Deposito(RealizarDepositoCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Realiza un retiro desde una cuenta existente.
        /// </summary>
        /// <param name="command">Comando que contiene el número de cuenta, el monto del retiro y una descripción.</param>
        /// <returns>El identificador de la transacción generada.</returns>
        [HttpPost("Retiro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Retiro(RealizarRetiroCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Obtiene un resumen de todas las transacciones realizadas en una cuenta bancaria y calcula el saldo final.
        /// </summary>
        /// <param name="numeroCuenta">El número de cuenta bancaria para la cual se solicita el resumen.</param>
        /// <returns>Un resumen de transacciones que incluye todas las operaciones y el saldo final.</returns>
        [HttpGet("Resumen/{numeroCuenta}")]
        [ProducesResponseType(typeof(TransaccionesSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TransaccionesSummaryDto>> GetResumenTransacciones(string numeroCuenta)
        {
            return await Mediator.Send(new GetTransaccionesSummaryQuery { NumeroCuenta = numeroCuenta });
        }
    }
}