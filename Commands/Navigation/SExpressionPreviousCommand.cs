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
    /// This command moves backward across one balanced expression (s-expression).
    /// With a prefix arg, the command moves backward that many times, but if prefix arg is negative, it goes forward that many.
    ///
    /// Keys: Ctrl+Alt+Left Arrow | Ctrl+Alt+B
    /// </summary>
    [EmacsCommand(EmacsCommandID.SExpressionPrevious, CanBeRepeated = true, InverseCommand = EmacsCommandID.SExpressionNext)]
    internal class SExpressionPreviousCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var startPosition = context.TextView.GetCaretPosition();
            context.EditorOperations.MoveToPreviousNonWhiteSpaceCharacter();

            var word = context.TextStructureNavigator.GetPreviousWord(context.TextView);

            if (word.HasValue)
            {
                var enclosing = context.TextStructureNavigator.GetSpanOfEnclosing(word.Value);
                if (startPosition == enclosing.End || context.TextView.GetCaretPosition() == enclosing.End)
                {
                    // The caret is at the end of an enclosing
                    // Move it to the beginning.
                    context.EditorOperations.MoveCaret(enclosing.Start);
                    // Sometimes the start of the enclosing may be marked with spaces or newlines
                    // Move beyond those to the first non-whitespace character
                    context.EditorOperations.MoveToNextNonWhiteSpaceCharacter();
                    // NOTE: moving in two steps ensures that the start of the enclosure remains
                    // visible within the final buffer
                }
                else
                {
                    // The caret is in the middle of an enclosing
                    // Move it to the beginning of the word
                    context.EditorOperations.MoveCaret(word.Value.Start);
                }
            }
        }
    }
}
