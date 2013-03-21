namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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
}
