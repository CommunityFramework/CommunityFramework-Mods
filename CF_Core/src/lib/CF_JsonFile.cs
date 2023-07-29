using Newtonsoft.Json;
using System.IO;

public class CF_JsonFile<T>
{
    private T data;
    private readonly string filePath;
    private readonly object fileLock = new object();

    public CF_JsonFile(string _filePath, T _jsonClass)
    {
        filePath = _filePath;
        data = _jsonClass;
    }
    public void Save()
    {
        if (data == null)
            return;

        lock (fileLock)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data));
        }
    }
    public bool Load(out object data)
    {
        lock (fileLock)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                data = JsonConvert.DeserializeObject<T>(json);
                return data != null;
            }
        }

        data = null;
        return false;
    }
}