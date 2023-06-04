using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Editor.EmacsEmulation.Commands;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation
{
    /// <summary>
    /// Custom filter to have a chance to control the status (enabled|disabled|etc) and execution of the Emacs commands
    /// </summary>
    internal class EmacsCommandsFilter : IOleCommandTarget
    {
        ITextView view;
        EmacsCommandsManager manager;
        CommandRouter router;

        public EmacsCommandsFilter(ITextView view, EmacsCommandsManager manager, CommandRouter router)
        {
            this.view = view;
            this.manager = manager;
            this.router = router;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (this.manager.IsEnabled)
            {
                var command = this.manager.GetCommandMetadata((int)nCmdID, pguidCmdGroup);

                if (command != null)
                {
                    // Make sure that MarkSession.ContinuousSelectionMode is set to false.
                    // This flag is used to update the selection during incremental search.
                    // HACK: Since we cannot detect when the search command has finished,
                    // set the flag to false at the beggining of each command.
                    {
                        var session = MarkSession.GetSession(view);
                        if (session != null)
                        {
                            session.ContinuousSelectionMode = false;
                        }
                    }

                    try
                    {
                        // clear the message bar
                        this.manager.ClearStatus();
                        // we did find a match so we execute the corresponding command
                        this.manager.Execute(this.view, command);
                    }
                    catch (Exception ex)
                    {
                        this.manager.UpdateStatus(ex.Message);
                        return VSConstants.S_FALSE;
                    }

                    // return S_OK to signal we successfully handled the command
                    return VSConstants.S_OK;
                }
                else
                {
                    // The command was not a command understood by the command manager and therefore not a kill command
                    // so we need to flush any accumulated kill string
                    if (this.IsKillwordFlushCommand(pguidCmdGroup, nCmdID))
                    {
                        view.FlushKillString(manager.ClipboardRing);
                    }

                    // Check if we should insert chars multiple times
                    if (pguidCmdGroup == VSConstants.VSStd2K &&
                        nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR &&
                        this.manager.UniversalArgument.HasValue &&
                        this.manager.UniversalArgument.Value > 1)
                    {
                        var count = this.manager.UniversalArgument.Value;
                        while (count-- > 0)
                        {
                            var result = router.ExecuteCommand(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                            if (result != VSConstants.S_OK)
                                return result;
                        }

                        return VSConstants.S_OK;
                    }
                    else if ((pguidCmdGroup == VSConstants.VSStd2K &&
                        (nCmdID == (uint)VSConstants.VSStd2KCmdID.ISEARCH || nCmdID == (uint)VSConstants.VSStd2KCmdID.ISEARCHBACK)))
                    {
                        var session = MarkSession.GetSession(view);
                        if (session.IsActiveAndValid())
                        {
                            // If there is already a selection, expand that selection interactively with each search operation
                            session.ContinuousSelectionMode = true;
                        }
                        else
                        {
                            // If there is no active selection, tell the MarkSession to avoid expanding the incoming selection,
                            // which is used only to highlight the search result
                            session.AfterSearch = true;
                        }
                    }
                    else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97 && nCmdID == (uint)VSConstants.VSStd97CmdID.Find)
                    {
                        // When a selection is active, the Find command searches for strings within the selection
                        // Leave this behavior as is, but avoid the automatic expansion of search results
                        MarkSession.GetSession(view).AfterSearch = true;
                    }
                    else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97 &&
                        (nCmdID == (uint)VSConstants.VSStd97CmdID.GotoDefn || nCmdID == (uint)VSConstants.VSStd97CmdID.GotoDecl || nCmdID == (uint)VSConstants.VSStd97CmdID.GotoRef))
                    {
                        // Goto commands determine the search result by the looking at the symbol at the
                        // start of the selection, and can highlight the results in the current or in another view.

                        // First, clear the selection to make sure that the search will be performed using the symbol
                        // under the caret (i.e. at the end of the current selection, if any)
                        MarkSession.GetSession(view).PushMark();

                        // Then, set the AfterSearch flag to indicate that the next selection result should not be extended by default
                        this.manager.AfterSearch = true;

                        // Finally, use the manager object to propagate the AfterSearch flag to potential text views activated by the search command.
                        // The manager AfterSearch flag is transfered to a MarkSession object during the SelectionChanged event
                        MarkSession.GetSession(view).AfterSearch = true;
                    }
                }
            }

            // if there is no match just pass the command along to the next registered filter
            return VSConstants.S_FALSE;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (this.manager.IsEnabled)
            {
                if (pguidCmdGroup == typeof(EmacsCommandID).GUID && cCmds > 0)
                {
                    var command = this.manager.GetCommandMetadata((int)prgCmds[0].cmdID, pguidCmdGroup);
                    if (command != null)
                    {
                        prgCmds[0].cmdf = (int)Microsoft.VisualStudio.OLE.Interop.Constants.MSOCMDF_ENABLED | (int)Microsoft.VisualStudio.OLE.Interop.Constants.MSOCMDF_SUPPORTED;
                        return VSConstants.S_OK;
                    }
                }
            }

            return VSConstants.S_FALSE;
        }

        /// <summary>
        /// Returns true for commands that would cause the currently accumulated killed words to be flushed to 
        /// the clipboard.
        /// </summary>
        private bool IsKillwordFlushCommand(Guid pguidCmdGroup, uint nCmdID)
        {
            if (manager.IsEnabled)
            {
                if (pguidCmdGroup == typeof(EmacsCommandID).GUID)
                {
                    if (nCmdID == (int)EmacsCommandID.DocumentEnd || nCmdID == (int)EmacsCommandID.DocumentStart)
                    {
                        return true;
                    }
                }

                if (pguidCmdGroup == typeof(VSConstants.VSStd97CmdID).GUID)
                {
                    if (nCmdID == (uint)VSConstants.VSStd97CmdID.Move)
                    {
                        return true;
                    }
                }

                if (pguidCmdGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
                {
                    if (nCmdID == (int)VSConstants.VSStd2KCmdID.BACKSPACE || nCmdID == (int)VSConstants.VSStd2KCmdID.DELETE
                            || nCmdID == (int)VSConstants.VSStd2KCmdID.UP || nCmdID == (int)VSConstants.VSStd2KCmdID.DOWN
                            || nCmdID == (int)VSConstants.VSStd2KCmdID.PAGEDN || nCmdID == (int)VSConstants.VSStd2KCmdID.PAGEUP
                            || nCmdID == (int)VSConstants.VSStd2KCmdID.LEFT || nCmdID == (int)VSConstants.VSStd2KCmdID.RIGHT
                            || nCmdID == (int)VSConstants.VSStd2KCmdID.TYPECHAR)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
