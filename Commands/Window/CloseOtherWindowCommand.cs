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
    /// This command kills the other pane, keeping focus where the current  window’s caret is, 
    /// centering the window around the current line.
    /// 
    /// Keys: Ctrl+X, 1
    /// </summary>
    [EmacsCommand(EmacsCommandID.CloseOtherWindow)]
    internal class CloseOtherWindowCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            if (vs.ActiveDocument != null && vs.ActiveDocument.ActiveWindow != null)
            {
                var textWindow = vs.ActiveDocument.ActiveWindow.Object as TextWindow;
                var documentWindows = vs.ActiveDocument.Windows;

                if (textWindow != null)
                {
                    // Close vertical panes
                    if (textWindow.Panes.Count == 2)
                    {
                        context.CommandRouter.ExecuteDTECommand("Window.Split");
                    }

                    // Close horizontal panes
                    if (documentWindows.Count > 1)
                    {
                        for (int i = 1; i <= documentWindows.Count; i++)
                        {
                            if (documentWindows.Item(i).Object !=  textWindow)
                            {
                                documentWindows.Item(i).Close();
                            }
                        }
                    }

                    // TODO: Merge tab groups
                    // context.CommandRouter.ExecuteDTECommand("Window.MoveAllToPreviousTabGroup");
                    // context.CommandRouter.ExecuteDTECommand("Window.MoveAllToNextTabGroup");
                }
            }
        }
    }
}
