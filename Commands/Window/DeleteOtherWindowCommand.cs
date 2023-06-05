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
    /// This command kills the other pane and closes all other windows,
    /// except for pinned ones.
    ///
    /// Keys:
    /// </summary>
    [EmacsCommand(EmacsCommandID.DeleteOtherWindow)]
    internal class DeleteOtherWindowCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            var isSingleVerticalPane = context.WindowOperations.IsSingleVerticalPane();

            if (isSingleVerticalPane.HasValue && !isSingleVerticalPane.Value)
            {
                // If there are multiple splits, calling the Window.Split
                // command one more time will fold them for us
                vs.ExecuteCommand("Window.Split");
            }

            // Close all other document windows that are not pinned down
            context.WindowOperations.CloseOtherWindows();
        }
    }
}
