using System.Text;

namespace CooperativaTransporte.Dominio;

/// <summary>
/// Informe de control operativo. Se calcula en el constructor recorriendo la lista y la matriz con ciclos:
/// - CONTADORES: registros, aprobados, rechazados, rechazos por causa, franjas llenas.
/// - ACUMULADORES: dinero recaudado, pasajeros y kilos transportados, recaudo por ruta (vector).
/// - BANDERAS: hayRechazados, superaMeta, hayFranjaLlena.
/// </summary>
public class InformeOperativo
{
    public DateTime? Fecha { get; }
    public FiltroTipo Filtro { get; }

    // Contadores
    public int TotalRegistros { get; private set; }
    public int Aprobados { get; private set; }
    public int Rechazados { get; private set; }
    public int DespachosPasajeros { get; private set; }
    public int DespachosCarga { get; private set; }
    public int RechazosSoat { get; private set; }
    public int RechazosCapacidad { get; private set; }
    public int RechazosSinCupo { get; private set; }
    public int RechazosVehiculoRepetido { get; private set; }
    public int RechazosOtros { get; private set; }
    public int DiasOperados { get; private set; }
    public int FranjasLlenas { get; private set; }

    // Acumuladores
    public double TotalRecaudado { get; private set; }
    public int TotalPasajeros { get; private set; }
    public double TotalKilos { get; private set; }
    public double[] RecaudoPorRuta { get; }          // vector: un acumulador por ruta
    public int[,] MatrizDespachos { get; }           // matriz: despachos aprobados por ruta × franja

    // Banderas
    public bool HayRechazados { get; private set; }
    public bool SuperaMeta { get; private set; }
    public bool HayFranjaLlena { get; private set; }

    public double Meta { get; private set; }
    public double PorcentajeAprobacion => TotalRegistros == 0 ? 0 : Aprobados * 100.0 / TotalRegistros;
    public double PromedioPorDespacho => Aprobados == 0 ? 0 : TotalRecaudado / Aprobados;

    public string RutaMasRentable { get; private set; } = "-";
    public string CeldaMasOcupada { get; private set; } = "-";

    public InformeOperativo(List<Despacho> despachos, DateTime? fecha, FiltroTipo filtro)
    {
        Fecha = fecha;
        Filtro = filtro;
        RecaudoPorRuta = new double[Catalogo.CantidadRutas];
        MatrizDespachos = new int[Catalogo.CantidadRutas, Catalogo.CantidadFranjas];

        RecorrerDespachos(despachos);
        RecorrerMatriz();
        RecorrerVectorRutas();
    }

    /// <summary>Ciclo 1: recorre la lista con contadores, acumuladores y banderas.</summary>
    private void RecorrerDespachos(List<Despacho> despachos)
    {
        var dias = new List<DateTime>();

        foreach (Despacho d in despachos)
        {
            TotalRegistros++;                                   // contador

            if (!dias.Contains(d.Fecha))
            {
                dias.Add(d.Fecha);
            }

            if (d is DespachoPasajeros)
            {
                DespachosPasajeros++;
            }
            else if (d is DespachoCarga)
            {
                DespachosCarga++;
            }

            if (d.EsAprobado)
            {
                Aprobados++;                                    // contador
                double total = d.CalcularTotal();               // polimorfismo
                TotalRecaudado += total;                        // acumulador

                bool rutaValida = d.RutaIndice >= 0 && d.RutaIndice < Catalogo.CantidadRutas;
                if (rutaValida)
                {
                    RecaudoPorRuta[d.RutaIndice] += total;      // acumulador en vector
                    MatrizDespachos[d.RutaIndice, (int)d.Franja]++;
                }

                if (d is DespachoPasajeros p)
                {
                    TotalPasajeros += p.Pasajeros;              // acumulador
                }
                else if (d is DespachoCarga c)
                {
                    TotalKilos += c.Kilos;                      // acumulador
                }
            }
            else
            {
                Rechazados++;                                   // contador
                HayRechazados = true;                           // bandera
                ClasificarRechazo(d.MotivoRechazo);
            }
        }

        DiasOperados = dias.Count;
        Meta = Catalogo.MetaRecaudoDiaria * Math.Max(1, DiasOperados);
        SuperaMeta = TotalRecaudado >= Meta;                    // bandera
    }

    /// <summary>Un despacho puede tener varios motivos; se cuenta una vez por cada causa.</summary>
    private void ClasificarRechazo(string motivo)
    {
        bool clasificado = false;   // bandera local

        if (motivo.Contains("SOAT"))
        {
            RechazosSoat++;
            clasificado = true;
        }
        if (motivo.Contains("Sobrecupo") || motivo.Contains("Sobrecarga"))
        {
            RechazosCapacidad++;
            clasificado = true;
        }
        if (motivo.Contains("Sin cupo"))
        {
            RechazosSinCupo++;
            clasificado = true;
        }
        if (motivo.Contains("ya tiene un despacho"))
        {
            RechazosVehiculoRepetido++;
            clasificado = true;
        }
        if (!clasificado)
        {
            RechazosOtros++;
        }
    }

