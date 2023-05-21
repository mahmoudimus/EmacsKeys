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
    [EmacsCommand(EmacsCommandID.EnclosingPrevious, CanBeRepeated = true, InverseCommand = EmacsCommandID.EnclosingNext)]
    internal class EnclosingPreviousCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var position = context.EditorOperations.GetPreviousEnclosing(context.TextStructureNavigator);
            context.EditorOperations.MoveCaret(position);
            // Sometimes the start of the enclosing may be marked with spaces or newlines
            // Move beyond those to the first non-whitespace character
            // Moving in two steps ensures that the start of the enclosure remains
            // visible within the final buffer
            context.EditorOperations.MoveToNextNonWhiteSpaceCharacter();
        }
    }
}
