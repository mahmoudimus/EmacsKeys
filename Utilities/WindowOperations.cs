using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation
{
    /// <summary>
    /// Defines window operations, including panes and tab group utilities
    /// </summary>
    internal class WindowOperations
    {
        DTE dte;

        public WindowOperations(DTE dte)
        {
            this.dte = dte;
        }

        public bool? IsSingleVerticalPane()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (dte.ActiveDocument == null || dte.ActiveDocument.ActiveWindow == null)
            {
                return null;
            }
            var textWindow = dte.ActiveDocument.ActiveWindow.Object as TextWindow;

            if (textWindow == null)
            {
                return null;
            }

            return textWindow.Panes.Count == 1;
        }

        public bool? IsSingleHorizontalPane()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (dte.ActiveWindow == null ||
                dte.ActiveWindow.Type != vsWindowType.vsWindowTypeDocument)
            {
                return null;
            }
            var activeWindow = dte.ActiveWindow;

            foreach (Window window in dte.Windows)
            {
                if (window.Type == activeWindow.Type &&
                    window.Left != activeWindow.Left)
                {
                    return false;
                }
            }
            return true;
        }

        public bool? IsLeftPane(Window activeWindow)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            // TODO: Find proper way to check for leftmost window
            // Sometimes non-active windows do not display Left value properly
            return activeWindow.Left < 100;
        }

        public bool? IsLeftPane()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (dte.ActiveWindow == null ||
                dte.ActiveWindow.Type != vsWindowType.vsWindowTypeDocument)
            {
                return null;
            }

            return IsLeftPane(dte.ActiveWindow);
        }

        public void CloseDuplicatedDocumentWindows()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (dte.ActiveDocument == null ||
                dte.ActiveDocument.ActiveWindow == null)
            {
                // No active document. Do nothing.
                return;
            }

            var activeDocumentWindow = dte.ActiveDocument.ActiveWindow;
            var documentWindows = dte.ActiveDocument.Windows;

            foreach (Window window in documentWindows)
            {
                if (window != activeDocumentWindow)
                {
                    window.Close();
                }
            }
        }
    }
}
