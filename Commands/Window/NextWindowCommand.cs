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
    /// This command switches the active tab to the ARGth next tab.
    /// 
    /// Keys: Ctrl+X+Right Arrow | Ctrl+X, Right Arrow
    /// </summary>
    [EmacsCommand(EmacsCommandID.NextWindow, CanBeRepeated = true, InverseCommand = EmacsCommandID.PreviousWindow)]
    internal class NextWindowCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            context.CommandRouter.ExecuteDTECommand("Window.NextTab");
        }
    }
}
