using System.Xml;

namespace GenieCoreLib;

// Support for legacy XML configuration files.
public class XMLConfig
{
    private XmlDocument xmlDoc = new XmlDocument();
    public XmlDocument XmlDoc
    {
        get
        {
            return xmlDoc;
        }
    }

    public XMLConfig(string fileName)
    {
        configFile = fileName;
        hasData = false;
        if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
        {
            LoadFile(fileName);
        }
    }

    private string configFile = string.Empty;
    public string ConfigFile
    {
        get
        {
            return configFile;
        }
    }

    private bool hasData = false;
    public bool HasData
    {
        get
        {
            return hasData;
        }
    }

    public bool LoadFile(string filename)
    {
        try
        {
            string contents = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(contents))
            {
                return false; // File is empty
            }

            hasData = LoadXml(contents);
            return hasData;
        }
        catch
        {
            return false;
        }
    }

    public bool LoadXml(string xmldata)
    {

        if (string.IsNullOrEmpty(xmldata)) return false;
        try
        {
            xmlDoc.LoadXml(xmldata);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetValue(string path, string attribute, string DefaultValue)
    {
        string ro = GetValueObject(path, attribute);
        if (!Information.IsNothing(ro))
        {
            try
            {
                // can this throw?
                return Convert.ToString(ro);
            }
            catch { }
        }
        return DefaultValue;
    }

    public bool GetValue(string path, string attribute, bool DefaultValue)
    {
        string ro = GetValueObject(path, attribute);
        if (ro != null && ro.Length > 0 && (ro.Equals("True") || ro.Equals("False")))
        {
            return Convert.ToBoolean(ro);
        }
        return DefaultValue;
    }

    public int GetValue(string path, string attribute, int DefaultValue)
    {
        string ro = GetValueObject(path, attribute);
        if (ro != null && ro.Length > 0)
        {
            try
            {
                return Convert.ToInt32(ro);
            }
            catch { }
        }
        // If value doesn't exist or is invalid, return default:
        return DefaultValue;
    }

    public double GetValue(string path, string attribute, double DefaultValue)
    {
        string ro = GetValueObject(path, attribute);
        if (ro != null && ro.Length > 0)
        {
            try
            {
                return Convert.ToDouble(ro);
            }
            catch { }
        }
        return DefaultValue;
    }

    public float GetValueSingle(string path, string attribute, float DefaultValue)
    {
        string ro = GetValueObject(path, attribute);
        if (ro != null && ro.Length > 0)
        {
            try
            {
                return Convert.ToSingle(ro);
            }
            catch { }
        }
        return DefaultValue;
    }

    public DateTime GetValue(string path, string attribute, DateTime DefaultValue)
    {
        string ro = GetValueObject(path, attribute);
        if (ro != null && ro.Length > 0)
        {
            try
            {
                return Convert.ToDateTime(ro);
            }
            catch { }
        }
        return DefaultValue;
    }

    private string GetValueObject(string path, string attribute)
    {
        string node;
        string key;
        int i;
        i = path.LastIndexOf("/");
        if (i < 0)
        {
            return null;
        }

        node = path.Substring(0, i);
        key = path.Substring(i + 1);
        if (xmlDoc is null)
        {
            throw new ArgumentNullException("getvalue", "No config to read from.");
        }

        try
        {
            var xmlNode = xmlDoc.SelectSingleNode(node);
            if (!(xmlNode is null))
            {
                XmlElement targetElem = (XmlElement)xmlNode.SelectSingleNode(key);
                if (!(targetElem is null) && targetElem.HasAttribute(attribute) != false)
                    return targetElem.GetAttribute(attribute);
            }
        }
        catch
        { }
        return null;
    }
}