using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
	/// Activates the region
    /// 
    /// Keys: ?
    /// </summary>
    [EmacsCommand(EmacsCommandID.ActivateRegion)]
    internal class ActivateRegionCommand : EmacsCommand
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