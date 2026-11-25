using System.Globalization;
using System.Text.RegularExpressions;

namespace CooperativaTransporte.Dominio;

/// <summary>Reglas de formato reutilizables (las usa el dominio y también la interfaz).</summary>
public static class Validaciones
{
    // Placa de servicio público en Colombia: 3 letras y 3 números (ej. SXA123).
    private static readonly Regex PatronPlaca = new Regex("^[A-Z]{3}[0-9]{3}$");

    public static bool EsPlacaValida(string? placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
        {
            return false;
        }
        return PatronPlaca.IsMatch(placa.Trim().ToUpperInvariant());
    }

    public static bool EsNombreValido(string? nombre)
    {
        return !string.IsNullOrWhiteSpace(nombre) && nombre.Trim().Length >= 3;
    }
}

/// <summary>Formatos de presentación (moneda colombiana, fechas) sin depender de la interfaz.</summary>
public static class Formatos
{
    public static readonly CultureInfo Cultura = new CultureInfo("es-CO");

    public static string Moneda(double valor) => valor.ToString("C0", Cultura);

    public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string Numero(double valor) => valor.ToString("N0", Cultura);
}
