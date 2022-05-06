using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BritoUtil : MonoBehaviour 
{ 

    public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
    {
        using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
        {
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            binaryFormatter.Serialize(stream, objectToWrite);
        }
    }
    public static T ReadFromBinaryFile<T>(string filePath)
    {
        using (Stream stream = File.Open(filePath, FileMode.Open))
        {
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            return (T)binaryFormatter.Deserialize(stream);
        }
    }

    public static void WriteSettings(string content)
    {
        string path = Application.persistentDataPath + @"\settings.txt";
        if (!File.Exists(path))
        {
            var file = File.CreateText(path);
            file.Write(content);
        }
        else
        {
            File.WriteAllText(path, content);
        }
    }

    public static string ReadSettings()
    {
        string path = Application.persistentDataPath + @"\settings.txt";

        if (!File.Exists(path))
        {
            File.CreateText(path);
            return null;
        }

        return File.ReadAllText(path); 
    }
}
