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
    /// This command moves the current buffer line according to a predefined cycling order (by default: center, top, bottom).
    ///  With a positive argument ARG, it repositions text to put point ARG lines down from the top.
    ///  With a negative argument, puts the point that many lines from the bottom of the window.
    ///  With plain ‘C-u’, move current line to window center.
    ///
    /// Keys: Ctrl+L
    /// </summary>
    [EmacsCommand(EmacsCommandID.ScrollLineCenter, InverseCommand = EmacsCommandID.ScrollLineCenter)]
    internal class ScrollLineCenterCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            // TODO: The current implementation makes no distinction between a 'plain C-u' and 'C-u 4'.
            // Therefore, 'C-u C-L' will place the current line at the 4th line from the top, instead of recentering it.

            if (context.Manager.UniversalArgument.HasValue)
            {
                var repeat = context.Manager.UniversalArgument.Value;

                context.EditorOperations.ScrollLineTop();

                // If the prefix arg is greater than the display lines of the window, just go that many lines down and redisplay.
                for (int i = 0; i < repeat; i++)
                {
                    context.EditorOperations.ScrollUpAndMoveCaretIfNecessary();
                }
            }
            else
            {
                int currentPosition = context.TextView.GetCaretPosition().Position;

                // reset index if last command is not ScrollLineCenter, or if the caret position
                // has been changed, for example through mouse operations
                if (context.Manager.LastExecutedCommand == null ||
                    context.Manager.LastExecutedCommand.Command != (int)EmacsCommandID.ScrollLineCenter ||
                    recenterOriginalPosition != currentPosition)
                {
                    recenterPositionIndex = 0;
                    recenterOriginalPosition = currentPosition;
                }

                // switch between center, top, and bottom in the designated order
                switch(this.recenterPositions[recenterPositionIndex])
                {
                    case ScrollLinePosition.Top:
                        context.EditorOperations.ScrollLineTop();
                        break;
                    case ScrollLinePosition.Bottom:
                        context.EditorOperations.ScrollLineBottom();
                        break;
                    case ScrollLinePosition.Center:
                        context.EditorOperations.ScrollLineCenter();
                        break;
                }
                recenterPositionIndex = (1 + recenterPositionIndex) % recenterPositions.Count;
            }
        }

        internal override void ExecuteInverse(EmacsCommandContext context)
        {
            var repeat = Math.Abs(context.Manager.GetUniversalArgumentOrDefault(1));

            context.EditorOperations.ScrollLineBottom();

            for (int i = 0; i < repeat; i++)
            {
                context.EditorOperations.ScrollDownAndMoveCaretIfNecessary();
            }
        }

        // Types of supported scroll operations
        internal enum ScrollLinePosition
        {
            Center,
            Top,
            Bottom,
        }

        // Consecutive operation counter
        internal int recenterPositionIndex = 0;

        // Initial caret position during consecutive operations
        internal int recenterOriginalPosition;

        // Order of operations to be executed on consecutive calls
        // Defaults to `center, top, bottom`
        internal List<ScrollLinePosition> recenterPositions = new List<ScrollLinePosition> {
            ScrollLinePosition.Center, ScrollLinePosition.Top, ScrollLinePosition.Bottom
        };
    }
}
