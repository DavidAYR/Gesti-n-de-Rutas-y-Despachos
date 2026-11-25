using System.Globalization;

namespace CooperativaTransporte.Dominio;

/// <summary>Despacho de encomiendas / carga. HEREDA de Despacho.</summary>
public class DespachoCarga : Despacho
{
    public const double RecargoFragil = 0.15;

    public double Kilos { get; }
    public double CapacidadKg { get; }
    public double TarifaPorKilo { get; }
    public bool Fragil { get; }

    public DespachoCarga(DateTime fecha, string placa, string conductor, int rutaIndice, Franja franja,
                         DateTime vencimientoSoat, double kilos, double capacidadKg, double tarifaPorKilo, bool fragil)
        : base(fecha, placa, conductor, rutaIndice, franja, vencimientoSoat)
    {
        Kilos = kilos;
        CapacidadKg = capacidadKg;
        TarifaPorKilo = tarifaPorKilo;
        Fragil = fragil;
    }

    public override string Tipo => "CARGA";

    public override string Detalle =>
        $"{Formatos.Numero(Kilos)}/{Formatos.Numero(CapacidadKg)} kg" + (Fragil ? " · frágil" : "");

    /// <summary>Recargo según la franja horaria (decisión con SWITCH).</summary>
    public double RecargoFranja()
    {
        switch (Franja)
        {
            case Franja.Manana:
                return 0.0;
            case Franja.Tarde:
                return 0.05;   // +5 %: congestión en la salida de Medellín
            case Franja.Noche:
                return 0.20;   // +20 %: vigilancia y seguro nocturno
            default:
                return 0.0;
        }
    }

    /// <summary>
    /// OVERRIDE: tasa de terminal (base) + kilos × tarifa por kilo, con recargo por franja y por carga frágil.
    /// </summary>
    public override double CalcularTotal()
    {
        double subtotal = Kilos * TarifaPorKilo;
        subtotal += subtotal * RecargoFranja();
        if (Fragil)
        {
            subtotal += subtotal * RecargoFragil;
        }
        return base.CalcularTotal() + subtotal;
    }

    /// <summary>OVERRIDE: reglas comunes + control de sobrecarga.</summary>
    public override List<string> ValidarReglas()
    {
        List<string> errores = base.ValidarReglas();

        if (Kilos <= 0)
        {
            errores.Add("El peso de la carga debe ser mayor que cero");
        }
        if (TarifaPorKilo <= 0)
        {
            errores.Add("La tarifa por kilo debe ser mayor que cero");
        }
        if (CapacidadKg <= 0)
        {
            errores.Add("La capacidad del vehículo debe ser mayor que cero");
        }
        else if (Kilos > CapacidadKg)
        {
            errores.Add($"Sobrecarga: {Kilos.ToString("0.##", CultureInfo.InvariantCulture)} kg " +
                        $"para capacidad de {CapacidadKg.ToString("0.##", CultureInfo.InvariantCulture)} kg");
        }

        return errores;
    }

    /// <summary>OVERRIDE: agrega kilos, capacidad, tarifa y frágil a la línea base.</summary>
    public override string ALineaTexto()
    {
        return base.ALineaTexto() + Separador
               + Kilos.ToString(CultureInfo.InvariantCulture) + Separador
               + CapacidadKg.ToString(CultureInfo.InvariantCulture) + Separador
               + TarifaPorKilo.ToString(CultureInfo.InvariantCulture) + Separador
               + Fragil;
    }
}
