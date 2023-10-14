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
    /// This command positions the cursor line according to a predefined cycling order (by default: center, top, bottom)
    ///  With a positive argument ARG, it positions the cursor to ARG lines down from the top.
    ///  With a negative argument, positions the cursor that many lines from the bottom of the window.
    ///  Position point relative to window.
    ///  With a prefix argument ARG, acts like ‘move-to-window-line’.
    ///
    /// Keys: Alt+R
    /// </summary>
    [EmacsCommand(EmacsCommandID.MoveToWindowLineCenter, InverseCommand = EmacsCommandID.MoveToWindowLineCenter)]
    internal class MoveToWindowLineCenterCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            // When supplied with a positive argument,
            // move to the zero-indexed N-th line counting from the top
            if (context.Manager.UniversalArgument.HasValue)
            {
                int repeat = context.Manager.UniversalArgument.Value;
                int firstLine = context.TextBuffer.GetLineNumber(context.TextView.GetFirstSufficientlyVisibleLine().Start);
                int lineNumber = Math.Min(firstLine + repeat, context.TextBuffer.CurrentSnapshot.LineCount - 1);

                context.EditorOperations.GotoLine(lineNumber);
            }
            else
            {
                int position = context.TextView.GetCaretPosition().Position;

                // reset index if last command is not MoveToWindowLineCenter, or if the caret position
                // has been changed, for example through mouse operations
                if (context.Manager.LastExecutedCommand == null ||
                    context.Manager.LastExecutedCommand.Command != (int)EmacsCommandID.MoveToWindowLineCenter ||
                    recenterLastPosition != position)
                {
                    recenterPositionIndex = 0;
                }

                // switch between center, top, and bottom in the designated order
                switch (this.recenterPositions[recenterPositionIndex])
                {
                    case MoveToWindowLinePosition.Top:
                        position = context.TextView.GetFirstSufficientlyVisibleLine().Start;
                        break;
                    case MoveToWindowLinePosition.Bottom:
                        position = context.TextView.GetLastSufficientlyVisibleLine().Start;
                        break;
                    case MoveToWindowLinePosition.Center:
                        {
                            double ycoord = (context.TextView.ViewportTop + context.TextView.ViewportBottom) / 2.0;
                            position = context.TextView.TextViewLines.GetTextViewLineContainingYCoordinate(ycoord).Start;
                            break;
                        }
                }

                context.EditorOperations.MoveCaret(position);
                recenterLastPosition = position;
                recenterPositionIndex = (1 + recenterPositionIndex) % recenterPositions.Count;
            }
        }

        internal override void ExecuteInverse(EmacsCommandContext context)
        {
            // When supplied with a negative argument,
            // move to the one-indexed N-th line counting from the bottom
            int repeat = Math.Abs(context.Manager.GetUniversalArgumentOrDefault(1));
            int lastLine = context.TextBuffer.GetLineNumber(context.TextView.GetLastSufficientlyVisibleLine().Start);
            int lineNumber = Math.Max(0, 1 + lastLine - repeat);

            context.EditorOperations.GotoLine(lineNumber);
        }

        // Types of supported scroll operations
        internal enum MoveToWindowLinePosition
        {
            Center,
            Top,
            Bottom,
        }

        // Consecutive operation counter
        internal int recenterPositionIndex = 0;

        // Initial caret position during consecutive operations
        internal int recenterLastPosition;

        // Order of operations to be executed on consecutive calls
        // Defaults to `center, top, bottom`
        internal List<MoveToWindowLinePosition> recenterPositions = new List<MoveToWindowLinePosition> {
            MoveToWindowLinePosition.Center, MoveToWindowLinePosition.Top, MoveToWindowLinePosition.Bottom
        };
    }
}
