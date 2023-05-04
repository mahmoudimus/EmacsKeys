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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command is the opposite of scroll-down-command.
    /// 
    /// Keys: PgUp | Alt+V
    /// </summary>
    [EmacsCommand(EmacsCommandID.ScrollPageUp, CanBeRepeated = true, InverseCommand = EmacsCommandID.ScrollPageDown)]
    internal class ScrollPageUpCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var caret = context.TextView.Caret;
            var firstLine = context.TextView.GetFirstSufficientlyVisibleLine();

            // number of lines in the current view that should remain visible after scrolling
            int visibleLines = 2;

            if (context.TextBuffer.GetLineNumber(firstLine.Start) == 0)
            {
                // Beginning of buffer reached. Do nothing
                return;
            }

            // Scroll up while keeping some overlapping lines
            context.TextView.DisplayTextLineContainingBufferPosition(
                firstLine.Start,
                context.TextView.LineHeight * Math.Max(0, visibleLines - 1),
                ViewRelativePosition.Bottom);

            if (caret.Top > context.TextView.ViewportTop && caret.Bottom < context.TextView.ViewportBottom)
            {
                // Caret is in the viewport. Leave it as is
                return;
            }

            // Move the caret to the bottom line
            context.TextView.Caret.MoveTo(context.TextView.GetLastSufficientlyVisibleLine().Start);
        }
    }
}
