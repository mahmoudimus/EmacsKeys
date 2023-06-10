using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.Windows.Media;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation
{
    /// <summary>
    /// Create a new MarkerTag class with the desired format
    /// </summary>
    public class MultipleCaretMarkerTag : TextMarkerTag 
    {
        public MultipleCaretMarkerTag() : base("MarkerFormatDefinition/MultipleCaretFormatDefinition")
        {}
    }

    /// <summary>
    /// Format definition for the multiple caret tag
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [Name("MarkerFormatDefinition/MultipleCaretFormatDefinition")]
    [UserVisible(true)]
    internal class MultipleCaretFormatDefinition : MarkerFormatDefinition
    {
        public MultipleCaretFormatDefinition()
        {
            this.BackgroundColor = Colors.Transparent;
            this.ForegroundColor = Colors.Black;
            this.DisplayName = "Virtual Cursor";
            this.ZOrder = 5;
        }
    }

    /// <summary>
    /// Tagger used to highlight 'virtual carets', which signalize the position in which
    /// actual carets will be placed when the Emacs multiple caret mode is activated.
    /// </summary>
    public class MultipleCaretTagger : ITagger<MultipleCaretMarkerTag>
    {
        private ITextView View { get; set; }
        private ITextBuffer Buffer { get; set; }

        // The position of each virtual caret
        public HashSet<ITrackingPoint> CaretPoints { get; private set; }

        public MultipleCaretTagger(ITextView view, ITextBuffer buffer)
        {
            this.View = view;
            this.Buffer = buffer;

            this.CaretPoints = new HashSet<ITrackingPoint>();
        }

        /// <summary>
        /// Adds a new virtual caret at the designated position
        /// </summary>
        internal void AddMarkerAtPosition(SnapshotPoint position)
        {
            this.CaretPoints.Add(CreateTrackingPoint(position));
            UpdateSpan();
        }

        private ITrackingPoint CreateTrackingPoint(int position)
        {
            return View.TextSnapshot.CreateTrackingPoint(position, PointTrackingMode.Negative);
        }

        /// <summary>
        /// Clears the current caret list
        /// </summary>
        internal void Clear()
        {
            this.CaretPoints.Clear();
            UpdateSpan();
        }

        /// <summary>
        /// Calls the TagsChanged method to update the view
        /// </summary>
        private void UpdateSpan()
        {
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(this.Buffer.CurrentSnapshot, 0, this.Buffer.CurrentSnapshot.Length)));
        }

        /// <summary>
        /// Yields every marker in the given span
        /// </summary>
        /// <param name="spans">A read-only span of text to be searched for instances of CurrentWord</param>
        public IEnumerable<ITagSpan<MultipleCaretMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || CaretPoints.Count == 0)
                yield break;

            var snapshot = spans[0].Snapshot;

            // Yield all markers
            // Each marker is represented as a span starting at the desired position and with a size of 1.
            foreach (ITrackingPoint point in this.CaretPoints)
            {
                SnapshotPoint position = point.GetPoint(snapshot);
                yield return new TagSpan<MultipleCaretMarkerTag>(new SnapshotSpan(position, position + 1), new MultipleCaretMarkerTag());
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}
