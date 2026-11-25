using System.Globalization;
using System.Text;

namespace CooperativaTransporte.Dominio;

/// <summary>Resultado de leer el archivo: despachos reconstruidos + contadores de líneas.</summary>
public class ResultadoCarga
{
    public List<Despacho> Despachos { get; } = new List<Despacho>();
    public List<string> Errores { get; } = new List<string>();
    public int LineasLeidas { get; set; }
    public int LineasValidas { get; set; }
    public int LineasDescartadas { get; set; }
}

/// <summary>
/// PERSISTENCIA en archivo de texto. Cada despacho es una línea separada por '|':
/// PASAJEROS|id|fecha|placa|conductor|ruta|franja|soat|estado|motivo|servicio|pasajeros|capacidad
/// CARGA|id|fecha|placa|conductor|ruta|franja|soat|estado|motivo|kilos|capacidadKg|tarifaKg|fragil
/// </summary>
public class ArchivoDespachos
{
    private const int CamposComunes = 10;
    private const string Encabezado =
        "# TIPO|ID|FECHA|PLACA|CONDUCTOR|RUTA|FRANJA|VENCE_SOAT|ESTADO|MOTIVO|...campos propios del tipo";

    public string RutaArchivo { get; }

    public ArchivoDespachos(string rutaArchivo)
    {
        RutaArchivo = rutaArchivo;
    }

    public bool Existe => File.Exists(RutaArchivo);

    public void Guardar(IEnumerable<Despacho> despachos)
    {
        string? carpeta = Path.GetDirectoryName(RutaArchivo);
        if (!string.IsNullOrEmpty(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }

        var lineas = new List<string> { Encabezado };
        foreach (Despacho d in despachos)
        {
            lineas.Add(d.ALineaTexto());   // polimorfismo: cada hija agrega sus campos
        }

        File.WriteAllLines(RutaArchivo, lineas, Encoding.UTF8);
    }

    public ResultadoCarga Cargar()
    {
        var resultado = new ResultadoCarga();
        if (!Existe)
        {
            return resultado;
        }

        string[] lineas = File.ReadAllLines(RutaArchivo, Encoding.UTF8);

        for (int i = 0; i < lineas.Length; i++)
        {
            string linea = lineas[i].Trim();
            if (linea.Length == 0 || linea.StartsWith('#'))
            {
                continue;   // líneas vacías y comentarios no cuentan
            }

            resultado.LineasLeidas++;
            Despacho? despacho = InterpretarLinea(linea, out string error);

            if (despacho != null)
            {
                resultado.Despachos.Add(despacho);
                resultado.LineasValidas++;
            }
            else
            {
                resultado.LineasDescartadas++;
                resultado.Errores.Add($"Línea {i + 1}: {error}");
            }
        }

        return resultado;
    }

    /// <summary>Reconstruye el objeto correcto (DespachoPasajeros o DespachoCarga) según el primer campo.</summary>
    private static Despacho? InterpretarLinea(string linea, out string error)
    {
        error = "";
        string[] p = linea.Split(Despacho.Separador);
        CultureInfo inv = CultureInfo.InvariantCulture;

        if (p.Length < CamposComunes)
        {
            error = "faltan campos";
            return null;
        }

        if (!int.TryParse(p[1], NumberStyles.Integer, inv, out int id) ||
            !DateTime.TryParseExact(p[2], Despacho.FormatoFecha, inv, DateTimeStyles.None, out DateTime fecha) ||
            !int.TryParse(p[5], NumberStyles.Integer, inv, out int ruta) ||
            !int.TryParse(p[6], NumberStyles.Integer, inv, out int franjaNum) ||
            !DateTime.TryParseExact(p[7], Despacho.FormatoFecha, inv, DateTimeStyles.None, out DateTime soat) ||
            !Enum.TryParse(p[8], out EstadoDespacho estado))
        {
            error = "datos comunes con formato inválido";
            return null;
        }

        if (franjaNum < 0 || franjaNum >= Catalogo.CantidadFranjas)
        {
            error = "franja inexistente";
            return null;
        }
        Franja franja = (Franja)franjaNum;

        Despacho despacho;
        switch (p[0])
        {
            case "PASAJEROS":
                if (p.Length < CamposComunes + 3 ||
                    !Enum.TryParse(p[10], out TipoServicio servicio) ||
                    !int.TryParse(p[11], NumberStyles.Integer, inv, out int pasajeros) ||
                    !int.TryParse(p[12], NumberStyles.Integer, inv, out int capacidad))
                {
                    error = "datos de pasajeros inválidos";
                    return null;
                }
                despacho = new DespachoPasajeros(fecha, p[3], p[4], ruta, franja, soat, servicio, pasajeros, capacidad);
                break;

            case "CARGA":
                if (p.Length < CamposComunes + 4 ||
                    !double.TryParse(p[10], NumberStyles.Float, inv, out double kilos) ||
                    !double.TryParse(p[11], NumberStyles.Float, inv, out double capacidadKg) ||
                    !double.TryParse(p[12], NumberStyles.Float, inv, out double tarifaKilo) ||
                    !bool.TryParse(p[13], out bool fragil))
                {
                    error = "datos de carga inválidos";
                    return null;
                }
                despacho = new DespachoCarga(fecha, p[3], p[4], ruta, franja, soat, kilos, capacidadKg, tarifaKilo, fragil);
                break;

            default:
                error = $"tipo de despacho desconocido '{p[0]}'";
                return null;
        }

        despacho.Id = id;
        if (estado == EstadoDespacho.Aprobado)
        {
            despacho.Aprobar();
        }
        else
        {
            despacho.Rechazar(p[9]);
        }
        return despacho;
    }
}
