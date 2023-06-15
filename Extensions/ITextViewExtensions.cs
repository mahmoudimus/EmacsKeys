using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Editor.EmacsEmulation.Utilities;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation
{
    internal static class ITextViewExtensions
    {
        private static string KillStringObjectID = "KillStringObjectID";

        internal static SnapshotPoint GetCaretPosition(this ITextView view)
        {
            return view.Caret.Position.BufferPosition;
        }

        internal static ITextViewLine GetFirstSufficientlyVisibleLine(this ITextView view, double threshold = 0.9)
        {
            if (threshold < 0.0 || threshold > 1.0)
            {
                throw new InvalidOperationException("Invalid threshold parameter, must be between 0.0 and 1.0");
            }

            ITextViewLine firstLine = view.TextViewLines.FirstVisibleLine;
            if (firstLine.Bottom - view.ViewportTop > threshold * view.LineHeight)
            {
                // line is sufficiently visible
                return firstLine;
            }
            // not sufficiently visible, return the line below it
            return view.TextViewLines.GetTextViewLineContainingYCoordinate(firstLine.Bottom + 0.5 * view.LineHeight);
        }

        internal static ITextViewLine GetLastSufficientlyVisibleLine(this ITextView view, double threshold = 0.9)
        {
            if (threshold < 0.0 || threshold > 1.0)
            {
                throw new InvalidOperationException("Invalid threshold parameter, must be between 0.0 and 1.0");
            }

            ITextViewLine lastLine = view.TextViewLines.LastVisibleLine;
            if (view.ViewportBottom - lastLine.Top > threshold * view.LineHeight)
            {
                // line is sufficiently visible
                return lastLine;
            }
            // not sufficiently visible, return the line above it
            return view.TextViewLines.GetTextViewLineContainingYCoordinate(lastLine.Top - 0.5 * view.LineHeight);
        }

        // A kill string stores the set of cut text in a view until it's pushed down to the clipboard. For instance,
        // if the user performs 4 kill word commands, the text for all those 4 commands is accumulated and then
        // pushed to the clipboard when the user performs a non kill command (for instance moving the caret)
        #region Kill String Management

        internal static string GetKillString(this ITextView view)
        {
            if (view.Properties.ContainsProperty(KillStringObjectID))
            {
                return view.Properties.GetProperty<StringBuilder>(KillStringObjectID).ToString();
            }
            else
            {
                return null;
            }
        }

        internal static void ResetKillString(this ITextView view)
        {
            view.Properties.RemoveProperty(KillStringObjectID);
        }

        internal static void AppendKillString(this ITextView view, string value)
        {
            view.Properties.GetOrCreateSingletonProperty<StringBuilder>(KillStringObjectID, () => new StringBuilder()).Append(value);
        }

        internal static void FlushKillString(this ITextView view, ClipboardRing clipboardRing)
        {
            string accumulatedKillString = view.GetKillString();

            if (!string.IsNullOrEmpty(accumulatedKillString))
            {
                clipboardRing.CopyToClipboard(accumulatedKillString);
                clipboardRing.Add(accumulatedKillString);

                view.ResetKillString();
            }
        }

        internal static ITrackingPoint CreateTrackingPoint(this ITextView view, int position)
        {
            return view.TextSnapshot.CreateTrackingPoint(position, PointTrackingMode.Negative);
        }

        internal static ITrackingPoint CreateTrackingPoint(this ITextView view)
        {
            return view.TextSnapshot.CreateTrackingPoint(view.Caret.Position.BufferPosition, PointTrackingMode.Negative);
        }

        #endregion
    }
}
