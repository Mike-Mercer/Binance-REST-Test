using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;


public class BinanceSettings
{
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";
    public bool KeysSet = false;

    public static BinanceSettings LoadSettings()
    {
        try
        {
            string s = ReadFromFile("config.txt");
            BinanceSettings settings = JsonConvert.DeserializeObject<BinanceSettings>(s);
            settings.KeysSet = !string.IsNullOrEmpty(settings.ApiKey);
            Logger.ConsoleOut(settings.ApiKey);
            return settings;
        }
        catch
        {
            BinanceSettings settings =  new BinanceSettings();
            string s2 = JsonConvert.SerializeObject(settings);
            SaveToFile("config.txt", s2);
            return settings;
        }
    }

    public BinanceSettings()
    {
        KeysSet = false;
    }


     


    #region Helpers
    public static void SaveToFile(string fileName, string textToFile)
    {
        using (FileStream fstream = File.OpenWrite(fileName))
        {
            Byte[] info = new UTF8Encoding(true).GetBytes(textToFile);
            fstream.Write(info, 0, info.Length);
        }
    }
    public static string ReadFromFile(string fileName)
    {
        using (FileStream fstream = File.OpenRead(fileName))
        {
            byte[] array = new byte[fstream.Length];
            fstream.Read(array, 0, array.Length);
            string textFromFile = System.Text.Encoding.UTF8.GetString(array);
            return textFromFile;
        }
    }
    #endregion

}
