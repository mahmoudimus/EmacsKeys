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
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            if (vs.ActiveDocument != null && vs.ActiveDocument.ActiveWindow != null)
            {
                var textWindow = vs.ActiveDocument.ActiveWindow.Object as TextWindow;

                if (textWindow != null && textWindow.Panes.Count > 1)
                {
                    context.CommandRouter.ExecuteDTECommand("Window.NextSplitPane");    
                }
                else
                {
                    // TODO
                    // The current EmacsEmulation version has limited support for tab groups,
                    // which are used as horizontal panes (Ctrl+X, 3).
                    // Relying on the NavigateTabGroups extension provide us some functionality,
                    // although we still need a way to check if there are multiple tab groups or
                    // not in order to swap between panes as in Emacs.
                    context.CommandRouter.ExecuteDTECommand("Tools.NavigateTabGroups.Next");
                }
            }
        }
    }
}
