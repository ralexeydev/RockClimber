using System.IO;
using System.Xml.Serialization;
using UnityEngine;

// Loads a single WinPool XML file. Sync File read is fine: small file, called
// once at boot, editor/standalone only. Returns null on missing file or parse error.
public static class WinPoolLoader
{
    public static WinPool Load(string path)
    {
        TryLoad(path, out WinPool pool, out _);
        return pool;
    }

    public static bool TryLoad(string path, out WinPool pool, out string error)
    {
        pool = null;
        error = null;

        if (!File.Exists(path))
        {
            error = $"Win-pool file not found: {path}";
            Debug.LogError(error);
            return false;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(WinPool));
            using var stream = File.OpenRead(path);
            pool = (WinPool)serializer.Deserialize(stream);
            return true;
        }
        catch (System.Exception e)
        {
            error = $"Failed to parse win pool '{path}': {e.Message}";
            Debug.LogError(error);
            return false;
        }
    }
}
