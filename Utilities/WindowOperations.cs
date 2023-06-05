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

        public IEnumerable<Window> GetDocumentWindows()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            // based from NavigateTabGroups implementation
            return from window in dte.Windows.Cast<Window>()
                   where window.Type == vsWindowType.vsWindowTypeDocument
                   select window;
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

            // TODO: Find proper way to check for single panes
            // Sometimes non-active windows do not display dimension values properly
            return (Math.Abs(dte.MainWindow.Width - activeWindow.Width) < 100);
        }

        public bool? IsLeftPane(Window activeWindow)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            // TODO: Find proper way to check for leftmost window
            // Sometimes non-active windows do not display dimension values properly
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

        public void CloseOtherWindows()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (dte.ActiveDocument == null ||
                dte.ActiveDocument.ActiveWindow == null)
            {
                // No active document. Do nothing.
                return;
            }

            var activeDocumentWindow = dte.ActiveDocument.ActiveWindow;

            foreach (Window window in dte.Windows)
            {
                if (window != activeDocumentWindow &&
                    window.Type == activeDocumentWindow.Type)
                {
                    window.Close();
                }
            }
        }
    }
}
