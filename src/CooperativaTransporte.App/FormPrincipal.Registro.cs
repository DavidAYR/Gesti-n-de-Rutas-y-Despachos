using System.Globalization;
using CooperativaTransporte.Dominio;

namespace CooperativaTransporte.App;

/// <summary>PESTAÑA 1 – Registro de despacho: controles de entrada, ErrorProvider y MessageBox.</summary>
public partial class FormPrincipal
{
    private DateTimePicker _dtpFecha = null!;
    private ComboBox _cmbRuta = null!;
    private ComboBox _cmbFranja = null!;
    private TextBox _txtPlaca = null!;
    private TextBox _txtConductor = null!;
    private DateTimePicker _dtpSoat = null!;
    private RadioButton _rbPasajeros = null!;
    private RadioButton _rbCarga = null!;

    private GroupBox _grpPasajeros = null!;
    private ComboBox _cmbServicio = null!;
    private TextBox _txtPasajeros = null!;
    private TextBox _txtCapacidad = null!;

    private GroupBox _grpCarga = null!;
    private TextBox _txtKilos = null!;
    private TextBox _txtCapacidadKg = null!;
    private TextBox _txtTarifaKilo = null!;
    private CheckBox _chkFragil = null!;

    private Label _lblVistaPrevia = null!;
    private ListBox _lstRecientes = null!;

    private TabPage CrearPestanaRegistro()
    {
        var pagina = new TabPage("1. Registro de despacho") { Padding = new Padding(10) };

        const int xEtiqueta = 25;
        const int xControl = 190;
        const int ancho = 270;
        int y = 20;

        var titulo = new Label
        {
            Text = "Nuevo despacho",
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            Location = new Point(xEtiqueta, y),
            AutoSize = true
        };
        pagina.Controls.Add(titulo);
        y += 45;

        _dtpFecha = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(xControl, y), Width = ancho };
        pagina.Controls.Add(CrearEtiqueta("Fecha del despacho:", xEtiqueta, y));
        pagina.Controls.Add(_dtpFecha);
        y += 36;

