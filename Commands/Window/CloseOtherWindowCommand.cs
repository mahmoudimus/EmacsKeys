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
            ThreadHelper.ThrowIfNotOnUIThread();
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
                        vs.ExecuteCommand("Window.Split");
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

                    // Merge tab groups
                    // Since we don't know if we are in the left pane or in the right pane,
                    // try merging both ways. Since one of the calls will always fail,
                    // remember to catch exceptions to avoid displaying on the minibuffer.
                    // Using vs.ExecuteCommand directly without going through the router give
                    // us more flexibilty to handle exceptions.
                    try
                    {
                        vs.ExecuteCommand("Window.MoveAllToPreviousTabGroup");
                    }
                    catch (COMException) {}
                    try
                    {
                        vs.ExecuteCommand("Window.MoveAllToNextTabGroup");
                    }
                    catch (COMException) {}
                }
            }
        }
    }
}
