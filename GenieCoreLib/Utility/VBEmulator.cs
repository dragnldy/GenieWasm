
namespace GenieCoreLib 
{
    public static class Conversion
    {
        public static string Hex(byte value)
        {
            return value.ToString("X2");
        }

    }
    public static class Conversions
    {
        public static byte ToByte(object value)
        {
            if (value == null) return 0;
            if (value is byte b) return b;
            if (value is int i && i >= 0 && i <= 255) return (byte)i;
            if (value is string str && byte.TryParse(str, out b)) return b;
            throw new InvalidCastException($"Cannot convert {value.GetType()} to Byte.");
        }
        public static DateTime ToDate(object value)
        {
            if (value == null) return DateTime.MinValue;
            if (value is DateTime dt) return dt;
            if (value is string str && DateTime.TryParse(str, out dt)) return dt;
            if (value is int i) return new DateTime(1970, 1, 1).AddSeconds(i);
            throw new InvalidCastException($"Cannot convert {value.GetType()} to Date.");
        }
        public static char ToChar(object value)
        {
            if (value == null) return Constants.vbNullChar;
            if (value is char c) return c;
            if (value is string str && str.Length > 0) return str[0];
            if (value is string str2 && str2.Length == 0) return Constants.vbNullChar;
            if (value is int i && i >= 0 && i <= 65535) return (char)i;
            throw new InvalidCastException($"Cannot convert {value.GetType()} to Char.");
        }
        public static bool ToBoolean(object value)
        {
            if (value == null) return false;
            if (value is bool b) return b;
            if (value is int i) return i != 0;
            if (value is double d) return d != 0.0;
            if (value is string str)
            {
                return str.Equals("True", StringComparison.OrdinalIgnoreCase) || str.Equals("1");
            }
            throw new InvalidCastException($"Cannot convert {value.GetType()} to Boolean.");
        }
        public static uint ToUInteger(object value)
        {
            if (value == null) return 0;
            if (value is int i) return (uint)i;
            if (value is double d) return (uint)Math.Round(d, 0);
            if (value is string str && int.TryParse(str, out i)) return (uint)i;
            throw new InvalidCastException($"Cannot convert {value.GetType()} to Integer.");
        }
        public static int ToInteger(object value)
        {
            if (value == null) return 0;
            if (value is int i) return i;
            if (value is double d) return (int)Math.Round(d,0);
            if (value is string str && int.TryParse(str, out i)) return i;
            throw new InvalidCastException($"Cannot convert {value.GetType()} to Integer.");
        }
        public static long ToLong(object value)
        {
            if (value == null) return 0l;
            if (value is int i) return (long)i;
            if (value is long l) return l;
            if (value is double d) return (long)Math.Round(d,0);
            if (value is string str && long.TryParse(str, out l)) return l;
            throw new InvalidCastException($"Cannot convert {value.GetType()} to Long.");
        }
        public static double ToDouble(object value)
        {
            if (value == null) return 0d;
            if (value is int i) return (double)i;
            if (value is long l) return (double)l;
            if (value is double d) return d;
            if (value is string str && double.TryParse(str, out d)) return d;
            throw new InvalidCastException($"Cannot convert {value.GetType()} to double.");
        }
        public static string ToString(object c)
        {
            return c != null ? c.ToString() : string.Empty;
        }
        //public static string ToString(char c)
        //{
        //    return c.ToString();
        //}
        //public static string ToString(int i)
        //{
        //    return i.ToString();
        //}
        //public static string ToString(double d)
        //{
        //    return d.ToString();
        //}
        public static string ToString(bool b)
        {
            return b ? "True" : "False";
        }
    }
    public class Collection: ArrayList
    {
    }
    public static class Information
    {
        public static int UBound(Array array, int dimension = 1)
        {
            if (array == null) return -1;
            if (dimension < 1 || dimension > array.Rank)
            {
                throw new ArgumentOutOfRangeException(nameof(dimension), "Dimension must be between 1 and the rank of the array.");
            }
            return array.GetLength(dimension - 1) - 1; // UBound is inclusive
        }

