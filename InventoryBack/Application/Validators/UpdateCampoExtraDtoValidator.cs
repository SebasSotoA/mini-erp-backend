using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

public class UpdateCampoExtraDtoValidator : AbstractValidator<UpdateCampoExtraDto>
{
    public UpdateCampoExtraDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del campo extra es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.TipoDato)
            .NotEmpty().WithMessage("El tipo de dato es requerido.")
            .Must(CampoExtraValueValidator.IsValidTipoDato)
            .WithMessage("El tipo de dato debe ser uno de: Texto, Número, Número Decimal, Fecha, SI/No");

        RuleFor(x => x.ValorPorDefecto)
            .MaximumLength(500).WithMessage("El valor por defecto no puede exceder 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.ValorPorDefecto));

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        // Validación personalizada: El ValorPorDefecto debe coincidir con el TipoDato
        RuleFor(x => x)
            .Must(dto => ValidateValorPorDefectoMatchesTipoDato(dto, out _))
            .WithMessage(dto => 
            {
                ValidateValorPorDefectoMatchesTipoDato(dto, out var errorMessage);
                return errorMessage ?? "El valor por defecto no es compatible con el tipo de dato.";
            })
            .When(x => !string.IsNullOrEmpty(x.ValorPorDefecto));
    }

    private bool ValidateValorPorDefectoMatchesTipoDato(
        UpdateCampoExtraDto dto, 
        out string? errorMessage)
    {
        return CampoExtraValueValidator.IsValidValorPorDefecto(
            dto.TipoDato,
            dto.ValorPorDefecto,
            out errorMessage
        );
    }
}