        _cmbRuta = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(xControl, y), Width = ancho };
        _cmbRuta.Items.AddRange(Catalogo.Rutas);
        pagina.Controls.Add(CrearEtiqueta("Ruta:", xEtiqueta, y));
        pagina.Controls.Add(_cmbRuta);
        y += 36;

        _cmbFranja = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(xControl, y), Width = ancho };
        for (int i = 0; i < Catalogo.CantidadFranjas; i++)
        {
            _cmbFranja.Items.Add($"{Catalogo.Franjas[i]} ({Catalogo.HorariosFranja[i]})");
        }
        pagina.Controls.Add(CrearEtiqueta("Franja horaria:", xEtiqueta, y));
        pagina.Controls.Add(_cmbFranja);
        y += 36;

        _txtPlaca = new TextBox { Location = new Point(xControl, y), Width = ancho, MaxLength = 6, CharacterCasing = CharacterCasing.Upper, PlaceholderText = "Ej: SXA123" };
        pagina.Controls.Add(CrearEtiqueta("Placa del vehículo:", xEtiqueta, y));
        pagina.Controls.Add(_txtPlaca);
        y += 36;

        _txtConductor = new TextBox { Location = new Point(xControl, y), Width = ancho, MaxLength = 60 };
        pagina.Controls.Add(CrearEtiqueta("Conductor:", xEtiqueta, y));
        pagina.Controls.Add(_txtConductor);
        y += 36;

        _dtpSoat = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(xControl, y), Width = ancho };
        pagina.Controls.Add(CrearEtiqueta("Vencimiento SOAT:", xEtiqueta, y));
        pagina.Controls.Add(_dtpSoat);
        y += 40;

        _rbPasajeros = new RadioButton { Text = "Pasajeros", Location = new Point(xControl, y), AutoSize = true, Checked = true };
        _rbCarga = new RadioButton { Text = "Carga / encomienda", Location = new Point(xControl + 110, y), AutoSize = true };
        pagina.Controls.Add(CrearEtiqueta("Tipo de despacho:", xEtiqueta, y));
        pagina.Controls.Add(_rbPasajeros);
        pagina.Controls.Add(_rbCarga);
        y += 36;

        // --- Datos propios de pasajeros
        _grpPasajeros = new GroupBox { Text = "Datos de pasajeros", Location = new Point(xEtiqueta - 5, y), Size = new Size(450, 135) };
        _cmbServicio = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(165, 25), Width = 260 };
        _cmbServicio.Items.AddRange(new object[] { "Corriente", "Ejecutivo (+30 %)", "Preferencial (+60 %)" });
        _cmbServicio.SelectedIndex = 0;
        _txtPasajeros = new TextBox { Location = new Point(165, 60), Width = 260 };
        _txtCapacidad = new TextBox { Location = new Point(165, 95), Width = 260, PlaceholderText = "Puestos del vehículo" };
        _grpPasajeros.Controls.Add(CrearEtiqueta("Tipo de servicio:", 10, 25));
        _grpPasajeros.Controls.Add(_cmbServicio);
        _grpPasajeros.Controls.Add(CrearEtiqueta("N.º de pasajeros:", 10, 60));
        _grpPasajeros.Controls.Add(_txtPasajeros);
        _grpPasajeros.Controls.Add(CrearEtiqueta("Capacidad (puestos):", 10, 95));
        _grpPasajeros.Controls.Add(_txtCapacidad);
        pagina.Controls.Add(_grpPasajeros);

        // --- Datos propios de carga (mismo lugar, se alterna la visibilidad)
        _grpCarga = new GroupBox { Text = "Datos de carga", Location = _grpPasajeros.Location, Size = new Size(450, 135), Visible = false };
        _txtKilos = new TextBox { Location = new Point(165, 22), Width = 260 };
        _txtCapacidadKg = new TextBox { Location = new Point(165, 52), Width = 260 };
        _txtTarifaKilo = new TextBox { Location = new Point(165, 82), Width = 260, PlaceholderText = "COP por kilo" };
        _chkFragil = new CheckBox { Text = "Carga frágil (+15 %)", Location = new Point(165, 110), AutoSize = true };
        _grpCarga.Controls.Add(CrearEtiqueta("Peso (kg):", 10, 22));
        _grpCarga.Controls.Add(_txtKilos);
        _grpCarga.Controls.Add(CrearEtiqueta("Capacidad (kg):", 10, 52));
        _grpCarga.Controls.Add(_txtCapacidadKg);
        _grpCarga.Controls.Add(CrearEtiqueta("Tarifa por kilo:", 10, 82));
        _grpCarga.Controls.Add(_txtTarifaKilo);
        _grpCarga.Controls.Add(_chkFragil);
        pagina.Controls.Add(_grpCarga);
        y += 150;

        var btnRegistrar = new Button { Text = "Registrar despacho", Location = new Point(xEtiqueta, y), Size = new Size(170, 38), BackColor = Color.FromArgb(32, 120, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        var btnCalcular = new Button { Text = "Calcular total", Location = new Point(xEtiqueta + 180, y), Size = new Size(130, 38) };
        var btnLimpiar = new Button { Text = "Limpiar", Location = new Point(xEtiqueta + 320, y), Size = new Size(110, 38) };
        pagina.Controls.Add(btnRegistrar);
        pagina.Controls.Add(btnCalcular);
        pagina.Controls.Add(btnLimpiar);
        y += 48;

        _lblVistaPrevia = new Label { Location = new Point(xEtiqueta, y), Size = new Size(450, 25), Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = Color.FromArgb(32, 78, 120) };
        pagina.Controls.Add(_lblVistaPrevia);

        // --- Lista de despachos recientes (control de lista)
        pagina.Controls.Add(new Label { Text = "Despachos registrados (más recientes primero)", Location = new Point(530, 25), AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) });
        _lstRecientes = new ListBox
        {
            Location = new Point(530, 55),
            Size = new Size(590, 560),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            Font = new Font("Consolas", 9.5f),
            HorizontalScrollbar = true
        };
        pagina.Controls.Add(_lstRecientes);

        // Eventos
        _rbPasajeros.CheckedChanged += (s, e) => AlternarTipo();
        btnRegistrar.Click += BtnRegistrar_Click;
        btnCalcular.Click += BtnCalcular_Click;
        btnLimpiar.Click += (s, e) => LimpiarFormulario();
        _txtPlaca.Leave += (s, e) => ValidarPlacaAlSalir();

        LimpiarFormulario();
        return pagina;
    }

    private void AlternarTipo()
    {
        _grpPasajeros.Visible = _rbPasajeros.Checked;
        _grpCarga.Visible = _rbCarga.Checked;
        _errorProvider.Clear();
        _lblVistaPrevia.Text = "";
    }

    private void ValidarPlacaAlSalir()
    {
        if (_txtPlaca.Text.Length > 0 && !Validaciones.EsPlacaValida(_txtPlaca.Text))
        {
            _errorProvider.SetError(_txtPlaca, "Formato AAA123: 3 letras y 3 números.");
        }
        else
        {
            _errorProvider.SetError(_txtPlaca, "");
        }
    }

    /// <summary>Validación de DATOS DE ENTRADA (vacíos, numéricos, mayores que cero) con ErrorProvider.</summary>
    private bool ValidarFormulario()
    {
        _errorProvider.Clear();
        bool valido = true;

        if (string.IsNullOrWhiteSpace(_txtPlaca.Text))
        {
            _errorProvider.SetError(_txtPlaca, "La placa es obligatoria.");
            valido = false;
        }
        else if (!Validaciones.EsPlacaValida(_txtPlaca.Text))
        {
            _errorProvider.SetError(_txtPlaca, "Formato AAA123: 3 letras y 3 números.");
            valido = false;
        }

        if (!Validaciones.EsNombreValido(_txtConductor.Text))
        {
            _errorProvider.SetError(_txtConductor, "Escriba el nombre del conductor (mínimo 3 letras).");
            valido = false;
        }

        if (_cmbRuta.SelectedIndex < 0)
        {
            _errorProvider.SetError(_cmbRuta, "Seleccione una ruta.");
            valido = false;
        }

        if (_cmbFranja.SelectedIndex < 0)
        {
            _errorProvider.SetError(_cmbFranja, "Seleccione la franja horaria.");
            valido = false;
        }

        if (_rbPasajeros.Checked)
        {
            valido &= ValidarEntero(_txtPasajeros, "pasajeros");
            valido &= ValidarEntero(_txtCapacidad, "capacidad");
        }
        else
        {
            valido &= ValidarDecimal(_txtKilos, "peso");
            valido &= ValidarDecimal(_txtCapacidadKg, "capacidad");
            valido &= ValidarDecimal(_txtTarifaKilo, "tarifa");
        }

        return valido;
    }

    private bool ValidarEntero(TextBox caja, string nombre)
    {
        if (!int.TryParse(caja.Text.Trim(), out int valor))
        {
            _errorProvider.SetError(caja, $"Ingrese un número entero para {nombre}.");
            return false;
        }
        if (valor <= 0)
        {
            _errorProvider.SetError(caja, $"El valor de {nombre} debe ser mayor que cero.");
            return false;
        }
        return true;
    }

    private bool ValidarDecimal(TextBox caja, string nombre)
    {
        if (!double.TryParse(caja.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out double valor))
        {
            _errorProvider.SetError(caja, $"Ingrese un número válido para {nombre}.");
            return false;
        }
        if (valor <= 0)
        {
            _errorProvider.SetError(caja, $"El valor de {nombre} debe ser mayor que cero.");
            return false;
        }
        return true;
    }

    /// <summary>Construye el objeto correcto según el tipo elegido (la variable es de tipo base Despacho).</summary>
    private Despacho CrearDespachoDesdeFormulario()
    {
        DateTime fecha = _dtpFecha.Value.Date;
        string placa = _txtPlaca.Text.Trim();
        string conductor = _txtConductor.Text.Trim();
        int ruta = _cmbRuta.SelectedIndex;
        Franja franja = (Franja)_cmbFranja.SelectedIndex;
        DateTime soat = _dtpSoat.Value.Date;

        if (_rbPasajeros.Checked)
        {
            return new DespachoPasajeros(fecha, placa, conductor, ruta, franja, soat,
                (TipoServicio)_cmbServicio.SelectedIndex,
                int.Parse(_txtPasajeros.Text.Trim()),
                int.Parse(_txtCapacidad.Text.Trim()));
        }

        return new DespachoCarga(fecha, placa, conductor, ruta, franja, soat,
            double.Parse(_txtKilos.Text.Trim(), CultureInfo.CurrentCulture),
            double.Parse(_txtCapacidadKg.Text.Trim(), CultureInfo.CurrentCulture),
            double.Parse(_txtTarifaKilo.Text.Trim(), CultureInfo.CurrentCulture),
            _chkFragil.Checked);
    }

    private void BtnCalcular_Click(object? sender, EventArgs e)
    {
        if (!ValidarFormulario())
        {
            _lblVistaPrevia.Text = "Complete los datos para calcular el total.";
            return;
        }
        Despacho d = CrearDespachoDesdeFormulario();
        _lblVistaPrevia.Text = "Total estimado: " + Formatos.Moneda(d.CalcularTotal());   // polimorfismo
    }

    private void BtnRegistrar_Click(object? sender, EventArgs e)
    {
        if (!ValidarFormulario())
        {
            MessageBox.Show("Hay datos incompletos o inválidos.\nRevise los campos marcados con el ícono rojo.",
                "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Despacho despacho = CrearDespachoDesdeFormulario();
        bool aprobado = _gestor.Registrar(despacho);

        if (aprobado)
        {
            MessageBox.Show(
                $"Despacho #{despacho.Id} APROBADO.\n\n" +
                $"Ruta: {despacho.NombreRuta} ({despacho.NombreFranja})\n" +
                $"Vehículo: {despacho.Placa} - {despacho.Conductor}\n" +
                $"Detalle: {despacho.Detalle}\n" +
                $"Total a cobrar: {Formatos.Moneda(despacho.CalcularTotal())}",
                "Despacho aprobado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
        }
        else
        {
            string motivos = "• " + despacho.MotivoRechazo.Replace("; ", "\n• ");
            MessageBox.Show(
                $"Despacho #{despacho.Id} RECHAZADO.\n\nMotivos:\n{motivos}\n\n" +
                "Quedó registrado como rechazado para la auditoría.",
                "Despacho rechazado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        RefrescarTodo();
    }

    private void LimpiarFormulario()
    {
        _errorProvider.Clear();
        _dtpFecha.Value = DateTime.Today;
        _dtpSoat.Value = DateTime.Today.AddMonths(6);
        _cmbRuta.SelectedIndex = -1;
        _cmbFranja.SelectedIndex = -1;
        _txtPlaca.Clear();
        _txtConductor.Clear();
        _cmbServicio.SelectedIndex = 0;
        _txtPasajeros.Clear();
        _txtCapacidad.Clear();
        _txtKilos.Clear();
        _txtCapacidadKg.Clear();
        _txtTarifaKilo.Clear();
        _chkFragil.Checked = false;
        _rbPasajeros.Checked = true;
        _lblVistaPrevia.Text = "";
        _txtPlaca.Focus();
    }

    private void RefrescarListaRecientes()
    {
        _lstRecientes.BeginUpdate();
        _lstRecientes.Items.Clear();
        // Recorrido de la lista al revés: el último registrado aparece primero.
        for (int i = _gestor.Cantidad - 1; i >= 0; i--)
        {
            Despacho d = _gestor.Despachos[i];
            string marca = d.EsAprobado ? "[OK]" : "[X] ";
            _lstRecientes.Items.Add($"{marca} {d}  {Formatos.Moneda(d.CalcularTotal())}");
        }
        _lstRecientes.EndUpdate();
    }
}
