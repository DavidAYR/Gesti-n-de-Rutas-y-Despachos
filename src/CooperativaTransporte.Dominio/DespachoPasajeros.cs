using System.Globalization;

namespace CooperativaTransporte.Dominio;

/// <summary>Despacho de un bus con pasajeros. HEREDA de Despacho.</summary>
public class DespachoPasajeros : Despacho
{
    public TipoServicio Servicio { get; }
    public int Pasajeros { get; }
    public int CapacidadPuestos { get; }

    public DespachoPasajeros(DateTime fecha, string placa, string conductor, int rutaIndice, Franja franja,
                             DateTime vencimientoSoat, TipoServicio servicio, int pasajeros, int capacidadPuestos)
        : base(fecha, placa, conductor, rutaIndice, franja, vencimientoSoat)
    {
        Servicio = servicio;
        Pasajeros = pasajeros;
        CapacidadPuestos = capacidadPuestos;
    }

    public override string Tipo => "PASAJEROS";

    public override string Detalle => $"{Pasajeros}/{CapacidadPuestos} pasajeros · {Servicio}";

    /// <summary>Factor de recargo según el tipo de servicio (decisión con SWITCH).</summary>
    public double FactorServicio()
    {
        switch (Servicio)
        {
            case TipoServicio.Corriente:
                return 1.0;
            case TipoServicio.Ejecutivo:
                return 1.3;    // +30 %: aire acondicionado, sillas reclinables
            case TipoServicio.Preferencial:
                return 1.6;    // +60 %: van con menos puestos y sin paradas
            default:
                return 1.0;
        }
    }

    /// <summary>
    /// OVERRIDE: tasa de terminal (base) + pasajeros × tarifa de la matriz [ruta, franja] × factor del servicio.
    /// </summary>
    public override double CalcularTotal()
    {
        double tarifaBase = 0;
        if (RutaIndice >= 0 && RutaIndice < Catalogo.CantidadRutas)
        {
            tarifaBase = Catalogo.TarifasBasePasajero[RutaIndice, (int)Franja];
        }
        return base.CalcularTotal() + Pasajeros * tarifaBase * FactorServicio();
    }

    /// <summary>OVERRIDE: reglas comunes + control de sobrecupo.</summary>
    public override List<string> ValidarReglas()
    {
        List<string> errores = base.ValidarReglas();

        if (Pasajeros <= 0)
        {
            errores.Add("La cantidad de pasajeros debe ser mayor que cero");
        }
        if (CapacidadPuestos <= 0)
        {
            errores.Add("La capacidad del vehículo debe ser mayor que cero");
        }
        else if (Pasajeros > CapacidadPuestos)
        {
            errores.Add($"Sobrecupo: {Pasajeros} pasajeros para {CapacidadPuestos} puestos");
        }

        return errores;
    }

    /// <summary>OVERRIDE: agrega servicio, pasajeros y capacidad a la línea base.</summary>
    public override string ALineaTexto()
    {
        return base.ALineaTexto() + Separador
               + Servicio + Separador
               + Pasajeros.ToString(CultureInfo.InvariantCulture) + Separador
               + CapacidadPuestos.ToString(CultureInfo.InvariantCulture);
    }
}
