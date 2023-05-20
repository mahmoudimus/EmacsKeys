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

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command moves forward across one balanced expression (s-expression).
    /// With a prefix arg, the command moves forward that many times, but if prefix arg is negative, it goes backwards that many.
    ///
    /// Keys: Ctrl+Alt+Right Arrow | Ctrl+Alt+F
    /// </summary>
    [EmacsCommand(EmacsCommandID.SExpressionNext, CanBeRepeated = true, InverseCommand = EmacsCommandID.SExpressionPrevious)]
    internal class SExpressionNextCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            // TODO: implement s-expression search
            var word = context.TextStructureNavigator.GetNextWord(context.TextView);

            if (word.HasValue)
            {
                context.EditorOperations.MoveCaret(word.Value.End);
            }
        }
    }
}
