using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using EnvDTE;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command kills from point to location forward-sexp would place the caret, including the prefix arg handling of forward-word.
    ///
    /// Keys: Ctrl+Alt+K
    /// </summary>
    [EmacsCommand(EmacsCommandID.EnclosingDeleteToEnd, IsKillCommand = true, InverseCommand = EmacsCommandID.EnclosingDeleteToStart, UndoName = "Cut")]
    internal class EnclosingDeleteToEndCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            if (context.MarkSession.IsActive && context.TextView.Selection.Mode == TextSelectionMode.Box ||
                context.TextView.Selection.SelectedSpans.Count() > 1)
            {
                // TODO: add proper support for s-expression with multiple carets
                for (int i = 0; i < context.Manager.GetUniversalArgumentOrDefault(1); i++)
                {
                    context.CommandRouter.ExecuteDTECommand("Edit.WordDeleteToEnd");
                }
                return;
            }

            var caretPosition = context.TextView.GetCaretPosition();
            var position = caretPosition;

            for (var counter = context.Manager.GetUniversalArgumentOrDefault(1); counter > 0; counter--)
            {
                position = context.EditorOperations.GetNextEnclosing(position, context.TextStructureNavigator, vs);
            }

            if (position != caretPosition)
            {
                context.EditorOperations.Delete(caretPosition, position - caretPosition);
            }
        }
    }
}
