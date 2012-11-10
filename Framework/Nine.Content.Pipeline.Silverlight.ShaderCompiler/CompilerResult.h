#pragma once

using namespace System;

BEGIN_NAMESPACE
{
	public ref class CompilerResult
	{
	public:
		property array<unsigned char>^ ShaderCode;
		property array<unsigned char>^ ConstantsDefinition;
	};
}
END_NAMESPACE

