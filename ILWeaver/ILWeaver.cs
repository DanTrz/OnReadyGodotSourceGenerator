using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;


class ILWeaver
{
    static void Main(string[] args)
    {
        Console.WriteLine("////-WEAVER-////Weaver Started");

        if (args.Length == 0)
        {
            Console.WriteLine("////-WEAVER-////Usage: ILWeaver <path_to_target_assembly>");
            return;
        }
        //Get the path to the executing assembly(the Weaver DLL)
        string sourceAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        //Get the path to the target assembly (the Godot DLL) - This comes from Project XML settings Exec Command arguments
        string targetAssemblyPath = args[0]; 

        //Get a temp Assembly path to work with, as we will need to copy the original assembly to a temp location to modify it
        string tempTargetAssemblyPath = GetTempAssemblyPath(targetAssemblyPath);

        //Start the process to inject code and modify assembly
        InjectCodeLogic(targetAssemblyPath, sourceAssemblyPath, tempTargetAssemblyPath);
    }
    
    static void InjectCodeLogic(string targetAssemblyPath, string sourceAssemblyPath, string tempTargetAssemblyPath)
    {
        // Load the source and target assemblies
        var sourceAssembly = ModuleDefinition.ReadModule(sourceAssemblyPath); //This the ILWeaver DLL
        var tempTargetAssembly = ModuleDefinition.ReadModule(tempTargetAssemblyPath); //This the Godot DLL

        Console.WriteLine($"////-WEAVER-//// Original Target: {targetAssemblyPath}");
        Console.WriteLine($"////-WEAVER-//// Temp Target: {tempTargetAssemblyPath}");
        Console.WriteLine($"////-WEAVER-//// Source: {sourceAssemblyPath}");

        // Get the source method in the source assembly
        var sourceAssemblyType = sourceAssembly.Types.FirstOrDefault(t => t.Methods.Any(m => m.Name == "TestMethodSource"));
        var sourceMethod = sourceAssemblyType.Methods.First(m => m.Name == "TestMethodSource");

        if (sourceMethod == null)
        {
            throw new Exception("////-WEAVER-//// Source method not found in the target assembly.");
        }

        // Get the target method in the target assembly
        var targetType = tempTargetAssembly.Types.FirstOrDefault(t => t.Methods.Any(m => m.Name == "TestMethod"));
        var targetMethod = targetType.Methods.First(m => m.Name == "TestMethod");

        if (targetMethod == null)
        {
            throw new Exception("////-WEAVER-//// Target method not found in the target assembly.");
        }

        // Clear the target method body
        var targetBody = targetMethod.Body;
        targetBody.Instructions.Clear();
        targetBody.ExceptionHandlers.Clear();
        targetBody.Variables.Clear();

        // Copy the method body from the source method
        var sourceBody = sourceMethod.Body;

        //ILProcessor is used internally to modify the method bodies of the assembly before saving it
        var ilProcessor = targetBody.GetILProcessor(); //Here we tell what element it shoudl modify - IN this case the targetBody in the tempTargetAssembly
        var instructionMap = new Dictionary<Instruction, Instruction>();

        // Copy variables
        foreach (var variable in sourceBody.Variables)
        {
            var importedVariableType = tempTargetAssembly.ImportReference(variable.VariableType);
            targetBody.Variables.Add(new VariableDefinition(importedVariableType));
        }
        Console.WriteLine($"////-WEAVER-//// Step 1: Variables Copied");

        // Copy instructions from the source method to the target method
        foreach (var instruction in sourceBody.Instructions)
        {
            var newInstruction = instruction.Operand switch
            {
                MethodReference methodRef => Instruction.Create(instruction.OpCode, tempTargetAssembly.ImportReference(methodRef)),
                FieldReference fieldRef => Instruction.Create(instruction.OpCode, tempTargetAssembly.ImportReference(fieldRef)),
                TypeReference typeRef => Instruction.Create(instruction.OpCode, tempTargetAssembly.ImportReference(typeRef)),
                ParameterDefinition paramDef => Instruction.Create(instruction.OpCode, targetMethod.Parameters[paramDef.Index]),
                VariableDefinition varDef => Instruction.Create(instruction.OpCode, targetBody.Variables[varDef.Index]),
                string str => Instruction.Create(instruction.OpCode, str),
                null => Instruction.Create(instruction.OpCode),
                _ => throw new NotSupportedException($"////-WEAVER-//// Unsupported operand type instruction: {instruction.ToString()} Operand: {instruction.Operand?.GetType().FullName}")
            };
            
            //When using the Processor (iLProcessor) we are modifing the assembly already directly. 
            ilProcessor.Append(newInstruction);
            instructionMap[instruction] = newInstruction; // Map old instructions to new ones for branch fixups
            
        }
        Console.WriteLine($"////-WEAVER-//// Step 2: instructions copied and mapped");

        // -Fix branch instructions and exception handlers
        foreach (var instruction in targetBody.Instructions)
        {
            if (instruction.Operand is Instruction targetInstruction && instructionMap.ContainsKey(targetInstruction))
            {
                instruction.Operand = instructionMap[targetInstruction];
            }
            else if (instruction.Operand is Instruction[] targets)
            {
                instruction.Operand = targets.Select(t => instructionMap[t]).ToArray();
            }
        }

        // Copy exception handlers
        foreach (var handler in sourceBody.ExceptionHandlers)
        {
            targetBody.ExceptionHandlers.Add(new ExceptionHandler(handler.HandlerType)
            {
                CatchType = handler.CatchType == null ? null : tempTargetAssembly.ImportReference(handler.CatchType),
                TryStart = instructionMap[handler.TryStart],
                TryEnd = instructionMap[handler.TryEnd],
                HandlerStart = instructionMap[handler.HandlerStart],
                HandlerEnd = instructionMap[handler.HandlerEnd],
                FilterStart = handler.FilterStart == null ? null : instructionMap[handler.FilterStart]
            });
        }
        Console.WriteLine($"////-WEAVER-//// Step 3: instructions exception handlers done");

        // Save the modified assembly to the target path (overwriting the original assembly)
        tempTargetAssembly.Write(targetAssemblyPath);

        Console.WriteLine($"////-WEAVER-//// Step 4: SUCCESS: saved the modified assembly to: {targetAssemblyPath}");

        printCodeOutput(targetAssemblyPath, "SampleScene", "TestMethod");
    }

