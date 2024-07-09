﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation
{
    /// <summary>
    /// Export a <see cref="IViewTaggerProvider"/>
    /// </summary>
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(MultipleCaretMarkerTag))]
    public class MultipleCaretTaggerProvider : IViewTaggerProvider
    {
        /// <summary>
        /// This method is called by VS to generate the tagger
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="textView"> The text view we are creating a tagger for</param>
        /// <param name="buffer"> The buffer that the tagger will examine for instances of the current word</param>
        /// <returns> Returns a MultipleCaretTagger instance</returns>
        public ITagger<T> CreateTagger<T>(ITextView view, ITextBuffer buffer) where T : ITag
        {
            // Only provide highlighting on the top-level buffer
            if (view.TextBuffer != buffer)
                return null;

            return view.Properties.GetOrCreateSingletonProperty<MultipleCaretTagger>(
                () => new MultipleCaretTagger(view, buffer)) as ITagger<T>;
        }
    }
}
