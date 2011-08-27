using StitchUp.Content.Pipeline.FragmentLinking.Parser;
using System.Globalization;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	internal class FloatToken : LiteralToken
	{
		public float Value { get; private set; }

		public FloatToken(float value, string sourcePath, BufferPosition position)
			: base(LiteralTokenType.Float, sourcePath, position)
		{
			Value = value;
		}

		public override string ToString()
		{
			return Value.ToString(new CultureInfo("en-US"));
		}
	}
}