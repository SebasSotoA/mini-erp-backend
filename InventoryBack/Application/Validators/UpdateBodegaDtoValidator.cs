using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

public class UpdateBodegaDtoValidator : AbstractValidator<UpdateBodegaDto>
{
    public UpdateBodegaDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la bodega es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Direccion)
            .MaximumLength(500).WithMessage("La dirección no puede exceder 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Direccion));

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));
    }
}
