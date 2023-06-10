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
    /// Places the mark at the end of the next balanced expression.
    /// With a prefix arg, the command moves forward that many times, but if prefix arg is negative, it goes backwards that many.
    /// The mark goes to the same place the caret would be placed with the EnclosingNextCommand.
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
            var arg = context.UniversalArgument.GetValueOrDefault(1);

            for (int i =0; i < Math.Abs(arg); i++)
            {
                if (arg > 0)
                {
                    position = context.EditorOperations.GetNextEnclosing(position, context.TextStructureNavigator, vs);
                }
                else if (arg < 0)
                {
                    position = context.EditorOperations.GetPreviousEnclosing(position, context.TextStructureNavigator, vs);
                }
            }

            context.MarkSession.PushMark(position, true);
            context.MarkSession.Activate();
        }
    }
}
