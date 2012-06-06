#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Text;
using Nine.Graphics.Materials;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.Collections;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Text.RegularExpressions;
using Nine.Graphics.Materials.MaterialParts;
using System.Security.Cryptography;
#endregion

namespace Nine.Content.Pipeline.Graphics.Materials
{
    class VariableDeclaration
    {
        public string Type;
        public string Name;
        public string Body;

        public override string ToString()
        {
            return Body;
        }
    }

    class ArgumentDeclaration
    {
        public bool In;
        public bool Out;
        public string Type;
        public string Name;
        public string Semantic;
        public string DefaultValue;

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Out)
            {
                sb.Append(In ? "inout" : "out");
                sb.Append(" ");
            }
            sb.Append(Type);
            sb.Append(" ");
            sb.Append(Name);
            sb.Append(":");
            sb.Append(Semantic);
            return sb.ToString();
        }
    }

    class ArgumentDeclarationEqualyComparer : IEqualityComparer<ArgumentDeclaration>
    {
        public bool Equals(ArgumentDeclaration x, ArgumentDeclaration y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(ArgumentDeclaration obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    class FunctionDeclaration
    {
        public string Name;
        public string Body;
        public string ReturnType;
        public ArgumentDeclaration[] Arguments;

        public override string ToString()
        {
            return Body;
        }

        public string ToInvokeString(string surffix)
        {
            return string.Concat("    ", Name, surffix, "(", string.Join(", ", Arguments.Select(a => a.Name)), ");");
        }
    }
    
    class MaterialPartDeclaration
    {
        public int Index;
        public string Body;
        public bool Tagged;
        public bool IsVertexShaderOutput;
        public bool IsPixelShaderOutput;
        public VariableDeclaration[] Variables;
        public FunctionDeclaration VertexShader;
        public FunctionDeclaration PixelShader;
        public MaterialPartDeclaration[] Dependencies;

        public override string ToString()
        {
            return ToString(string.Concat("_", Index));
        }

        private string ToString(string surffix)
        {
            StringBuilder sb = new StringBuilder();
            if (Variables != null)
            {
                foreach (var variable in Variables)
                {
                    var str = variable.Body;
                    foreach (var replace in Variables)
                        str = Lexer.ReplaceMatchWord(str, replace.Name, string.Concat(replace.Name, surffix), false, true);
                    sb.Append(str);
                }
            }
            if (VertexShader != null)
            {
                var str = VertexShader.Body;
                foreach (var replace in Variables)
                    str = Lexer.ReplaceMatchWord(str, replace.Name, string.Concat(replace.Name, surffix), false, false);
                str = Lexer.ReplaceMatchWord(str, "VertexShader", string.Concat("VertexShader", surffix), true, false);
                sb.Append(str);
            }
            if (PixelShader != null)
            {
                var str = PixelShader.Body;
                foreach (var replace in Variables)
                    str = Lexer.ReplaceMatchWord(str, replace.Name, string.Concat(replace.Name, surffix), false, false);
                str = Lexer.ReplaceMatchWord(str, "PixelShader", string.Concat("PixelShader", surffix), true, false);
                sb.Append(str);
            }
            return sb.ToString();
        }
    }

    class MaterialPartDeclarationDependencyProvider : IDependencyProvider<MaterialPartDeclaration>
    {
        public int GetDependencies(IList<MaterialPartDeclaration> elements, int index, int[] dependencies)
        {
            int i = 0;
            foreach (var part in elements[index].Dependencies)
                dependencies[i++] = ((IList)elements).IndexOf(part);
            return i;
        }
    }

    class Lexer
    {
        private string Code;
        private int Position;

        public Lexer(string code)
        {
            Code = code;
        }

        public bool Eof()
        {
            return Position >= Code.Length;
        }

        private void EatLine()
        {
            while (!Eof() && !(Code[Position] == '\r' || Code[Position] == '\n'))
                Position++;
        }

        private void EatComment()
        {
            if (Code[Position] == '/' && Position + 1 < Code.Length && Code[Position + 1] == '/')
                EatLine();
        }

        public bool ReadSeperator()
        {
            bool result = false;
            while (!Eof())
            {
                EatComment();

                var current = Code[Position];
                if (current == ' ' || current == '\t' || current == '\r' || current == '\n')
                {
                    result = true;
                    Position++;
                }
                else break;
            }
            return result;
        }

        public bool ReadSymbol(char symbol)
        {
            ReadSeperator();
            if (Code[Position] == symbol)
            {
                Position++;
                return true;
            }
            return false;
        }

        private static bool IsValidCharactor(char current)
        {
            return (current >= 'a' && current <= 'z') ||
                   (current >= 'A' && current <= 'Z') ||
                   (current >= '0' && current <= '9') || current == '_';
        }

        public string ReadIdentifier()
        {
            ReadSeperator();

            int start = Position;
            int end = Position;

            while (!Eof())
            {
                if (IsValidCharactor(Code[Position]))
                    end = ++Position;
                else break;
            }

            return end > start ? Code.Substring(start, end - start) : null;
        }

        public string ReadBlock(int startPosition)
        {
            Position = startPosition;
            int start = Position;
            int end = Position;

            Stack<int> brackets = new Stack<int>();
            while (!Eof())
            {
                EatComment();

                var current = Code[Position];
                if (current == '{')
                    brackets.Push(Position);
                if (current == '}')
                    brackets.Pop();
                if (current == ';' && brackets.Count <= 0)
                {
                    end = ++Position;
                    break;
                }
                Position++;
            }
            return end > start ? Code.Substring(start, end - start) : null;
        }

        private string ReadBracketBlock(int startPosition)
        {
            Position = startPosition;
            int start = Position;
            int end = Position;

            Stack<int> brackets = new Stack<int>();
            while (!Eof())
            {
                EatComment();

                var current = Code[Position];
                if (current == '{')
                    brackets.Push(Position);
                if (current == '}')
                {
                    brackets.Pop();
                    if (brackets.Count <= 0)
                    {
                        end = ++Position;
                        break;
                    }
                }
                Position++;
            }
            return end > start ? Code.Substring(start, end - start) : null;
        }

        public VariableDeclaration ReadVariableDeclaration()
        {
            var initial = Position;
            var declaration = new VariableDeclaration();
            if ((declaration.Type = ReadIdentifier()) == null ||
                !ReadSeperator() ||
                (declaration.Name = ReadIdentifier()) == null ||                
                (declaration.Body = ReadBlock(initial)) == null)
            {
                Position = initial;
                return null;
            }
            return declaration;
        }

        private ArgumentDeclaration ReadArgumentDeclaration()
        {
            var declaration = new ArgumentDeclaration();
            var modifier = ReadIdentifier();
            
            if (modifier == "in" || modifier == "inout")
                declaration.In = true;
            if (modifier == "out" || modifier == "inout")
                declaration.Out = true;

            if (declaration.In || declaration.Out)
            {
                if (!ReadSeperator())
                    return null;
                declaration.Type = ReadIdentifier();
            }
            else
            {
                declaration.In = true;
                declaration.Type = modifier;
            }

            if (declaration.Type == null)
                return null;
            if (!ReadSeperator())
                return null;
            if ((declaration.Name = ReadIdentifier()) == null)
                return null;
            if (ReadSymbol(':') && (declaration.Semantic = ReadIdentifier()) == null)
                return null;
            if (ReadSymbol('=') && (declaration.DefaultValue = ReadIdentifier()) == null)
                return null;
            ReadSymbol(',');
            return declaration;
        }

        private ArgumentDeclaration[] ReadArguments()
        {
            ArgumentDeclaration argument;
            List<ArgumentDeclaration> arguments = new List<ArgumentDeclaration>();
            while ((argument = ReadArgumentDeclaration()) != null)
                arguments.Add(argument);
            return arguments.ToArray();
        }

        public FunctionDeclaration ReadFunctionDeclaration()
        {
            var initial = Position;
            var declaration = new FunctionDeclaration();
            if ((declaration.ReturnType = ReadIdentifier()) == null ||
                !ReadSeperator() ||
                (declaration.Name = ReadIdentifier()) == null ||
                !ReadSymbol('(') ||
                (declaration.Arguments = ReadArguments()) == null ||  
                !ReadSymbol(')') ||
                (declaration.Body = ReadBracketBlock(initial)) == null)
            {
                Position = initial;
                return null;
            }
            return declaration;
        }

        public MaterialPartDeclaration Read()
        {
            var declaration = new MaterialPartDeclaration();
            declaration.Body = Code;
            
            VariableDeclaration variable;
            List<VariableDeclaration> variables = new List<VariableDeclaration>();
            while ((variable = ReadVariableDeclaration()) != null)
                variables.Add(variable);
            declaration.Variables = variables.ToArray();

            FunctionDeclaration function;
            List<FunctionDeclaration> functions = new List<FunctionDeclaration>();
            while ((function = ReadFunctionDeclaration()) != null)
                functions.Add(function);

            declaration.Variables = variables.ToArray();
            declaration.PixelShader = functions.SingleOrDefault(f => f.Name == "PixelShader");
            declaration.VertexShader = functions.SingleOrDefault(f => f.Name == "VertexShader");

            if (declaration.PixelShader == null && declaration.VertexShader == null)
                throw new InvalidOperationException("Cannot find function PixelShader or VertexShader");

            if ((declaration.PixelShader != null && declaration.PixelShader.ReturnType != "void") ||
                (declaration.PixelShader != null && declaration.PixelShader.ReturnType != "void"))
                throw new NotSupportedException("Function does not support return types. Use out instead");

            return declaration;
        }

        public static string ReplaceMatchWord(string input, string oldString, string newString, bool firstMatchOnly, bool lastMatchOnly)
        {
            var startIndex = 0;
            var oldLength = oldString.Length;
            var replaces = new List<int>();
            while ((startIndex = input.IndexOf(oldString, startIndex)) >= 0)
            {
                if ((startIndex + oldLength >= input.Length || !IsValidCharactor(input[startIndex + oldLength])) &&
                    (startIndex <= 0 || !IsValidCharactor(input[startIndex - 1])))
                {
                    replaces.Add(startIndex);
                }
                startIndex += oldLength;
            }

            if (lastMatchOnly)
            {
                if (replaces.Count > 0)
                {
                    var index = replaces[replaces.Count - 1];
                    input = input.Remove(index, oldLength).Insert(index, newString);
                }
            }
            else if (firstMatchOnly)
            {
                if (replaces.Count > 0)
                {
                    var index = replaces[0];
                    input = input.Remove(index, oldLength).Insert(index, newString);
                }
            }
            else
            {
                var accumulate = 0;
                foreach (var index in replaces)
                {
                    input = input.Remove(index + accumulate, oldLength).Insert(index + accumulate, newString);
                    accumulate += newString.Length - oldString.Length;
                }
            }
            return input;
        }
    }

    static class MaterialGroupBuilder
    {
        const string WorkingPath = "MaterialGroups";

        public static string LastIdentity;
        public static string LastEffectCode;
        private static int NextValidSemanticIndex = 0;
        
        public static CompiledEffectContent Build(MaterialGroup materialGroup, MaterialUsage usage, ContentProcessorContext context)
        {
            // Make sure we have the nesessary building blocks of a shader
            if (!materialGroup.MaterialParts.OfType<VertexTransformMaterialPart>().Any())
                materialGroup.MaterialParts.Add(new VertexTransformMaterialPart());
            if (!materialGroup.MaterialParts.OfType<BeginLightMaterialPart>().Any())
                materialGroup.MaterialParts.Add(new BeginLightMaterialPart());
            if (!materialGroup.MaterialParts.OfType<EndLightMaterialPart>().Any())
                materialGroup.MaterialParts.Add(new EndLightMaterialPart());

            // Step 1: Parse material part declarations from input material group.
            var materialPartDeclarations = (from code in materialGroup.MaterialParts.Select(part => part.GetShaderCode(usage))
                                            where !string.IsNullOrEmpty(code)
                                            select new Lexer(code).Read()).ToArray();

            var argumentDictionary = new Dictionary<string, ArgumentDeclaration>();
            
            foreach (var arg in (from part in materialPartDeclarations select part.VertexShader).Concat(
                                 from part in materialPartDeclarations select part.PixelShader).OfType<FunctionDeclaration>()
                                .SelectMany(f => f.Arguments))
            {
                ArgumentDeclaration argument;
                if (argumentDictionary.TryGetValue(arg.Name, out argument))
                {
                    if (argument.Type != arg.Type)
                        throw new InvalidOperationException(string.Format("Paramter {0} has two different types {1}, {2}", arg.Name, argument, arg.Type));
                    
                    if (!string.IsNullOrEmpty(argument.Semantic))
                    {
                        if (!string.IsNullOrEmpty(arg.Semantic) && arg.Semantic != argument.Semantic)
                            throw new InvalidOperationException(string.Format("Paramter {0} has two different sementics {1}, {2}", arg.Name, argument, arg.Semantic));
                        arg.Semantic = argument.Semantic;
                    }
                    else if (!string.IsNullOrEmpty(arg.Semantic))
                        argument.Semantic = arg.Semantic;

                    if (!string.IsNullOrEmpty(argument.DefaultValue))
                    {
                        if (!string.IsNullOrEmpty(arg.DefaultValue) && arg.DefaultValue != argument.DefaultValue)
                            throw new InvalidOperationException(string.Format("Paramter {0} has two different default value {1}, {2}", arg.Name, argument, arg.DefaultValue));
                        arg.DefaultValue = argument.DefaultValue;
                    }
                    else if (!string.IsNullOrEmpty(arg.DefaultValue))
                        argument.DefaultValue = arg.DefaultValue;
                }
                else
                {
                    argumentDictionary.Add(arg.Name, arg);
                }
            }

            foreach (var arg in argumentDictionary.Values)
                arg.DefaultValue = arg.DefaultValue ?? "0";


            // Step 2: Figure out the dependencies of material parts based on function input arguments
            var validPixelShaderOutputSemantic = new Regex("^COLOR[0-9]$");
            var validVertexShaderOutputSemantic = new Regex("^(COLOR[0-9]+)|(POSITION[0-9]+)|(TEXCOORD[0-9]+)$");

            for (int i = 0; i < materialPartDeclarations.Length; i++)
            {
                var part = materialPartDeclarations[i];
                part.Index = materialGroup.MaterialParts[i].Index = i;
                part.IsVertexShaderOutput = part.VertexShader != null && part.VertexShader.Arguments.Any(a => a != null && a.Semantic != null && a.Out && validVertexShaderOutputSemantic.IsMatch(a.Semantic));
                part.IsPixelShaderOutput = part.PixelShader != null && part.PixelShader.Arguments.Any(a => a != null && a.Semantic != null && a.Out && validPixelShaderOutputSemantic.IsMatch(a.Semantic));
            }

            foreach (var materialDeclaration in materialPartDeclarations)
            {
                materialDeclaration.Dependencies =
                    (from part in materialPartDeclarations
                     where part != materialDeclaration && part.VertexShader != null &&
                           part.VertexShader.Arguments.Any(arg => arg.Out && (!arg.In || part.Index < materialDeclaration.Index) &&
                               materialDeclaration.VertexShader != null && materialDeclaration.VertexShader.Arguments.Any(a => a.In && a.Name == arg.Name))
                     select part).Concat
                    (from part in materialPartDeclarations
                     where part != materialDeclaration && part.PixelShader != null &&
                           part.PixelShader.Arguments.Any(arg => arg.Out && (!arg.In || part.Index < materialDeclaration.Index) &&
                               materialDeclaration.PixelShader != null && materialDeclaration.PixelShader.Arguments.Any(a => a.In && a.Name == arg.Name))
                     select part).Concat
                    (from part in materialPartDeclarations
                     where part != materialDeclaration && part.VertexShader != null &&
                           part.VertexShader.Arguments.Any(arg => arg.Out && (!arg.In || part.Index < materialDeclaration.Index) &&
                               materialDeclaration.PixelShader != null && materialDeclaration.PixelShader.Arguments.Any(a => a.In && a.Name == arg.Name))
                     select part).ToArray();
            }

            // Step 3: Dependency sorting
            int[] order = new int[materialPartDeclarations.Length];
            DependencyGraph.Sort(materialPartDeclarations, order, new MaterialPartDeclarationDependencyProvider());
            materialPartDeclarations = order.Select(i => materialPartDeclarations[i]).ToArray();

            // Remove pixel shader parts that don't have a path to the pixel shader output         
            foreach (var part in materialPartDeclarations.Reverse())
                if (part.IsPixelShaderOutput || part.Tagged)
                {
                    part.Tagged = true;
                    foreach (var d in part.Dependencies)
                        d.Tagged = true;
                }

            var offset = 0;
            for (int i = 0; i < materialPartDeclarations.Length; i++)
                if (!materialPartDeclarations[i].Tagged && materialPartDeclarations[i].PixelShader != null)
                    materialGroup.MaterialParts.RemoveAt(i + offset--);

            materialPartDeclarations = (from part in materialPartDeclarations where part.Tagged || part.PixelShader == null select part).ToArray();            

            // Step 4: Get shader input/output argument semantics
            var argumentEqualtyComparer = new ArgumentDeclarationEqualyComparer();

            var vertexShaderInputs = new List<ArgumentDeclaration>();
            var vertexShaderOutputs = new List<ArgumentDeclaration>();

            for (int i = 0; i < materialPartDeclarations.Length; i++)
            {
                if (materialPartDeclarations[i].VertexShader != null)
                {
                    vertexShaderInputs.AddRange(from arg in materialPartDeclarations[i].VertexShader.Arguments
                                                 where arg.In && !materialPartDeclarations.Take(i).Select(p => p.VertexShader)
                                                                 .OfType<FunctionDeclaration>().SelectMany(f => f.Arguments)
                                                                 .Any(a => a.Out && a.Name == arg.Name)
                                                 select arg);

                    vertexShaderOutputs.AddRange(from arg in materialPartDeclarations[i].VertexShader.Arguments
                                                where arg.Out && !materialPartDeclarations.Skip(i + 1).Select(p => p.VertexShader)
                                                                .OfType<FunctionDeclaration>().SelectMany(f => f.Arguments)
                                                                .Any(a => a.In && a.Name == arg.Name)
                                                select arg);
                }
            }

            var pixelShaderInputs = new List<ArgumentDeclaration>();
            var pixelShaderOutputs = new List<ArgumentDeclaration>();

            for (int i = 0; i < materialPartDeclarations.Length; i++)
            {
                if (materialPartDeclarations[i].PixelShader != null)
                {
                    pixelShaderInputs.AddRange(from arg in materialPartDeclarations[i].PixelShader.Arguments
                                               where arg.In && !materialPartDeclarations.Take(i).Select(p => p.PixelShader)
                                                                .OfType<FunctionDeclaration>().SelectMany(f => f.Arguments)
                                                                .Any(a => a.Out && a.Name == arg.Name)
                                               select arg);

                    pixelShaderOutputs.AddRange(from arg in materialPartDeclarations[i].PixelShader.Arguments
                                                where arg.Out && !materialPartDeclarations.Skip(i + 1).Select(p => p.PixelShader)
                                                                 .OfType<FunctionDeclaration>().SelectMany(f => f.Arguments)
                                                                 .Any(a => a.In && a.Name == arg.Name)
                                                select arg);
                }
            }

            pixelShaderInputs = pixelShaderInputs.Distinct(argumentEqualtyComparer).ToList();
            pixelShaderOutputs = pixelShaderOutputs.Distinct(argumentEqualtyComparer).ToList();
            vertexShaderInputs = vertexShaderInputs.Distinct(argumentEqualtyComparer).ToList();
            vertexShaderOutputs = vertexShaderOutputs.Distinct(argumentEqualtyComparer).ToList();

            // Step 5: Argument simplification and validation
            var temporaryPixelShaderVariables = (from arg in pixelShaderOutputs 
                                                 where arg.Semantic == null || !validPixelShaderOutputSemantic.IsMatch(arg.Semantic) 
                                                 select arg).ToList();
            
            // Pixel shader inputs that does not have a matching vertes shader output and a valid semantic
            for (int i = 0; i < pixelShaderInputs.Count; i++)
            {
                var psi = pixelShaderInputs[i];			 
                if (vertexShaderOutputs.Any(vso => vso.Name == psi.Name))
                    continue;
                if (string.IsNullOrEmpty(psi.Semantic))
                {
                    temporaryPixelShaderVariables.Add(psi);
                    pixelShaderInputs.RemoveAt(i--);
                }
                else
                {
                    var arg = new ArgumentDeclaration { Name = psi.Name, Type = psi.Type, Semantic = psi.Semantic, In = true, Out = true };
                    vertexShaderInputs.Add(arg);
                    vertexShaderOutputs.Add(arg);
                }
            }

            // Valid vertex shader input semantic
            foreach (var input in vertexShaderInputs)
                if (string.IsNullOrEmpty(input.Semantic))
                    throw new InvalidOperationException(string.Concat("Cannot find semantics for vertex shader input ", input.Name));

            // Remove vertex shader outputs that do not have a corresponding pixel shader input
            vertexShaderOutputs.RemoveAll(vso => vso.Semantic != "POSITION0" && !pixelShaderInputs.Any(psi => psi.Name == vso.Name));

            // Expand vertex shader outputs that do not have a valid semantic
            NextValidSemanticIndex = 0;
            var vertexShaderOutputSemanticMapping = vertexShaderOutputs.Where(vso => vso.Semantic == null || !validVertexShaderOutputSemantic.IsMatch(vso.Semantic))
                                                                       .ToDictionary(arg => arg, arg => NextValidSemantic(vertexShaderOutputs));
            
            vertexShaderOutputs.RemoveAll(vso => vertexShaderInputs.Any(vsi => vsi.Name == vso.Name && vsi.Out));
            vertexShaderOutputs.RemoveAll(vso => vertexShaderOutputSemanticMapping.Any(m => m.Key.Name == vso.Name));

            if (vertexShaderOutputs.Any(vso => vso.Semantic == "POSITION0"))
                vertexShaderInputs.Where(vsi => vsi.Semantic == "POSITION0").ForEach(vsi => vsi.Out = false);

            // Fix vertex shader input modifier based on the above mapping
            foreach (var vsi in vertexShaderInputs)
            {
                if ((!pixelShaderInputs.Any(psi => psi.Name == vsi.Name) && vsi.Semantic != "POSITION0") ||
                    vertexShaderOutputSemanticMapping.Any(p => vsi.Name == p.Key.Name))
                    vsi.Out = false;
            }

            // Fix pixel shader input semantics based on the above mapping
            foreach (var psi in pixelShaderInputs)
            {
                psi.Out = pixelShaderOutputs.Any(pso => pso.Name == psi.Name);
                foreach (var p in vertexShaderOutputSemanticMapping)
                    if (psi.Name == p.Key.Name)
                    {
                        psi.Semantic = p.Value;
                        break;
                    }
            }
            pixelShaderOutputs.RemoveAll(pso => pixelShaderInputs.Any(psi => psi.Name == pso.Name) || temporaryPixelShaderVariables.Any(t => t.Name == pso.Name));

            if (pixelShaderOutputs.Count <= 0)
                return null;

            // Step 5: Build effect code
            var builder = new StringBuilder();
            foreach (var materialPart in materialPartDeclarations)
            {
                builder.AppendLine(materialPart.ToString());
                builder.AppendLine();
            }

            builder.Append("void VS(");
            builder.Append(string.Join(", ", vertexShaderInputs.Select(arg => arg.ToString())
                                 .Concat(vertexShaderOutputs.Select(arg => arg.ToString()))
                                 .Concat(vertexShaderOutputSemanticMapping.Select(p => 
                                     string.Concat("out ", p.Key.Type, " o_", p.Key.Name, ":", p.Value)))));
            builder.AppendLine(")");
            builder.AppendLine("{");
            foreach (var vsi in vertexShaderInputs)
            {
                if (vsi.In && vsi.Out)
                    builder.AppendLine(string.Concat("    ", vsi.Name, " = ", vsi.Name, ";"));                
            }
            builder.AppendLine();
            foreach (var arg in argumentDictionary)
            {
                if (!vertexShaderInputs.Any(vsi => vsi.Name == arg.Key) && !vertexShaderOutputs.Any(vso => vso.Name == arg.Key))
                    builder.AppendLine(string.Concat("    ", arg.Value.Type, " ", arg.Value.Name, " = ", arg.Value.DefaultValue, ";"));
            }
            builder.AppendLine();
            foreach (var materialPart in materialPartDeclarations)
            {
                if (materialPart.VertexShader != null)
                    builder.AppendLine(materialPart.VertexShader.ToInvokeString(string.Concat("_", materialPart.Index)));
            }
            builder.AppendLine();
            foreach (var p in vertexShaderOutputSemanticMapping)
            {
                builder.AppendLine(string.Concat("    o_", p.Key.Name, " = ", p.Key.Name, ";"));
            }
            builder.AppendLine("}");

            builder.AppendLine();

            builder.Append("void PS(");
            builder.Append(string.Join(", ", pixelShaderInputs.Select(arg => arg.ToString())
                                     .Concat(pixelShaderOutputs.Select(arg => string.Concat("out ", arg.Type, " ", arg.Name, ":", arg.Semantic)))));
            builder.AppendLine(")");
            builder.AppendLine("{");
            foreach (var psi in temporaryPixelShaderVariables)
            {
                builder.AppendLine(string.Concat("    ", psi.Type, " ", psi.Name, " = ", psi.DefaultValue, ";"));
            }
            builder.AppendLine();
            foreach (var psi in pixelShaderInputs)
            {
                if (psi.In && psi.Out)
                    builder.AppendLine(string.Concat("    ", psi.Name, " = ", psi.Name, ";"));
            }
            builder.AppendLine();
            foreach (var arg in argumentDictionary)
            {
                if (!pixelShaderInputs.Any(psi => psi.Name == arg.Key) &&
                    !pixelShaderOutputs.Any(t => t.Name == arg.Key) &&
                    !temporaryPixelShaderVariables.Any(t => t.Name == arg.Key))
                {
                    builder.AppendLine(string.Concat("    ", arg.Value.Type, " ", arg.Value.Name, " = ", arg.Value.DefaultValue, ";"));
                }
            }
            builder.AppendLine();
            foreach (var materialPart in materialPartDeclarations)
            {
                if (materialPart.PixelShader != null)
                    builder.AppendLine(materialPart.PixelShader.ToInvokeString(string.Concat("_", materialPart.Index)));
            }
            builder.AppendLine("}");

            builder.AppendLine();
            builder.AppendLine
            (
                "technique Default" + Environment.NewLine +
                "{" + Environment.NewLine +
                "   pass Default" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "       VertexShader = compile vs_2_0 VS();" + Environment.NewLine +
                "       PixelShader = compile ps_2_0 PS();" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "}" + Environment.NewLine
            );

            LastEffectCode = builder.ToString();

            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(LastEffectCode));
            var hashString = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                hashString.Append(hash[i].ToString("X2"));
            LastIdentity = hashString.ToString();

            if (!Directory.Exists(Path.Combine(context.IntermediateDirectory, WorkingPath)))
                Directory.CreateDirectory(Path.Combine(context.IntermediateDirectory, WorkingPath));

            string resultEffectFile = Path.Combine(context.IntermediateDirectory, WorkingPath, LastIdentity + ".fx");
            string resultAsmFile = Path.Combine(context.IntermediateDirectory, WorkingPath, LastIdentity + ".asm");
            File.WriteAllText(resultEffectFile, MaterialGroupBuilder.LastEffectCode);
            context.Logger.LogImportantMessage(string.Format("{0} : (double-click this message to view HLSL file).", resultEffectFile));
            
            Disassemble(resultEffectFile, resultAsmFile, context);

            // Step 5: Compile effect
            var effectContent = new EffectContent { EffectCode = LastEffectCode };
            var effectProcessor = new EffectProcessor();
            return effectProcessor.Process(effectContent, context);
        }
        

        private static string NextValidSemantic(List<ArgumentDeclaration> vertexShaderOutputs)
        {
            string result = null;
            do { result = string.Concat("TEXCOORD", NextValidSemanticIndex++); }
            while (vertexShaderOutputs.Any(arg => arg.Semantic == result));
            vertexShaderOutputs.RemoveAll(arg => arg.Semantic == result);
            return result;
        }

        private static void Disassemble(string effectFile, string asmFile, ContentProcessorContext context)
        {
            string dxSdkDir = Environment.GetEnvironmentVariable("DXSDK_DIR");

            if (!string.IsNullOrEmpty(dxSdkDir))
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = Path.Combine(context.IntermediateDirectory, WorkingPath);
                process.StartInfo.FileName = Path.Combine(dxSdkDir, @"Utilities\bin\x86\fxc.exe");
                process.StartInfo.Arguments = "/nologo /Tfx_2_0 /O3 /Fc \"" + asmFile + "\" \"" + effectFile + "\"";
                context.Logger.LogMessage(process.StartInfo.FileName + " " + process.StartInfo.Arguments);

                process.Start();
                process.WaitForExit();
                context.Logger.LogImportantMessage(string.Format("{0} : (double-click this message to view disassembly file).", asmFile));
            }
        }
    }
}
