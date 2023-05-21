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
    /// This command switches the active tab to the ARGth previous tab.
    /// 
    /// Keys: Ctrl+X+Left Arrow | Ctrl+X, Left Arrow
    /// </summary>
    [EmacsCommand(EmacsCommandID.PreviousWindow, CanBeRepeated = true, InverseCommand = EmacsCommandID.NextWindow)]
    internal class PreviousWindowCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            context.CommandRouter.ExecuteDTECommand("Window.PreviousTab");
        }
    }
}
