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

            var isSingleHorizontalPane = context.WindowOperations.IsSingleHorizontalPane();
            var isSingleVerticalPane = context.WindowOperations.IsSingleVerticalPane();

            if (isSingleVerticalPane.HasValue && !isSingleVerticalPane.Value)
            {
                // If there are multiple splits, calling the Window.Split
                // command one more time will fold them for us
                vs.ExecuteCommand("Window.Split");
            }

            // Close duplicated views of the active document
            context.WindowOperations.CloseDuplicatedDocumentWindows();

            // Merge tab groups
            if (isSingleHorizontalPane.HasValue && !isSingleHorizontalPane.Value)
            {
                var isLeftPane = context.WindowOperations.IsLeftPane();
                if (isLeftPane.HasValue && isLeftPane.Value)
                {
                    // The active window is in the left.
                    // Merge them to the right.
                    vs.ExecuteCommand("Window.MoveAllToNextTabGroup");
                }
                if (isLeftPane.HasValue && !isLeftPane.Value)
                {
                    // The active window is in the right.
                    // Merge them to the left.
                    vs.ExecuteCommand("Window.MoveAllToPreviousTabGroup");
                }
            }
        }
    }
}