    public static string GetTempAssemblyPath(string godotDllPath)
    {
        //TODO CHANGE THIS - THIS IS HARDCODED PATHS
        //string godotSourceAssemblyPath = @"C:\Local Documents\Development\Godot\Source Generator Tests\OnReadyGodotSourceGenerator\samplegodotproject_onreadysourcegenerator\.godot\mono\temp\bin\Debug\SampleGodotProject_OnReadySourceGenerator.dll";
        string godotOriginalAssemblyPath = godotDllPath;

        if (string.IsNullOrEmpty(godotOriginalAssemblyPath))
        {
            Console.WriteLine($"////-WEAVER-////Failed to get the path to the original assembly");
        }

        string godotAssemblyFileName = Path.GetFileName(godotOriginalAssemblyPath);
        string godotAssemblydirectory = Path.GetDirectoryName(godotOriginalAssemblyPath);
        string tempAssemblyPath = godotAssemblydirectory + "\\tempAssembly.dll";

        //string tempAssemblyPath = @"C:\Local Documents\Development\Godot\Source Generator Tests\OnReadyGodotSourceGenerator\samplegodotproject_onreadysourcegenerator\.godot\mono\temp\bin\Debug\Temp_SampleGodotProject_OnReadySourceGenerator.dll";

        try
        {
            File.Copy(godotOriginalAssemblyPath, tempAssemblyPath, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"////-WEAVER-////Failed to copy the original assembly: {ex.Message}");
        }

        return tempAssemblyPath;


        //return @"C:\Local Documents\Development\Godot\Source Generator Tests\OnReadyGodotSourceGenerator\samplegodotproject_onreadysourcegenerator\.godot\mono\temp\bin\Debug\SampleGodotProject_OnReadySourceGenerator.dll";
    }

    public static void printCodeOutput(string assemblyToReadPath, string className, string methodName)
    {
        // Path to the assembly you want to inspect
        var assemblyPath = assemblyToReadPath;

        // Load the assembly
        var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);

        // Find the type and method you are interested in
        var type = assembly.MainModule.GetType(className); // Replace with your class
        var method = type.Methods.FirstOrDefault(m => m.Name == methodName);

        Console.WriteLine($"///WEAVER OUTPUT/// Check Result in path: {assemblyToReadPath}");
        Console.WriteLine($"///WEAVER OUTPUT/// Check Method Name: {method.Name}");

        if (method != null)
        {
            Console.WriteLine($"///WEAVER OUTPUT/// Method Name: {method.Name}");
            Console.WriteLine(FormatMethodAsCSharp(method));
        }
        else
        {
            Console.WriteLine("///WEAVER OUTPUT/// Method not found!");
        }
    }

    static string FormatMethodAsCSharp(MethodDefinition method)
    {
        var modifiers = method.IsPublic ? "public" : method.IsPrivate ? "private" : method.IsFamily ? "protected" : "internal";
        if (method.IsStatic)
        {
            modifiers += " static";
        }

        var returnType = method.ReturnType.Name;
        var methodName = method.Name;

        var parameters = string.Join(", ", method.Parameters
            .Select(p => $"{p.ParameterType.Name} {p.Name}"));

        var body = FormatBody(method.Body);

        return $"{modifiers} {returnType} {methodName}({parameters})\n{{\n{body}\n}}";
    }

    static string FormatBody(MethodBody body)
    {
        var instructions = body.Instructions.Select(i => $"    {i.OpCode} {i.Operand}");
        return string.Join("\n", instructions);
    }

}