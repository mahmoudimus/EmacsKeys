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

                // TODO
                // The current EmacsEmulation version has limited support for tab groups,
                // which are used as horizontal panes (Ctrl+X, 3).
                // Relying on the NavigateTabGroups extension provide us some functionality,
                // although we still need a way to check if there are multiple tab groups or
                // not in order to swap between panes as in Emacs.
                context.CommandRouter.ExecuteDTECommand("Tools.NavigateTabGroups.Next");
            }
            else
            {
                // Multiple vertical planes detected. Switch to the next one.
                context.CommandRouter.ExecuteDTECommand("Window.NextSplitPane");
            }
        }
    }
}
