namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	internal class ShaderCodeBlockNode : CodeBlockNodeBase
	{
		public ShaderType ShaderType { get; set; }
		public ShaderProfile? ShaderProfile { get; set; }		
	}
}