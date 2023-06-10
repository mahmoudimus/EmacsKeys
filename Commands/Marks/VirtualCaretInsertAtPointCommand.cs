using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// Inserts a virtual caret at the cursor point.
    /// Virtual carets are turned into actual carets when activated, allowing
    /// to add carets at arbitrary points without any mouse operation.
    /// 
    /// Keys: Ctrl+M Space
    /// </summary>
    [EmacsCommand(EmacsCommandID.VirtualCaretInsertAtPoint)]
    internal class VirtualCaretInsertAtPointCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            var caretTagger = context.Manager.GetMultipleCaretTagger(context.TextView);
            caretTagger?.AddMarkerAtPosition(context.TextView.GetCaretPosition());
        }
    }
}