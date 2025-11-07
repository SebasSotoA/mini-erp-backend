using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

public class CreateVendedorDtoValidator : AbstractValidator<CreateVendedorDto>
{
    public CreateVendedorDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Identificacion)
            .NotEmpty().WithMessage("La identificación es requerida.")
            .MinimumLength(5).WithMessage("La identificación debe tener al menos 5 caracteres.")
            .MaximumLength(100).WithMessage("La identificación no puede exceder 100 caracteres.")
            .Matches(@"^[0-9\-]+$").WithMessage("La identificación solo puede contener números y guiones.");

        RuleFor(x => x.Correo)
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
            .MaximumLength(150).WithMessage("El correo no puede exceder 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Correo));

        RuleFor(x => x.Observaciones)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden exceder 1000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
}

public class UpdateVendedorDtoValidator : AbstractValidator<UpdateVendedorDto>
{
    public UpdateVendedorDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Identificacion)
            .NotEmpty().WithMessage("La identificación es requerida.")
            .MinimumLength(5).WithMessage("La identificación debe tener al menos 5 caracteres.")
            .MaximumLength(100).WithMessage("La identificación no puede exceder 100 caracteres.")
            .Matches(@"^[0-9\-]+$").WithMessage("La identificación solo puede contener números y guiones.");

        RuleFor(x => x.Correo)
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
            .MaximumLength(150).WithMessage("El correo no puede exceder 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Correo));

        RuleFor(x => x.Observaciones)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden exceder 1000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
}
