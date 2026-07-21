namespace NuclearCommander.Configurator;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length > 0 && string.Equals(args[0], "--validate", StringComparison.OrdinalIgnoreCase))
        {
            return ValidateConfiguration(args.ElementAtOrDefault(1));
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(args.FirstOrDefault()));
        return 0;
    }

    private static int ValidateConfiguration(string? requestedPath)
    {
        try
        {
            string? path = CommanderConfigLocator.Find(requestedPath);
            if (path == null)
            {
                return 2;
            }

            CommanderConfig config = CommanderConfig.Load(path);
            return config.VehiclePrices.Count > 0 ? 0 : 3;
        }
        catch
        {
            return 1;
        }
    }
}
