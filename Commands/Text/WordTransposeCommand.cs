using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// 
    /// Keys: 
    /// </summary>
    [EmacsCommand(EmacsCommandID.WordTranspose, UndoName="Transpose words")]
    internal class WordTransposeCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var previousWordSpan = context.TextStructureNavigator.GetPreviousWord(context.EditorOperations);

            if (previousWordSpan.HasValue)
            {
                var position = context.EditorOperations.GetNextAlphanumericCharacter(previousWordSpan.Value.End);
                var nextWordSpan = context.TextStructureNavigator.GetNextWord(position);

                if (nextWordSpan.HasValue)
                {
                    var previousWord = context.TextView.TextSnapshot.GetText(previousWordSpan.Value);
                    var nextWord = context.TextView.TextSnapshot.GetText(nextWordSpan.Value);

                    using (var edit = context.TextView.TextBuffer.CreateEdit())
                    {
                        edit.Replace(nextWordSpan.Value, previousWord);
                        edit.Replace(previousWordSpan.Value, nextWord);                        

                        edit.Apply();
                    }

                    context.TextView.Caret.MoveTo(new SnapshotPoint(context.TextView.TextSnapshot, nextWordSpan.Value.End));
                    context.TextView.Caret.EnsureVisible();
                }
            }
        }
    }
}
