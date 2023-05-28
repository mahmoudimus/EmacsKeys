using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// Put point at beginning and mark at end of buffer (thus selecting the whole buffer).
    /// Also push mark at point before pushing mark at end of buffer.
    /// 
    /// Keys: Ctrl+X H
    /// </summary>
    [EmacsCommand(EmacsCommandID.SelectAll)]
    internal class SelectAllCommand : EmacsCommand
    {        
        internal override void Execute(EmacsCommandContext context)
        {
            var bufferStart = context.TextBuffer.CurrentSnapshot.Lines.First().Start;
            var bufferEnd = context.TextBuffer.CurrentSnapshot.Lines.Last().End;

            // First, push mark at point
            context.MarkSession.PushMark(false);
            // Then, push mark at the end of the buffer
            context.EditorOperations.MoveCaret(bufferEnd);
            context.MarkSession.PushMark(false);
            // Finally, move to the beginning of the buffer and activate the selection
            context.EditorOperations.MoveCaret(bufferStart);
            context.MarkSession.Activate();
        }
    }
}
