using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// Join this line to the previous line and fix up whitespace at the connection point.
    /// With prefix ARG, join the current line to the following line.
    /// If the region is active and there is no prefix ARG, join all lines in the region.
    /// 
    /// Keys: Alt+^
    /// </summary>
    [EmacsCommand(EmacsCommandID.JoinLines)]
    internal class JoinLinesCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            // The Edit.JoinLines command runs forward
            // Add some modifications to conform to the Emacs specifications

            if (context.Manager.UniversalArgument.HasValue)
            {
                // The command is called with a prefix argument
                // Deactivate the selection and join the following line
                context.MarkSession.Deactivate();
                JoinLines(context);
                return;
            }

            if (context.MarkSession.IsActiveAndValid())
            {
                // The command is called with a selection and without a prefix
                // Join all lines in the selection
                JoinLines(context);
                return;
            }

            if (context.TextView.GetCaretPosition().GetContainingLine().LineNumber == 0)
            {
                // The caret is on the first line of the file
                // Move to beginning of the buffer and quit
                context.EditorOperations.MoveCaret(0);
                return;
            }

            // The command has no prefixes and no selection
            // Join the previous line by moving one line up and then running the forward command
            context.EditorOperations.MoveLineUp();
            context.EditorOperations.MoveCaretToEndOfPhysicalLine();
            JoinLines(context);
        }

        private void JoinLines(EmacsCommandContext context)
        {
            context.CommandRouter.ExecuteDTECommand("Edit.JoinLines");
            // TODO: deactivate selection?
        }

    }
}
