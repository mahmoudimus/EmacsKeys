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
    /// This command clears the window and redisplays it with the current line in the center of the window, leaving the caret at the same char offset on the line.
    /// If the prefix arg is greater than the display lines of the window, just go that many lines down and redisplay.
    /// 
    /// Keys: Ctrl+L
    /// </summary>
    [EmacsCommand(EmacsCommandID.ScrollLineCenter)]
    internal class ScrollLineCenterCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            // TODO: Add universal argument support (P3?)
            //From the gnu-emacs manual (I think this is P3 since most people don’t know about it, and it is rarely used):
            //Another way to do scrolling is with C-l with a numeric argument. C-l does not clear the screen when given an argument; it only scrolls the selected window. With a positive argument n, it repositions text to put point n lines down from the top. An argument of zero puts point on the very top line. Point does not move with respect to the text; rather, the text and point move rigidly on the screen. C-l with a negative argument puts point that many lines from the bottom of the window. For example, C-u - 1 C-l puts point on the bottom line, and C-u - 5 C-l puts it five lines from the bottom. Just C-u as argument, as in C-u C-l, scrolls point to the center of the selected window. 
            //If the prefix arg is greater than the display lines of the window, just go that many lines down and redisplay.

            if (context.Manager.UniversalArgument.HasValue)
            {
                var repeat = context.Manager.UniversalArgument.Value;

                context.EditorOperations.ScrollLineTop();

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
