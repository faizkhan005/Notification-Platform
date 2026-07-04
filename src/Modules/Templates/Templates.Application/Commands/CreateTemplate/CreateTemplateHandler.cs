using BuildingBlocks.Application;
using System;
using System.Collections.Generic;
using System.Text;
using Templates.Domain;
using Tenants.Domain;

namespace Templates.Application.Commands.CreateTemplate;

public sealed class CreateTemplateHandler
    : ICommandHandler<CreateTemplateCommand, CreateTemplateResponse>
{
    private readonly ITemplateRepository _repository;
    private readonly ITemplatesUnitOfWork _unitOfWork;
    private readonly ITenantRepository _tenantRepository;

    public CreateTemplateHandler(
        ITemplateRepository repository,
        ITemplatesUnitOfWork unitOfWork,
        ITenantRepository tenantRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantRepository = tenantRepository;
    }

    public async Task<CreateTemplateResponse> Handle(
        CreateTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.From(command.TenantId);
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            throw new Templates.Domain.Exceptions.TemplateDomainException(
                $"Tenant '{command.TenantId}' not found.");

        var name = TemplateName.Create(command.Name);
        var content = TemplateContent.Create(command.Subject, command.Body);

        var template = Template.Create(tenantId, name, content);

        await _repository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTemplateResponse(
            template.Id.Value,
            template.Name.Value,
            template.Content.Subject,
            template.Content.Body,
            content.GetRequiredVariables().ToList(),
            template.CreatedAt);
    }
}

