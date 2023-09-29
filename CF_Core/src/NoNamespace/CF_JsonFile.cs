using Newtonsoft.Json;
using System;
using System.IO;
using static CF_Core.API;

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
    public bool Load(out T dataOut, out string _err)
    {
        lock (fileLock)
        {
            if (!File.Exists(filePath))
            {
                _err = $"File not found: {filePath}";
                dataOut = default;
                return false;
            }
            else
            {
                try
                {
                    dataOut = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
                    _err = null;
                    return true;
                }
                catch (Exception e)
                {
                    _err = $"Deserialization failed: {e.Message}";
                    dataOut = default;
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