using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Shell;
using System.Globalization;
using EnvDTE;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
    /// <summary>
    /// Recompute the indentation for the current line, delete the indentation on the line, and re-indent it.  
    /// When this command terminates, the caret is between the same two characters it was between when the command started.  
    /// However, if it was in the indentation, then the caret moves to be after the newly inserted indentation.  
    /// The indentation inserted is language context dependent (smart).
    /// If there's a multi line selection, then no-op.
    /// 
    /// Keys: Tab
    /// </summary>
    [EmacsCommand(VSConstants.VSStd2KCmdID.TAB, UndoName = "Indent")]
    internal class LineIndentCommand : EmacsCommand
    {
        internal override void Execute(EmacsCommandContext context)
        {
            ITextSelection selection = context.TextView.Selection;
            var line = context.EditorOperations.GetCaretPhysicalLine();

            // Return immediately if the buffer is read-only.
            if (context.TextBuffer.IsReadOnly(line.Extent))
            {
                return;
            }

            // If there's a multi-line selection, use the Edit.FormatSelection command and quit
            // Otherwise, clear the selection to avoid messing up with other formatting aspects
            // not related to the indentation.
            if (!selection.IsEmpty)
            {
                VirtualSnapshotSpan selectionSpan = selection.StreamSelectionSpan;

                if (selectionSpan.Start.Position.GetContainingLine().LineNumber != selectionSpan.End.Position.GetContainingLine().LineNumber)
                {
                    IndentSelection(context);
                    return;
                }
                context.MarkSession.Deactivate();
            }

            // If the line is empty, add indentation. Otherwise, use the Edit.FormatSelection
            // command to set the right indentation value and type (spaces or tabs).
            if (String.IsNullOrWhiteSpace(line.GetText()))
            {
                IndentEmptyLine(context);
            }
            else
            {
                IndentLine(context);
            }
        }

        /// <summary>
        ///Indent multi-line selection
        /// </summary>
        private void IndentSelection(EmacsCommandContext context)
        {
            DTE vs = context.Manager.ServiceProvider.GetService<DTE>();
            var position = context.TextView.CreateTrackingPoint();

            vs.ExecuteCommand("Edit.FormatSelection");
            context.MarkSession.Deactivate();

            // After executing the Edit.FormatSelection command, the caret is always
            // placed at the end of the selection. Move it back to the starting point.
            context.EditorOperations.MoveCaret(position.GetPosition(context.TextView.TextSnapshot));
        }


        /// <summary>
        /// Insert proper indentation on an empty line
        /// </summary>
        /// <param name="context"></param>
        private void IndentEmptyLine(EmacsCommandContext context)
        {
            var line = context.EditorOperations.GetCaretPhysicalLine();
            int? indentation = context.Manager.SmartIndentationService.GetDesiredIndentation(context.TextView, line);
            context.EditorOperations.Delete(line.Extent);
            if (indentation.HasValue)
            {
                var startPosition = context.TextView.GetCaretPosition().GetContainingLine().Start.Position;
                context.TextBuffer.Insert(startPosition, new String(' ', indentation.Value));
                context.EditorOperations.MoveCaret(startPosition + indentation.Value);
            }
            else
            {
                // We couldn't find any indentation level for the line, try the Indent command as the last resort
                context.EditorOperations.Indent();
            }

            // Finally, are any tab/spaces conversions necessary?
            if (!context.TextView.Options.IsConvertTabsToSpacesEnabled())
            {
                var position = context.TextView.CreateTrackingPoint(PointTrackingMode.Positive);
                context.EditorOperations.ConvertSpacesToTabs();
                context.EditorOperations.MoveCaret(position.GetPosition(context.TextView.TextSnapshot));
            }
        }

        /// <summary>
        /// Correct the indentation of the current line
        /// </summary>
        private void IndentLine(EmacsCommandContext context)
        {
            var position = context.TextView.CreateTrackingPoint();

            // SmartIndentationService seldom returns a value, and is not
            // smart enough to distinguish between tabs and spaces based
            // on the file context. Prefer Edit.FormatSelection on most cases.
            context.CommandRouter.ExecuteDTECommand("Edit.FormatSelection");

            // Recalculate the new indent position
            var indentPosition = context.EditorOperations.GetIndentationPosition();

            // Move the caret forward when needed
            if (indentPosition > position.GetPoint(context.TextView.TextSnapshot))
            {
                context.EditorOperations.MoveCaret(indentPosition);
            }
        }
    }
}