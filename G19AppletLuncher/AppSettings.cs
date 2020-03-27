using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

[Serializable]
public class AppSettings
{

    public GlobalSettings GlobalSettings { get; set; }
    public List<Apps> Applications { get; set; }
    
    
    public static AppSettings Load()
    {
        string appPath = AppDomain.CurrentDomain.BaseDirectory;

        if (!File.Exists(appPath + @"settings.xml"))
            return new AppSettings();

        string xmlText = File.ReadAllText(appPath + @"settings.xml");
        var xs = new XmlSerializer(typeof(AppSettings));
        return (AppSettings)xs.Deserialize(new StringReader(xmlText));
    }

    public static void Save(AppSettings settings)
    {
        string xmlText = string.Empty;
        var xs = new XmlSerializer(settings.GetType());
        using (var xml = new StringWriter())
        {
            xs.Serialize(xml, settings);
            xml.Flush();
            xmlText = xml.ToString();
        }
        string appPath = AppDomain.CurrentDomain.BaseDirectory;
        File.WriteAllText(appPath + @"settings.xml", xmlText);
    }

}




[Serializable]
public class GlobalSettings
{
    public Boolean DebugMode { get; set; }
    public string AppTitle { get; set; }
    public string TitelColor { get; set; }
    public Boolean ShowClockInsteadOfTitel { get; set; }
    public string LineColor { get; set; }
    public string SelectedEntryColor { get; set; }
    public string PrefixSelector { get; set; }
    public string SuffixSelector { get; set; }
    public string paddingLeft { get; set; }
}


[Serializable]
public class Apps
{
    public string Name { get; set; }
    public string Path { get; set; }
    
}




