using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Editor;
using EnvDTE;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation
{
    internal static class IEditorOperationsExtensions
    {
        private static bool ShouldExtendSelection(IEditorOperations editorOperations)
        {
            var session = MarkSession.GetSession(editorOperations.TextView);
            session.DeactivateAfterSearch();
            return session.IsActiveAndValid();
        }

        internal static void MoveToTopOfView(this IEditorOperations editorOperations)
        {
            editorOperations.MoveToTopOfView(ShouldExtendSelection(editorOperations));
        }

        internal static void MoveToBottomOfView(this IEditorOperations editorOperations)
        {
            editorOperations.MoveToBottomOfView(ShouldExtendSelection(editorOperations));
        }

        internal static void PageDown(this IEditorOperations editorOperations)
        {
            editorOperations.PageDown(ShouldExtendSelection(editorOperations));
        }

        internal static void PageUp(this IEditorOperations editorOperations)
        {
            editorOperations.PageUp(ShouldExtendSelection(editorOperations));
        }

        internal static void DeleteToEndOfPhysicalLine(this IEditorOperations editorOperations)
        {
            var caretPosition = editorOperations.TextView.GetCaretPosition().Position;
            editorOperations.Delete(caretPosition, editorOperations.GetCaretPhysicalLine().End - caretPosition);
        }

        internal static void DeleteToBeginningOfPhysicalLine(this IEditorOperations editorOperations)
        {
            var caretPosition = editorOperations.TextView.GetCaretPosition().Position;
            editorOperations.Delete(editorOperations.GetCaretPhysicalLine().Start, caretPosition - editorOperations.GetCaretPhysicalLine().Start);
        }

        //When word wrapping is enabled, the end of the visible line is different to the end of the physical line
        //The end of the physical line is where the first line break is found.
        //The end of the visible line is the end of the line where the caret is.
        internal static ITextSnapshotLine GetCaretPhysicalLine(this IEditorOperations editorOperations)
        {
            return editorOperations.TextView.Caret.Position.BufferPosition.GetContainingLine();
        }

        internal static void MoveCaretToStartOfPhysicalLine(this IEditorOperations editorOperations)
        {
            editorOperations.MoveCaretToStartOfPhysicalLine(ShouldExtendSelection(editorOperations));
        }

        internal static void MoveCaretToStartOfPhysicalLine(this IEditorOperations editorOperations, bool extendSelection)
        {
            editorOperations.MoveCaret(editorOperations.GetCaretPhysicalLine().Start, extendSelection);
        }

        internal static void MoveCaretToEndOfPhysicalLine(this IEditorOperations editorOperations)
        {
            editorOperations.MoveCaretToEndOfPhysicalLine(ShouldExtendSelection(editorOperations));
        }

        internal static void MoveCaretToEndOfPhysicalLine(this IEditorOperations editorOperations, bool extendSelection)
        {
            editorOperations.MoveCaret(editorOperations.GetCaretPhysicalLine().End, extendSelection);
        }

        internal static void MoveCaret(this IEditorOperations editorOperations, int position)
        {
            editorOperations.MoveCaret(new SnapshotPoint(editorOperations.TextView.TextSnapshot, position));
        }

        internal static void MoveCaret(this IEditorOperations editorOperations, SnapshotPoint bufferPosition)
        {
            editorOperations.MoveCaret(bufferPosition, ShouldExtendSelection(editorOperations));
        }

        internal static void MoveCaret(this IEditorOperations editorOperations, SnapshotPoint bufferPosition, bool extendSelection)
        {
            if (extendSelection && !editorOperations.TextView.Selection.IsEmpty)
            {
                // Extend selection to the desired position
                editorOperations.ExtendSelection(bufferPosition);
            }
            else if (extendSelection)
            {
                // We are in selection mode, but haven't started any actual selection
                // Using the Select() function will help us begin the selection
                VirtualSnapshotPoint anchorPoint = editorOperations.TextView.Selection.AnchorPoint;
                editorOperations.TextView.Caret.MoveTo(bufferPosition);
                editorOperations.TextView.Selection.Select(anchorPoint.TranslateTo(editorOperations.TextView.TextSnapshot), editorOperations.TextView.Caret.Position.VirtualBufferPosition);
            }
            else
            {
                editorOperations.TextView.Selection.Clear();
                editorOperations.TextView.Caret.MoveTo(bufferPosition);
            }

            editorOperations.TextView.Caret.EnsureVisible();
        }

        internal static void MoveToStartOfDocument(this IEditorOperations editorOperations)
        {
            editorOperations.MoveToStartOfDocument(ShouldExtendSelection(editorOperations));
        }

        internal static void MoveToEndOfDocument(this IEditorOperations editorOperations)
        {
            editorOperations.MoveToEndOfDocument(ShouldExtendSelection(editorOperations));
        }

        internal static void MoveToNextCharacter(this IEditorOperations editorOperations)
        {            
            editorOperations.MoveToNextCharacter(ShouldExtendSelection(editorOperations));
        }

        internal static void MoveToPreviousCharacter(this IEditorOperations editorOperations)
        {
            editorOperations.MoveToPreviousCharacter(ShouldExtendSelection(editorOperations));
        }

        private static SnapshotPoint GetNextCharacter(this IEditorOperations editorOperations, SnapshotPoint position, Func<Char, bool> predicate)
        {
            try
            {
                while (!predicate(position.GetChar()))
                {
                    position += 1;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // end of buffer reached
            }
            return position;
        }

        private static SnapshotPoint GetPreviousCharacter(this IEditorOperations editorOperations, SnapshotPoint position, Func<Char, bool> predicate)
        {
            try
            {
                while (!predicate((position - 1).GetChar()))
                {
                    position -= 1;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // beginning of buffer reached
            }
            return position;
        }

        internal static SnapshotPoint GetNextNonWhiteSpaceCharacter(this IEditorOperations editorOperations, SnapshotPoint position)
        {
            return GetNextCharacter(editorOperations, position, letter => !Char.IsWhiteSpace(letter));
        }

        internal static SnapshotPoint GetPreviousNonWhiteSpaceCharacter(this IEditorOperations editorOperations, SnapshotPoint position)
        {
            return GetPreviousCharacter(editorOperations, position, letter => !Char.IsWhiteSpace(letter));
        }

        internal static SnapshotPoint GetNextAlphanumericCharacter(this IEditorOperations editorOperations, SnapshotPoint position)
        {
            return GetNextCharacter(editorOperations, position, Char.IsLetterOrDigit);
        }

        internal static SnapshotPoint GetPreviousAlphanumericCharacter(this IEditorOperations editorOperations, SnapshotPoint position)
        {
            return GetPreviousCharacter(editorOperations, position, Char.IsLetterOrDigit);
        }

        internal static SnapshotPoint GetNextNonWhiteSpaceCharacter(this IEditorOperations editorOperations)
        {
            return GetNextNonWhiteSpaceCharacter(editorOperations, editorOperations.TextView.Caret.Position.BufferPosition);
        }

        internal static SnapshotPoint GetPreviousNonWhiteSpaceCharacter(this IEditorOperations editorOperations)
        {
            return GetPreviousNonWhiteSpaceCharacter(editorOperations, editorOperations.TextView.Caret.Position.BufferPosition);
        }

        internal static SnapshotPoint GetNextAlphanumericCharacter(this IEditorOperations editorOperations)
        {
            return GetNextAlphanumericCharacter(editorOperations, editorOperations.TextView.Caret.Position.BufferPosition);
        }

        internal static SnapshotPoint GetPreviousAlphanumericCharacter(this IEditorOperations editorOperations)
        {
            return GetPreviousAlphanumericCharacter(editorOperations, editorOperations.TextView.Caret.Position.BufferPosition);
        }

        internal static void MoveToNextNonWhiteSpaceCharacter(this IEditorOperations editorOperations)
        {
            editorOperations.MoveCaret(GetNextNonWhiteSpaceCharacter(editorOperations));
        }

        internal static void MoveToPreviousNonWhiteSpaceCharacter(this IEditorOperations editorOperations)
        {
            editorOperations.MoveCaret(GetPreviousNonWhiteSpaceCharacter(editorOperations));
        }

        internal static bool IsAtEmptyPair(SnapshotPoint position, bool forward)
        {
            // Check wether the caret is immediately before or after an empty pair,
            // such as () [] <> "" or ''
            var startPosition = forward ? position : position - 2;
            var endPosition = forward ? position + 1 : position - 1;
            switch (startPosition.GetChar())
            {
                case '(':
                    return endPosition.GetChar() == ')';
                case '[':
                    return endPosition.GetChar() == ']';
                case '{':
                    return endPosition.GetChar() == '}';
                case '<':
                    return endPosition.GetChar() == '>';
                case '"':
                    return endPosition.GetChar() == '"';
                case '\'':
                    return endPosition.GetChar() == '\'';
                default:
                    return false;
            }
        }

        internal static SnapshotPoint GetPairPosition(this IEditorOperations editorOperations, SnapshotPoint position, DTE dte, bool forward)
        {
            // TODO: call IVsTextViewFilter.GetPairExtents directly to avoid
            // unnecessary dte calls and caret operations

            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            String language = dte.ActiveDocument.Language;

            // The behavior of Edit.GotoBrace change depending on the language server implementation
            // In C/C++, the brace at point is prioritized, and the caret is placed on the match position
            // In CSharp, opening braces at or before point are prioritized, and the caret is placed after the match position
            // TODO: Verify the behavior in other languages

            // Handle empty pairs separately to avoid confusions when the caret is placed within braces
            // For example, in CSharp Edit.GotoBrace will never match to the first pair in ()()
            if (IsAtEmptyPair(position, forward))
            {
                return forward ? position + 2 : position - 2;
            }

            var caretPosition = editorOperations.TextView.GetCaretPosition();
            var startPosition = (language == "C/C++" && !forward) ? position - 1 : position;
            editorOperations.TextView.Caret.MoveTo(startPosition);
            dte.ExecuteCommand("Edit.GotoBrace");
            var matchingPosition = editorOperations.TextView.GetCaretPosition();
            editorOperations.TextView.Caret.MoveTo(caretPosition);

            return (language == "C/C++" && forward) ? matchingPosition + 1 : matchingPosition;
        }

        internal static SnapshotPoint GetPairPosition(this IEditorOperations editorOperations, DTE dte, bool forward)
        {
            return GetPairPosition(editorOperations, editorOperations.TextView.GetCaretPosition(), dte, forward);
        }

        internal static SnapshotPoint GetNextEnclosing(this IEditorOperations editorOperations, SnapshotPoint position, ITextStructureNavigator navigator, DTE dte)
        {
            // navigator.GetSpanOfEnclosing proved to be too unreliable for our purposes
            // use Edit.GotoBrace to find matching expressions instead

            String enclosingStartCharacters = "([{<'\"";

            var startPosition = editorOperations.GetNextNonWhiteSpaceCharacter(position);

            if (startPosition.Position == editorOperations.TextView.TextBuffer.CurrentSnapshot.Length)
            {
                // end of buffer reached
                return startPosition;
            }

            if (enclosingStartCharacters.Contains(startPosition.GetChar()))
            {
                // The caret is at potentially at the beginning of an s-expression.
                // Try getting the matching pair
                var endPosition = editorOperations.GetPairPosition(startPosition, dte, forward: true);

                if (endPosition > startPosition)
                {
                    return endPosition;
                }
            }

            var word = navigator.GetNextWord(editorOperations.GetNextAlphanumericCharacter(startPosition));

            if (word.HasValue)
            {
                return word.Value.End;
            }

            return startPosition;
        }

        internal static SnapshotPoint GetPreviousEnclosing(this IEditorOperations editorOperations, SnapshotPoint position, ITextStructureNavigator navigator, DTE dte)
        {
            // navigator.GetSpanOfEnclosing proved to be too unreliable for our purposes
            // use Edit.GotoBrace to find matching expressions instead

            String enclosingEndCharacters = ")]}>'\"";

            var endPosition = editorOperations.GetPreviousNonWhiteSpaceCharacter(position);

            if (endPosition == 0)
            {
                // beginning of buffer reached
                return endPosition;
            }

            if (enclosingEndCharacters.Contains((endPosition - 1).GetChar()))
            {
                // The caret is at potentially at the end of an s-expression.
                // Try getting the matching pair
                var startPosition = editorOperations.GetPairPosition(endPosition, dte, forward: false);

                if (startPosition < endPosition)
                {
                    return startPosition;
                }
            }

            var word = navigator.GetPreviousWord(editorOperations.GetPreviousAlphanumericCharacter(endPosition));

            if (word.HasValue)
            {
                return word.Value.Start;
            }

            return endPosition;
        }

        internal static SnapshotPoint GetNextEnclosing(this IEditorOperations editorOperations, ITextStructureNavigator navigator, DTE dte)
        {
            return GetNextEnclosing(editorOperations, editorOperations.TextView.Caret.Position.BufferPosition, navigator, dte);
        }

        internal static SnapshotPoint GetPreviousEnclosing(this IEditorOperations editorOperations, ITextStructureNavigator navigator, DTE dte)
        {
            return GetPreviousEnclosing(editorOperations, editorOperations.TextView.Caret.Position.BufferPosition, navigator, dte);
        }

        internal static void MoveToEndOfLine(this IEditorOperations editorOperations)
        {
            editorOperations.MoveToEndOfLine(ShouldExtendSelection(editorOperations));
        }

        internal static void MoveToStartOfLine(this IEditorOperations editorOperations)
        {
            editorOperations.MoveToStartOfLine(ShouldExtendSelection(editorOperations));
        }
        
        internal static void MoveLineUp(this IEditorOperations editorOperations)
        {
            editorOperations.MoveLineUp(ShouldExtendSelection(editorOperations));
        }

        internal static void MoveLineDown(this IEditorOperations editorOperations)
        {
            editorOperations.MoveLineDown(ShouldExtendSelection(editorOperations));
        }

        internal static void MoveToNextWord(this IEditorOperations editorOperations)
        {
            editorOperations.MoveToNextWord(ShouldExtendSelection(editorOperations));
        }

        internal static void MoveToPreviousWord(this IEditorOperations editorOperations)
        {
            editorOperations.MoveToPreviousWord(ShouldExtendSelection(editorOperations));
        }

        internal static void Delete(this IEditorOperations editorOperations, int start, int length)
        {
            editorOperations.Delete(new Span(start, length));
        }

        internal static void Delete(this IEditorOperations editorOperations, Span span)
        {            
            editorOperations.TextView.TextBuffer.Delete(span);
        }
    }
}
