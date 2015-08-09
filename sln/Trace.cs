using System;
using System.Text.RegularExpressions;
using PluginCore;
using PluginCore.Managers;
using ScintillaNet;

namespace Macros
{
    public class Trace
    {
        public static void Execute()
        {
            ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
            if (sci == null) return; // document not editable

            // get selection or word at cursor position
            string expr = sci.SelTextSize > 0
                ? sci.SelText
                : sci.GetWordFromPosition(sci.CurrentPos);

            // wrap all the manipulations as one undo action
            sci.BeginUndoAction();
            try
            {
                if (IsMethod(sci, expr)) TraceMethod(sci, expr);
                else TraceExpr(sci, expr);
            }
            finally
            {
                sci.EndUndoAction();
            }
        }

        private static void TraceExpr(ScintillaControl sci, string expr)
        {
            if (IsMethoDecl(sci)) SkipMethod(sci);
            else sci.LineEnd();

            sci.NewLine();
            sci.InsertText(sci.CurrentPos, String.Format("trace(\"{0} = \" + {1});", expr, SafeExpr(expr)));
            sci.LineEnd();
        }

        private static string SafeExpr(string expr)
        {
            return Regex.IsMatch(expr, "^[a-z0-9_.]+$", RegexOptions.IgnoreCase) ? expr : "(" + expr + ")";
        }

        private static bool IsMethoDecl(ScintillaControl sci)
        {
            string line = sci.GetLine(sci.CurrentLine);
            return Regex.IsMatch(line, "\\bfunction\\b");
        }

        private static void TraceMethod(ScintillaControl sci, string name)
        {
            SkipMethod(sci);

            sci.NewLine();
            sci.InsertText(sci.CurrentPos, String.Format("trace(\"{0}()\");", name));
            sci.LineEnd();
        }

        private static void SkipMethod(ScintillaControl sci)
        {
            do
            {
                sci.LineDown();
                sci.LineEnd();
            }
            while (sci.CurrentLine < sci.LineCount && !IsMethodBodyStart(sci));
        }

        private static bool IsMethodBodyStart(ScintillaControl sci)
        {
            string line = sci.GetLine(sci.CurrentLine);
            int openBrace = line.LastIndexOf('{');
            return openBrace > 0 && openBrace > line.LastIndexOf('}') && openBrace > line.LastIndexOf(')');
        }

        private static bool IsMethod(ScintillaControl sci, string name)
        {
            if (!Regex.IsMatch(name, "^[a-z0-9_]+$", RegexOptions.IgnoreCase))
                return false;

            string line = sci.GetLine(sci.CurrentLine);
            string pattern = "\\bfunction\\s+" + Regex.Escape(name) + "\\s*\\(";
            return Regex.IsMatch(line, pattern);
        }
    }
}