using System.Collections.Generic;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	internal class TechniquePassNode : ParseNode
	{
		public string Name { get; set; }
		public List<Token> Fragments { get; set; }
	}
}