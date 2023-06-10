using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Design;
using System.ComponentModel.Composition;
using EnvDTE;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// Insert virtual caret at point, and then turns all virtual carets into actual carets.
    /// 
    /// Keys: Ctrl+M Ctrl+Space
    /// </summary>
    [EmacsCommand(EmacsCommandID.VirtualCaretActivate, UndoName ="Activate caret")]
    internal class VirtualCaretActivateCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var caretTagger = context.Manager.GetMultipleCaretTagger(context.TextView);

            if (caretTagger == null || caretTagger.CaretPoints.Count == 0)
            {
                return;
            }

            String identifier = "\u0000";
            var selection = context.TextView.Selection;
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();

            // First, add a caret at the position
            caretTagger.AddMarker(context.TextView.GetCaretPosition());

            // Then, insert an unique identifier at each caret position
            foreach (ITrackingPoint point in caretTagger.CaretPoints)
            {
                var snapshotPoint = point.GetPoint(context.TextBuffer.CurrentSnapshot);
                context.EditorOperations.MoveCaret(snapshotPoint);
                context.EditorOperations.InsertText(identifier);
            }

            // Once all identifiers are placed, select one and call Edit.InsertCaretsatAllMatching
            var position = context.TextView.GetCaretPosition();
            selection.Clear();
            selection.Select(new SnapshotSpan(position - identifier.Length, position), false);
            vs.ExecuteCommand("Edit.InsertCaretsatAllMatching");

            // Cleanup the identifiers and reset the tags
            context.EditorOperations.Delete();
            caretTagger.Clear();
        }
    }
}