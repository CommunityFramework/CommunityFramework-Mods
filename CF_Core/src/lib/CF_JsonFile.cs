using Newtonsoft.Json;
using System;
using System.IO;

public class CF_JsonFile<T>
{
    public T data;
    private readonly string filePath;
    private readonly object fileLock = new object();
    private readonly Formatting format = Formatting.None;

    public CF_JsonFile(string _filePath, T _jsonClass, Formatting _format = Formatting.Indented)
    {
        filePath = _filePath;
        data = _jsonClass;
        format = _format;
    }
    public void Save()
    {
        if (data == null)
            return;

        lock (fileLock)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data, format));
        }
    }
    public bool Load<T>(out T data, out string _err)
    {
        lock (fileLock)
        {
            if (!File.Exists(filePath))
            {
                _err = $"File not found: {filePath}";
                data = default(T);
                return false;
            }
            else
            {
                try
                {
                    data = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
                    _err = null;
                    return true;
                }
                catch (Exception e)
                {
                    _err = $"Deserialization failed: {e.Message}";
                    data = default(T);
                    return false;
                }
            }
        }
    }
    public bool LoadJson(out string data, out string _err)
    {
        lock (fileLock)
        {
            if (!File.Exists(filePath))
            {
                data = "";
                _err = $"File not found: {filePath}";
                return false;
            }

            try
            {
                data = File.ReadAllText(filePath);

                if (string.IsNullOrEmpty(data))
                {
                    _err = "File is empty";
                    return false;
                }
                else
                {
                    _err = null;
                    return true;
                }
            }
            catch (Exception e)
            {
                data = "";
                _err = $"Failed to read file: {e.Message}";
                return false;
            }
        }
    }

}