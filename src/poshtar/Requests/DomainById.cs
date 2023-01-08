using FluentValidation;
using MediatR;
using poshtar.Entities;
using poshtar.Models;

namespace poshtar.Requests;

public class DomainById : IRequest<DomainVM>
{
    public int Id { get; set; }
}
public class DomainByIdValidator : AbstractValidator<DomainById>
{
    public DomainByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty().NotEqual(0);
    }
}
public class DomainByIdHandler : IRequestHandler<DomainById, DomainVM>
{
    readonly ILogger<DomainByIdHandler> _logger;
    readonly AppDbContext _db;
    public DomainByIdHandler(ILogger<DomainByIdHandler> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    public async Task<DomainVM> Handle(DomainById request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        //return default!;
        return new DomainVM(new()
        {
            Name = "ica.hr",
            Host = "abcd.ica.hr",
            Port = 587,
            IsSecure = true,
            Username = "home",
            Password = "P@ssw0rd",
        });
    }
}