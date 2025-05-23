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
using EnvDTE;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command goes to the start of the current word or the previous word if the caret is between words or at the start of a word already.  
    /// Moving to the start of a word places the caret immediately before the first character in the word.  
    /// With a prefix arg, the command moves backward that many times, but if prefix arg is negative, it goes forwards that many.
    /// 
    /// Keys: Ctrl+Left Arrow | Alt+B | Alt+Left Arrow
    /// </summary>
    [EmacsCommand(EmacsCommandID.WordPrevious, CanBeRepeated = true, InverseCommand = EmacsCommandID.WordNext)]
    internal class WordPreviousCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            // EditorOperations.MovePreviousWord is not working as expected for our spec.
            // For example: When the caret is at the indent position of a line, this command
            // move the caret to the beginning of the previous word in the previous line.
            // EditorOperations moves the caret to the beggining of the line instead.

            if (context.TextView.Selection.SelectedSpans.Count() > 1)
            {
                // Although not a perfect match for the our specifications, the Edit.WordPrevious
                // command provides easy support for multiline caret operations.
                DTE vs = context.Manager.ServiceProvider.GetService<DTE>();
                vs.ExecuteCommand("Edit.WordPreviousExtend");
                return;
            }

            var word = context.TextStructureNavigator.GetPreviousWord(context.EditorOperations);

            if (word.HasValue)
            {
                context.EditorOperations.MoveCaret(word.Value.Start);
            }
        }
    }
}
