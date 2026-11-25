# Gestión de Rutas y Despachos – Cooperativa de Transporte Intermunicipal

Aplicación de escritorio en **C# / Windows Forms (creada 100 % por código)** para el curso *Herramientas de Programación 2* (ITM).

## 1. Problema que resuelve

En muchas cooperativas de transporte intermunicipal el despacho de buses y el control de pasajeros y encomiendas se lleva en hojas sueltas o libretas. Eso genera:

- **Vehículos mal asignados**: el mismo bus despachado dos veces en la misma franja.
- **Sobrecupos y sobrecargas**: más pasajeros que puestos, o más kilos que la capacidad del vehículo.
- **Documentación vencida**: buses que salen con el SOAT vencido.
- **Franjas saturadas**: más salidas de las autorizadas por la terminal en una ruta y horario.
- **Desorden en la auditoría**: no se sabe qué despachos fueron válidos, cuáles se rechazaron ni por qué, ni cuánto se recaudó.

La aplicación permite **registrar despachos** (de pasajeros o de carga), **validarlos automáticamente** con las reglas del negocio, **consultar la programación diaria** con su matriz de ocupación y **generar un informe de control operativo**. Todo queda guardado en un archivo de texto.

## 2. Cómo ejecutarlo

**Requisitos:** Windows 10/11 y el SDK de [.NET 8](https://dotnet.microsoft.com/download) o superior (la app también corre con .NET 9 o 10).

**Con Visual Studio 2022:**
1. Abrir `GestionRutasDespachos.sln`.
2. Marcar `CooperativaTransporte.App` como proyecto de inicio.
3. Presionar **F5**.

**Por consola:**
```bash
dotnet run --project src/CooperativaTransporte.App
```

En la primera ejecución se cargan datos de ejemplo (`datos/ejemplo_despachos.txt`) con despachos del 01/12/2026 y del 02/12/2026. Para verlos en la pestaña 2, escoja una de esas fechas o marque *Mostrar todas las fechas*.

## 3. Funcionalidades (pantallas)

| Pestaña | Qué hace |
|---|---|
| **1. Registro de despacho** | Formulario con fecha, ruta, franja, placa, conductor, SOAT y tipo (pasajeros o carga). Valida con **ErrorProvider** y responde con **MessageBox** (aprobado con su total, o rechazado con los motivos). Botón *Calcular total* y lista de despachos recientes. |
| **2. Programación y matriz operativa** | **DataGridView** con los despachos (los rechazados en rojo), filtro por fecha y opción de eliminar. Abajo, la **matriz de ocupación** rutas × franjas (aprobados / cupo, coloreada) y la **matriz de tarifas** base. |
| **3. Informe y persistencia** | Informe con filtros (fecha y tipo), alertas según las banderas, exportación del informe a `.txt` y botones **Guardar / Cargar datos**. |

### Reglas del negocio

| Regla | Dónde se valida | Resultado |
|---|---|---|
| Campos vacíos, números inválidos o ≤ 0 | Interfaz (`ErrorProvider`) | No se registra; se marca el campo |
| Placa con formato `AAA123` | Interfaz y `Despacho.ValidarReglas()` | Error en el campo / rechazo |
| SOAT vencido en la fecha del despacho | `Despacho.ValidarReglas()` | **Rechazado** |
| Pasajeros > puestos | `DespachoPasajeros.ValidarReglas()` (override) | **Rechazado** (sobrecupo) |
| Kilos > capacidad | `DespachoCarga.ValidarReglas()` (override) | **Rechazado** (sobrecarga) |
| La ruta y franja ya llegó a su cupo diario | `GestorDespachos.Registrar()` con la matriz | **Rechazado** (sin cupo) |
| El mismo vehículo ya salió en esa fecha y franja | `GestorDespachos.Registrar()` | **Rechazado** |

Los despachos rechazados **sí se guardan** (con su motivo), porque la cooperativa necesita saber qué se rechazó y por qué.

### Cálculo del total (polimorfismo)

- **Despacho (base):** tasa de uso de terminal = $5.000.
- **DespachoPasajeros:** base + `pasajeros × tarifa[ruta, franja] × factor` (con `switch`: Corriente ×1,0 · Ejecutivo ×1,3 · Preferencial ×1,6).
- **DespachoCarga:** base + `kilos × tarifa por kilo`, con recargo por franja (con `switch`: Mañana 0 % · Tarde 5 % · Noche 20 %) y +15 % si es frágil.

## 4. Cómo se cumple cada requisito

| # | Requisito | Implementación |
|---|---|---|
| 1 | GUI por código, 3 pantallas, entradas, tabla, eventos | `FormPrincipal` (partial) con `TabControl` de 3 pestañas, sin diseñador. `DataGridView`, `ListBox`, `ComboBox`, `TextBox`, `DateTimePicker`, `RadioButton`, `CheckBox`. Eventos `Click`, `CheckedChanged`, `ValueChanged`, `Leave`, `Load`, `FormClosing`. |
| 2 | Validación y decisiones | `ErrorProvider` + `MessageBox`. `if` para aprobar o rechazar; `switch` en `FactorServicio()`, `RecargoFranja()`, `Filtrar()` y al leer el archivo. |
| 3 | POO: 3+ clases, constructores, herencia, override | `Despacho` (abstracta) → `DespachoPasajeros`, `DespachoCarga`. Sobrescriben `CalcularTotal()`, `ValidarReglas()`, `ALineaTexto()`, `Tipo` y `Detalle`. Otras clases: `GestorDespachos`, `InformeOperativo`, `ArchivoDespachos`, `ResultadoCarga`. |
| 4 | Lógica separada de la interfaz | Proyecto aparte `CooperativaTransporte.Dominio` (librería `net8.0`, **sin referencia a Windows Forms**: no podría usarlo aunque quisiera). |
| 5 | Vector/lista y matriz | `List<Despacho>`; vectores `Catalogo.Rutas`, `Franjas` y `RecaudoPorRuta`; matrices `TarifasBasePasajero[ruta, franja]`, `CuposPorFranja[ruta, franja]` y la matriz de ocupación calculada. |
| 6 | Informe con contadores, acumuladores y banderas | `InformeOperativo`: contadores (procesados, aprobados, rechazados por causa, franjas llenas), acumuladores (recaudo, pasajeros, kilos, recaudo por ruta) y banderas (`HayRechazados`, `SuperaMeta`, `HayFranjaLlena`). |
| 7 | Persistencia en `.txt` | `ArchivoDespachos`: una línea por despacho separada por `|`. Carga al iniciar y guarda al cerrar, además de los botones. Al leer reconstruye `DespachoPasajeros` o `DespachoCarga` según el primer campo. |
| 8 | GitHub | Commits de ambos integrantes (ver sección 7). |

## 5. Estructura del repositorio

```
GestionRutasDespachos.sln
src/
├── CooperativaTransporte.Dominio/     ← LÓGICA (sin Windows Forms)
│   ├── Enumeraciones.cs               Franja, TipoServicio, EstadoDespacho, FiltroTipo
│   ├── Catalogo.cs                    Vector de rutas + matrices de tarifas y cupos
│   ├── Validaciones.cs                Formato de placa, nombres, moneda
│   ├── Despacho.cs                    Clase base abstracta
│   ├── DespachoPasajeros.cs           Hija: pasajeros
│   ├── DespachoCarga.cs               Hija: carga / encomiendas
│   ├── GestorDespachos.cs             Lista, matriz de ocupación, reglas de aprobación
│   ├── InformeOperativo.cs            Informe con contadores, acumuladores y banderas
│   └── ArchivoDespachos.cs            Guardar / cargar .txt
└── CooperativaTransporte.App/         ← INTERFAZ (Windows Forms por código)
    ├── Program.cs
    ├── FormPrincipal.cs               Ventana, TabControl, carga y guardado automáticos
    ├── FormPrincipal.Registro.cs      Pestaña 1
    ├── FormPrincipal.Programacion.cs  Pestaña 2
    ├── FormPrincipal.Informe.cs       Pestaña 3
    └── datos/ejemplo_despachos.txt    Datos de demostración
```

### Formato del archivo `despachos.txt`

Se guarda junto al ejecutable, en `datos/despachos.txt`:

```
PASAJEROS|id|fecha|placa|conductor|ruta|franja|venceSoat|estado|motivo|servicio|pasajeros|capacidad
CARGA|id|fecha|placa|conductor|ruta|franja|venceSoat|estado|motivo|kilos|capacidadKg|tarifaKilo|fragil
```

Ejemplo:
```
PASAJEROS|1|2026-12-01|SXA123|Carlos Restrepo|0|0|2027-03-15|Aprobado||Corriente|38|40
CARGA|11|2026-12-02|GHT321|Andrés Zapata|3|1|2027-05-30|Rechazado|Sobrecarga: 2300 kg para capacidad de 1500 kg|2300|1500|400|False
```

Las líneas dañadas no detienen la carga: se cuentan como **descartadas** y se informa en qué línea están.