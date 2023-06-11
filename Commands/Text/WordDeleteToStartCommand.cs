using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command kills from point to location backward-word would place the caret, including the prefix arg handling of backward-word.
    /// 
    /// Keys: Alt+Bkspace | Alt+Del
    /// </summary>
    [EmacsCommand(EmacsCommandID.WordDeleteToStart, CanBeRepeated = false, IsKillCommand = true, InverseCommand = EmacsCommandID.WordDeleteToEnd, UndoName = "Cut")]
    internal class WordDeleteToStartCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            if (context.MarkSession.IsActive && context.TextView.Selection.Mode == TextSelectionMode.Box ||
                context.TextView.Selection.SelectedSpans.Count() > 1)
            {
                // Edit.WordDeleteToStart does not exactly match our specs (see WordPreviousCommand).
                // Looping execution also creates multiple undo entries, which can be annoying
                // However, it provides easy support for multiline caret operations
                for (int i = 0; i < context.Manager.GetUniversalArgumentOrDefault(1); i++)
                {
                    context.CommandRouter.ExecuteDTECommand("Edit.WordDeleteToStart");
                }
                return;
            }

            SnapshotSpan? word = null;

            for (var counter = context.Manager.GetUniversalArgumentOrDefault(1); counter > 0; counter--)
            {
                if (word.HasValue)
                {
                    var position = context.EditorOperations.GetPreviousAlphanumericCharacter(word.Value.Start);
                    word = context.TextStructureNavigator.GetPreviousWord(position);
                }
                else
                    word = context.TextStructureNavigator.GetPreviousWord(context.EditorOperations);
            }

            if (word.HasValue)
            {
                var caretPosition = context.TextView.GetCaretPosition();
                context.EditorOperations.Delete(word.Value.Start, caretPosition - word.Value.Start);
            }
        }
    }
}