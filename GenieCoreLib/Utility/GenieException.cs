using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenieCoreLib;

public class ExceptionMessage: TextMessage
{
    public string Description { get; set; }
    public string Section { get; set; }
    public ExceptionMessage(string text, string targetPanel, string section, string description = null) : base(text, targetPanel)
    {
        Description = description;
        Section = section;
    }
}
public static class GenieException
{
    public static void HandleGenieException(string section, string message, string description = null)
    {
        TextFunctions.SendTextMessage(new ExceptionMessage(message, "exception", section, description));
    }
}
