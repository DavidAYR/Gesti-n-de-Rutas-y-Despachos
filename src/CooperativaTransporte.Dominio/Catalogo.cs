namespace CooperativaTransporte.Dominio;

/// <summary>
/// Datos fijos de la operación de la cooperativa.
/// - Rutas: VECTOR con las rutas que cubre la cooperativa (el índice es la fila de las matrices).
/// - TarifasBasePasajero y CuposPorFranja: MATRICES rutas × franjas horarias.
/// </summary>
public static class Catalogo
{
    /// <summary>Vector de rutas. Fila i de las matrices = Rutas[i].</summary>
    public static readonly string[] Rutas =
    {
        "Medellín - Rionegro",
        "Medellín - La Ceja",
        "Medellín - Santa Fe de Antioquia",
        "Medellín - Marinilla",
        "Medellín - Caldas"
    };

    /// <summary>Vector de franjas. Columna j de las matrices = Franjas[j].</summary>
    public static readonly string[] Franjas = { "Mañana", "Tarde", "Noche" };

    public static readonly string[] HorariosFranja = { "05:00 - 11:59", "12:00 - 17:59", "18:00 - 22:00" };

    /// <summary>
    /// Matriz [ruta, franja] con la tarifa base por pasajero (COP).
    /// La noche es más costosa porque la cooperativa paga recargo nocturno al conductor.
    /// </summary>
    public static readonly double[,] TarifasBasePasajero =
    {
        // Mañana  Tarde   Noche
        { 12000, 12000, 13500 },   // Rionegro
        { 14000, 14000, 15500 },   // La Ceja
        { 22000, 22000, 24000 },   // Santa Fe de Antioquia
        { 13000, 13000, 14500 },   // Marinilla
        {  6000,  6000,  7000 }    // Caldas
    };

    /// <summary>
    /// Matriz [ruta, franja] con la cantidad máxima de despachos aprobados por día.
    /// Evita asignar más buses de los que la terminal autoriza en esa franja.
    /// </summary>
    public static readonly int[,] CuposPorFranja =
    {
        // Mañana Tarde Noche
        { 3, 3, 2 },   // Rionegro
        { 2, 2, 1 },   // La Ceja
        { 2, 2, 1 },   // Santa Fe de Antioquia
        { 2, 2, 1 },   // Marinilla
        { 4, 4, 2 }    // Caldas
    };

    /// <summary>Meta de recaudo de la cooperativa por día de operación (COP).</summary>
    public const double MetaRecaudoDiaria = 1_500_000;

    public static int CantidadRutas => Rutas.Length;
    public static int CantidadFranjas => Franjas.Length;
}
