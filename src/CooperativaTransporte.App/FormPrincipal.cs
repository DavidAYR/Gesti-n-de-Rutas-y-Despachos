using CooperativaTransporte.Dominio;

namespace CooperativaTransporte.App;

/// <summary>
/// Ventana principal creada 100 % por código (sin diseñador).
/// La clase es "partial": cada pestaña está en su propio archivo.
///   FormPrincipal.Registro.cs      → Pestaña 1
///   FormPrincipal.Programacion.cs  → Pestaña 2
///   FormPrincipal.Informe.cs       → Pestaña 3
/// El formulario solo captura datos, llama al dominio y muestra resultados.
/// </summary>
public partial class FormPrincipal : Form
{
    private readonly GestorDespachos _gestor = new GestorDespachos();
    private readonly ArchivoDespachos _archivo;
    private readonly ErrorProvider _errorProvider = new ErrorProvider();
    private readonly TabControl _tabs = new TabControl();

    public FormPrincipal()
    {
        Text = "Cooperativa de Transporte Intermunicipal - Gestión de Rutas y Despachos";
        Size = new Size(1180, 740);
        MinimumSize = new Size(1050, 680);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);

        _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        string carpetaDatos = Path.Combine(AppContext.BaseDirectory, "datos");
        _archivo = new ArchivoDespachos(Path.Combine(carpetaDatos, "despachos.txt"));

        _tabs.Dock = DockStyle.Fill;
        _tabs.TabPages.Add(CrearPestanaRegistro());
        _tabs.TabPages.Add(CrearPestanaProgramacion());
        _tabs.TabPages.Add(CrearPestanaInforme());
        Controls.Add(_tabs);

        // Eventos del formulario
        _tabs.SelectedIndexChanged += (s, e) => RefrescarTodo();
        Load += FormPrincipal_Load;
        FormClosing += FormPrincipal_FormClosing;
    }

    private void FormPrincipal_Load(object? sender, EventArgs e)
    {
        // Primera ejecución: si no hay datos, se copian los de ejemplo para la demostración.
        string ejemplo = Path.Combine(AppContext.BaseDirectory, "datos", "ejemplo_despachos.txt");
        if (!_archivo.Existe && File.Exists(ejemplo))
        {
            File.Copy(ejemplo, _archivo.RutaArchivo);
        }

        CargarDatos(mostrarMensaje: false);
    }

    private void FormPrincipal_FormClosing(object? sender, FormClosingEventArgs e)
    {
        try
        {
            _archivo.Guardar(_gestor.Despachos);   // guardado automático al cerrar
        }
        catch (Exception ex)
        {
            DialogResult r = MessageBox.Show(
                "No se pudieron guardar los datos:\n" + ex.Message + "\n\n¿Desea cerrar de todas formas?",
                "Error al guardar", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (r == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }

    private void RefrescarTodo()
    {
        RefrescarListaRecientes();
        RefrescarGrilla();
        RefrescarMatrices();
        ActualizarEstadoArchivo();
    }

    // ----------------------------------------------------------------- utilidades de construcción

    private static Label CrearEtiqueta(string texto, int x, int y)
    {
        return new Label { Text = texto, Location = new Point(x, y + 3), AutoSize = true };
    }

    private static DataGridView CrearGrilla()
    {
        var grilla = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            BackgroundColor = Color.White
        };
        grilla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        grilla.EnableHeadersVisualStyles = false;
        grilla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(32, 78, 120);
        grilla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        return grilla;
    }
}
