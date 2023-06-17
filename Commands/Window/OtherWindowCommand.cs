using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command switches focus from one pane to the other when the window is split, 
    /// ensuring point is displayed and centering the current line if the point is out of display.
    /// 
    /// Keys: Ctrl+X, O
    /// </summary>
    [EmacsCommand(EmacsCommandID.OtherWindow)]
    internal class OtherWindowCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var isSingleVerticalPane = context.WindowOperations.IsSingleVerticalPane();

            if (!isSingleVerticalPane.HasValue)
            {
                // Could not identify active window or document.
                // Do nothing
                return;
            }

            if (isSingleVerticalPane.Value)
            {
                // No other vertical panes. Switch to the other horizontal pane.
                context.WindowOperations.ActivateNextPane();
            }
            else
            {
                // Multiple vertical planes detected. Switch to the next one.
                context.CommandRouter.ExecuteDTECommand("Window.NextSplitPane");
            }
        }
    }
}
