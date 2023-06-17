﻿using System;
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

        public SplitLayout GetSplitLayout()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var panes = this.tabNavigator.GetActivePanes(dte).ToList();
            int numPanes = panes.Count();

            if (numPanes == 0)
            {
                return SplitLayout.Invalid;
            }

            if (numPanes == 1)
            {
                return SplitLayout.Single;
            }

            if (panes.Skip(1).Any(pane => pane.Bounds.left != panes[0].Bounds.left))
            {
                return SplitLayout.Horizontal;
            }

            if (panes.Skip(1).Any(pane => pane.Bounds.top != panes[0].Bounds.top))
            {
                return SplitLayout.Vertical;
            }

            return SplitLayout.Invalid;
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

        private bool? IsFirstPaneInOrder(Window activeWindow, Func<TabNavigator.ActivePane, int> orderFunction)
        {
            if (activeWindow == null || activeWindow.Type != vsWindowType.vsWindowTypeDocument)
            {
                return null;
            }

            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var panes = this.tabNavigator.GetActivePanes(dte);

            var firstPane = panes.OrderBy(orderFunction).FirstOrDefault();

            if (firstPane == null)
            {
                return null;
            }

            return firstPane.Window == activeWindow;
        }

        public bool? IsLeftPane(Window activeWindow)
        {
            return IsFirstPaneInOrder(activeWindow, pane => pane.Bounds.left);
        }

        public bool? IsLeftPane()
        {
            return IsLeftPane(dte.ActiveWindow);
        }

        public bool? IsTopPane(Window activeWindow)
        {
            return IsFirstPaneInOrder(activeWindow, pane => pane.Bounds.top);
        }

        public bool? IsTopPane()
        {
            return IsTopPane(dte.ActiveWindow);
        }

        public bool? IsFirstPane(Window activeWindow, SplitLayout layout)
        {
            switch (layout)
            {
                case SplitLayout.Single:
                    return true;
                case SplitLayout.Horizontal:
                    return IsLeftPane(activeWindow);
                case SplitLayout.Vertical:
                    return IsTopPane(activeWindow);
                case SplitLayout.Invalid:
                    return null;
            }

            return null;
        }

        public bool? IsFirstPane()
        {
            return IsFirstPane(dte.ActiveWindow, GetSplitLayout());
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

        public void ToggleSplitLayout()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var panes = this.tabNavigator.GetActivePanes(dte).ToList();

            // TODO: currently only support two panes
            if (panes.Count != 2)
            {
                return;
            }

            // TODO: this implementation only swaps the active panes.
            // All panes that are opened in other tab groups but are
            // not active are merged into the 'main' window.
            void MaybeToggleLayout(Func<TabNavigator.ActivePane, int> orderFunction, String command)
            {
                if (orderFunction(panes[0]) == orderFunction(panes[1]))
                {
                    // Both panes have the same alignment.
                    // Not the group that we are searching for.
                    return;
                }

                var activeWindow = dte.ActiveWindow;
                var orderedPanes = panes.OrderBy(orderFunction).ToList();
                var isFirstPane = (activeWindow == orderedPanes[0].Window);

                if (isFirstPane)
                {
                    // The active window is the first on the order.
                    // Merge to the next tab group
                    dte.ExecuteCommand("Window.MoveAllToNextTabGroup");
                }
                else
                {
                    // The active window is the last on the order.
                    // Merge to the previous tab group.
                    dte.ExecuteCommand("Window.MoveAllToPreviousTabGroup");
                }

                if (isFirstPane) orderedPanes[1].Window.Activate();
                dte.ExecuteCommand(command);
                if (isFirstPane) activeWindow.Activate();
            };

            // Multiple horizontal splits.
            // Turn into vertical split (horizontal tab groups).
            MaybeToggleLayout(pane => pane.Bounds.left, "Window.NewHorizontalTabGroup");

            // Multiple vertical splits.
            // Turn into horizontal split (vertical tab groups).
            MaybeToggleLayout(pane => pane.Bounds.top, "Window.NewVerticalTabGroup");
        }

        public enum SplitLayout
        {
            Vertical,
            Horizontal,
            Single,
            Invalid,
        }
    }
}
