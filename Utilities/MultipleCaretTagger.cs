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

        // The position of each virtual caret, represented by a span starting at the
        // desired position and with a size of 1.
        private NormalizedSnapshotSpanCollection CaretSpans { get; set; }

        public MultipleCaretTagger(ITextView view, ITextBuffer buffer)
        {
            this.View = view;
            this.Buffer = buffer;

            this.CaretSpans = new NormalizedSnapshotSpanCollection();
        }
        
        /// <summary>
        /// Adds a new virtual caret at the designated position
        /// </summary>
        internal void AddMarkerAtPosition(SnapshotPoint position)
        {
            List<SnapshotSpan> newCaretSpans = new List<SnapshotSpan>();
            SnapshotSpan span = new SnapshotSpan(position, position + 1);

            foreach (SnapshotSpan caret in this.CaretSpans)
            {
                newCaretSpans.Add(caret);
            }
            newCaretSpans.Add(span);

            this.CaretSpans = new NormalizedSnapshotSpanCollection(newCaretSpans);
            UpdateSpan();
        }

        /// <summary>
        /// Clears the current caret list
        /// </summary>
        internal void Clear()
        {
            this.CaretSpans = new NormalizedSnapshotSpanCollection();
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
            if (spans.Count == 0 || CaretSpans.Count == 0)
                yield break;

            // Translate the spans to a new collection when required
            if (spans[0].Snapshot != this.CaretSpans[0].Snapshot)
            {
                CaretSpans = new NormalizedSnapshotSpanCollection(
                    CaretSpans.Select(span => span.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive)));
            }

            // Yield all marker spans
            foreach (SnapshotSpan span in NormalizedSnapshotSpanCollection.Overlap(spans, CaretSpans))
            {
                yield return new TagSpan<MultipleCaretMarkerTag>(span, new MultipleCaretMarkerTag());
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}
