using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using EnvDTE;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command splits the current window in half, centering the display of each window 
    /// around the current line and giving focus to the top pane.  
    ///
    /// Keys: Ctrl+X, 2
    /// </summary>
    [EmacsCommand(EmacsCommandID.SplitVertical)]
    internal class SplitVerticalCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var isSingleVerticalPane = context.WindowOperations.IsSingleVerticalPane();

            if (!isSingleVerticalPane.HasValue)
            {
                // Could not get the active window. Do nothing.
                return;
            }

            if (isSingleVerticalPane.Value)
            {
                // Recenter the liine before splitting the window
                ITextCaret caret = context.TextView.Caret;
                double viewHeight = context.TextView.ViewportHeight;

                if ((caret.Top + caret.Height) > (viewHeight / 2.0))
                {
                    // Move line to a quarter page position from the top.
                    // When the window is split, the carret will be on the center of each pane
                    // Note that we only need to render the top half of the page,
                    // since the bottom half will be replaced by the split window
                    context.TextView.DisplayTextLineContainingBufferPosition(caret.Position.BufferPosition, viewHeight / 4.0, ViewRelativePosition.Top,
                        context.TextView.ViewportWidth, viewHeight / 2.0);
                }

                // TODO: setting the below should manage the caret position on the newly
                // created pane. However, it is currently not showing the desired behavior...
                // context.Manager.StashView = context.TextView;
                context.CommandRouter.ExecuteDTECommand("Window.Split");
            }
            else
            {
                // Having more than 2 vertical panes is not currently supported.
                // If the window is already split, then fold it back
                // Calling the Window.Split command will toggle it for us
                context.CommandRouter.ExecuteDTECommand("Window.Split");
            }
        }
    }
}
