using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

public class CreateCampoExtraDtoValidator : AbstractValidator<CreateCampoExtraDto>
{
    private static readonly string[] TiposDatoPermitidos = { "Texto", "Número", "Número Decimal", "Fecha", "SI/No" };

    public CreateCampoExtraDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del campo extra es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.TipoDato)
            .NotEmpty().WithMessage("El tipo de dato es requerido.")
            .Must(BeAValidTipoDato).WithMessage($"El tipo de dato debe ser uno de: {string.Join(", ", TiposDatoPermitidos)}");

        RuleFor(x => x.ValorPorDefecto)
            .MaximumLength(500).WithMessage("El valor por defecto no puede exceder 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.ValorPorDefecto));

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));
    }

    private bool BeAValidTipoDato(string tipoDato)
    {
        return TiposDatoPermitidos.Contains(tipoDato, StringComparer.OrdinalIgnoreCase);
    }
}
