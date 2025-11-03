using FluentValidation;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;

namespace InventoryBack.Application.Validators;

public class CreateProveedorDtoValidator : AbstractValidator<CreateProveedorDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProveedorDtoValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Identificacion)
            .NotEmpty().WithMessage("La identificación es requerida.")
            .MinimumLength(5).WithMessage("La identificación debe tener al menos 5 caracteres.")
            .MaximumLength(100).WithMessage("La identificación no puede exceder 100 caracteres.")
            .Matches(@"^[0-9\-]+$").WithMessage("La identificación solo puede contener números y guiones.")
            .MustAsync(BeUniqueIdentificacion)
            .WithMessage("Ya existe un proveedor con esta identificación.");

        RuleFor(x => x.Correo)
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
            .MaximumLength(150).WithMessage("El correo no puede exceder 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Correo));

        RuleFor(x => x.Observaciones)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden exceder 1000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }

    private async Task<bool> BeUniqueIdentificacion(string identificacion, CancellationToken ct)
    {
        var existing = await _unitOfWork.Proveedores.GetByIdentificacionAsync(identificacion, ct);
        return existing == null;
    }
}

public class UpdateProveedorDtoValidator : AbstractValidator<UpdateProveedorDto>
{
    public UpdateProveedorDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Correo)
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
            .MaximumLength(150).WithMessage("El correo no puede exceder 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Correo));

        RuleFor(x => x.Observaciones)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden exceder 1000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
}
