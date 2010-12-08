using System.Collections.Generic;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	internal class TechniqueNode : ParseNode
	{
		public string Name { get; set; }
		public List<TechniquePassNode> Passes { get; set; }
	}
}