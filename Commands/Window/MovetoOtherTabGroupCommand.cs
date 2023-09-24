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
    /// This command moves the current window to the next tab group.
    /// If there is no other tab group, create a new vertical group.
    /// 
    /// Keys:
    /// </summary>
    [EmacsCommand(EmacsCommandID.MovetoOtherTabGroup)]
    internal class MovetoOtherTabGroupCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            switch (context.WindowOperations.GetSplitLayout())
            {
                case WindowOperations.SplitLayout.Invalid:
                {
                    // Could not get the active window. Do nothing.
                    context.Manager.UpdateStatus("Couldn't get the window layout.");
                    break;
                }

                case WindowOperations.SplitLayout.Single:
                {
                    // There is no other tab group to move to.
                    // Create a new vertical one.
                    vs.ExecuteCommand("Window.NewVerticalTabGroup");
                    break;
                }

                case WindowOperations.SplitLayout.Horizontal:
                {
                    if (context.WindowOperations.IsLeftPane().Value)
                    {
                        vs.ExecuteCommand("Window.MovetoNextTabGroup");
                    }
                    else
                    {
                        vs.ExecuteCommand("Window.MovetoPreviousTabGroup");
                    }
                    break;
                }
                
                case WindowOperations.SplitLayout.Vertical:
                {
                    if (context.WindowOperations.IsTopPane().Value)
                    {
                        vs.ExecuteCommand("Window.MovetoNextTabGroup");
                    }
                    else
                    {
                        vs.ExecuteCommand("Window.MovetoPreviousTabGroup");
                    }
                    break;
                }
            }
        }
    }
}
