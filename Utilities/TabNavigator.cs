// The code below is provided by the `Navigate Tab Groups` extension version 1.4.0,
// which is copyrighted by Magnus Osterlind and Mackenzie Zastrow under the MIT license.
// https://marketplace.visualstudio.com/items?itemName=MackenzieZastrow.NavigateTabGroups
// https://github.com/zastrowm/vs-NavigateTabGroups

// MIT License
//
// Copyright (c) 2016 Magnus Osterlind
// Copyright (c) 2017 Mackenzie Zastrow
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


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

namespace Microsoft.VisualStudio.Editor.EmacsEmulation
{
    internal sealed class TabNavigator
    {
        private readonly IVsUIShell _uiShell;

        public TabNavigator(IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _uiShell = (IVsUIShell)serviceProvider.GetService(typeof(SVsUIShell));
            Assumes.Present(_uiShell);
        }

        /// <summary> Get all of the Windows that have an associated frame. </summary>
        internal IEnumerable<ActivePane> GetActivePanes(DTE2 dte)
        {
            var framesAndAssociatedWindows = GetActiveWindowToFramesLookup(dte.ActiveWindow);

            foreach (var window in GetActiveWindows(dte))
            {
                if (framesAndAssociatedWindows.TryGetValue(window, out var frame))
                {
                    yield return new ActivePane(window, frame);
                }
            }
        }

        internal ActivePane GetNextPane(DTE2 dte)
        {
            var currentlyActiveWindow = dte.ActiveWindow;
            var activePanes = GetActivePanes(dte).Where(pane => pane.Window != null).ToList();
            var activePane = LookupPaneByWindow(currentlyActiveWindow, activePanes)
                ?? LookupPaneByHierarchy(currentlyActiveWindow, activePanes);

            if (activePane == null)
            {
                return null;
            }

            var filteredPanesEnumerable = from pane in activePanes
                                          orderby pane.Bounds.left, pane.Bounds.top
                                          select pane;
            var filteredPanes = filteredPanesEnumerable.ToList();
            var indexOfCurrentTabGroup = filteredPanes.IndexOf(activePane);
            int nextIndex = Clamp(filteredPanes.Count, indexOfCurrentTabGroup + 1);

            return filteredPanes[nextIndex];
        }

        /// <summary> Gets all of the windows that are currently positioned with a valid Top or Left. </summary>
        internal IEnumerable<Window> GetActiveWindows(DTE2 dte)
        {
            // documents that are not the focused document in a group will have Top == 0 && Left == 0
            return from window in dte.Windows.Cast<Window>()
                   where window.Kind == "Document"
                   select window;
        }

        /// <summary> Get all known <see cref="IVsWindowFrame"/>, lazily, that are active/on-screen. </summary>
        private Dictionary<Window, IVsWindowFrame> GetActiveWindowToFramesLookup(Window activeWindow)
        {
            bool IsOnScreen(IVsWindowFrame frame)
            {
                if (frame.IsOnScreen(out int onScreen) != VSConstants.S_OK) return false;
                return onScreen != 0;
            }

            var actives = from frame in GetFrames()
                          let window = VsShellUtilities.GetWindowObject(frame)
                          where window != null
                          where IsOnScreen(frame) || window == activeWindow
                          select new { window, frame };

            var windowToFrameLookup = new Dictionary<Window, IVsWindowFrame>();
            foreach (var active in actives)
            {
                windowToFrameLookup[active.window] = active.frame;
            }

            return windowToFrameLookup;
        }

        /// <summary> Get all known <see cref="IVsWindowFrame"/>, lazily. </summary>
        internal IEnumerable<IVsWindowFrame> GetFrames()
        {
            var array = new IVsWindowFrame[1];
            _uiShell.GetDocumentWindowEnum(out var frames);

            while (true)
            {
                var errorCode = frames.Next(1, array, out _);
                if (errorCode != VSConstants.S_OK)
                    break;

                yield return array[0];
            }
        }

        /// <summary> Looks up a pane by comparing the window of the pane to a given value. </summary>
        private static ActivePane LookupPaneByWindow(Window windowToSearchFor, List<ActivePane> activePanes)
        {
            ActivePane activePane = null;
            foreach (var pane in activePanes)
            {
                if (pane.Window == windowToSearchFor)
                {
                    activePane = pane;
                    break;
                }
            }
            return activePane;
        }

        /// <summary>
        ///  Searches for a pane by seeing if any of the window's parents are a pane that we know about.
        /// </summary>
        private static ActivePane LookupPaneByHierarchy(Window childWindow, List<ActivePane> activePanes)
        {
            // welp, we're not directly in a document.  BUT, what if the window is inside of a
            // document?  In that case, try to go up until we find a document pane that we know about.
            //
            // This happens for Project Property panes
            var currentHwnd = new IntPtr(childWindow.HWnd);

            // max out at 20 just in case we keep going up and never find anything.
            for (int i = 0; i < 20 && currentHwnd != IntPtr.Zero; i++)
            {
                currentHwnd = GetParent(currentHwnd);
                var activePane = LookupPaneByHwnd(currentHwnd);
                if (activePane != null)
                {
                    return activePane;
                }
            }

            return null;

            ActivePane LookupPaneByHwnd(IntPtr searchHwnd)
            {
                foreach (var pane in activePanes)
                {
                    if (new IntPtr(pane.Window.HWnd) == searchHwnd)
                    {
                        return pane;
                    }
                }

                return null;
            }
        }

        /// <summary> Clamp the given value to be between 0 and <paramref name="count"/>. </summary>
        private static int Clamp(int count, int number)
          => (number < 0 ? number + count : number) % count;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        /// <summary> Information about an active window. </summary>
        internal class ActivePane
        {
            public ActivePane(Window window, IVsWindowFrame associatedFrame, RECT? bounds = null)
            {
                Window = window;
                AssociatedFrame = associatedFrame;
                if (bounds.HasValue)
                {
                    Bounds = bounds.Value;
                }
                else
                {
                    Bounds = MeasureBounds();
                }
            }

            public Window Window { get; }

            public IVsWindowFrame AssociatedFrame { get; }

            public RECT Bounds { get; private set; }

            /// <summary> Measure the bounds of the given window </summary>
            private RECT MeasureBounds()
            {
                var window = Window;
                var textView = VsShellUtilities.GetTextView(AssociatedFrame);

                if (Window.Object != null && textView != null &&
                    GetWindowRect(textView.GetWindowHandle(), out var rect))
                {
                    return rect;
                }

                if (Window.HWnd != 0 && GetWindowRect(new IntPtr(Window.HWnd), out var rect2))
                {
                    return rect2;
                }

                // fallback where Top is wrong for windows that are vertically split. 
                return new RECT
                {
                    left = window.Left,
                    right = window.Left + window.Width,
                    top = window.Top,
                    bottom = window.Top + window.Height
                };
            }
        }
    }
}
