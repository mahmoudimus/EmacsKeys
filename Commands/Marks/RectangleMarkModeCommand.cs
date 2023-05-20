using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// Toggle the region as rectangular.
    /// If called interactively, enable Rectangle-Mark mode if ARG is
    /// positive, and disable it if ARG is zero or negative.
    /// Activates the region if needed. Only lasts until the region is deactivated.
    /// 
    /// Keys: Ctrl+X SPC
    /// </summary>
    [EmacsCommand(EmacsCommandID.RectangleMarkMode)]
    internal class RectangleMarkModeCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            ITextSelection selection = context.TextView.Selection;

            if (selection.Mode == TextSelectionMode.Box)
            {
                // Already has a box selection, toggle stream mode instead
                selection.Mode = TextSelectionMode.Stream;
                return;
            }

            // HACK: move one char forward and then back, since we cannot start an empty selection
            context.CommandRouter.ExecuteDTECommand("Edit.CharRightExtendColumn");
            context.CommandRouter.ExecuteDTECommand("Edit.CharLeftExtendColumn");
        }
    }
}