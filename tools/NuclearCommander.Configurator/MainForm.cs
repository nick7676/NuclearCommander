using System.Diagnostics;
using System.Globalization;

namespace NuclearCommander.Configurator;

internal sealed class MainForm : Form
{
    private readonly CheckBox _enabled = new() { Text = "Enable Nuclear Commander", AutoSize = true };
    private readonly TextBox _toggleKey = new();
    private readonly TextBox _rotateLeftKey = new();
    private readonly TextBox _rotateRightKey = new();
    private readonly TextBox _confirmKey = new();
    private readonly NumericUpDown _rotationSpeed = CreateNumber(1, 720, 90, 1);
    private readonly NumericUpDown _maximumSlope = CreateNumber(0, 89, 18, 1);
    private readonly NumericUpDown _fobRadius = CreateNumber(0, 100000, 1000, 0);
    private readonly CheckBox _holdPosition = new() { Text = "New vehicles hold their position", AutoSize = true };
    private readonly DataGridView _pricesGrid = new();
    private readonly TextBox _priceSearch = new();
    private readonly Label _priceNotice = new();
    private readonly Label _status = new();
    private readonly Label _pathLabel = new();

    private CommanderConfig? _config;
    private bool _loading;
    private bool _dirty;

    public MainForm(string? requestedPath)
    {
        Text = "Nuclear Commander Configurator";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(680, 650);
        Size = new Size(720, 700);
        Font = new Font("Segoe UI", 9.5F);
        BackColor = Color.FromArgb(244, 246, 250);

        BuildInterface();
        RegisterChangeEvents();
        Shown += (_, _) => OpenConfiguration(CommanderConfigLocator.Find(requestedPath));
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_dirty)
        {
            DialogResult result = MessageBox.Show(
                this,
                "Close without saving your changes?",
                "Unsaved changes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            e.Cancel = result != DialogResult.Yes;
        }

        base.OnFormClosing(e);
    }

    private void BuildInterface()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        Controls.Add(root);

