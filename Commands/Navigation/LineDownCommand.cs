using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command goes down one line, placing the caret at the same relative char position from the start of the line.  
    /// If the line is shorter, then goes to the EOL.  With a prefix arg, it goes down that many lines.  
    /// If the prefix arg is negative, this command goes up that many lines.
    /// 
    /// Keys: Ctrl-N | Down Arrow
    /// </summary>
    [EmacsCommand(EmacsCommandID.LineDown, CanBeRepeated = true, InverseCommand = EmacsCommandID.LineUp)]
    internal class LineDownCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            // EOF check
            var caretLine = context.EditorOperations.GetCaretPhysicalLine();
            var lineCount = context.TextBuffer.CurrentSnapshot.LineCount;
            if (caretLine.LineNumber == (lineCount - 1))
            {
                context.EditorOperations.MoveCaretToEndOfPhysicalLine();
                context.Manager.UpdateStatus("End of buffer");
                return;
            }

            context.EditorOperations.MoveLineDown();
        }
    }
}
