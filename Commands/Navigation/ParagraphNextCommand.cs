using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// Move forward to end of paragraph (delimited by empty lines).
    /// With argument ARG, do it ARG times;
    /// a negative argument ARG = -N means move backward N paragraphs.
    /// 
    /// Keys: Ctrl+Shift+]
    /// </summary>
    [EmacsCommand(EmacsCommandID.ParagraphNext, CanBeRepeated = true, InverseCommand = EmacsCommandID.ParagraphPrevious)]
    internal class ParagraphNextCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var snapshot = context.TextBuffer.CurrentSnapshot;
            int startLine = snapshot.GetLineNumberFromPosition(context.EditorOperations.GetNextNonWhiteSpaceCharacter());

            // search for empty lines from the next line until the end of the buffer
            foreach (ITextSnapshotLine line in context.TextBuffer.CurrentSnapshot.Lines.Skip(startLine + 1))
            {
                if (string.IsNullOrWhiteSpace(line.GetText()))
                {
                    // Empty line found. Move caret to the beginning of the line.
                    context.EditorOperations.MoveCaret(line.Start);
                    return;
                }
            }

            // Reached the end of the file without finding any empty line.
            // Place the caret at the end of the file.
            context.EditorOperations.MoveCaret(snapshot.Lines.Last().End);
        }
    }
}
