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
    /// This command switches the active tab to the ARGth next tab.
    /// 
    /// Keys: Ctrl+X+Right Arrow | Ctrl+X, Right Arrow
    /// </summary>
    [EmacsCommand(EmacsCommandID.NextWindow, InverseCommand = EmacsCommandID.PreviousWindow)]
    internal class NextWindowCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            int windowCount = context.WindowOperations.GetDocumentWindows().Count();
            int repeatNum = context.Manager.GetUniversalArgumentOrDefault(1) % windowCount;

            for (int i = 0; i < repeatNum; i++)
            {
                vs.ExecuteCommand("Window.NextTab");
            }
        }
    }
}
