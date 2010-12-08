using System.Collections.Generic;

namespace StitchUp.Content.Pipeline.FragmentLinking.EffectModel
{
	internal class TechniqueSymbol
	{
		private List<TechniquePassSymbol> _passes;

		public string Name { get; set; }

		public List<TechniquePassSymbol> Passes
		{
			get { return _passes; }
			set
			{
				_passes = value;
				_passes.ForEach(p => p.Technique = this);
			}
		}
	}
}