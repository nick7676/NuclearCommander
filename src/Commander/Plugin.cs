using BepInEx;
using Commander.Placement;
using Commander.UI;

namespace Commander;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.nick7676.nuclearcommander";
    public const string PluginName = "Nuclear Commander";
    public const string PluginVersion = "0.14.0";

    private CommanderSettings _settings = null!;
    private CommanderWindow _window = null!;
    private PlacementController _placement = null!;

    private void Awake()
    {
        _settings = new CommanderSettings(Config);
        _window = new CommanderWindow();
        _placement = new PlacementController(_settings, Logger, _window.IsMouseOver);

        if (!_settings.Enabled.Value)
        {
            Logger.LogInfo($"{PluginName} is disabled in the configuration.");
            return;
        }

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded successfully.");
    }

    private void Update()
    {
        _placement?.Tick(_settings.Enabled.Value);
    }

    private void OnGUI()
    {
        if (_settings?.Enabled.Value == true && _placement?.IsActive == true)
        {
            _window.Draw(_placement);
        }
    }

    private void OnDestroy()
    {
        _placement?.Dispose();

        if (_settings?.Enabled.Value == true)
        {
            Logger.LogInfo($"{PluginName} unloaded.");
        }
    }
}
