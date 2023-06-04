using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command converts the selection to lower case and then clears the selection.
    /// If there is no active selection, does nothing.
    ///
    /// Keys: Ctrl+X, L
    /// </summary>
    [EmacsCommand(EmacsCommandID.LowercaseSelection)]
    internal class LowercaseSelectionCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            if (!context.TextView.Selection.IsEmpty)
            {
                context.EditorOperations.MakeLowercase();
                context.TextView.Selection.Clear();
            }
        }
    }
}