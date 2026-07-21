using System.Globalization;
using System.Text;
using NuclearCommander.Shared;

namespace NuclearCommander.Configurator;

internal sealed class CommanderConfig
{
    public const string FileName = "com.nick7676.nuclearcommander.cfg";
    public const bool DefaultEnabled = true;
    public const string DefaultTogglePlacementMode = "F6";
    public const string DefaultRotatePreviewLeft = "Q";
    public const string DefaultRotatePreviewRight = "E";
    public const string DefaultConfirmPlacement = "Mouse0";
    public const decimal DefaultRotationSpeed = 90m;
    public const decimal DefaultMaximumSlope = 18m;
    public const decimal DefaultFobPlacementRadius = 1000m;
    public const bool DefaultHoldPosition = true;

    private readonly List<string> _lines;

    private CommanderConfig(string path, List<string> lines)
    {
        Path = path;
        _lines = lines;
    }

    public string Path { get; }
    public bool Enabled { get; set; } = DefaultEnabled;
    public string TogglePlacementMode { get; set; } = DefaultTogglePlacementMode;
    public string RotatePreviewLeft { get; set; } = DefaultRotatePreviewLeft;
    public string RotatePreviewRight { get; set; } = DefaultRotatePreviewRight;
    public string ConfirmPlacement { get; set; } = DefaultConfirmPlacement;
    public decimal RotationSpeed { get; set; } = DefaultRotationSpeed;
    public decimal MaximumSlope { get; set; } = DefaultMaximumSlope;
    public decimal FobPlacementRadius { get; set; } = DefaultFobPlacementRadius;
    public bool HoldPosition { get; set; } = DefaultHoldPosition;
    public List<VehiclePriceSetting> VehiclePrices { get; } = new();

    public static CommanderConfig Load(string path)
    {
        List<string> lines = File.Exists(path)
            ? File.ReadAllLines(path).ToList()
            : CreateDefaultLines();

        CommanderConfig config = new(path, lines);
        Dictionary<string, string> values = ParseValues(lines);

        config.Enabled = ReadBoolean(values, "General/Enabled", config.Enabled);
        config.TogglePlacementMode = ReadString(values, "Input/TogglePlacementMode", config.TogglePlacementMode);
        config.RotatePreviewLeft = ReadString(values, "Input/RotatePreviewLeft", config.RotatePreviewLeft);
        config.RotatePreviewRight = ReadString(values, "Input/RotatePreviewRight", config.RotatePreviewRight);
        config.ConfirmPlacement = ReadString(values, "Input/ConfirmPlacement", config.ConfirmPlacement);
        config.RotationSpeed = ReadDecimal(values, "Placement/RotationSpeed", config.RotationSpeed);
        config.MaximumSlope = ReadDecimal(values, "Placement/MaximumSlope", config.MaximumSlope);
        config.FobPlacementRadius = ReadDecimal(values, "Placement/FobPlacementRadius", config.FobPlacementRadius);
        config.HoldPosition = ReadBoolean(values, "Placement/HoldPosition", config.HoldPosition);
        List<VehiclePriceSetting> savedPrices = ParseVehiclePrices(lines).ToList();
        Dictionary<string, VehiclePriceSetting> savedPricesByKey = new(StringComparer.OrdinalIgnoreCase);
        foreach (VehiclePriceSetting savedPrice in savedPrices)
        {
            savedPricesByKey[savedPrice.Key] = savedPrice;
        }

        foreach (VehiclePriceDefinition definition in VehiclePriceCatalog.All)
        {
            decimal value = savedPricesByKey.TryGetValue(definition.Key, out VehiclePriceSetting? savedPrice)
                ? savedPrice.Value
                : definition.DefaultPrice;
            config.VehiclePrices.Add(new VehiclePriceSetting(
                definition.Key,
                definition.DisplayName,
                value,
                definition.DefaultPrice));
        }

        HashSet<string> catalogKeys = VehiclePriceCatalog.All
            .Select(definition => definition.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        config.VehiclePrices.AddRange(savedPrices.Where(price => !catalogKeys.Contains(price.Key)));
        return config;
    }

    public void Save()
    {
        SetValue("General", "Enabled", Enabled.ToString().ToLowerInvariant());
        SetValue("Input", "TogglePlacementMode", TogglePlacementMode);
        SetValue("Input", "RotatePreviewLeft", RotatePreviewLeft);
        SetValue("Input", "RotatePreviewRight", RotatePreviewRight);
        SetValue("Input", "ConfirmPlacement", ConfirmPlacement);
        SetValue("Placement", "RotationSpeed", FormatDecimal(RotationSpeed));
        SetValue("Placement", "MaximumSlope", FormatDecimal(MaximumSlope));
        SetValue("Placement", "FobPlacementRadius", FormatDecimal(FobPlacementRadius));
        SetValue("Placement", "HoldPosition", HoldPosition.ToString().ToLowerInvariant());

        foreach (VehiclePriceSetting price in VehiclePrices)
        {
            SetValue("Vehicle Prices", price.Key, FormatDecimal(price.Value));
        }

        string? directory = System.IO.Path.GetDirectoryName(Path);
        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(Path))
        {
            File.Copy(Path, $"{Path}.bak", true);
        }

        File.WriteAllLines(Path, _lines, new UTF8Encoding(false));
    }

