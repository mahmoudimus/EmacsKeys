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
            var startPosition = context.TextView.GetCaretPosition();
            context.EditorOperations.MoveToNextNonWhiteSpaceCharacter();

            var word = context.TextStructureNavigator.GetNextWord(context.TextView);

            if (word.HasValue)
            {
                var enclosing = context.TextStructureNavigator.GetSpanOfEnclosing(word.Value);

                // Sometimes the start of the enclosing may be marked with spaces or newlines
                // Move beyond those to the first non-whitespace character
                var startEnclosing = context.EditorOperations.GetNextNonWhiteSpaceCharacter(enclosing.Start);

                if (startPosition == startEnclosing || context.TextView.GetCaretPosition() == startEnclosing)
                {
                    // The caret is at the beginning of an enclosing
                    // Move it to the end.
                    context.EditorOperations.MoveCaret(enclosing.End);
                }
                else
                {
                    // The caret is in the middle of an enclosing
                    // Move it to the end of the word
                    context.EditorOperations.MoveCaret(word.Value.End);
                }
            }
        }
    }
}
