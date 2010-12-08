using System.Collections.Generic;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	internal class TechniqueBlockNode : ParseNode
	{
		public List<TechniqueNode> Techniques { get; set; }
	}
}