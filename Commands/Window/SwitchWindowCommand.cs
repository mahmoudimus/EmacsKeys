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
    /// This command switches the active tab to the designated tab, or to the last active tab by default.
    /// 
    /// Keys: Ctrl+X, B
    /// </summary>
    [EmacsCommand(EmacsCommandID.SwitchWindow)]
    internal class SwitchWindowCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            context.CommandRouter.ExecuteDTECommand("Edit.GoToRecentFile");
        }
    }
}
