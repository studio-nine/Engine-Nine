namespace Nine.Content.Pipeline.Graphics.Materials
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Materials.MaterialParts;

    //--------------------------------------------------------------------------
    //
    //
    //      WELCOME TO THE WORLD OF HOLLY SHIT! GOOD LUCK.
    //
    //
    //                                                    MR. YUFEI    O(∩_∩)O
    //
    //--------------------------------------------------------------------------

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
        public bool ContainsClip;
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
        public MaterialPart MaterialPart;
        public VariableDeclaration[] Variables;
        public FunctionDeclaration[] Functions;
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
            if (Functions != null)
            {
                foreach (var function in Functions)
                {
                    var str = function.Body;
                    foreach (var replace in Variables)
                        str = Lexer.ReplaceMatchWord(str, replace.Name, string.Concat(replace.Name, surffix), false, false);
                    foreach (var replace in Functions)
                        str = Lexer.ReplaceMatchWord(str, replace.Name, string.Concat(replace.Name, surffix), false, false);
                    sb.Append(str);
                }
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
            // TODO: Canot simply judge based on this....
            declaration.ContainsClip = declaration.Body.Contains("clip(");
            return declaration;
        }

        public MaterialPartDeclaration Read()
        {
            var declaration = new MaterialPartDeclaration();
            declaration.Body = Code;
            
            VariableDeclaration variable;
            List<VariableDeclaration> variables = new List<VariableDeclaration>();

            FunctionDeclaration function;
            List<FunctionDeclaration> functions = new List<FunctionDeclaration>();

            while (true)
            {
                if ((function = ReadFunctionDeclaration()) != null)
                    functions.Add(function);
                else if ((variable = ReadVariableDeclaration()) != null)
                    variables.Add(variable);
                else
                    break;                    
            }

            declaration.Variables = variables.ToArray();
            declaration.PixelShader = functions.SingleOrDefault(f => f.Name == "PixelShader");
            declaration.VertexShader = functions.SingleOrDefault(f => f.Name == "VertexShader");

            if (declaration.PixelShader == null && declaration.VertexShader == null)
                throw new InvalidOperationException("Cannot find function PixelShader or VertexShader");

            if ((declaration.PixelShader != null && declaration.PixelShader.ReturnType != "void") ||
                (declaration.PixelShader != null && declaration.PixelShader.ReturnType != "void"))
                throw new NotSupportedException("Function does not support return types. Use out instead");

            declaration.Functions = functions.ToArray();
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

    class MaterialGroupBuilderContext
    {
        public List<MaterialPartDeclaration> MaterialPartDeclarations;
        public Dictionary<string, ArgumentDeclaration> ArgumentDictionary;
        public List<ArgumentDeclaration> VertexShaderInputs;
        public List<ArgumentDeclaration> VertexShaderOutputs;
        public List<ArgumentDeclaration> PixelShaderInputs;
        public List<ArgumentDeclaration> PixelShaderOutputs;
        public List<ArgumentDeclaration> TemporaryPixelShaderVariables;
        public Dictionary<ArgumentDeclaration, string> VertexShaderOutputSemanticMapping;
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
            if (materialGroup.MaterialParts.Count <= 0)
                materialGroup.MaterialParts.Add(new DiffuseMaterialPart() { DiffuseColorEnabled = false, TextureEnabled = false });

            var dependentPartTypes = new List<Type>();
            for (int p = 0; p < materialGroup.MaterialParts.Count; p++)
            {
                var currentPart = materialGroup.MaterialParts[p];
                currentPart.GetDependentParts(usage, dependentPartTypes);
                for (int i = 0; i < dependentPartTypes.Count; ++i)
                {
                    var type = dependentPartTypes[i];
                    if (!materialGroup.MaterialParts.Any(x => x.GetType() == type))
                    {
                        var part = (MaterialPart)Activator.CreateInstance(type);
                        part.GetDependentParts(usage, dependentPartTypes);
                        materialGroup.MaterialParts.Insert(++p, part);
                    }
                    else
                    {
                        var parts = materialGroup.MaterialParts.Where(x => x.GetType() == type && x != currentPart).ToArray();
                        foreach (var pt in parts)
                            materialGroup.MaterialParts.Remove(pt);
                        p = materialGroup.MaterialParts.IndexOf(currentPart);
                        foreach (var pt in parts)
                            materialGroup.MaterialParts.Insert(++p, pt);
                    }
                }
                dependentPartTypes.Clear();
            }

            if (!materialGroup.MaterialParts.OfType<VertexTransformMaterialPart>().Any())
                materialGroup.MaterialParts.Add(new VertexTransformMaterialPart());

            var builderContext = CreateMaterialGroupBuilderContext(materialGroup.MaterialParts, usage, true);
            if (builderContext.PixelShaderOutputs.Count <= 0)
                return null;

            context.Logger.LogImportantMessage("Building material group {0}", usage);

            try
            {
                // Force 3_0 when using instancing
                if (materialGroup.MaterialParts.OfType<InstancedMaterialPart>().Any())
                    throw new InvalidOperationException();

                LastEffectCode = GetShaderCodeForProfile(builderContext, "2_0");
                return BuildEffect(context);
            }
            catch
            {
                LastEffectCode = GetShaderCodeForProfile(builderContext, "3_0");
                return BuildEffect(context);
            }
        }

        internal static CompiledEffectContent BuildEffect(ContentProcessorContext context)
        {
            if (!Directory.Exists(Path.Combine(context.IntermediateDirectory, WorkingPath)))
                Directory.CreateDirectory(Path.Combine(context.IntermediateDirectory, WorkingPath));

            File.WriteAllText(Path.Combine(context.IntermediateDirectory, WorkingPath, "LastEffect.fx"), LastEffectCode);

            var effectContent = new EffectContent { EffectCode = LastEffectCode };
            var effectProcessor = new EffectProcessor();
            var result = effectProcessor.Process(effectContent, context);

            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(LastEffectCode));
            var hashString = new StringBuilder();
            for (int i = 0; i < hash.Length; ++i)
                hashString.Append(hash[i].ToString("X2"));
            LastIdentity = hashString.ToString();

            try
            {
                string resultEffectFile = Path.Combine(context.IntermediateDirectory, WorkingPath, LastIdentity + ".fx");
                string resultAsmFile = Path.Combine(context.IntermediateDirectory, WorkingPath, LastIdentity + ".asm");
                File.WriteAllText(resultEffectFile, MaterialGroupBuilder.LastEffectCode);
                context.Logger.LogImportantMessage(string.Format("{0} : (double-click this message to view HLSL file).", resultEffectFile));

                Disassemble(resultEffectFile, resultAsmFile, context);
            }
            catch
            {

            }
            return result;
        }

        internal static MaterialGroupBuilderContext CreateMaterialGroupBuilderContext(IList<MaterialPart> materialParts, MaterialUsage usage, bool simplify)
        {
            var builderContext = new MaterialGroupBuilderContext();

            // Step 1: Parse material part declarations from input material group.
            builderContext.MaterialPartDeclarations = new List<MaterialPartDeclaration>();
            foreach (var part in materialParts)
            {
                var code = part.GetShaderCode(usage);
                if (!string.IsNullOrEmpty(code))
                {
                    var newPart = new Lexer(code).Read();
                    newPart.MaterialPart = part;
                    builderContext.MaterialPartDeclarations.Add(newPart);
                }
            }

            builderContext.ArgumentDictionary = new Dictionary<string, ArgumentDeclaration>();

            foreach (var arg in (from part in builderContext.MaterialPartDeclarations select part.VertexShader).Concat(
                                 from part in builderContext.MaterialPartDeclarations select part.PixelShader).OfType<FunctionDeclaration>()
                                .SelectMany(f => f.Arguments))
            {
                ArgumentDeclaration argument;
                if (builderContext.ArgumentDictionary.TryGetValue(arg.Name, out argument))
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
                    builderContext.ArgumentDictionary.Add(arg.Name, arg);
                }
            }

            foreach (var arg in builderContext.ArgumentDictionary.Values)
                arg.DefaultValue = arg.DefaultValue ?? "0";


            // Step 2: Figure out the dependencies of material parts based on function input arguments
            Regex validPixelShaderOutputSemantic;
            Regex validVertexShaderOutputSemantic;

            validPixelShaderOutputSemantic = new Regex("^COLOR[0-9]$");
            validVertexShaderOutputSemantic = new Regex("^(COLOR[0-9]+)|(POSITION[0-9]+)|(TEXCOORD[0-9]+)$");

            var psOutputSemantics = new List<string>();
            var vsOutputSemantics = new List<string>();
            for (int i = builderContext.MaterialPartDeclarations.Count - 1; i >= 0; i--)
            {
                var part = builderContext.MaterialPartDeclarations[i];
                part.Index = i;
                part.MaterialPart.ParameterSuffix = string.Concat("_", i);

                if (part.VertexShader != null)
                {
                    var vsOutputArgs = part.VertexShader.Arguments.Where(a => a != null && a.Semantic != null && a.Out && validVertexShaderOutputSemantic.IsMatch(a.Semantic)).Select(a => a.Semantic).ToArray();
                    if (vsOutputArgs.Length > 0 && !vsOutputSemantics.Intersect(vsOutputArgs).Any())
                    {
                        vsOutputSemantics.AddRange(vsOutputArgs);
                        part.IsVertexShaderOutput = true;
                    }
                }

                if (part.PixelShader != null)
                {
                    var psOutputArgs = part.PixelShader.Arguments.Where(a => a != null && a.Semantic != null && a.Out && validPixelShaderOutputSemantic.IsMatch(a.Semantic)).Select(a => a.Semantic).ToArray();
                    if (psOutputArgs.Length > 0 && !psOutputSemantics.Intersect(psOutputArgs).Any())
                    {
                        psOutputSemantics.AddRange(psOutputArgs);
                        part.IsPixelShaderOutput = true;
                    }
                }
            }            

            foreach (var materialDeclaration in builderContext.MaterialPartDeclarations)
            {
                materialDeclaration.Dependencies =
                    (from part in builderContext.MaterialPartDeclarations
                     where part != materialDeclaration && part.VertexShader != null &&
                           part.VertexShader.Arguments.Any(arg => arg.Out && (!arg.In || part.Index < materialDeclaration.Index) &&
                               materialDeclaration.VertexShader != null && materialDeclaration.VertexShader.Arguments.Any(a => a.In && a.Name == arg.Name))
                     select part).Concat
                    (from part in builderContext.MaterialPartDeclarations
                     where part != materialDeclaration && part.PixelShader != null &&
                           part.PixelShader.Arguments.Any(arg => arg.Out && (!arg.In || part.Index < materialDeclaration.Index) &&
                               materialDeclaration.PixelShader != null && materialDeclaration.PixelShader.Arguments.Any(a => a.In && a.Name == arg.Name))
                     select part).Concat
                    (from part in builderContext.MaterialPartDeclarations
                     where part != materialDeclaration && part.VertexShader != null &&
                           part.VertexShader.Arguments.Any(arg => arg.Out &&
                               materialDeclaration.PixelShader != null && materialDeclaration.PixelShader.Arguments.Any(a => a.In && a.Name == arg.Name))
                     select part).ToArray();
            }

            // Step 3: Dependency sorting
            int[] order = new int[builderContext.MaterialPartDeclarations.Count];
            DependencyGraph.Sort(builderContext.MaterialPartDeclarations, order, new MaterialPartDeclarationDependencyProvider());
            builderContext.MaterialPartDeclarations = order.Select(i => builderContext.MaterialPartDeclarations[i]).ToList();

            // Remove pixel shader parts that don't have a path to the pixel shader output 
            foreach (var part in Enumerable.Reverse(builderContext.MaterialPartDeclarations))
                if (!simplify || part.IsPixelShaderOutput || part.Tagged || (part.PixelShader != null && part.PixelShader.ContainsClip))
                {
                    part.Tagged = true;
                    foreach (var d in part.Dependencies)
                        d.Tagged = true;
                }

            builderContext.MaterialPartDeclarations = (from part in builderContext.MaterialPartDeclarations where part.Tagged || part.PixelShader == null select part).ToList();

            // Step 4: Get shader input/output argument semantics
            var argumentEqualtyComparer = new ArgumentDeclarationEqualyComparer();

            builderContext.VertexShaderInputs = new List<ArgumentDeclaration>();
            builderContext.VertexShaderOutputs = new List<ArgumentDeclaration>();

            for (int i = 0; i < builderContext.MaterialPartDeclarations.Count; ++i)
            {
                if (builderContext.MaterialPartDeclarations[i].VertexShader != null)
                {
                    builderContext.VertexShaderInputs.AddRange(from arg in builderContext.MaterialPartDeclarations[i].VertexShader.Arguments
                                                               where arg.In && !builderContext.MaterialPartDeclarations.Take(i).Select(p => p.VertexShader)
                                                                .OfType<FunctionDeclaration>().SelectMany(f => f.Arguments)
                                                                .Any(a => a.Out && a.Name == arg.Name)
                                                select arg);

                    builderContext.VertexShaderOutputs.AddRange(from arg in builderContext.MaterialPartDeclarations[i].VertexShader.Arguments
                                                                where arg.Out && !builderContext.MaterialPartDeclarations.Skip(i + 1).Select(p => p.VertexShader)
                                                                 .OfType<FunctionDeclaration>().SelectMany(f => f.Arguments)
                                                                 .Any(a => a.In && a.Name == arg.Name)
                                                 select arg);
                }
            }

            builderContext.PixelShaderInputs = new List<ArgumentDeclaration>();
            builderContext.PixelShaderOutputs = new List<ArgumentDeclaration>();

            for (int i = 0; i < builderContext.MaterialPartDeclarations.Count; ++i)
            {
                if (builderContext.MaterialPartDeclarations[i].PixelShader != null)
                {
                    builderContext.PixelShaderInputs.AddRange(from arg in builderContext.MaterialPartDeclarations[i].PixelShader.Arguments
                                                              where arg.In && !builderContext.MaterialPartDeclarations.Take(i).Select(p => p.PixelShader)
                                                                .OfType<FunctionDeclaration>().SelectMany(f => f.Arguments)
                                                                .Any(a => a.Out && a.Name == arg.Name)
                                               select arg);

                    builderContext.PixelShaderOutputs.AddRange(from arg in builderContext.MaterialPartDeclarations[i].PixelShader.Arguments
                                                               where arg.Out && !builderContext.MaterialPartDeclarations.Skip(i + 1).Select(p => p.PixelShader)
                                                                 .OfType<FunctionDeclaration>().SelectMany(f => f.Arguments)
                                                                 .Any(a => a.In && a.Name == arg.Name)
                                                select arg);
                }
            }

            builderContext.PixelShaderInputs = builderContext.PixelShaderInputs.Distinct(argumentEqualtyComparer).ToList();
            builderContext.PixelShaderOutputs = builderContext.PixelShaderOutputs.Distinct(argumentEqualtyComparer).ToList();
            builderContext.VertexShaderInputs = builderContext.VertexShaderInputs.Distinct(argumentEqualtyComparer).ToList();
            builderContext.VertexShaderOutputs = builderContext.VertexShaderOutputs.Distinct(argumentEqualtyComparer).ToList();

            builderContext.PixelShaderInputs.ForEach(a => a.DefaultValue = builderContext.ArgumentDictionary[a.Name].DefaultValue);
            builderContext.PixelShaderOutputs.ForEach(a => a.DefaultValue = builderContext.ArgumentDictionary[a.Name].DefaultValue);
            builderContext.VertexShaderInputs.ForEach(a => a.DefaultValue = builderContext.ArgumentDictionary[a.Name].DefaultValue);
            builderContext.VertexShaderOutputs.ForEach(a => a.DefaultValue = builderContext.ArgumentDictionary[a.Name].DefaultValue);
            
            // Step 5: Argument simplification and validation
            builderContext.TemporaryPixelShaderVariables = (from arg in builderContext.PixelShaderOutputs
                                             where simplify && (arg.Semantic == null || !validPixelShaderOutputSemantic.IsMatch(arg.Semantic)) &&
                                             !builderContext.PixelShaderInputs.Any(psi => psi.Name == arg.Name)
                                             select arg).ToList();

            // Remove duplicated pixel shader output semantics, keep only the last one.
            for (int i = 0; i < builderContext.PixelShaderOutputs.Count; ++i)
            {
                if (builderContext.PixelShaderOutputs.Skip(i + 1).Any(a => a.Semantic == builderContext.PixelShaderOutputs[i].Semantic))
                {
                    builderContext.TemporaryPixelShaderVariables.Add(builderContext.PixelShaderOutputs[i]);
                    foreach (var psi in builderContext.PixelShaderInputs.Where(p => p.Name == builderContext.PixelShaderOutputs[i].Name))
                        psi.Out = false;
                    builderContext.PixelShaderOutputs.RemoveAt(i);
                    i--;
                }
            }

            // Pixel shader inputs that does not have a matching vertex shader output and a valid semantic            
            for (int i = 0; i < builderContext.PixelShaderInputs.Count; ++i)
            {
                var psi = builderContext.PixelShaderInputs[i];
                if (builderContext.VertexShaderOutputs.Any(vso => vso.Name == psi.Name))
                    continue;
                if (string.IsNullOrEmpty(psi.Semantic))
                {
                    builderContext.TemporaryPixelShaderVariables.Add(psi);
                    builderContext.PixelShaderInputs.RemoveAt(i--);
                }
                else
                {
                    var arg = new ArgumentDeclaration { Name = psi.Name, Type = psi.Type, Semantic = psi.Semantic, In = true, Out = true };
                    if (!builderContext.VertexShaderInputs.Any(vsi => vsi.Name == psi.Name))
                        builderContext.VertexShaderInputs.Add(arg);
                    else
                        builderContext.VertexShaderInputs.Where(vsi => vsi.Name == psi.Name).ForEach(vsi => vsi.Out = true);
                    builderContext.VertexShaderOutputs.Add(arg);
                }
            }

            // Remove temporary duplicates
            builderContext.TemporaryPixelShaderVariables = builderContext.TemporaryPixelShaderVariables.Distinct(argumentEqualtyComparer).ToList();            

            // Valid vertex shader input semantic
            foreach (var input in builderContext.VertexShaderInputs)
                if (string.IsNullOrEmpty(input.Semantic))
                    throw new InvalidOperationException(string.Concat("Cannot find semantics for vertex shader input ", input.Name));

            // Remove vertex shader outputs that do not have a corresponding pixel shader input            
            builderContext.VertexShaderOutputs.RemoveAll(vso => vso.Semantic != "POSITION0" && !builderContext.PixelShaderInputs.Any(psi => psi.Name == vso.Name));

            // Expand vertex shader outputs that do not have a valid semantic
            NextValidSemanticIndex = 0;
            builderContext.VertexShaderOutputSemanticMapping = builderContext.VertexShaderOutputs.Where(vso => vso.Semantic == null || !validVertexShaderOutputSemantic.IsMatch(vso.Semantic))
                                                       .ToDictionary(arg => arg, arg => NextValidSemantic(builderContext.VertexShaderOutputs));

            // POSITION0 is not a valid pixel shader input semantics            
            if (builderContext.PixelShaderInputs.Any(psi => psi.Semantic == "POSITION0"))
            {
                var vso = builderContext.VertexShaderOutputs.Single(a => a.Semantic == "POSITION0");
                var arg = new ArgumentDeclaration()
                {
                    Out = true,
                    Name = vso.Name + "_pos0_",
                    Type = vso.Type,
                };
                var semantic = NextValidSemantic(builderContext.VertexShaderOutputs);
                builderContext.VertexShaderOutputSemanticMapping.Add(arg, semantic);
                builderContext.PixelShaderInputs.Single(psi => psi.Semantic == "POSITION0").Semantic = semantic;
            }
            
            builderContext.VertexShaderOutputs.RemoveAll(vso => builderContext.VertexShaderOutputSemanticMapping.Any(m => m.Key.Name == vso.Name));
            builderContext.VertexShaderOutputs.RemoveAll(vso => builderContext.VertexShaderInputs.Any(vsi => vsi.Name == vso.Name && vsi.Out));

            if (builderContext.VertexShaderOutputs.Any(vso => vso.Semantic == "POSITION0"))
                builderContext.VertexShaderInputs.Where(vsi => vsi.Semantic == "POSITION0").ForEach(vsi => vsi.Out = false);

            // Fix vertex shader input modifier based on the above mapping
            foreach (var vsi in builderContext.VertexShaderInputs)
            {
                if ((!builderContext.PixelShaderInputs.Any(psi => psi.Name == vsi.Name) && vsi.Semantic != "POSITION0") ||
                    builderContext.VertexShaderOutputSemanticMapping.Any(p => vsi.Name == p.Key.Name))
                    vsi.Out = false;
            }

            // Fix pixel shader input semantics based on the above mapping
            foreach (var psi in builderContext.PixelShaderInputs)
            {
                psi.Out = builderContext.PixelShaderOutputs.Any(pso => pso.Name == psi.Name);
                foreach (var p in builderContext.VertexShaderOutputSemanticMapping)
                    if (psi.Name == p.Key.Name)
                    {
                        psi.Semantic = p.Value;
                        break;
                    }
            }
            builderContext.PixelShaderOutputs.RemoveAll(pso => builderContext.PixelShaderInputs.Any(psi => psi.Name == pso.Name) || builderContext.TemporaryPixelShaderVariables.Any(t => t.Name == pso.Name));
            if (simplify)
                foreach (var psi in builderContext.PixelShaderInputs)
                    psi.Out = false;

            // Merge vertex shader inout parameters
            builderContext.VertexShaderOutputs.RemoveAll(vso => vso.In && builderContext.VertexShaderInputs.Any(vsi =>vsi.Name == vso.Name));
            return builderContext;
        }

        internal static string GetShaderCodeForProfile(MaterialGroupBuilderContext builderContext, string profile)
        {
            var builder = new StringBuilder();
            builder.Append(GetShaderCodeBody(builderContext, "VS", "PS"));

            builder.AppendLine();
            builder.AppendLine
            (
                "technique Default" + Environment.NewLine +
                "{" + Environment.NewLine +
                "   pass Default" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "       VertexShader = compile vs_" + profile + " VS();" + Environment.NewLine +
                "       PixelShader = compile ps_" + profile + " PS();" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "}" + Environment.NewLine
            );

            return builder.ToString();
        }

        internal static string GetShaderCodeBody(MaterialGroupBuilderContext builderContext, string vsName, string psName)
        {
            var builder = new StringBuilder();
            foreach (var materialPart in builderContext.MaterialPartDeclarations)
            {
                builder.AppendLine(materialPart.ToString());
                builder.AppendLine();
            }

            builder.Append(string.Concat("void ", vsName, "("));
            builder.Append(string.Join(", ", builderContext.VertexShaderInputs.Select(arg => arg.ToString())
                                 .Concat(builderContext.VertexShaderOutputs.Select(arg => arg.ToString()))
                                 .Concat(builderContext.VertexShaderOutputSemanticMapping.Select(p =>
                                     string.Concat("out ", p.Key.Type, " o_", p.Key.Name, ":", p.Value)))));
            builder.AppendLine(")");
            builder.AppendLine("{");
            foreach (var vsi in builderContext.VertexShaderInputs)
            {
                if (vsi.In && vsi.Out)
                    builder.AppendLine(string.Concat("    ", vsi.Name, " = ", vsi.Name, ";"));
            }
            builder.AppendLine();
            foreach (var arg in builderContext.ArgumentDictionary)
            {
                if (!builderContext.VertexShaderInputs.Any(vsi => vsi.Name == arg.Key) && !builderContext.VertexShaderOutputs.Any(vso => vso.Name == arg.Key))
                    builder.AppendLine(string.Concat("    ", arg.Value.Type, " ", arg.Value.Name, " = ", arg.Value.DefaultValue, ";"));
            }
            builder.AppendLine();
            foreach (var materialPart in builderContext.MaterialPartDeclarations)
            {
                if (materialPart.VertexShader != null)
                    builder.AppendLine(materialPart.VertexShader.ToInvokeString(string.Concat("_", materialPart.Index)));
            }
            builder.AppendLine();
            foreach (var p in builderContext.VertexShaderOutputSemanticMapping)
            {
                builder.AppendLine(string.Concat("    o_", p.Key.Name, " = ",
                    p.Key.Name.EndsWith("_pos0_") ? p.Key.Name.Replace("_pos0_", "") : p.Key.Name, ";"));
            }
            builder.AppendLine("}");

            builder.AppendLine();

            builder.Append(string.Concat("void ", psName, "("));
            builder.Append(string.Join(", ", builderContext.PixelShaderInputs.Select(arg => arg.ToString())
                                     .Concat(builderContext.PixelShaderOutputs.Select(arg =>
                                         string.IsNullOrEmpty(arg.Semantic) ? string.Concat("out ", arg.Type, " ", arg.Name)
                                                                            : string.Concat("out ", arg.Type, " ", arg.Name, ":", arg.Semantic)))));
            builder.AppendLine(")");
            builder.AppendLine("{");
            foreach (var psi in builderContext.TemporaryPixelShaderVariables)
            {
                if (string.IsNullOrEmpty(psi.DefaultValue))
                    builder.AppendLine(string.Concat("    ", psi.Type, " ", psi.Name, ";"));
                else
                    builder.AppendLine(string.Concat("    ", psi.Type, " ", psi.Name, " = ", psi.DefaultValue, ";"));
            }
            builder.AppendLine();
            foreach (var psi in builderContext.PixelShaderInputs)
            {
                if (psi.In && psi.Out)
                    builder.AppendLine(string.Concat("    ", psi.Name, " = ", psi.Name, ";"));
            }
            builder.AppendLine();
            foreach (var arg in builderContext.ArgumentDictionary)
            {
                if (!builderContext.PixelShaderInputs.Any(psi => psi.Name == arg.Key) &&
                    !builderContext.PixelShaderOutputs.Any(t => t.Name == arg.Key) &&
                    !builderContext.TemporaryPixelShaderVariables.Any(t => t.Name == arg.Key))
                {
                    builder.AppendLine(string.Concat("    ", arg.Value.Type, " ", arg.Value.Name, " = ", arg.Value.DefaultValue, ";"));
                }
            }
            builder.AppendLine();
            foreach (var materialPart in builderContext.MaterialPartDeclarations)
            {
                if (materialPart.PixelShader != null)
                    builder.AppendLine(materialPart.PixelShader.ToInvokeString(string.Concat("_", materialPart.Index)));
            }
            builder.AppendLine("}");
            return builder.ToString();
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
