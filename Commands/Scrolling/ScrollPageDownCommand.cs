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
    /// This command move the window farther through the buffer or moves more lines of text up into view.  
    /// The last two lines in the display are shown at the top of the window.  
    /// If the caret location is visible after the scrolling operation, it stays where it is; otherwise, it moves to the physical start of the first line displayed.  
    /// With a prefix arg, this command shows that many more lines at the bottom of the window, and if the arg is negative, then it shows that many lines at the top of the window.
    /// The scrolling behavior is based on display lines, not physical lines, so if the buffer were all one line, you could still scroll through it.
    ///
    /// Keys: PgDn | Ctrl+V
    /// </summary>
    [EmacsCommand(EmacsCommandID.ScrollPageDown, InverseCommand = EmacsCommandID.ScrollPageUp)]
    internal class ScrollPageDownCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var caret = context.TextView.Caret;
            var lastLine = context.TextView.GetLastSufficientlyVisibleLine();
            var lineCount = context.TextBuffer.CurrentSnapshot.LineCount;

            // number of lines in the current view that should remain visible after scrolling
            int visibleLines = 2;

            if (context.TextBuffer.GetLineNumber(lastLine.Start) == (lineCount - 1) &&
                !context.Manager.UniversalArgument.HasValue)
            {
                // End of buffer reached. Do nothing
                return;
            }

            if (context.Manager.UniversalArgument.HasValue)
            {
                // Scroll down the designated number of lines
                // Always measure based on the first line, since the last line
                // must not be on the bottom of the viewport
                var firstLine = context.TextView.GetFirstSufficientlyVisibleLine();
                context.TextView.DisplayTextLineContainingBufferPosition(
                    firstLine.Start,
                    context.TextView.LineHeight * (-1) * context.Manager.GetUniversalArgumentOrDefault(0),
                    ViewRelativePosition.Top);
            }
            else
            {
                // Scroll down while keeping some overlapping lines
                context.TextView.DisplayTextLineContainingBufferPosition(
                    lastLine.Start,
                    context.TextView.LineHeight * Math.Max(0, visibleLines - 1),
                    ViewRelativePosition.Top);
            }

            if (caret.Top > context.TextView.ViewportTop && caret.Bottom < context.TextView.ViewportBottom)
            {
                // Caret is in the viewport. Leave it as is
                return;
            }

            // Move the caret to the top line
            context.TextView.Caret.MoveTo(context.TextView.GetFirstSufficientlyVisibleLine().Start);
        }
    }
}