    /// <summary>Ciclo 2: recorre la matriz con for anidados (filas = rutas, columnas = franjas).</summary>
    private void RecorrerMatriz()
    {
        int maximo = 0;

        for (int fila = 0; fila < Catalogo.CantidadRutas; fila++)
        {
            for (int columna = 0; columna < Catalogo.CantidadFranjas; columna++)
            {
                int valor = MatrizDespachos[fila, columna];

                if (valor > maximo)
                {
                    maximo = valor;
                    CeldaMasOcupada = $"{Catalogo.Rutas[fila]} / {Catalogo.Franjas[columna]} ({valor} despachos)";
                }

                // Comparar contra el cupo solo tiene sentido para un día concreto.
                if (Fecha.HasValue && valor >= Catalogo.CuposPorFranja[fila, columna])
                {
                    FranjasLlenas++;
                    HayFranjaLlena = true;                      // bandera
                }
            }
        }
    }

    /// <summary>Ciclo 3: busca en el vector la ruta con mayor recaudo.</summary>
    private void RecorrerVectorRutas()
    {
        double mayor = 0;
        for (int i = 0; i < RecaudoPorRuta.Length; i++)
        {
            if (RecaudoPorRuta[i] > mayor)
            {
                mayor = RecaudoPorRuta[i];
                RutaMasRentable = $"{Catalogo.Rutas[i]} ({Formatos.Moneda(mayor)})";
            }
        }
    }

    public string GenerarTexto()
    {
        var sb = new StringBuilder();
        string linea = new string('=', 72);

        sb.AppendLine(linea);
        sb.AppendLine("  INFORME DE CONTROL OPERATIVO - COOPERATIVA DE TRANSPORTE");
        sb.AppendLine(linea);
        sb.AppendLine($"  Periodo : {(Fecha.HasValue ? Formatos.Fecha(Fecha.Value) : "Todas las fechas")}");
        sb.AppendLine($"  Tipo    : {Filtro}");
        sb.AppendLine($"  Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine();

        sb.AppendLine("  REGISTROS (contadores)");
        sb.AppendLine($"    Despachos procesados ........ {TotalRegistros}");
        sb.AppendLine($"    Aprobados ................... {Aprobados}  ({PorcentajeAprobacion:0.0} %)");
        sb.AppendLine($"    Rechazados .................. {Rechazados}");
        sb.AppendLine($"      - SOAT vencido ............ {RechazosSoat}");
        sb.AppendLine($"      - Sobrecupo / sobrecarga .. {RechazosCapacidad}");
        sb.AppendLine($"      - Franja sin cupo ......... {RechazosSinCupo}");
        sb.AppendLine($"      - Vehículo repetido ....... {RechazosVehiculoRepetido}");
        sb.AppendLine($"      - Otros ................... {RechazosOtros}");
        sb.AppendLine($"    De pasajeros / de carga ..... {DespachosPasajeros} / {DespachosCarga}");
        sb.AppendLine($"    Días con operación .......... {DiasOperados}");
        sb.AppendLine();

        sb.AppendLine("  RESULTADOS (acumuladores, solo despachos aprobados)");
        sb.AppendLine($"    Total recaudado ............. {Formatos.Moneda(TotalRecaudado)}");
        sb.AppendLine($"    Promedio por despacho ....... {Formatos.Moneda(PromedioPorDespacho)}");
        sb.AppendLine($"    Pasajeros transportados ..... {TotalPasajeros}");
        sb.AppendLine($"    Kilos transportados ......... {Formatos.Numero(TotalKilos)} kg");
        sb.AppendLine($"    Meta de recaudo ............. {Formatos.Moneda(Meta)}");
        sb.AppendLine();

        sb.AppendLine("  RECAUDO POR RUTA (vector)");
        for (int i = 0; i < RecaudoPorRuta.Length; i++)
        {
            sb.AppendLine($"    {Catalogo.Rutas[i],-34} {Formatos.Moneda(RecaudoPorRuta[i]),15}");
        }
        sb.AppendLine();

        sb.AppendLine("  DESPACHOS APROBADOS POR RUTA Y FRANJA (matriz)");
        sb.Append($"    {"Ruta",-34}");
        for (int c = 0; c < Catalogo.CantidadFranjas; c++)
        {
            sb.Append($"{Catalogo.Franjas[c],8}");
        }
        sb.AppendLine();
        for (int f = 0; f < Catalogo.CantidadRutas; f++)
        {
            sb.Append($"    {Catalogo.Rutas[f],-34}");
            for (int c = 0; c < Catalogo.CantidadFranjas; c++)
            {
                sb.Append($"{MatrizDespachos[f, c],8}");
            }
            sb.AppendLine();
        }
        sb.AppendLine();

        sb.AppendLine("  OBSERVACIONES (banderas)");
        sb.AppendLine($"    Ruta más rentable: {RutaMasRentable}");
        sb.AppendLine($"    Ruta/franja más usada: {CeldaMasOcupada}");
        if (TotalRegistros == 0)
        {
            sb.AppendLine("    [i] No hay despachos para los filtros seleccionados.");
        }
        if (HayRechazados)
        {
            sb.AppendLine($"    [!] Hay {Rechazados} despacho(s) rechazado(s): revisar documentación y capacidad.");
        }
        else if (TotalRegistros > 0)
        {
            sb.AppendLine("    [OK] Todos los despachos cumplieron las reglas.");
        }
        if (SuperaMeta)
        {
            sb.AppendLine("    [OK] Se superó la meta de recaudo.");
        }
        else if (TotalRegistros > 0)
        {
            sb.AppendLine($"    [!] Faltan {Formatos.Moneda(Meta - TotalRecaudado)} para la meta de recaudo.");
        }
        if (HayFranjaLlena)
        {
            sb.AppendLine($"    [!] {FranjasLlenas} franja(s) llegaron al cupo máximo: evaluar más buses.");
        }
        sb.AppendLine(linea);

        return sb.ToString();
    }
}