    public void ResetDefaults()
    {
        Enabled = DefaultEnabled;
        TogglePlacementMode = DefaultTogglePlacementMode;
        RotatePreviewLeft = DefaultRotatePreviewLeft;
        RotatePreviewRight = DefaultRotatePreviewRight;
        ConfirmPlacement = DefaultConfirmPlacement;
        RotationSpeed = DefaultRotationSpeed;
        MaximumSlope = DefaultMaximumSlope;
        FobPlacementRadius = DefaultFobPlacementRadius;
        HoldPosition = DefaultHoldPosition;

        foreach (VehiclePriceSetting price in VehiclePrices)
        {
            price.Value = price.DefaultValue;
        }
    }

    private void SetValue(string section, string key, string value)
    {
        string currentSection = string.Empty;
        int sectionLine = -1;

        for (int index = 0; index < _lines.Count; index++)
        {
            string trimmed = _lines[index].Trim();
            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                currentSection = trimmed[1..^1].Trim();
                if (string.Equals(currentSection, section, StringComparison.OrdinalIgnoreCase))
                {
                    sectionLine = index;
                }

                continue;
            }

            if (!string.Equals(currentSection, section, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int separator = _lines[index].IndexOf('=');
            if (separator <= 0 ||
                !string.Equals(_lines[index][..separator].Trim(), key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _lines[index] = $"{key} = {value}";
            return;
        }

        if (sectionLine < 0)
        {
            if (_lines.Count > 0 && _lines[^1].Length > 0)
            {
                _lines.Add(string.Empty);
            }

            _lines.Add($"[{section}]");
            _lines.Add($"{key} = {value}");
            return;
        }

        int insertionLine = sectionLine + 1;
        while (insertionLine < _lines.Count && !_lines[insertionLine].TrimStart().StartsWith('['))
        {
            insertionLine++;
        }

        _lines.Insert(insertionLine, $"{key} = {value}");
    }

    private static Dictionary<string, string> ParseValues(IEnumerable<string> lines)
    {
        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
        string section = string.Empty;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                section = trimmed[1..^1].Trim();
                continue;
            }

            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            int separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            values[$"{section}/{line[..separator].Trim()}"] = line[(separator + 1)..].Trim();
        }

        return values;
    }

    private static IEnumerable<VehiclePriceSetting> ParseVehiclePrices(IReadOnlyList<string> lines)
    {
        string section = string.Empty;
        string description = string.Empty;
        decimal? defaultValue = null;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                section = trimmed[1..^1].Trim();
                description = string.Empty;
                defaultValue = null;
                continue;
            }

            if (!string.Equals(section, "Vehicle Prices", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (trimmed.StartsWith("##", StringComparison.Ordinal))
            {
                description = trimmed[2..].Trim();
                continue;
            }

            if (trimmed.StartsWith("# Default value:", StringComparison.OrdinalIgnoreCase))
            {
                string defaultText = trimmed[16..].Trim();
                defaultValue = decimal.TryParse(
                    defaultText,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out decimal parsedDefault)
                    ? parsedDefault
                    : null;
                continue;
            }

            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            int separator = line.IndexOf('=');
            if (separator <= 0 ||
                !decimal.TryParse(
                    line[(separator + 1)..].Trim(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out decimal value))
            {
                continue;
            }

            string key = line[..separator].Trim();
            string displayName = description;
            const string prefix = "Purchase price for ";
            if (displayName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                displayName = displayName[prefix.Length..].Trim().TrimEnd('.');
            }

            if (displayName.Length == 0)
            {
                displayName = key;
            }

            yield return new VehiclePriceSetting(key, displayName, value, defaultValue ?? value);
            description = string.Empty;
            defaultValue = null;
        }
    }

    private static bool ReadBoolean(Dictionary<string, string> values, string key, bool fallback)
    {
        return values.TryGetValue(key, out string? value) && bool.TryParse(value, out bool parsed)
            ? parsed
            : fallback;
    }

    private static decimal ReadDecimal(Dictionary<string, string> values, string key, decimal fallback)
    {
        return values.TryGetValue(key, out string? value) &&
               decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsed)
            ? parsed
            : fallback;
    }

    private static string ReadString(Dictionary<string, string> values, string key, string fallback)
    {
        return values.TryGetValue(key, out string? value) && value.Length > 0 ? value : fallback;
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static List<string> CreateDefaultLines()
    {
        return new List<string>
        {
            "[General]",
            $"Enabled = {DefaultEnabled.ToString().ToLowerInvariant()}",
            string.Empty,
            "[Input]",
            $"TogglePlacementMode = {DefaultTogglePlacementMode}",
            $"RotatePreviewLeft = {DefaultRotatePreviewLeft}",
            $"RotatePreviewRight = {DefaultRotatePreviewRight}",
            $"ConfirmPlacement = {DefaultConfirmPlacement}",
            string.Empty,
            "[Placement]",
            $"RotationSpeed = {FormatDecimal(DefaultRotationSpeed)}",
            $"MaximumSlope = {FormatDecimal(DefaultMaximumSlope)}",
            $"FobPlacementRadius = {FormatDecimal(DefaultFobPlacementRadius)}",
            $"HoldPosition = {DefaultHoldPosition.ToString().ToLowerInvariant()}"
        };
    }
}

internal sealed class VehiclePriceSetting
{
    public VehiclePriceSetting(string key, string displayName, decimal value, decimal defaultValue)
    {
        Key = key;
        DisplayName = displayName;
        Value = value;
        DefaultValue = defaultValue;
    }

    public string Key { get; }
    public string DisplayName { get; }
    public decimal Value { get; set; }
    public decimal DefaultValue { get; }
}
