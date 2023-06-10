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
    /// Moves the mark to the next (forward or backward) balanced expression.
    /// If there is no active selection, places the mark at the forward balanced expression.
    /// With a prefix arg, the mark moves forward that many times, but if prefix arg is negative, it goes backwards that many.
    ///
    /// Keys: Ctrl+Alt+@ | Ctrl+Alt+Space
    /// </summary>
    [EmacsCommand(EmacsCommandID.MarkEnclosing)]
    internal class MarkEnclosingCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();
            SnapshotPoint position = context.TextView.GetCaretPosition();

            bool hasArgument = context.UniversalArgument.HasValue;
            int arg = context.UniversalArgument.GetValueOrDefault(1);
            bool backwards = (arg < 0);

            if (context.MarkSession.IsActive)
            {
                // When there is an active selection, move the mark in order to expand the existing selection.
                // However, if the user provided an argument, move in the specified direction instead.
                SnapshotPoint markPosition = context.MarkSession.GetMarkPoint();
                if (!hasArgument)
                {
                    backwards = (position > markPosition);
                }
                position = markPosition;
            }

            for (int i = Math.Abs(arg); i > 0; i--)
            {
                if (backwards)
                {
                    position = context.EditorOperations.GetPreviousEnclosing(position, context.TextStructureNavigator, vs);
                }
                else
                {
                    position = context.EditorOperations.GetNextEnclosing(position, context.TextStructureNavigator, vs);
                }
            }

            context.MarkSession.PushMark(position);
        }
    }
}
