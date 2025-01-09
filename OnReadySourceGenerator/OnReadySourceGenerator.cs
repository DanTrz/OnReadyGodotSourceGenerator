using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis.CSharp;
using SourceGenerator;
using System.Runtime.CompilerServices;

[Generator]
public class OnReadySourceGenerator : ISourceGenerator
{
    /// <summary>
    /// Path to save the log file to. Change to save to any specific path you want. By default will save on root of the project
    /// </summary>
    private static string SaveFilePath()
    {
        //// This will get the current WORKING directory (i.e. \bin\Debug)
        //string workingDirectory = Environment.CurrentDirectory;     // or: Directory.GetCurrentDirectory() gives the same result

        //// This will get the current TOP LEVEL PROJECT directory
        //string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName +"/";

        string projectDirectory = "C:\\Local Documents\\Development\\Godot\\Source Generator Tests\\OnReadyGodotSourceGenerator\\samplegodotproject_onreadysourcegenerator\\.godot\\mono\\temp\\";
        return projectDirectory;
    }

    /// <summary>
    /// Comment this method or it's contents out to disable logging to a file
    /// </summary>
    private static void saveLogToFile(string logMessage)
    {
        //File.AppendAllText($@"{SaveFilePath()}{"MasterLog"}", logMessage + "\r\n");
    }

    /// <summary>
    /// Called by the compiler to initialize the generator. This is the first method called and where the generator register the syntax receiver to be used.
    /// </summary>
    /// <param name="context">The context of the source generator.</param>
    public void Initialize(GeneratorInitializationContext context)
    {
        //DEBUGGER: Uncomment this line to run a debugger and navigate the code.
        //if (!Debugger.IsAttached) Debugger.Launch();

        // Register a syntax receiver to capture FIELD declarations with the OnReadyAttribute (Custom syntax receiver)
        context.RegisterForSyntaxNotifications(() => new OnReadySyntaxReceiver());
    }

    /// <summary>
    /// Syntax Receiver: Captures field  declarations with the OnReadyAttribute. 
    /// This method is called by the compiler on Initiliaze.
    /// </summary>
    private class OnReadySyntaxReceiver : ISyntaxReceiver
    {
        /// <summary>
        /// The list of field declarations with the OnReadyAttribute.
        /// </summary>
        public List<BaseFieldDeclarationSyntax> ItemsFields { get; } = new();
        public List<BasePropertyDeclarationSyntax> ItemsProperties { get; } = new();//TODO: this is never used/Consider removing it

