namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	internal class StitchedEffectNode
	{
		public string Name { get; set; }
		public FragmentBlockNode Fragments { get; set; }
		public TechniqueBlockNode Techniques { get; set; }
		public HeaderCodeBlockNode HeaderCode { get; set; }
	}
}