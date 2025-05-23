﻿using System;
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
    /// This command goes to the end of the current word or the next word if the caret is between words or at the end of a word already.  
    /// Moving to the end of a word places the caret immediately after the last character in the word.  
    /// With a prefix arg, the command moves forward that many times, but if prefix arg is negative, it goes backwards that many.
    ///
    /// Keys: Ctrl+Right Arrow | Alt+F | Alt+Right Arrow
    /// </summary>
    [EmacsCommand(EmacsCommandID.WordNext, CanBeRepeated = true, InverseCommand = EmacsCommandID.WordPrevious)]
    internal class WordNextCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            // EditorOperations.MoveNextWord is not working as expected for our spec.
            // For example: When the caret is in the middle of a word this command should
            // move the caret to the end of the same word. EditorOperations moves the caret
            // to the next word instead.

            if (context.TextView.Selection.SelectedSpans.Count() > 1)
            {
                // Although not a perfect match for the our specifications, the Edit.WordNext
                // command provides easy support for multiline caret operations.
                DTE vs = context.Manager.ServiceProvider.GetService<DTE>();
                vs.ExecuteCommand("Edit.WordNextExtend");
                return;
            }

            var word = context.TextStructureNavigator.GetNextWord(context.EditorOperations);

            if (word.HasValue)
            {
                context.EditorOperations.MoveCaret(word.Value.End);
            }
        }
    }
}
