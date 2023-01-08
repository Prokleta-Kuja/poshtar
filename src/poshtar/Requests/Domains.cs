using FluentValidation;
using MediatR;
using poshtar.Entities;
using poshtar.Models;

namespace poshtar.Requests;

public class Domains : IRequest<List<DomainVM>> { }
public class DomainsValidator : AbstractValidator<Domains> { }
public class DomainsHandler : IRequestHandler<Domains, List<DomainVM>>
{
    readonly ILogger<DomainsHandler> _logger;
    readonly AppDbContext _db;
    public DomainsHandler(ILogger<DomainsHandler> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    public async Task<List<DomainVM>> Handle(Domains request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        // return default!;
        return new(){
            new DomainVM(new ()
            {
                Name = "ica.hr",
                Host = "abcd.ica.hr",
                Port = 587,
                IsSecure = true,
                Username = "home",
                Password = "P@ssw0rd",
            }),
            new DomainVM(new ()
            {
                Name = "kica.hr",
                Host = "abcd.kica.hr",
                Port = 587,
                IsSecure = true,
                Username = "home",
                Password = "P@ssw0rd",
            }),
        };
    }
}