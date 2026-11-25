using System.Globalization;

namespace CooperativaTransporte.Dominio;

/// <summary>
/// Clase BASE abstracta: datos comunes a cualquier despacho de la cooperativa.
/// Las clases hijas (DespachoPasajeros, DespachoCarga) sobrescriben CalcularTotal(),
/// ValidarReglas() y ALineaTexto()  →  POLIMORFISMO.
/// </summary>
public abstract class Despacho
{
    /// <summary>Valor fijo que todo despacho paga por usar la terminal.</summary>
    public const double TasaUsoTerminal = 5000;

    public const char Separador = '|';
    public const string FormatoFecha = "yyyy-MM-dd";

    public int Id { get; internal set; }
    public DateTime Fecha { get; }
    public string Placa { get; }
    public string Conductor { get; }
    public int RutaIndice { get; }
    public Franja Franja { get; }
    public DateTime VencimientoSoat { get; }
    public EstadoDespacho Estado { get; private set; }
    public string MotivoRechazo { get; private set; }

    protected Despacho(DateTime fecha, string placa, string conductor, int rutaIndice,
                       Franja franja, DateTime vencimientoSoat)
    {
        Fecha = fecha.Date;
        Placa = (placa ?? "").Trim().ToUpperInvariant();
        Conductor = (conductor ?? "").Trim();
        RutaIndice = rutaIndice;
        Franja = franja;
        VencimientoSoat = vencimientoSoat.Date;
        Estado = EstadoDespacho.Aprobado;
        MotivoRechazo = "";
    }

    /// <summary>Código del tipo, se usa en la grilla y como primer campo del archivo .txt.</summary>
    public abstract string Tipo { get; }

    /// <summary>Descripción corta de la carga del despacho (pasajeros o kilos).</summary>
    public abstract string Detalle { get; }

    public bool EsAprobado => Estado == EstadoDespacho.Aprobado;

    public string NombreRuta =>
        RutaIndice >= 0 && RutaIndice < Catalogo.CantidadRutas ? Catalogo.Rutas[RutaIndice] : "(ruta desconocida)";

    public string NombreFranja => Catalogo.Franjas[(int)Franja];

    /// <summary>
    /// Método VIRTUAL: el despacho base solo paga la tasa de terminal.
    /// Las hijas lo sobrescriben (override) y suman su propio cobro llamando a base.CalcularTotal().
    /// </summary>
    public virtual double CalcularTotal()
    {
        return TasaUsoTerminal;
    }

    /// <summary>
    /// Reglas de negocio comunes. Devuelve la lista de motivos de rechazo (vacía = cumple).
    /// Las hijas agregan sus reglas propias (sobrecupo / sobrecarga).
    /// </summary>
    public virtual List<string> ValidarReglas()
    {
        var errores = new List<string>();

        if (!Validaciones.EsPlacaValida(Placa))
        {
            errores.Add("Placa con formato inválido (debe ser AAA123)");
        }
        if (!Validaciones.EsNombreValido(Conductor))
        {
            errores.Add("Nombre del conductor incompleto");
        }
        if (RutaIndice < 0 || RutaIndice >= Catalogo.CantidadRutas)
        {
            errores.Add("Ruta inexistente");
        }
        if (VencimientoSoat < Fecha)
        {
            errores.Add("SOAT vencido el " + Formatos.Fecha(VencimientoSoat));
        }

        return errores;
    }

    internal void Aprobar()
    {
        Estado = EstadoDespacho.Aprobado;
        MotivoRechazo = "";
    }

    internal void Rechazar(string motivo)
    {
        Estado = EstadoDespacho.Rechazado;
        MotivoRechazo = motivo;
    }

    /// <summary>
    /// Serializa los campos comunes en una línea separada por '|'.
    /// Las hijas lo sobrescriben y agregan sus campos al final.
    /// </summary>
    public virtual string ALineaTexto()
    {
        return string.Join(Separador,
            Tipo,
            Id.ToString(CultureInfo.InvariantCulture),
            Fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture),
            Placa,
            LimpiarTexto(Conductor),
            RutaIndice.ToString(CultureInfo.InvariantCulture),
            ((int)Franja).ToString(CultureInfo.InvariantCulture),
            VencimientoSoat.ToString(FormatoFecha, CultureInfo.InvariantCulture),
            Estado.ToString(),
            LimpiarTexto(MotivoRechazo));
    }

    /// <summary>Evita que un texto rompa el formato del archivo.</summary>
    protected static string LimpiarTexto(string texto)
    {
        return texto.Replace(Separador, '/').Replace('\r', ' ').Replace('\n', ' ');
    }

    public override string ToString()
    {
        return $"#{Id} {Formatos.Fecha(Fecha)} {Placa} {NombreRuta} ({NombreFranja}) - {Estado}";
    }
}
