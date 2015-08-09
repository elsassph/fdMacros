using System;
using System.Linq;
using System.Text;
using PluginCore;
using ScintillaNet;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Macros
{
    class PrettyPrint
    {
        public static void Execute()
        {
            ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
            if (sci == null) return; // document not editable

            string src = sci.SelTextSize == 0 ? sci.Text : sci.SelText;
            src = src.TrimStart();
            if (src.Length == 0) return;

            // "detect" if it's XML or Json
            if (src[0] == '<') src = ReformatXml(sci, src);
            else if (src[0] == '{' || src[0] == '[') src = ReformatJson(sci, src);
            else return;
            
            if (sci.SelTextSize == 0) sci.Text = src;
            else sci.ReplaceSel(src);
        }

        private static string ReformatXml(ScintillaControl sci, string src)
        {
            Match header = Regex.Match(src, "^<\\?[^?]+\\?>");
            
            XDocument doc = XDocument.Parse(src); // Linq

            if (header.Success) return header.Value + "\n" + doc.ToString();
            else return doc.ToString();
        }

        private static string ReformatJson(ScintillaControl sci, string src)
        {
            JsonHelper.INDENT_STRING = GetIndent(sci);
            return JsonHelper.FormatJson(src);
        }

        private static string GetIndent(ScintillaControl sci)
        {
            if (sci.IsTabIndents) return "\t";
            string indent = "";
            for (int i = 0; i < sci.Indent; i++) indent += " ";
            return indent;
        }
    }

    // Adapted from http://stackoverflow.com/questions/4580397/json-formatter-in-c
    // - modified to follow .NET 2 syntax (ie. no 'var') and to ignore existing whitespace
    class JsonHelper
    {
        public static string INDENT_STRING = "  ";

        public static string FormatJson(string str)
        {
            int indent = 0;
            bool quoted = false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            indent++;
                            for (int j = 0; j < indent; j++) sb.Append(INDENT_STRING);
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            indent--;
                            for (int j = 0; j < indent; j++) sb.Append(INDENT_STRING);
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        int index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            for (int j = 0; j < indent; j++) sb.Append(INDENT_STRING);
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    case '\r': 
                        continue;
                    case '\n': 
                        continue;
                    case '\t':
                        continue;
                    case ' ':
                        if (quoted) sb.Append(ch);
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