        public static bool IsNumeric(object value)
        {
            if (value == null) return false;
            if (value is int || value is double || value is float || value is decimal) return true;
            if (value is string str)
            {
                return double.TryParse(str, out _);
            }
            if (value is char c)
            {
                return double.TryParse(c.ToString(), out _);
            }
            return false;
        }
        public static bool IsNothing(object obj)
        {
            return obj == null || (obj is string str && string.IsNullOrEmpty(str));
        }
    }
    public static class Interaction
    {
        public static object IIf(bool condition, object truePart, object falsePart)
        {
            return condition ? truePart : falsePart;
        }
        public static void Beep()
        {
            // This is a placeholder for a beep implementation.
            // In a real application, you would use a sound library to play a beep.
            //System.Media.SystemSounds.Beep.Play();
//            Console.Beep();
        }
        public enum AppWinStyle
        {
            NormalFocus,
            MinimizedFocus,
            Hidden,
            MaximizedFocus,
            MinimizedNoFocus,
            NormalNoFocus
        }

        public static void Shell(string commandPlusArgs, AppWinStyle style, bool hrmmm)
        {
            // This is a placeholder for a shell implementation.
            // We don't currently use the style or hrmmm parameters.- just there to maintain compatibility
            var parts = commandPlusArgs.Split(new[] { ' ' }, 2);
            if (parts.Length == 0 || parts.Length > 2)
            {
                throw new ArgumentException("Command should not contain more than one space but should contain at least a command.");
            }
            string command = parts[0];
            string args = parts.Length > 1 ? parts[1] : string.Empty;
            System.Diagnostics.Process.Start(command, args);
        }


        public const string[] commandargs = null;
        public static string Command()
        {
            var args = Environment.GetCommandLineArgs();
            // return the command line arguments as a string array
            if (args.Length > 1)
            {
                string arg = string.Join(" ", args, 1, args.Length - 1);
                return arg;
            }
            return string.Empty;
        }

    }
    public static class Strings
    {
        public static int Len(string str)
        {
            return str?.Length ?? 0;
        }
        public static string FormatDateTime(DateTime date, string format)
        {
            
            string format2use = format switch
            {
                "ShortDate" => "MM/dd/yyyy",
                "LongDate" => "dddd, MMMM dd, yyyy",
                "ShortTime" => "HH:mm",
                "LongTime" => "hh:mm:ss tt",
                _ => format
            }; 
            return date.ToString(format2use);
        }
        //public static string Trim(string str)
        //{
        //    return str?.Trim() ?? string.Empty;
        //}
        //public static string LTrim(string str)
        //{
        //    return str?.TrimStart() ?? string.Empty;
        //}
        //public static string RTrim(string str)
        //{
        //    return str?.TrimEnd() ?? string.Empty;
        //}
    }
    public static class Operators
    {
        public static bool ConditionalCompareObjectEqual(object left, object right, bool caseSensitive = true)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            if (left is string leftStr && right is string rightStr)
            {
                return caseSensitive ? leftStr.Equals(rightStr) : leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase);
            }
            return left.Equals(right);
        }
        public static bool And(bool left, bool right)
        {
            return left && right;
        }
        public static bool Or(bool left, bool right)
        {
            return left || right;
        }
        public static bool Not(bool value)
        {
            return !value;
        }
        public static bool Xor(bool left, bool right)
        {
            return left ^ right;
        }
    }
    public static class Constants
    {
        public const char vbNullChar = '\0';
        public const string vbTab = "\t";
        public const string vbCr = "\r";
        public const char vbNewLine = '\n';
        public const string vbLf = "\n";
        public const string vbCrLf = "\r\n";
        public const string vbArchive = "32";
    }

    public static class FileSystem
    {
        public static IEnumerable<string> Dir(string searchPattern = "*.*")
        {
            return Directory.EnumerateFiles(searchPattern);
        }
    }
}