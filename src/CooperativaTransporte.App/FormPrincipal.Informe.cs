using CooperativaTransporte.Dominio;

namespace CooperativaTransporte.App;

/// <summary>PESTAÑA 3 – Informe operativo, filtros y persistencia en archivo de texto.</summary>
public partial class FormPrincipal
{
    private CheckBox _chkInformeFecha = null!;
    private DateTimePicker _dtpInforme = null!;
    private ComboBox _cmbFiltroTipo = null!;
    private TextBox _txtInforme = null!;
    private Label _lblAlertas = null!;
    private Label _lblArchivo = null!;

    private TabPage CrearPestanaInforme()
    {
        var pagina = new TabPage("3. Informe y persistencia") { Padding = new Padding(10) };

        // --- Panel izquierdo: filtros y botones
        var panel = new Panel { Dock = DockStyle.Left, Width = 300, Padding = new Padding(5) };
        int y = 10;

        panel.Controls.Add(new Label { Text = "Filtros del informe", Location = new Point(10, y), AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold) });
        y += 35;

        _chkInformeFecha = new CheckBox { Text = "Solo la fecha:", Location = new Point(10, y + 3), AutoSize = true };
        _dtpInforme = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(130, y), Width = 140, Enabled = false };
        panel.Controls.Add(_chkInformeFecha);
        panel.Controls.Add(_dtpInforme);
        y += 38;

        _cmbFiltroTipo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(130, y), Width = 140 };
        _cmbFiltroTipo.Items.AddRange(new object[] { "Todos", "Pasajeros", "Carga" });
        _cmbFiltroTipo.SelectedIndex = 0;
        panel.Controls.Add(CrearEtiqueta("Tipo:", 10, y));
        panel.Controls.Add(_cmbFiltroTipo);
        y += 45;

        var btnGenerar = new Button { Text = "Generar informe", Location = new Point(10, y), Size = new Size(260, 38), BackColor = Color.FromArgb(32, 78, 120), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        panel.Controls.Add(btnGenerar);
        y += 48;

        var btnExportar = new Button { Text = "Exportar informe a .txt", Location = new Point(10, y), Size = new Size(260, 34) };
        panel.Controls.Add(btnExportar);
        y += 60;

        panel.Controls.Add(new Label { Text = "Archivo de datos", Location = new Point(10, y), AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold) });
        y += 32;

        var btnGuardar = new Button { Text = "Guardar datos", Location = new Point(10, y), Size = new Size(125, 34) };
        var btnCargar = new Button { Text = "Cargar datos", Location = new Point(145, y), Size = new Size(125, 34) };
        panel.Controls.Add(btnGuardar);
        panel.Controls.Add(btnCargar);
        y += 44;

        _lblArchivo = new Label { Location = new Point(10, y), Size = new Size(270, 95), ForeColor = Color.DimGray };
        panel.Controls.Add(_lblArchivo);
        y += 105;

        _lblAlertas = new Label { Location = new Point(10, y), Size = new Size(270, 140), Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        panel.Controls.Add(_lblAlertas);

        // --- Área del informe
        _txtInforme = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font("Consolas", 10f),
            BackColor = Color.White
        };

        pagina.Controls.Add(_txtInforme);
        pagina.Controls.Add(panel);

        // Eventos
        _chkInformeFecha.CheckedChanged += (s, e) => _dtpInforme.Enabled = _chkInformeFecha.Checked;
        btnGenerar.Click += (s, e) => GenerarInforme();
        btnExportar.Click += BtnExportar_Click;
        btnGuardar.Click += (s, e) => GuardarDatos();
        btnCargar.Click += BtnCargar_Click;

        return pagina;
    }

    private void GenerarInforme()
    {
        DateTime? fecha = _chkInformeFecha.Checked ? _dtpInforme.Value.Date : null;
        FiltroTipo tipo = (FiltroTipo)_cmbFiltroTipo.SelectedIndex;

        List<Despacho> lista = _gestor.Filtrar(fecha, tipo);
        var informe = new InformeOperativo(lista, fecha, tipo);
        _txtInforme.Text = informe.GenerarTexto();

        // Las banderas del informe deciden las alertas visuales.
        if (informe.TotalRegistros == 0)
        {
            _lblAlertas.ForeColor = Color.DimGray;
            _lblAlertas.Text = "Sin datos para los filtros elegidos.";
        }
        else if (informe.HayRechazados || informe.HayFranjaLlena)
        {
            _lblAlertas.ForeColor = Color.Firebrick;
            _lblAlertas.Text = "ATENCIÓN\n" +
                               (informe.HayRechazados ? $"• {informe.Rechazados} despacho(s) rechazado(s)\n" : "") +
                               (informe.HayFranjaLlena ? $"• {informe.FranjasLlenas} franja(s) sin cupo\n" : "") +
                               (informe.SuperaMeta ? "• Meta de recaudo cumplida" : "• Meta de recaudo NO cumplida");
        }
        else
        {
            _lblAlertas.ForeColor = Color.ForestGreen;
            _lblAlertas.Text = "Operación sin novedades.\n" +
                               (informe.SuperaMeta ? "Meta de recaudo cumplida." : "Meta de recaudo NO cumplida.");
        }
    }

    private void BtnExportar_Click(object? sender, EventArgs e)
    {
        if (_txtInforme.TextLength == 0)
        {
            GenerarInforme();
        }

        using var dialogo = new SaveFileDialog
        {
            Filter = "Archivo de texto (*.txt)|*.txt",
            FileName = $"informe_operativo_{DateTime.Now:yyyyMMdd_HHmm}.txt"
        };

        if (dialogo.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                File.WriteAllText(dialogo.FileName, _txtInforme.Text);
                MessageBox.Show("Informe exportado correctamente.", "Exportar",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo exportar el informe:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void GuardarDatos()
    {
        try
        {
            _archivo.Guardar(_gestor.Despachos);
            ActualizarEstadoArchivo();
            MessageBox.Show($"Se guardaron {_gestor.Cantidad} despacho(s) en:\n{_archivo.RutaArchivo}",
                "Datos guardados", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo guardar el archivo:\n" + ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCargar_Click(object? sender, EventArgs e)
    {
        DialogResult r = MessageBox.Show(
            "Se reemplazarán los datos en pantalla por los del archivo.\n¿Desea continuar?",
            "Cargar datos", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (r == DialogResult.Yes)
        {
            CargarDatos(mostrarMensaje: true);
        }
    }

    private void CargarDatos(bool mostrarMensaje)
    {
        try
        {
            ResultadoCarga resultado = _archivo.Cargar();
            _gestor.ReemplazarTodos(resultado.Despachos);
            RefrescarTodo();

            if (mostrarMensaje || resultado.LineasDescartadas > 0)
            {
                string detalle = $"Líneas leídas: {resultado.LineasLeidas}\n" +
                                 $"Despachos cargados: {resultado.LineasValidas}\n" +
                                 $"Líneas descartadas: {resultado.LineasDescartadas}";
                if (resultado.Errores.Count > 0)
                {
                    detalle += "\n\n" + string.Join("\n", resultado.Errores.Take(5));
                }
                MessageBox.Show(detalle, "Carga de datos", MessageBoxButtons.OK,
                    resultado.LineasDescartadas > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo leer el archivo:\n" + ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ActualizarEstadoArchivo()
    {
        _lblArchivo.Text = $"Ruta: {_archivo.RutaArchivo}\n\n" +
                           $"Despachos en memoria: {_gestor.Cantidad}\n" +
                           "(Se guarda automáticamente al cerrar)";
    }
}
