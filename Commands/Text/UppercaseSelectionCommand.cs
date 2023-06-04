using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command converts the selection to upper case and then clears the selection.
    /// If there is no active selection, does nothing.
    ///
    /// Keys: Ctrl+X, U
    /// </summary>
    [EmacsCommand(EmacsCommandID.UppercaseSelection)]
    internal class UppercaseSelectionCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            if (!context.TextView.Selection.IsEmpty)
            {
                context.EditorOperations.MakeUppercase();
                context.TextView.Selection.Clear();
            }
        }
    }
}