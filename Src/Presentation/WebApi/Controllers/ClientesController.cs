using BancaCore.Application.Clientes.Commands.CreateCliente;
using BancaCore.Application.Clientes.Commands.UpdateCliente;
using BancaCore.Application.Clientes.Queries;
using BancaCore.Application.Clientes.Queries.GetAllClientes;
using BancaCore.Application.Clientes.Queries.GetClienteById;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaCore.WebApi.Controllers
{
    public class ClientesController : BaseController
    {

        /// <summary>
        /// Obtiene la lista de todos los clientes registrados.
        /// </summary>
        /// <returns>Una lista de objetos <see cref="ClienteDto"/> con la información de los clientes.</returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ClienteDto>>> GetAll()
        {
            return await Mediator.Send(new GetAllClientesQuery { });
        }

        /// <summary>
        /// Obtiene los detalles de un cliente específico por su identificador.
        /// </summary>
        /// <param name="id">El identificador único del cliente.</param>
        /// <returns>Un objeto <see cref="ClienteDto"/> con la información del cliente solicitado.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ClienteDto>> Get(int id)
        {
            return await Mediator.Send(new GetClienteByIdQuery { Id = id });
        }

        /// <summary>
        /// Crea un nuevo cliente sin crear una cuenta asociada.
        /// </summary>
        /// <param name="command">Datos del cliente a crear, incluyendo toda la información necesaria para la creación.</param>
        /// <returns>El identificador único del cliente recién creado.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Create(CreateClienteCommand command)
        {
            var clienteId = await Mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id = clienteId }, clienteId);
        }

        /// <summary>
        /// Actualiza un cliente existente
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <param name="command">Datos actualizados del cliente</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(int id, UpdateClienteCommand command)
        {
            command.Id = id;
            await Mediator.Send(command);
            return NoContent();
        }
    }
}