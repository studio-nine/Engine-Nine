using System.Collections.Generic;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	internal class FragmentBlockNode : ParseNode
	{
		public Dictionary<string, FragmentSource> FragmentDeclarations { get; set; }
	}
}