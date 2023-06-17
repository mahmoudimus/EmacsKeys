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

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// This command moves the point to the first non-whitespace character on this line.
    /// 
    /// Keys: Alt+M
    /// </summary>
    [EmacsCommand(EmacsCommandID.BackToIndentation)]
    internal class BackToIndentationCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var position = context.EditorOperations.GetIndentationPosition();
            context.EditorOperations.MoveCaret(position);
        }
    }
}