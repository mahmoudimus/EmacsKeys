using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command deletes one character before the caret.
    /// With a prefix arg, delete that many characters backwards.
    /// If the arg is negative, delete that many characters forward.
    /// 
    /// Keys: Bkspace
    /// </summary>
    [EmacsCommand(VSConstants.VSStd2KCmdID.BACKSPACE, IsKillCommand = true, CanBeRepeated = true, UndoName = "Delete")]
    internal class DeleteBackwardsCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            // Redecaring the command allow us to incorporate it to the universal argument and kill ring logic.
            // Since this is all handled by EmmacsCommandAttributes, we don't need any special handling here.
            context.EditorOperations.Backspace();
        }

        internal override void ExecuteInverse(EmacsCommandContext context)
        {
            context.EditorOperations.Delete();
        }
    }
}