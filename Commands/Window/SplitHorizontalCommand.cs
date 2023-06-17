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
            var layout = context.WindowOperations.GetSplitLayout();

            if (layout == WindowOperations.SplitLayout.Invalid)
            {
                // Could not get the active window. Do nothing.
                return;
            }

            if (layout == WindowOperations.SplitLayout.Vertical)
            {
                // Visual Studio currently doesn't support any way of making a horizontal split in a vertical layout.
                // Having both vertical and horizontal tab groups is not allowed, and there is no "Window.Split"
                // equivalent for creating horizontal views of the same text.
                context.Manager.UpdateStatus("Creating horizontal splits in a vertical section is not supported.");
                return;
            }

            // Create a new view of the active document with the same formatting
            context.Manager.StashView = context.TextView;
            vs.ActiveDocument.NewWindow().Activate();

            if (layout == WindowOperations.SplitLayout.Single)
            {
                // Create a new tab group
                context.CommandRouter.ExecuteDTECommand("Window.NewVerticalTabGroup");
            }
            else
            {
                var isLeftPane = context.WindowOperations.IsLeftPane();

                // Move to the other tab group
                if (isLeftPane.Value)
                {
                    context.CommandRouter.ExecuteDTECommand("Window.MovetoNextTabGroup");
                }
                else
                {
                    context.CommandRouter.ExecuteDTECommand("Window.MovetoPreviousTabGroup");
                }
            }
        }
    }
}
