using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation
{
    /// <summary>
    /// Defines window operations, including panes and tab group utilities
    /// </summary>
    internal class WindowOperations
    {
        DTE2 dte;
        TabNavigator tabNavigator;

        public WindowOperations(IServiceProvider serviceProvider)
        {
            this.tabNavigator = new TabNavigator(serviceProvider);
            this.dte = (DTE2)serviceProvider.GetService<DTE>();
            Assumes.Present(dte);
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
            var panes = this.tabNavigator.GetActivePanes(dte);
            
            if (!panes.Any())
            {
                return null;
            }

            // Return true when there is no second active pane
            return !(panes.Skip(1).Any());
        }

        public bool? IsLeftPane(Window activeWindow)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var panes = this.tabNavigator.GetActivePanes(dte);

            var leftPane = panes.OrderBy(pane => pane.Bounds.left).FirstOrDefault();
            
            if (leftPane == null)
            {
                return null;
            }

            return leftPane.Window == activeWindow;
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

            // Only leave the active window of each document open
            foreach(Document document in dte.Documents)
            {
                var activeDocumentWindow = document.ActiveWindow;
                foreach(Window window in document.Windows)
                {
                    if (window != activeDocumentWindow)
                    {
                        window.Close();
                    }
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

            // close all other windows of the same type as the active one
            foreach (Window window in dte.Windows)
            {
                if (window != activeDocumentWindow &&
                    window.Type == activeDocumentWindow.Type)
                {
                    window.Close();
                }
            }
        }

        public void ActivateNextPane()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var nextPane = this.tabNavigator.GetNextPane(dte);

            if (nextPane == null)
            {
                return;
            }

            nextPane.Window.Activate();
        }
    }
}
