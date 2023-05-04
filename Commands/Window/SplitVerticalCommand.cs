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
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            if (vs.ActiveDocument != null && vs.ActiveDocument.ActiveWindow != null)
            {
                var textWindow = vs.ActiveDocument.ActiveWindow.Object as TextWindow;

                if (textWindow != null && textWindow.Panes.Count == 1)
                {
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

                    context.CommandRouter.ExecuteDTECommand("Window.Split");
                }
            }
        }
    }
}
