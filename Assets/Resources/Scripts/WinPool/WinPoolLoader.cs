using System.IO;
using System.Xml.Serialization;
using UnityEngine;

// Loads a single WinPool XML file. Sync File read is fine: small file, called
// once at boot, editor/standalone only. Returns null on missing file or parse error.
public static class WinPoolLoader
{
    public static WinPool Load(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"WinPoolLoader: file not found — {path}");
            return null;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(WinPool));
            using var stream = File.OpenRead(path);
            return (WinPool)serializer.Deserialize(stream);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WinPoolLoader: failed to parse {path} — {e.Message}");
            return null;
        }
    }
}
