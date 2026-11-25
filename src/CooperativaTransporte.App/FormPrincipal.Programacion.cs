using CooperativaTransporte.Dominio;

namespace CooperativaTransporte.App;

/// <summary>PESTAÑA 2 – Programación: DataGridView de despachos y matrices rutas × franjas.</summary>
public partial class FormPrincipal
{
    private DateTimePicker _dtpProgramacion = null!;
    private CheckBox _chkTodasFechas = null!;
    private DataGridView _dgvDespachos = null!;
    private DataGridView _dgvOcupacion = null!;
    private DataGridView _dgvTarifas = null!;
    private Label _lblOcupacion = null!;

    private TabPage CrearPestanaProgramacion()
    {
        var pagina = new TabPage("2. Programación y matriz operativa") { Padding = new Padding(8) };

        var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        // --- Fila 0: filtros
        var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        _dtpProgramacion = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 130 };
        _chkTodasFechas = new CheckBox { Text = "Mostrar todas las fechas en la tabla", AutoSize = true, Margin = new Padding(15, 8, 3, 3) };
        var btnHoy = new Button { Text = "Hoy", Width = 70, Height = 30 };
        var btnEliminar = new Button { Text = "Eliminar seleccionado", Width = 170, Height = 30, Margin = new Padding(30, 3, 3, 3) };
        barra.Controls.Add(new Label { Text = "Fecha de programación:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        barra.Controls.Add(_dtpProgramacion);
        barra.Controls.Add(btnHoy);
        barra.Controls.Add(_chkTodasFechas);
        barra.Controls.Add(btnEliminar);
        tabla.Controls.Add(barra, 0, 0);

        // --- Fila 1: grilla de despachos
        _dgvDespachos = CrearGrilla();
        _dgvDespachos.Columns.Add("Id", "Id");
        _dgvDespachos.Columns.Add("Fecha", "Fecha");
        _dgvDespachos.Columns.Add("Tipo", "Tipo");
        _dgvDespachos.Columns.Add("Ruta", "Ruta");
        _dgvDespachos.Columns.Add("Franja", "Franja");
        _dgvDespachos.Columns.Add("Placa", "Placa");
        _dgvDespachos.Columns.Add("Conductor", "Conductor");
        _dgvDespachos.Columns.Add("Detalle", "Detalle");
        _dgvDespachos.Columns.Add("Total", "Total");
        _dgvDespachos.Columns.Add("Estado", "Estado");
        _dgvDespachos.Columns.Add("Motivo", "Motivo de rechazo");
        _dgvDespachos.Columns["Id"]!.FillWeight = 30;
        _dgvDespachos.Columns["Ruta"]!.FillWeight = 160;
        _dgvDespachos.Columns["Detalle"]!.FillWeight = 140;
        _dgvDespachos.Columns["Motivo"]!.FillWeight = 200;
        _dgvDespachos.Columns["Total"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        tabla.Controls.Add(_dgvDespachos, 0, 1);

        // --- Fila 2: título de las matrices
        _lblOcupacion = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        tabla.Controls.Add(_lblOcupacion, 0, 2);

        // --- Fila 3: matriz de ocupación y matriz de tarifas lado a lado
        var division = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        division.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        division.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        _dgvOcupacion = CrearMatrizGrilla();
        _dgvTarifas = CrearMatrizGrilla();
        division.Controls.Add(_dgvOcupacion, 0, 0);
        division.Controls.Add(_dgvTarifas, 1, 0);
        tabla.Controls.Add(division, 0, 3);

        pagina.Controls.Add(tabla);

        // Eventos
        _dtpProgramacion.ValueChanged += (s, e) => { RefrescarGrilla(); RefrescarMatrices(); };
        _chkTodasFechas.CheckedChanged += (s, e) => RefrescarGrilla();
        btnHoy.Click += (s, e) => _dtpProgramacion.Value = DateTime.Today;
        btnEliminar.Click += BtnEliminar_Click;

        return pagina;
    }

    private static DataGridView CrearMatrizGrilla()
    {
        DataGridView grilla = CrearGrilla();
        grilla.SelectionMode = DataGridViewSelectionMode.CellSelect;
        grilla.Columns.Add("Ruta", "Ruta");
        grilla.Columns[0].FillWeight = 180;
        for (int c = 0; c < Catalogo.CantidadFranjas; c++)
        {
            int indice = grilla.Columns.Add("F" + c, Catalogo.Franjas[c]);
            grilla.Columns[indice].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        return grilla;
    }

    private void RefrescarGrilla()
    {
        DateTime? fecha = _chkTodasFechas.Checked ? null : _dtpProgramacion.Value.Date;
        List<Despacho> lista = _gestor.Filtrar(fecha, FiltroTipo.Todos);

        _dgvDespachos.Rows.Clear();
        foreach (Despacho d in lista)
        {
            int fila = _dgvDespachos.Rows.Add(
                d.Id,
                Formatos.Fecha(d.Fecha),
                d.Tipo,
                d.NombreRuta,
                d.NombreFranja,
                d.Placa,
                d.Conductor,
                d.Detalle,
                Formatos.Moneda(d.CalcularTotal()),   // polimorfismo: cada objeto calcula a su manera
                d.Estado,
                d.MotivoRechazo);

            if (!d.EsAprobado)
            {
                _dgvDespachos.Rows[fila].DefaultCellStyle.BackColor = Color.MistyRose;
            }
        }
    }

    /// <summary>Recorre la matriz de ocupación con dos for anidados y colorea cada celda.</summary>
    private void RefrescarMatrices()
    {
        DateTime fecha = _dtpProgramacion.Value.Date;
        int[,] ocupacion = _gestor.ConstruirMatrizOcupacion(fecha);

        _lblOcupacion.Text = $"Ocupación del {Formatos.Fecha(fecha)} (aprobados / cupo)            " +
                             "Tarifa base por pasajero (COP)";

        _dgvOcupacion.Rows.Clear();
        _dgvTarifas.Rows.Clear();

        for (int fila = 0; fila < Catalogo.CantidadRutas; fila++)
        {
            int filaOcupacion = _dgvOcupacion.Rows.Add(Catalogo.Rutas[fila]);
            int filaTarifa = _dgvTarifas.Rows.Add(Catalogo.Rutas[fila]);

            for (int col = 0; col < Catalogo.CantidadFranjas; col++)
            {
                int usados = ocupacion[fila, col];
                int cupo = Catalogo.CuposPorFranja[fila, col];
                DataGridViewCell celda = _dgvOcupacion.Rows[filaOcupacion].Cells[col + 1];
                celda.Value = $"{usados} / {cupo}";

                if (usados >= cupo)
                {
                    celda.Style.BackColor = Color.LightCoral;      // franja llena
                }
                else if (usados > 0)
                {
                    celda.Style.BackColor = Color.LightGreen;      // con despachos y cupo disponible
                }

                _dgvTarifas.Rows[filaTarifa].Cells[col + 1].Value =
                    Formatos.Moneda(Catalogo.TarifasBasePasajero[fila, col]);
            }
        }
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        if (_dgvDespachos.CurrentRow == null)
        {
            MessageBox.Show("Seleccione un despacho en la tabla.", "Eliminar",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        int id = Convert.ToInt32(_dgvDespachos.CurrentRow.Cells["Id"].Value);
        DialogResult respuesta = MessageBox.Show($"¿Eliminar el despacho #{id}?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (respuesta == DialogResult.Yes && _gestor.Eliminar(id))
        {
            RefrescarTodo();
        }
    }
}
