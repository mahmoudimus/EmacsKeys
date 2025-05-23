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
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            if (context.TextView.Selection.SelectedSpans.Count() > 1)
            {
                // TODO: proper support s-expression navigation with multiple carets
                vs.ExecuteCommand("Edit.WordPreviousExtend");
                return;
            }

            var position = context.EditorOperations.GetPreviousEnclosing(context.TextStructureNavigator, vs);
            context.EditorOperations.MoveCaret(position);

            // Update the selection. This is needed to ensure proper rendering,
            // since calling the Edit.GotoBrace can cancel existing highlights
            if (context.MarkSession.IsActive)
            {
                context.MarkSession.Activate();
            }
        }
    }
}
