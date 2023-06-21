using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using WindowFrame = Microsoft.VisualStudio.Platform.WindowManagement.WindowFrame;

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
            return this.tabNavigator.GetActiveWindows(dte);
        }

        /// <summary>
        /// Get all active panes, excluding duplicated views of the same document
        /// in cases such as a XAML editor with an attached designer.
        /// </summary>
        /// <returns>
        /// An enumerable with all active panes.
        /// </returns>
        private IEnumerable<TabNavigator.ActivePane> GetActivePanes()
        {
            // HACK: WPF windows with a XAML and a designer view will generate a total of three panes:
            // - An editor pane with the XAML text
            // - A viewer pane with the design preview
            // - A pane with both the editor and the viewer
            // Both of these panes will have the same Window.Document and AssociatedFrame.AnnotatedTitle,
            // but the Window.Object of the viewer and of the overall pane will be set to null.
            // For our purposes, we want to handle the above as a single instance, which points to the text
            // pane but holds the size of the whole window (i.e. the size of the overall pane).
            //
            // related to: https://github.com/zastrowm/vs-NavigateTabGroups/issues/7

            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var activePanes = this.tabNavigator.GetActivePanes(dte).ToList();

            foreach (TabNavigator.ActivePane pane in activePanes)
            {
                if (pane.Window.Object != null)
                {
                    var parent = FindParentPane(activePanes, pane);

                    if (parent != null)
                    {
                        yield return new TabNavigator.ActivePane(pane.Window, pane.AssociatedFrame, parent.Bounds);
                    }
                    else
                    {
                        yield return pane;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all panes, active or inactive.
        /// Note that the boundaries of inactive panes is innacurate.
        /// </summary>
        /// <returns>
        /// An enumerable with all detected panes.
        /// </returns>
        private IEnumerable<TabNavigator.ActivePane> GetPanes()
        {
            return from frame in this.tabNavigator.GetFrames()
                   let window = VsShellUtilities.GetWindowObject(frame)
                   where window != null
                   select new TabNavigator.ActivePane(window, frame);
        }

        private TabNavigator.ActivePane FindParentPane(IEnumerable<TabNavigator.ActivePane> activePanes, TabNavigator.ActivePane pane)
        {
            bool Contains(RECT lhs, RECT rhs)
            {
                return !lhs.Equals(rhs) &&
                       lhs.left <= rhs.left &&
                       lhs.top <= rhs.top &&
                       lhs.bottom >= rhs.bottom &&
                       lhs.right >= rhs.right;
            }

            bool IsParentPane(TabNavigator.ActivePane parent, TabNavigator.ActivePane child)
            {
                return parent.Window.Object == null &&
                    parent.Window.Document == child.Window.Document &&
                    Contains(parent.Bounds, child.Bounds);
            }

            return activePanes.FirstOrDefault(parent => IsParentPane(parent, pane));
        }

        public SplitLayout GetSplitLayout()
        {
            var panes = GetActivePanes().ToList();
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
            var panes = this.tabNavigator.GetActivePanes(dte).Where(pane => pane.Window.Object != null);

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

            var panes = GetActivePanes();
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
            if (dte.ActiveDocument == null ||
                dte.ActiveDocument.ActiveWindow == null)
            {
                // No active document. Do nothing.
                return;
            }

            var panes = GetPanes().ToList();

            // Only leave the active window of each document open
            foreach (Document document in dte.Documents)
            {
                var activeWindow = document.ActiveWindow;
                var activeFrame = panes.FirstOrDefault(pane => pane.Window == activeWindow)?.AssociatedFrame as WindowFrame;
                if (activeFrame?.Clones != null)
                {
                    // Compute which windows to close before we actually start closing them,
                    // to avoid having the data altered during operation.
                    var clones = activeFrame.Clones.Where(frame => frame != activeFrame).ToList();
                    foreach (WindowFrame frame in clones)
                    {
                        var window = panes.FirstOrDefault(pane => pane.AssociatedFrame == frame)?.Window;
                        window?.Close();
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
                    // FIXME: if the active window has attached views, such as a designer in a XAML editor,
                    // we don't want to close that. For now, avoid closing all attached views with the same
                    // Document instance of the active window, although this may be unable to close some windows.
                    if (!(window.Object == null && window.Document == activeDocumentWindow.Document))
                    {
                        window.Close();
                    }
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
            var panes = GetActivePanes().ToList();

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
