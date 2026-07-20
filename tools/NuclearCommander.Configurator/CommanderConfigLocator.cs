namespace NuclearCommander.Configurator;

internal static class CommanderConfigLocator
{
    public static string? Find(string? requestedPath)
    {
        string? result = ResolveCandidate(requestedPath);
        if (result != null)
        {
            return result;
        }

        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        for (int level = 0; level < 8 && directory != null; level++, directory = directory.Parent)
        {
            result = ResolveCandidate(directory.FullName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    public static string? ResolveCandidate(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        string fullPath;
        try
        {
            fullPath = System.IO.Path.GetFullPath(path);
        }
        catch
        {
            return null;
        }

        if (File.Exists(fullPath) &&
            string.Equals(System.IO.Path.GetFileName(fullPath), CommanderConfig.FileName, StringComparison.OrdinalIgnoreCase))
        {
            return fullPath;
        }

        if (!Directory.Exists(fullPath))
        {
            return null;
        }

        string[] directories =
        {
            fullPath,
            System.IO.Path.Combine(fullPath, "config"),
            System.IO.Path.Combine(fullPath, "BepInEx", "config")
        };

        foreach (string directory in directories)
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            if (string.Equals(new DirectoryInfo(directory).Name, "config", StringComparison.OrdinalIgnoreCase))
            {
                return System.IO.Path.Combine(directory, CommanderConfig.FileName);
            }
        }

        return null;
    }
}
