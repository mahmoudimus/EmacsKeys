using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command capitalizes the selection and then clears the selection.
    /// If there is no active selection, does nothing.
    /// 
    /// Keys: 
    /// </summary>
    [EmacsCommand(EmacsCommandID.CapitalizeSelection)]
    internal class CapitalizeSelectionCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            // TODO: the EditorOperations.Capitalize() is not quite working as expected
            // For example, it is not able to handle box selections accurately, and sometimes
            // also fails to capitalize the first word in a selection starting with empty space
            if (!context.TextView.Selection.IsEmpty)
            {
                context.EditorOperations.Capitalize();
                context.TextView.Selection.Clear();
            }
        }
    }
}