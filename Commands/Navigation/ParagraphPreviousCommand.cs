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
    /// Move backwards to beginning of paragraph (delimited by empty lines).
    /// With argument ARG, do it ARG times;
    /// a negative argument ARG = -N means move forward N paragraphs.
    /// 
    /// Keys: Ctrl+Shift+[
    /// </summary>
    [EmacsCommand(EmacsCommandID.ParagraphPrevious, CanBeRepeated = true, InverseCommand = EmacsCommandID.ParagraphNext)]
    internal class ParagraphPreviousCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var snapshot = context.TextBuffer.CurrentSnapshot;
            int startLine = snapshot.GetLineNumberFromPosition(context.EditorOperations.GetPreviousNonWhiteSpaceCharacter());

            // search for empty lines from the previous line until the beginning of the buffer
            foreach (ITextSnapshotLine line in context.TextBuffer.CurrentSnapshot.Lines.Take(startLine).Reverse())
            {
                if (string.IsNullOrWhiteSpace(line.GetText()))
                {
                    // Empty line found. Move caret to the beginning of the line.
                    context.EditorOperations.MoveCaret(line.Start);
                    return;
                }
            }

            // Reached the beginning of the file without finding any empty line.
            // Place the caret at the beginning of the file.
            context.EditorOperations.MoveCaret(snapshot.Lines.First().Start);
        }
    }
}
