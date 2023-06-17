using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command switches between horizontal and vertical split views.
    /// For example, if there are two horizontal panes, the pane in the right is moved to the bottom.
    /// On the other hand, if there are two vertical panes, the pane in the bottom is moved to the right.
    /// 
    /// Keys: Ctrl+X, 4
    /// </summary>
    [EmacsCommand(EmacsCommandID.ToggleWindowSplitLayout)]
    internal class ToggleWindowSplitLayoutCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            context.WindowOperations.ToggleSplitLayout();
        }
    }
}
