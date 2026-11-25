namespace CooperativaTransporte.Dominio;

/// <summary>Franja horaria del despacho. El valor numérico es la columna de las matrices.</summary>
public enum Franja
{
    Manana = 0,
    Tarde = 1,
    Noche = 2
}

/// <summary>Tipo de servicio para despachos de pasajeros (define el recargo con switch).</summary>
public enum TipoServicio
{
    Corriente,
    Ejecutivo,
    Preferencial
}

/// <summary>Resultado del control operativo de un despacho.</summary>
public enum EstadoDespacho
{
    Aprobado,
    Rechazado
}

/// <summary>Filtro por tipo usado en la grilla y en el informe.</summary>
public enum FiltroTipo
{
    Todos,
    Pasajeros,
    Carga
}
