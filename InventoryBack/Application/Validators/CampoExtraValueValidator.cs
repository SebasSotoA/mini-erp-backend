namespace InventoryBack.Application.Validators;

/// <summary>
/// Validator for CampoExtra ValorPorDefecto based on TipoDato.
/// Ensures that the default value matches the data type.
/// </summary>
public static class CampoExtraValueValidator
{
    private static readonly string[] ValidTiposDato = 
    {
        "Texto",
        "Número",
        "Número Decimal",
        "Fecha",
        "SI/No"
    };

    /// <summary>
    /// Validates if the TipoDato is valid.
    /// </summary>
    public static bool IsValidTipoDato(string tipoDato)
    {
        return ValidTiposDato.Contains(tipoDato, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if the ValorPorDefecto is compatible with the TipoDato.
    /// </summary>
    /// <param name="tipoDato">Data type (Texto, Número, Número Decimal, Fecha, SI/No)</param>
    /// <param name="valorPorDefecto">Default value to validate</param>
    /// <param name="errorMessage">Output error message if validation fails</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidValorPorDefecto(
        string tipoDato, 
        string? valorPorDefecto, 
        out string? errorMessage)
    {
        errorMessage = null;

        // Empty value is always valid (optional default value)
        if (string.IsNullOrWhiteSpace(valorPorDefecto))
        {
            return true;
        }

        var valor = valorPorDefecto.Trim();

        return tipoDato.ToUpper() switch
        {
            "TEXTO" => ValidateTexto(valor, out errorMessage),
            "NÚMERO" => ValidateNumero(valor, out errorMessage),
            "NÚMERO DECIMAL" => ValidateNumeroDecimal(valor, out errorMessage),
            "FECHA" => ValidateFecha(valor, out errorMessage),
            "SI/NO" => ValidateSiNo(valor, out errorMessage),
            _ => SetError(out errorMessage, $"Tipo de dato '{tipoDato}' no válido.")
        };
    }

    private static bool ValidateTexto(string valor, out string? errorMessage)
    {
        errorMessage = null;
        
        // Text type: any string is valid, but enforce max length
        if (valor.Length > 500)
        {
            errorMessage = "El valor por defecto para tipo 'Texto' no puede exceder 500 caracteres.";
            return false;
        }

        return true;
    }

    private static bool ValidateNumero(string valor, out string? errorMessage)
    {
        errorMessage = null;

        // Must be a valid integer
        if (!int.TryParse(valor, out _))
        {
            errorMessage = "El valor por defecto para tipo 'Número' debe ser un número entero válido (ej: 0, 10, -5).";
            return false;
        }

        return true;
    }

    private static bool ValidateNumeroDecimal(string valor, out string? errorMessage)
    {
        errorMessage = null;

        // Must be a valid decimal (support both . and , as decimal separator)
        var normalizedValue = valor.Replace(',', '.');
        
        if (!decimal.TryParse(normalizedValue, System.Globalization.NumberStyles.Any, 
            System.Globalization.CultureInfo.InvariantCulture, out _))
        {
            errorMessage = "El valor por defecto para tipo 'Número Decimal' debe ser un número decimal válido (ej: 0.5, 10.25, -3.14).";
            return false;
        }

        return true;
    }

    private static bool ValidateFecha(string valor, out string? errorMessage)
    {
        errorMessage = null;

        // Must be a valid date in ISO 8601 format (yyyy-MM-dd or full ISO datetime)
        if (!DateTime.TryParse(valor, out var date))
        {
            errorMessage = "El valor por defecto para tipo 'Fecha' debe ser una fecha válida en formato ISO 8601 (ej: 2024-01-15 o 2024-01-15T10:30:00).";
            return false;
        }

        // Optional: Validate date is not too far in the past or future
        if (date.Year < 1900 || date.Year > 2100)
        {
            errorMessage = "El valor por defecto para tipo 'Fecha' debe estar entre los años 1900 y 2100.";
            return false;
        }

        return true;
    }

    private static bool ValidateSiNo(string valor, out string? errorMessage)
    {
        errorMessage = null;

        var normalizedValue = valor.Trim().ToUpper();

        // Must be exactly "SI" or "NO"
        if (normalizedValue != "SI" && normalizedValue != "SÍ" && normalizedValue != "NO")
        {
            errorMessage = "El valor por defecto para tipo 'SI/No' debe ser exactamente 'SI' o 'NO'.";
            return false;
        }

        return true;
    }

    private static bool SetError(out string? errorMessage, string message)
    {
        errorMessage = message;
        return false;
    }

    /// <summary>
    /// Gets example values for each TipoDato.
    /// </summary>
    public static string GetExampleValue(string tipoDato)
    {
        return tipoDato.ToUpper() switch
        {
            "TEXTO" => "Ejemplo de texto",
            "NÚMERO" => "123",
            "NÚMERO DECIMAL" => "123.45",
            "FECHA" => "2024-01-15",
            "SI/NO" => "SI",
            _ => ""
        };
    }
}
