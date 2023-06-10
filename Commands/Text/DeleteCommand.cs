using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Text.Formatting;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Editor.EmacsEmulation.Commands
{
	/// <summary>
	/// Deletes one character forward.
	/// With a prefix arg, delete that many characters forward.
	/// If the arg is negative, delete that many characters backward.
	/// 
	/// Keys: Delete
	/// </summary>
	[EmacsCommand(VSConstants.VSStd97CmdID.Delete, IsKillCommand = true, CanBeRepeated = true, UndoName="Delete")]
	internal class DeleteCommand : EmacsCommand
	{
		internal override void Execute(EmacsCommandContext context)
		{
			// Redecaring the command allow us to incorporate it to the universal argument and kill ring logic.
			// Since this is all handled by EmmacsCommandAttributes, we don't need any special handling here.
			context.EditorOperations.Delete();
		}

		internal override void ExecuteInverse(EmacsCommandContext context)
		{
			context.EditorOperations.Backspace();
		}
	}
}