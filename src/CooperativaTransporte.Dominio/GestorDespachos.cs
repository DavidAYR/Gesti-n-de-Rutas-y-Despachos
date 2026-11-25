namespace CooperativaTransporte.Dominio;

/// <summary>
/// Coordina la LISTA de despachos y construye la MATRIZ de ocupación rutas × franjas.
/// Decide si un despacho se aprueba o se rechaza.
/// </summary>
public class GestorDespachos
{
    private readonly List<Despacho> _despachos;
    private int _siguienteId;

    public GestorDespachos()
    {
        _despachos = new List<Despacho>();
        _siguienteId = 1;
    }

    public IReadOnlyList<Despacho> Despachos => _despachos;

    public int Cantidad => _despachos.Count;

    /// <summary>
    /// Registra el despacho aplicando las reglas del negocio.
    /// Aunque se rechace, queda guardado (con su motivo) para la auditoría.
    /// </summary>
    /// <returns>true si quedó APROBADO.</returns>
    public bool Registrar(Despacho despacho)
    {
        // 1. Reglas propias del objeto (polimorfismo: cada hija agrega las suyas).
        List<string> errores = despacho.ValidarReglas();

        bool rutaValida = despacho.RutaIndice >= 0 && despacho.RutaIndice < Catalogo.CantidadRutas;

        // 2. Reglas que dependen de los demás despachos.
        if (rutaValida)
        {
            int[,] ocupacion = ConstruirMatrizOcupacion(despacho.Fecha);
            int fila = despacho.RutaIndice;
            int columna = (int)despacho.Franja;
            int cupo = Catalogo.CuposPorFranja[fila, columna];

            if (ocupacion[fila, columna] >= cupo)
            {
                errores.Add($"Sin cupo en {despacho.NombreRuta} franja {despacho.NombreFranja} " +
                            $"({ocupacion[fila, columna]}/{cupo} despachos)");
            }
        }

        if (VehiculoYaAsignado(despacho.Placa, despacho.Fecha, despacho.Franja))
        {
            errores.Add($"Vehículo {despacho.Placa} ya tiene un despacho aprobado " +
                        $"el {Formatos.Fecha(despacho.Fecha)} en la franja {despacho.NombreFranja}");
        }

        // 3. Decisión final.
        despacho.Id = _siguienteId;
        _siguienteId++;

        if (errores.Count == 0)
        {
            despacho.Aprobar();
        }
        else
        {
            despacho.Rechazar(string.Join("; ", errores));
        }

        _despachos.Add(despacho);
        return despacho.EsAprobado;
    }

    /// <summary>
    /// Recorre la lista y cuenta los despachos APROBADOS de una fecha en una matriz [ruta, franja].
    /// </summary>
    public int[,] ConstruirMatrizOcupacion(DateTime fecha)
    {
        int[,] matriz = new int[Catalogo.CantidadRutas, Catalogo.CantidadFranjas];

        foreach (Despacho d in _despachos)
        {
            bool rutaValida = d.RutaIndice >= 0 && d.RutaIndice < Catalogo.CantidadRutas;
            if (d.EsAprobado && rutaValida && d.Fecha == fecha.Date)
            {
                matriz[d.RutaIndice, (int)d.Franja]++;
            }
        }

        return matriz;
    }

    /// <summary>Un mismo bus no puede salir dos veces en la misma fecha y franja.</summary>
    private bool VehiculoYaAsignado(string placa, DateTime fecha, Franja franja)
    {
        foreach (Despacho d in _despachos)
        {
            if (d.EsAprobado && d.Placa == placa && d.Fecha == fecha.Date && d.Franja == franja)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Filtra por fecha (opcional) y por tipo de despacho.</summary>
    public List<Despacho> Filtrar(DateTime? fecha, FiltroTipo tipo)
    {
        var resultado = new List<Despacho>();

        foreach (Despacho d in _despachos)
        {
            if (fecha.HasValue && d.Fecha != fecha.Value.Date)
            {
                continue;
            }

            bool cumpleTipo;
            switch (tipo)
            {
                case FiltroTipo.Pasajeros:
                    cumpleTipo = d is DespachoPasajeros;
                    break;
                case FiltroTipo.Carga:
                    cumpleTipo = d is DespachoCarga;
                    break;
                default:
                    cumpleTipo = true;
                    break;
            }

            if (cumpleTipo)
            {
                resultado.Add(d);
            }
        }

        return resultado;
    }

    public bool Eliminar(int id)
    {
        for (int i = 0; i < _despachos.Count; i++)
        {
            if (_despachos[i].Id == id)
            {
                _despachos.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    /// <summary>Reemplaza los datos en memoria por los cargados desde el archivo.</summary>
    public void ReemplazarTodos(List<Despacho> nuevos)
    {
        _despachos.Clear();
        _siguienteId = 1;

        foreach (Despacho d in nuevos)
        {
            _despachos.Add(d);
            if (d.Id >= _siguienteId)
            {
                _siguienteId = d.Id + 1;
            }
        }
    }
}