        Panel header = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 36, 52),
            Padding = new Padding(24, 17, 24, 12)
        };
        root.Controls.Add(header, 0, 0);

        Label title = new()
        {
            Text = "Nuclear Commander",
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 20F),
            AutoSize = true,
            Location = new Point(20, 14)
        };
        header.Controls.Add(title);

        Label subtitle = new()
        {
            Text = "Configure the mod without starting the game",
            ForeColor = Color.FromArgb(185, 196, 214),
            AutoSize = true,
            Location = new Point(23, 59)
        };
        header.Controls.Add(subtitle);

        TabControl tabs = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Point(16, 6)
        };
        root.Controls.Add(tabs, 0, 1);
        tabs.TabPages.Add(CreateSettingsPage());
        tabs.TabPages.Add(CreateVehiclePricesPage());

        Panel footer = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20, 12, 20, 10)
        };
        root.Controls.Add(footer, 0, 2);

        _status.Text = "Close Nuclear Option before saving changes.";
        _status.ForeColor = Color.DimGray;
        _status.AutoSize = true;
        _status.Location = new Point(20, 18);
        footer.Controls.Add(_status);

        FlowLayoutPanel buttons = new()
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        footer.Controls.Add(buttons);

        buttons.Controls.Add(CreateButton("Choose game folder", ChooseGameFolder, false));
        buttons.Controls.Add(CreateButton("Reload", Reload, false));
        buttons.Controls.Add(CreateButton("Defaults", ResetDefaults, false));
        buttons.Controls.Add(CreateButton("Save", Save, true));
    }

    private TabPage CreateSettingsPage()
    {
        TabPage page = new("Mod settings")
        {
            BackColor = Color.FromArgb(244, 246, 250),
            Padding = new Padding(14)
        };

        Panel body = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
        page.Controls.Add(body);

        TableLayoutPanel sections = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 4
        };
        body.Controls.Add(sections);

        sections.Controls.Add(CreateGeneralSection(), 0, 0);
        sections.Controls.Add(CreateControlsSection(), 0, 1);
        sections.Controls.Add(CreatePlacementSection(), 0, 2);

        _pathLabel.AutoSize = true;
        _pathLabel.ForeColor = Color.DimGray;
        _pathLabel.Margin = new Padding(8, 10, 8, 0);
        sections.Controls.Add(_pathLabel, 0, 3);
        return page;
    }

    private TabPage CreateVehiclePricesPage()
    {
        TabPage page = new("Vehicle prices")
        {
            BackColor = Color.FromArgb(244, 246, 250),
            Padding = new Padding(14)
        };

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        page.Controls.Add(layout);

        _priceSearch.Dock = DockStyle.Fill;
        _priceSearch.PlaceholderText = "Search vehicles...";
        _priceSearch.TextChanged += (_, _) =>
        {
            CaptureVehiclePrices();
            PopulateVehiclePrices();
        };
        layout.Controls.Add(_priceSearch, 0, 0);

        _pricesGrid.Dock = DockStyle.Fill;
        _pricesGrid.AllowUserToAddRows = false;
        _pricesGrid.AllowUserToDeleteRows = false;
        _pricesGrid.AllowUserToResizeRows = false;
        _pricesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _pricesGrid.BackgroundColor = Color.White;
        _pricesGrid.BorderStyle = BorderStyle.FixedSingle;
        _pricesGrid.RowHeadersVisible = false;
        _pricesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _pricesGrid.MultiSelect = false;
        _pricesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Vehicle",
            HeaderText = "Vehicle",
            ReadOnly = true,
            FillWeight = 58
        });
        _pricesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Price",
            HeaderText = "Price ($)",
            FillWeight = 22
        });
        _pricesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Default",
            HeaderText = "Game default ($)",
            ReadOnly = true,
            FillWeight = 20
        });
        _pricesGrid.CellValueChanged += (_, _) => MarkDirty();
        _pricesGrid.CellValidating += ValidatePriceCell;
        layout.Controls.Add(_pricesGrid, 0, 1);

        _priceNotice.AutoSize = true;
        _priceNotice.ForeColor = Color.DimGray;
        _priceNotice.Anchor = AnchorStyles.Left;
        layout.Controls.Add(_priceNotice, 0, 2);
        return page;
    }

    private GroupBox CreateGeneralSection()
    {
        GroupBox group = CreateSection("General", 76);
        _enabled.Location = new Point(18, 32);
        group.Controls.Add(_enabled);
        return group;
    }

    private GroupBox CreateControlsSection()
    {
        GroupBox group = CreateSection("Controls", 190);
        TableLayoutPanel table = CreateSettingsTable(4);
        AddRow(table, 0, "Open / close menu", _toggleKey);
        AddRow(table, 1, "Rotate left", _rotateLeftKey);
        AddRow(table, 2, "Rotate right", _rotateRightKey);
        AddRow(table, 3, "Purchase / place", _confirmKey);
        group.Controls.Add(table);
        return group;
    }

    private GroupBox CreatePlacementSection()
    {
        GroupBox group = CreateSection("Placement", 190);
        TableLayoutPanel table = CreateSettingsTable(4);
        AddRow(table, 0, "Rotation speed", _rotationSpeed, "degrees / second");
        AddRow(table, 1, "Maximum slope", _maximumSlope, "degrees");
        AddRow(table, 2, "FOB placement radius", _fobRadius, "metres");
        AddRow(table, 3, string.Empty, _holdPosition);
        group.Controls.Add(table);
        return group;
    }

    private static GroupBox CreateSection(string title, int height)
    {
        return new GroupBox
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = height,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(12),
            BackColor = Color.White,
            Font = new Font("Segoe UI Semibold", 10F)
        };
    }

    private static TableLayoutPanel CreateSettingsTable(int rows)
    {
        TableLayoutPanel table = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = rows,
            Padding = new Padding(6, 6, 6, 2)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        for (int index = 0; index < rows; index++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
        }

        return table;
    }

    private static void AddRow(TableLayoutPanel table, int row, string label, Control editor, string suffix = "")
    {
        Label name = new()
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9.5F)
        };
        table.Controls.Add(name, 0, row);

        editor.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        editor.Font = new Font("Segoe UI", 9.5F);
        table.Controls.Add(editor, 1, row);

        if (suffix.Length > 0)
        {
            Label unit = new()
            {
                Text = suffix,
                AutoSize = true,
                ForeColor = Color.DimGray,
                Anchor = AnchorStyles.Left
            };
            table.Controls.Add(unit, 2, row);
        }
    }

    private static Button CreateButton(string text, EventHandler handler, bool primary)
    {
        Button button = new()
        {
            Text = text,
            AutoSize = true,
            Height = 36,
            Padding = new Padding(10, 3, 10, 3),
            Margin = new Padding(6, 0, 0, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Color.FromArgb(45, 113, 220) : Color.White,
            ForeColor = primary ? Color.White : Color.FromArgb(35, 45, 60)
        };
        button.FlatAppearance.BorderColor = primary
            ? Color.FromArgb(45, 113, 220)
            : Color.FromArgb(190, 196, 207);
        button.Click += handler;
        return button;
    }

    private static NumericUpDown CreateNumber(decimal minimum, decimal maximum, decimal value, int decimalPlaces)
    {
        return new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            DecimalPlaces = decimalPlaces,
            Increment = decimalPlaces > 0 ? 0.5m : 100m,
            ThousandsSeparator = true
        };
    }

    private void RegisterChangeEvents()
    {
        _enabled.CheckedChanged += SettingChanged;
        _toggleKey.TextChanged += SettingChanged;
        _rotateLeftKey.TextChanged += SettingChanged;
        _rotateRightKey.TextChanged += SettingChanged;
        _confirmKey.TextChanged += SettingChanged;
        _rotationSpeed.ValueChanged += SettingChanged;
        _maximumSlope.ValueChanged += SettingChanged;
        _fobRadius.ValueChanged += SettingChanged;
        _holdPosition.CheckedChanged += SettingChanged;
    }

    private void OpenConfiguration(string? path)
    {
        if (path == null)
        {
            ChooseGameFolder(this, EventArgs.Empty);
            return;
        }

        try
        {
            _config = CommanderConfig.Load(path);
            DisplayConfiguration();
            _pathLabel.Text = path;
            SetStatus(File.Exists(path) ? "Configuration loaded." : "New configuration ready. Click Save to create it.", false);
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Unable to load configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DisplayConfiguration()
    {
        if (_config == null)
        {
            return;
        }

        _loading = true;
        _enabled.Checked = _config.Enabled;
        _toggleKey.Text = _config.TogglePlacementMode;
        _rotateLeftKey.Text = _config.RotatePreviewLeft;
        _rotateRightKey.Text = _config.RotatePreviewRight;
        _confirmKey.Text = _config.ConfirmPlacement;
        _rotationSpeed.Value = Clamp(_config.RotationSpeed, _rotationSpeed);
        _maximumSlope.Value = Clamp(_config.MaximumSlope, _maximumSlope);
        _fobRadius.Value = Clamp(_config.FobPlacementRadius, _fobRadius);
        _holdPosition.Checked = _config.HoldPosition;
        PopulateVehiclePrices();
        _dirty = false;
        _loading = false;
    }

    private void ReadConfiguration()
    {
        if (_config == null)
        {
            return;
        }

        _config.Enabled = _enabled.Checked;
        _config.TogglePlacementMode = NormalizeKey(_toggleKey.Text, "F6");
        _config.RotatePreviewLeft = NormalizeKey(_rotateLeftKey.Text, "Q");
        _config.RotatePreviewRight = NormalizeKey(_rotateRightKey.Text, "E");
        _config.ConfirmPlacement = NormalizeKey(_confirmKey.Text, "Mouse0");
        _config.RotationSpeed = _rotationSpeed.Value;
        _config.MaximumSlope = _maximumSlope.Value;
        _config.FobPlacementRadius = _fobRadius.Value;
        _config.HoldPosition = _holdPosition.Checked;
        CaptureVehiclePrices();
    }

    private void ChooseGameFolder(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = "Select the folder containing NuclearOption.exe",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
            SelectedPath = AppContext.BaseDirectory
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        string? path = CommanderConfigLocator.ResolveCandidate(dialog.SelectedPath);
        if (path == null)
        {
            MessageBox.Show(
                this,
                "BepInEx/config was not found in the selected folder.",
                "Invalid game folder",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        OpenConfiguration(path);
    }

    private void Reload(object? sender, EventArgs e)
    {
        if (_config != null)
        {
            OpenConfiguration(_config.Path);
        }
    }

    private void ResetDefaults(object? sender, EventArgs e)
    {
        if (_config == null)
        {
            return;
        }

        _config.ResetDefaults();
        DisplayConfiguration();
        _dirty = true;
        SetStatus("Default values loaded. Click Save to apply them.", true);
    }

    private void Save(object? sender, EventArgs e)
    {
        if (_config == null)
        {
            ChooseGameFolder(sender, e);
            return;
        }

        try
        {
            ReadConfiguration();
            _config.Save();
            _dirty = false;
            SetStatus("Configuration saved. A .bak backup was created.", false);
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Unable to save configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SettingChanged(object? sender, EventArgs e)
    {
        MarkDirty();
    }

    private void PopulateVehiclePrices()
    {
        _loading = true;
        _pricesGrid.Rows.Clear();

        if (_config == null || _config.VehiclePrices.Count == 0)
        {
            _priceNotice.Text = "Start Nuclear Option once with Nuclear Commander 0.15.0 to generate the vehicle catalogue.";
            _loading = false;
            return;
        }

        string search = _priceSearch.Text.Trim();
        foreach (VehiclePriceSetting price in _config.VehiclePrices.Where(price =>
                     search.Length == 0 ||
                     price.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                     price.Key.Contains(search, StringComparison.OrdinalIgnoreCase)))
        {
            int rowIndex = _pricesGrid.Rows.Add(
                price.DisplayName,
                FormatPrice(price.Value),
                FormatPrice(price.DefaultValue));
            _pricesGrid.Rows[rowIndex].Tag = price;
        }

        _priceNotice.Text = $"{_pricesGrid.Rows.Count} of {_config.VehiclePrices.Count} vehicles shown. Prices cannot be negative.";
        _loading = false;
    }

    private void CaptureVehiclePrices()
    {
        if (_config == null)
        {
            return;
        }

        foreach (DataGridViewRow row in _pricesGrid.Rows)
        {
            if (row.Tag is not VehiclePriceSetting price)
            {
                continue;
            }

            string value = Convert.ToString(row.Cells["Price"].Value) ?? string.Empty;
            if (TryParsePrice(value, out decimal parsed))
            {
                price.Value = parsed;
            }
        }
    }

    private void ValidatePriceCell(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        if (_pricesGrid.Columns[e.ColumnIndex].Name != "Price")
        {
            return;
        }

        string value = Convert.ToString(e.FormattedValue) ?? string.Empty;
        if (TryParsePrice(value, out _))
        {
            _pricesGrid.Rows[e.RowIndex].ErrorText = string.Empty;
            return;
        }

        e.Cancel = true;
        _pricesGrid.Rows[e.RowIndex].ErrorText = "Enter a valid price greater than or equal to zero.";
    }

    private void MarkDirty()
    {
        if (_loading)
        {
            return;
        }

        _dirty = true;
        SetStatus("Unsaved changes.", true);
    }

    private void SetStatus(string text, bool warning)
    {
        _status.Text = text;
        _status.ForeColor = warning ? Color.FromArgb(185, 91, 25) : Color.DimGray;
    }

    private static decimal Clamp(decimal value, NumericUpDown control)
    {
        return Math.Min(control.Maximum, Math.Max(control.Minimum, value));
    }

    private static string NormalizeKey(string value, string fallback)
    {
        string normalized = value.Trim();
        return normalized.Length > 0 ? normalized : fallback;
    }

    private static bool TryParsePrice(string value, out decimal price)
    {
        bool parsed = decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out price) ||
                      decimal.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out price);
        return parsed && price >= 0m;
    }

    private static string FormatPrice(decimal price)
    {
        return price.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
