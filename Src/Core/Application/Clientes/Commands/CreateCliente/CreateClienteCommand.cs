using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BancaCore.Application.Clientes.Commands.CreateCliente
{
    public class CreateClienteCommand : IRequest<int>
    {
        public string Nombre { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public decimal Ingresos { get; set; }
    }

    /// <summary>
    /// Manejador del comando CreateClienteCommand 
    /// Implementa la lógica de negocio para crear un nuevo cliente
    /// </summary>
    public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, int>
    {
        private readonly IBancaDbContext _context;

        public CreateClienteCommandHandler(IBancaDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
        {
            var entity = new Cliente
            {
                Nombre = request.Nombre,
                FechaNacimiento = request.FechaNacimiento,
                Sexo = request.Sexo,
                Ingresos = request.Ingresos
            };

            _context.Clientes.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}