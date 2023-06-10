using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// Kill current line.
    /// With prefix ARG, kill that many lines starting from the current line.
    /// If ARG is negative, kill backward. Also kill the preceding newline.
    /// If ARG is zero, kill current line but exclude the trailing newline.  
    /// 
    /// Keys: Ctrl+Shift+Backspace | Ctrl+Shift+Delete
    /// </summary>
    [EmacsCommand(EmacsCommandID.DeleteWholeLine, IsKillCommand=true, UndoName="Cut")]
    internal class DeleteWholeLineCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            int arg = context.UniversalArgument.GetValueOrDefault(1);

            // Clear the selection to ensure that editorOperations.DeleteFullLine will
            // only target the active line (and not a potentially multi-line selection)
            context.TextView.Selection.Clear();

            if (arg > 0)
            {
                for (int i = 0; i < arg; i++)
                {
                    context.EditorOperations.DeleteFullLine();
                }
            }
            else if (arg < 0)
            {
                var line = context.EditorOperations.GetCaretPhysicalLine();
                int index = line.LineNumber + arg;
                if (index >= 0)
                {
                    var startLine = context.TextView.TextViewLines[index];
                    context.EditorOperations.Delete(startLine.End, line.End - startLine.End);
                }
                else
                {
                    // delete all the way until the beginning of the buffer
                    context.EditorOperations.Delete(0, line.End);
                }
            }
            else if (arg == 0)
            {
                var line = context.EditorOperations.GetCaretPhysicalLine();
                context.EditorOperations.Delete(line.Start, line.End - line.Start);
            }
        }
    }
}