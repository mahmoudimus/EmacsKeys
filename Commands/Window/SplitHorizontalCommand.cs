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
    /// This command splits the selected window in half. 
    /// The selected window is on the left. The newly split-off 
    /// window is on the right and displays the same buffer.
    /// 
    /// Keys: Ctrl+X, 3
    /// </summary>
    [EmacsCommand(EmacsCommandID.SplitHorizontal)]
    internal class SplitHorizontalCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            if (vs.ActiveDocument != null && vs.ActiveDocument.ActiveWindow != null)
            {
                context.Manager.StashView = context.TextView;
                vs.ActiveDocument.NewWindow().Activate();
                context.CommandRouter.ExecuteDTECommand("Window.NewVerticalTabGroup");
            }
        }
    }
}
