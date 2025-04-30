using BancaCore.Common.Exceptions;
using BancaCore.Common.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BancaCore.Application.Clientes.Commands.UpdateCliente
{
    public class UpdateClienteCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public decimal Ingresos { get; set; }
    }

    /// <summary>
    /// Manejador del comando UpdateClienteCommand 
    /// Implementa la lógica de negocio para editar un cliente
    /// </summary>
    public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, int>
    {
        private readonly IBancaDbContext _context;

        public UpdateClienteCommandHandler(IBancaDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Clientes
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Clientes), request.Id);
            }

            entity.Nombre = request.Nombre;
            entity.FechaNacimiento = request.FechaNacimiento;
            entity.Sexo = request.Sexo;
            entity.Ingresos = request.Ingresos;

            await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }
}