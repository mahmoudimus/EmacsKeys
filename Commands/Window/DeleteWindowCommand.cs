using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.ComponentModel.Composition;
using EnvDTE;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command deletes the current window.
    /// 
    /// Keys: Ctrl+X, 0
    /// </summary>
    [EmacsCommand(EmacsCommandID.DeleteWindow)]
    internal class DeleteWindowCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            var isSingleVerticalPane = context.WindowOperations.IsSingleVerticalPane();

            if (!isSingleVerticalPane.HasValue)
            {
                // Could not get the active window. Do nothing.
                return;
            }

            if (isSingleVerticalPane.Value)
            {
                // No active split panes. Close window
                vs.ActiveWindow.Close();
            }
            else
            {
                // Multiple split panes.
                // Calling Window.Split again will fold them for us.
                vs.ExecuteCommand("Window.NextSplitPane");
                vs.ExecuteCommand("Window.Split");
            }
        }
    }
}
