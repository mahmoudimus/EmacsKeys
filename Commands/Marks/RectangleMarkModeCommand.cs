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

            // Disable Rectangle-Mark mode if ARG is zero or negative,
            // or if it is already enabled and no arguments were passed
            if (!context.UniversalArgument.HasValue && (selection.Mode == TextSelectionMode.Box) ||
                context.UniversalArgument.HasValue && (context.UniversalArgument.Value <= 0))
            {
                // Already has a box selection, toggle stream mode instead
                selection.Mode = TextSelectionMode.Stream;
                return;
            }

            // Otherwise, enable the Rectangle-Mark mode.
            // i.e. if ARG is positive, or if the Rectangle-Mark mode is not enabled
            // and no arguments were passed

            if (selection.IsEmpty)
            {
                // Activate the region if needed
                context.MarkSession.PushMark();
            }

            selection.Mode = TextSelectionMode.Box;
        }
    }
}