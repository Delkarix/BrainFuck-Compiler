using System;
using System.Linq;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;

// Possible source? https://docs.microsoft.com/en-us/dotnet/api/system.codedom.compiler.compilerparameters?view=netframework-4.7.2#examples

class Controller
{
    // Hello World: ++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>.,
    public static string[] cmdline_options =
    {
        "--help" // Retrieves help message
    };

    public static string exeFile = "";
    public static string file = "";
    public static string originalFile = "";
    public static int loopStart;
    public static bool inLoop = false;
    public static char[] Memory_Temp = new char[30000];
    public static int Block_Temp = 0;

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Help(null);
            return;
        }
        else
        {
            foreach (string arg in args)
            {
                if (!cmdline_options.Contains(arg))
                {
                    try
                    {
                        exeFile = Path.GetFileNameWithoutExtension(arg) + ".exe";
                        file = Path.GetFileNameWithoutExtension(arg);
                        originalFile = arg;
                        string line = File.ReadAllText(arg);
                        Compile(line);
                    }
                    catch (FileNotFoundException)
                    {
                        ConsoleColor original = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine("ERROR: Could not find the specified file.");
                        Console.ForegroundColor = original;
                        return;
                    }
                }
            }
        }
    }

    public static CodeCompileUnit BuildGraph(string Code)
    {
        CodeCompileUnit compileUnit = new CodeCompileUnit();

        CodeNamespace samples = new CodeNamespace("BrainF_Code");
        compileUnit.Namespaces.Add(samples);

        samples.Imports.Add(new CodeNamespaceImport("System"));

        CodeTypeDeclaration class1 = new CodeTypeDeclaration("BrainF");

        CodeEntryPointMethod start = new CodeEntryPointMethod();
        CodeVariableDeclarationStatement Memory = new CodeVariableDeclarationStatement("Char[]", "Memory", new CodeArrayCreateExpression("Char", Code.Split('>').Length));
        start.Statements.Add(Memory);
        CodeTypeReferenceExpression csSystemConsoleType = new CodeTypeReferenceExpression("System.Console");
        CodeVariableDeclarationStatement Block = new CodeVariableDeclarationStatement("Int32", "Block", new CodePrimitiveExpression(0));
        start.Statements.Add(Block);

        for (int i = 0; i < Code.Length; i++)
        {
            switch (Code.ToCharArray()[i])
            {
                case '>':
                    // Increment Block
                    CodeAssignStatement assignRight = new CodeAssignStatement(new CodeVariableReferenceExpression("Block"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("Block"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1)));
                    start.Statements.Add(assignRight);
                    Block_Temp++;
                    break;
                case '<':
                    // Decrement Block
                    CodeAssignStatement assignLeft = new CodeAssignStatement(new CodeVariableReferenceExpression("Block"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("Block"), CodeBinaryOperatorType.Subtract, new CodePrimitiveExpression(1)));
                    start.Statements.Add(assignLeft);
                    Block_Temp--;
                    break;
                case '+':
                    // Increment Memory
                    CodeAssignStatement assignAdd = new CodeAssignStatement(new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("Memory"), new CodeVariableReferenceExpression("Block")), new CodeCastExpression("Char", new CodeBinaryOperatorExpression(new CodeCastExpression("Int32", new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("Memory"), new CodeVariableReferenceExpression("Block"))), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))));
                    start.Statements.Add(assignAdd);
                    Memory_Temp[Block_Temp]++;
                    break;
                case '-':
                    // Decrement Memory
                    CodeAssignStatement assignSubtract = new CodeAssignStatement(new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("Memory"), new CodeVariableReferenceExpression("Block")), new CodeCastExpression("Char", new CodeBinaryOperatorExpression(new CodeCastExpression("Int32", new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("Memory"), new CodeVariableReferenceExpression("Block"))), CodeBinaryOperatorType.Subtract, new CodePrimitiveExpression(1))));
                    start.Statements.Add(assignSubtract);
                    Memory_Temp[Block_Temp]--;
                    break;
                case '[':
                    loopStart = i;
                    break;
                case ']':
                    // Set the iteration variable to the loopStart
                    if (Memory_Temp[Block_Temp] != 0)
                    {
                        i = loopStart;
                    }
                    break;
                case '.':
                    // Print the current block of memory
                    CodeMethodInvokeExpression print = new CodeMethodInvokeExpression(csSystemConsoleType, "Write", new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("Memory"), new CodeVariableReferenceExpression("Block")));
                    start.Statements.Add(print);
                    break;
                case ',':
                    // Get first letter of input and assign to memory
                    CodeVariableDeclarationStatement variable = new CodeVariableDeclarationStatement("String", "input", new CodePrimitiveExpression("Placeholder"));
                    start.Statements.Add(variable);
                    CodeAssignStatement assignVar = new CodeAssignStatement(new CodeVariableReferenceExpression("input"), new CodeMethodInvokeExpression(csSystemConsoleType, "ReadLine"));
                    start.Statements.Add(assignVar);
                    CodeStatement[] trueStatement = {new CodeAssignStatement(
                        new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("Memory"),
                            new CodeVariableReferenceExpression("Block")), new CodePrimitiveExpression(' '))};
                    CodeStatement[] falseStatement = {new CodeAssignStatement(new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("Memory"),
                        new CodeVariableReferenceExpression("Block")), new CodeIndexerExpression(new CodeVariableReferenceExpression("input"), new CodePrimitiveExpression(0)))};
                    CodeConditionStatement if_then = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("input"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression("")), trueStatement, falseStatement);
                    //CodeAssignStatement assignInput = new CodeAssignStatement(new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("Memory"), new CodeVariableReferenceExpression("Block")), new CodeIndexerExpression(new CodeMethodInvokeExpression(csSystemConsoleType, "ReadLine"), new CodePrimitiveExpression(0)));
                    start.Statements.Add(if_then);
                    break;
            }
        }

        class1.Members.Add(start);

        samples.Types.Add(class1);

        return compileUnit;
    }

    public static string GenerateCode(CodeDomProvider provider,
                                      CodeCompileUnit compileunit)
    {
        // Build the source file name with the language
        // extension (vb, cs, js).
        string sourceFile;
        if (provider.FileExtension[0] == '.')
        {
            sourceFile = file + provider.FileExtension;
        }
        else
        {
            sourceFile = file + "." + provider.FileExtension;
        }

        IndentedTextWriter tw = new IndentedTextWriter(new StreamWriter(sourceFile, false), "    ");
        provider.GenerateCodeFromCompileUnit(compileunit, tw, new CodeGeneratorOptions());
        tw.Close();

        return sourceFile;
    }

    public static bool CompileCode(CodeDomProvider provider,
        string sourceFile,
        string exeFile)
    {

        CompilerParameters cp = new CompilerParameters()
        {
            GenerateExecutable = true,
            OutputAssembly = exeFile,
            IncludeDebugInformation = true,
            GenerateInMemory = false,
            WarningLevel = 3,
            TreatWarningsAsErrors = false,
            CompilerOptions = "/optimize",
            TempFiles = new TempFileCollection(".", false),
            MainClass = "BrainF_Code.BrainF"
        };
        cp.ReferencedAssemblies.Add("System.dll");
        CompilerResults cr = provider.CompileAssemblyFromFile(cp, sourceFile);

        if (cr.Errors.Count > 0)
        {
            Console.WriteLine("Errors building {0} into {1}",
                sourceFile, cr.PathToAssembly);
            foreach (CompilerError ce in cr.Errors)
            {
                Console.WriteLine("  {0}", ce.ToString());
                Console.WriteLine();
            }
        }
        else
        {
            ConsoleColor og = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Source {0} built into {1} successfully.", originalFile, cr.PathToAssembly);
            Console.ForegroundColor = og;
        }

        if (cr.Errors.Count > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static void Compile(string code)
    {
        CodeDomProvider provider = null;

        provider = CodeDomProvider.CreateProvider("cs");

        CodeCompileUnit helloWorld = BuildGraph(code);

        string sourceFile = GenerateCode(provider, helloWorld);

        CompileCode(provider, sourceFile, exeFile);
        File.Delete(file + ".cs");
        File.Delete(file + ".exe.mdb");
        File.Delete(file + ".pdb");
    }

    public static void Help(string command)
    {
        if (command == null)
        {
            Console.WriteLine("Usage: brainf [file] options...");
            Console.WriteLine("--help       Displays this message.");
        }
    }
}