        /// <summary>
        /// Called by the compiler to visit a syntax node. Filters to retrive only field declarations with the OnReady attribute.
        /// </summary>
        /// <param name="syntaxNode">The syntax node to visit.</param>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Capture fields annotated only
            if (syntaxNode is FieldDeclarationSyntax fieldDeclaration)
            {
                // Check if the field declaration has the OnReady attribute 
                var hasOnReadyAttribute = fieldDeclaration.AttributeLists
                    .SelectMany(attrList => attrList.Attributes)
                    .Any(attr => attr.Name.ToString() == Const.ONREADY);

                // Capture fields annotated with [OnReady]
                if (hasOnReadyAttribute)
                {
                    // Add the field declaration to the list
                    ItemsFields.Add(fieldDeclaration);
                }
            }
            else if (syntaxNode is PropertyDeclarationSyntax propertyDeclaration) //TODO: this is never used/Consider removing it
            {
                // Check if the field declaration has the OnReady attribute 
                var hasOnReadyAttribute = propertyDeclaration.AttributeLists
                    .SelectMany(attrList => attrList.Attributes)
                    .Any(attr => attr.Name.ToString() == Const.ONREADY);

                // Capture fields annotated with [OnReady]
                if (hasOnReadyAttribute)
                {
                    // Add the field declaration to the list
                    ItemsProperties.Add(propertyDeclaration);
                }

            }



        }
    }

    /// <summary>
    /// Called by the compiler to generate code based on the sources provided and based on the syntax receiver output
    /// </summary>
    /// <param name="context">The context of the source generator.</param>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not OnReadySyntaxReceiver receiver) return;

        //If no fields with Onready attributes are found, we return a warning message
        if (!receiver.ItemsFields.Any())
        {
            var desc = new DiagnosticDescriptor(
              "ONREADYSG01",
              "No OnReady attributes or variables found",
              "OnReady Source Generator Error: Either no attributes declared or sintaxe is wrong. Likely to encounter Null fields and variables",
              "Problem",
              DiagnosticSeverity.Warning,
              true);
            context.ReportDiagnostic(Diagnostic.Create(desc, Location.None));

            //return;
        }

        //Debug and Log
        saveLogToFile($@"Total OnReady Declaration Count: " + receiver.ItemsFields.Count().ToString() + "\r\n");

        var modePathString = string.Empty;
        var filedTypeString = string.Empty;
        var fieldSymbolString = string.Empty;
        var classNameString = string.Empty;
        var intializerString = string.Empty;
        IList<INamedTypeSymbol> classSymbolsList = new List<INamedTypeSymbol>();

        //Dictonary to store all OnReady Variables /-/ Dic Key = ClassName /-/ Dic Value = List of OnReady Variables
        Dictionary<string, List<(string fieldName, string fieldType, string nodePath, string initializer)>> onReadyVariablesList = new();

        // Process each field marked with OnReadyAttribute
        foreach (var field in receiver.ItemsFields)
        {
            // Check if the field is a field declaration, if not, go to the next field to check.
            if (field is not FieldDeclarationSyntax fieldDeclaration) continue;

            var model = context.Compilation.GetSemanticModel(field.SyntaxTree);

            //this provides the entire class declaration, by getting the parent of the field
            var classDeclaration = field.Parent as ClassDeclarationSyntax;
            if (classDeclaration == null) continue;

            var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
            //Returns the Class Symbol (e.g. Node2D or Baselevel, etc)
            if (classSymbol == null) continue;
            classNameString = classSymbol.Name.ToString();

            if (!classSymbolsList.Contains(classSymbol))
            {
                classSymbolsList.Add(classSymbol);
            }

            // Check the variables of type fieldDelcararion and then retrive it's details
            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                var fieldSymbol = model.GetDeclaredSymbol(variable) as IFieldSymbol;
                if (fieldSymbol == null) continue;

                //Retrives the field name / This is the variable name in Godot Script E.g. _myAudioStreamPlayer)
                fieldSymbolString = fieldSymbol.Name.ToString();

                // Retrieve the initializer expression if available (e.g., what comes after "=", like "GD.Load<PackedScene>(...)")
                var variableInitializer = variable.Initializer;
                intializerString = variableInitializer?.Value?.ToString() ?? string.Empty;
                
                saveLogToFile($@"From Variable: " + variable.ToFullString() + " -> Initializer: " + intializerString + "\r\n"); ;


                // Retrieve the OnReady attribute details
                var onReadyAttribute = fieldSymbol.GetAttributes()
                    .FirstOrDefault(attr => attr.AttributeClass?.Name == Const.ONREADY_ATTRIBUTE);
                if (onReadyAttribute == null) continue;

                // Extract the NodePath - This is the path to the node within the OnReady attribute
                modePathString = onReadyAttribute.ConstructorArguments[0].Value as string;
                if (modePathString == null) continue;

                // Extract the Node type (this is the Godot Object type - E.g. AudioStreamPlayer)
                filedTypeString = fieldSymbol.Type.ToString();
                if (filedTypeString == null) continue;

                // Dict Key  = Classes // Dict Values = Fields in Classes with OnReady
                // We check if if already have a Key for that class, otherwise we create one.
                if (!onReadyVariablesList.ContainsKey(classNameString))
                {
                    onReadyVariablesList.Add(classNameString, new List<(string, string, string, string)> {
                        (fieldSymbolString, filedTypeString, modePathString, intializerString) });
                }
                else
                {
                    onReadyVariablesList[classNameString].Add((fieldSymbolString, filedTypeString, modePathString, intializerString));
                }
            }
        }

        //Check for Errors and add error messaages
        if (receiver.ItemsFields.Count == 0)
        {
            var desc = new DiagnosticDescriptor(
                  "ONREADYSG02",
                  "OnReady Source Generator error: attributes or variables found",
                  "OnReady Source Generator error:No fields declared with OnReady attribute or wrong syntax.Check spelling and OnReady attributes",
                  "Problem",
                  DiagnosticSeverity.Warning,
                  true);
        }
        if (classSymbolsList.Count == 0)
        {
            var desc = new DiagnosticDescriptor(
                  "ONREADYSG03",
                  "OnReady Source Generator error: attributes or variables found",
                  "OnReady Source Generator error:OnReady Error: No Classes declared with OnReady attributes or OnReady Attribute not found",
                  "Problem",
                  DiagnosticSeverity.Info,
                  true);
            context.ReportDiagnostic(Diagnostic.Create(desc, Location.None));
        }
        if (onReadyVariablesList.Count == 0)
        {
            var desc = new DiagnosticDescriptor(
                  "ONREADYSG04",
                  "OnReady Source Generator error: attributes or variables found",
                  "OnReady Error: No fields declared with OnReady attribute or wrong syntax",
                  "Problem",
                  DiagnosticSeverity.Info,
                  true);
            context.ReportDiagnostic(Diagnostic.Create(desc, Location.None));
        }

        // Flag to track if any source was added
        bool sourceAdded = false;

        //Iterate through each class that has OnReady Attributes and generate the source code for each class
        foreach (var classSymbol in classSymbolsList)
        {
            string className = classSymbol.Name.ToString();
            string fieldName = string.Empty;
            string fieldType = string.Empty;
            string nodePath = string.Empty;
            string initializer = string.Empty;

            StringBuilder tempAllNodeDeclarations = new();

            //Goes through all the OnReady variables and generates the "GetNode" code string
            foreach (var onReadyfield in onReadyVariablesList[className])
            {
                fieldName = onReadyfield.fieldName;
                fieldType = onReadyfield.fieldType;
                nodePath = onReadyfield.nodePath;
                initializer = onReadyfield.initializer;


                if (!string.IsNullOrEmpty(initializer) && nodePath.StartsWith(Const.INITIALIZER_SYMBOL))
                {

                    //Create the node declaration string with the GetNode method and Error Handler
                    tempAllNodeDeclarations.Append($@"myNode.{fieldName} = {initializer};");
                    tempAllNodeDeclarations.Append("\n");
                    tempAllNodeDeclarations.Append($@"
                    if ({fieldName} == null || myNode.{fieldName} == null)
                    {{
                        GD.PrintErr(""ONREADYSG201: Could not resolve OnReady member:{fieldName} Class:{className}  Check if special $ symbol was added or if path is incorrect"");
                        GD.PrintErr(""Fields or Variables with Initializer require special $ symbol, e.g. [OnReady({Const.INITIALIZER_SYMBOL})] "");
                    }}
                    ");

                    tempAllNodeDeclarations.Append("\n");
                }
                else if (!string.IsNullOrEmpty(initializer))
                {
                    //Create the node declaration string with the GetNode method and Error Handler
                    tempAllNodeDeclarations.Append($@"myNode.{fieldName} = {initializer};");
                    tempAllNodeDeclarations.Append("\n");
                    tempAllNodeDeclarations.Append($@"
                    if ({fieldName} == null || myNode.{fieldName} == null)
                    {{
                        GD.PrintErr(""ONREADYSG202: Could not resolve OnReady member:{fieldName} Class:{className}  Check if special $ symbol was added or if path is incorrect"");
                        GD.PrintErr(""Fields or Variables with Initializer require special $ symbol, e.g. [OnReady({Const.INITIALIZER_SYMBOL})] "");
                    }}
                    ");

                    tempAllNodeDeclarations.Append("\n");
                }
                else
                {
                    //Create the node declaration string with the GetNode method and Error Handler
                    tempAllNodeDeclarations.Append($@"myNode.{fieldName} = node.GetNode<{fieldType}>(""{nodePath}"");");
                    tempAllNodeDeclarations.Append("\n");
                    tempAllNodeDeclarations.Append($@"
                    if ({fieldName} == null || myNode.{fieldName} == null)
                    {{
                        GD.PrintErr(""ONREADYSG203: Could not resolve OnReady member:{fieldName}  NodePath:{nodePath}  Class:{className}."");
                    }}
                    ");

                    tempAllNodeDeclarations.Append("\n");

                }


            }

            //final source code generation method and then we add it to the context to be compiled
            var source = GenerateOnReadyCodeForClass(className, tempAllNodeDeclarations.ToString());
            context.AddSource($"{className}_OnReady.g.cs", SourceText.From(source, Encoding.UTF8));

            sourceAdded = true;

            //SAVE to LOG the Generated Source Code(If you want to monitor the results of the generator without debugging it)

            saveLogToFile($@"SOURCE CODE ADDED:" + SourceText.From(source, Encoding.UTF8).ToString() + "\r\n" + "\r\n");


            //File.AppendAllText($@"{SaveFilePath()}{"MasterLog"}",
            // "SOURCE:" + SourceText.From(source, Encoding.UTF8).ToString() + "\r\n" + "\r\n");
        }

        if (!sourceAdded)
        {
            var desc = new DiagnosticDescriptor(
              "ONREADYSG05",
              "OnReady Source Generator error: Compile error, attributes or variables found",
              "OnReady Source Generator error. No Code Added via Source Generator. One or more Node Paths are incorret or OnReady Attributes are incorrectly defined. Run with Debugging active to check error details",
              "Problem",
              DiagnosticSeverity.Warning,
              true);
            context.ReportDiagnostic(Diagnostic.Create(desc, Location.None));
        }
    }


    /// <summary>
    /// Generates the code for the OnReady extension method for the given class.
    /// </summary>
    /// <param name="className">The name of the class the extension method is for.</param>
    /// <param name="allFieldDelcarations">The code for all the field declarations.</param>
    /// <returns>The code for the OnReady extension method.</returns>
    private string GenerateOnReadyCodeForClass(string className, string allFieldDelcarations)
    {
        return $@"
                using Godot;
                using System;

                    partial class {className}: OnReadyInterface.IOnReady
                    {{
                        public void OnReady(Godot.Node node)
                        {{
                            if (node is {className} myNode)
                            {{
                                    {allFieldDelcarations}
                            }}
                        }}
                    }}
                ";
    }



}

