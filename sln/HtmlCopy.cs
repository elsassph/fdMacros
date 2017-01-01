using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using PluginCore;
using PluginCore.Managers;
using ScintillaNet;
using ScintillaNet.Configuration;

namespace Macros
{
    class StyleColor 
    {
        public string Fore;
        public string Back;
        public StyleColor(string fore, string back) 
        {
            Fore = fore;
            Back = back;
        }
    }

    public class HtmlCopy
    {
        public static void Execute()
        {
            ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
            if (sci == null) return; // document not editable

            // get selection or word at cursor position
            if (sci.SelTextSize == 0) 
            {
                TraceManager.Add("No selection");
                return;
            }

            // get styles names
            string syntax = sci.ConfigurationLanguage;
            List<string> names = GetStylesNames(syntax);

            // build html
            Dictionary<int, StyleColor> colors = new Dictionary<int, StyleColor>();
            int style = -1;
            int end = sci.SelectionEnd;
            string defaultStyle = GetName(syntax, names, 0);
            string buffer = "<div class=\"" + defaultStyle + "\">";

            int i = sci.SelectionStart;
            while (i < end)
            {
                int cur = sci.BaseStyleAt(i);
                int c = sci.CharAt(i++);
                if (c == '\n') {
                    if (style > 0) buffer += "</span>";
                    buffer += "</div>\n<div class=\"" + defaultStyle + "\">";
                    style = -1;
                    continue;
                }
                if (cur != style) {
                    if (!colors.ContainsKey(cur)) colors.Add(cur, GetColor(sci, cur));
                    if (style > 0) buffer += "</span>";
                    if (cur > 0) buffer += "<span class=\"" + GetName(syntax, names, cur) + "\">";
                    style = cur;
                }
                if (c == '<') buffer += "&lt;";
                else if (c == '>') buffer += "&gt;";
                else if (c > 0) buffer += ((char)c).ToString();
                else 
                {
                    // multibyte to utf8
                    int ulen = GetUTF8Length(c);
                    if (ulen == 0) continue;
                    byte[] b = new byte[ulen];
                    b[0] = (byte)c;
                    for (int j = 1; j < ulen; j++) b[j] = (byte)sci.CharAt(i++);
                    string u = UTF8Encoding.UTF8.GetString(b);
                    buffer += u;
                }
            }
            buffer += "</div>";
            
            // build styles
            string styles = "<style>\n" +
            "." + GetName(syntax, names, 0) + " {\n" +
            "  white-space: pre;\n" +
            "  font-family: monospace;\n" +
            "}\n";
            foreach(KeyValuePair<int, StyleColor> col in colors)
                styles += "." + GetName(syntax, names, col.Key) + " {\n" +
                "  color: " + col.Value.Fore + ";\n" +
                "  background: " + col.Value.Back + ";\n" +
                "}\n";
            styles += "</style>\n";
            
            // to clipboard
            Clipboard.SetText(styles + buffer);
			TraceManager.Add("HTML copied to clipboard!");
        }

        /// Detect multibyte character length
        static int GetUTF8Length(int c)
        {
            if (c > 0) return 1;
            if ((c & 0xE0) == 0xC0) return 2;
            if ((c & 0xF0) == 0xE0) return 3;
            if ((c & 0xF8) == 0xF0) return 4;
            return 0;
        }

        /// Use internal Scintilla APIs to get text colors
        static StyleColor GetColor(ScintillaControl sci, int style)
        {
            string fore = FormatColor(sci.SPerform(2481, style, 0));
            string back = FormatColor(sci.SPerform(2482, style, 0));
            return new StyleColor(fore, back);
        }

        /// int to hex (note: Scintilla is BVR, not RVB)
        static string FormatColor(int bvr)
        {
            int b = bvr >> 16;
            int v = (bvr >> 8) & 0xff;
            int r = bvr & 0xff;
            return "#" + r.ToString("X2") + v.ToString("X2") + b.ToString("X2");
        }

        /// Style friendly name
        static string GetName(string syntax, List<string> names, int index)
        {
            if (names == null || index >= names.Count) return syntax + "-" + index.ToString();
            return syntax + "-" + names[index];
        }

        // Reflect lexer style names
        static List<string> GetStylesNames(string syntax)
        {
            Scintilla config = ScintillaControl.Configuration;
            foreach (Language lang in config.AllLanguages)
                if (lang.name == syntax)
                {
                    Type lexer = GetLexer(lang.lexer.name);
                    if (lexer == null) break;
                    List<string> names = new List<string>();
                    foreach (object ename in Enum.GetValues(lexer))
                        names.Add(ename.ToString());
                    return names;
                }
            return null;
        }

        /// Find lexer style enum from friendly name
        static Type GetLexer(string name)
        {
            switch (name)
            {
                case "cpp": return typeof(ScintillaNet.Lexers.CPP);
                case "html": return typeof(ScintillaNet.Lexers.HTML);
                default: return null;
            }
        }
    }
}
