using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;


class ILWeaver
{
    public static string GodotTempcopyAssemblyPath()
    {
        string godotSourceAssemblyPath = @"C:\Local Documents\Development\Godot\Source Generator Tests\OnReadyGodotSourceGenerator\samplegodotproject_onreadysourcegenerator\.godot\mono\temp\bin\Debug\SampleGodotProject_OnReadySourceGenerator.dll";
        string tempAssemblyPath = @"C:\Local Documents\Development\Godot\Source Generator Tests\OnReadyGodotSourceGenerator\samplegodotproject_onreadysourcegenerator\.godot\mono\temp\bin\Debug\Temp_SampleGodotProject_OnReadySourceGenerator.dll";

        try
        {
            File.Copy(godotSourceAssemblyPath, tempAssemblyPath, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"////-WEAVER-////Failed to copy the original assembly: {ex.Message}");
        }

        return tempAssemblyPath;


        //return @"C:\Local Documents\Development\Godot\Source Generator Tests\OnReadyGodotSourceGenerator\samplegodotproject_onreadysourcegenerator\.godot\mono\temp\bin\Debug\SampleGodotProject_OnReadySourceGenerator.dll";
    }
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

        //string targetAssemblyPath = args[0];
        string targetAssemblyPath = @"C:\Local Documents\Development\Godot\Source Generator Tests\OnReadyGodotSourceGenerator\samplegodotproject_onreadysourcegenerator\.godot\mono\temp\bin\Debug\SampleGodotProject_OnReadySourceGenerator.dll";

        string tempTargetAssemblyPath = GodotTempcopyAssemblyPath();

        //InjectLogging(assemblyPath);
        InjectLogging2(targetAssemblyPath, sourceAssemblyPath, tempTargetAssemblyPath);
    }

    static void CopyModifiedDll(string tempAssemblyPath, string targetAssemblyPath)
    {

        Console.WriteLine($"////-WEAVER-////Started Trying to Modify Godot DLL: {targetAssemblyPath}");
        int retries = 20;
        while (retries > 0)
        {
            try
            {
                File.Copy(tempAssemblyPath, targetAssemblyPath, overwrite: true);
                Console.WriteLine($"////-WEAVER-////Successfully copied the modified assembly to: {targetAssemblyPath}");
                return;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"////-WEAVER-////Failed to copy assembly. Retrying... ({retries} attempts left)");
                Console.WriteLine(ex.Message);
                retries--;
                Thread.Sleep(500);  // Wait for 500ms before retrying
            }
        }

        Console.WriteLine("////-WEAVER-////Failed to copy the modified assembly after several attempts.");
    }

    public static void ProcessCurrentAssembly()
    {
        // Get the path to the executing assembly (the Weaver DLL)
        string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Load the assembly using Mono.Cecil
        var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);

        Console.WriteLine($"////-WEAVER-////-CURRENT => Loaded Assembly: {assembly.Name}");

        // Example: List all types in the assembly
        foreach (var type in assembly.MainModule.Types)
        {
            Console.WriteLine($"////-WEAVER-////-CURRENT =>Type: {type.FullName}");

            // Example: List methods for each type
            foreach (var method in type.Methods)
            {
                Console.WriteLine($"////-WEAVER-////-CURRENT =>  Method: {method.Name}");
            }
        }

        // Perform modifications or analysis...
        // e.g., Modify or add a method, copy it to another DLL, etc.

        // Save changes back if needed (e.g., overwrite the DLL)
        // assembly.Write(assemblyPath);
    }


    static void InjectLogging2(string targetAssemblyPath, string sourceAssemblyPath, string tempTargetAssemblyPath)
    {
        // Load the source and target assemblies
        var sourceAssembly = ModuleDefinition.ReadModule(sourceAssemblyPath);
        var tempTargetAssembly = ModuleDefinition.ReadModule(tempTargetAssemblyPath);

        Console.WriteLine($"////-WEAVER-//// Original Target: {targetAssemblyPath}");
        Console.WriteLine($"////-WEAVER-//// Temp Target: {tempTargetAssemblyPath}");
        Console.WriteLine($"////-WEAVER-//// Source: {sourceAssemblyPath}");

        // Get the source method
        var sourceAssemblyType = sourceAssembly.Types.FirstOrDefault(t => t.Methods.Any(m => m.Name == "TestMethodSource"));
        var sourceMethod = sourceAssemblyType.Methods.First(m => m.Name == "TestMethodSource");

        // Find the target method in the target assembly
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
        var ilProcessor = targetBody.GetILProcessor();
        var instructionMap = new Dictionary<Instruction, Instruction>();

        // Copy variables
        foreach (var variable in sourceBody.Variables)
        {
            var importedVariableType = tempTargetAssembly.ImportReference(variable.VariableType);
            targetBody.Variables.Add(new VariableDefinition(importedVariableType));
        }
        Console.WriteLine($"////-WEAVER-//// Variables Copied");

        // Copy instructions
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
                _ => throw new NotSupportedException($"////-WEAVER-//// Unsupported operand type: {instruction.Operand?.GetType().FullName}")
            };
            Console.WriteLine($"////-WEAVER-//// instructions Copied");

            ilProcessor.Append(newInstruction);
            instructionMap[instruction] = newInstruction; // Map old instructions to new ones for branch fixups
            Console.WriteLine($"////-WEAVER-//// instructions mapped");
        }

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
        Console.WriteLine($"////-WEAVER-//// instructions exception handlers done");

        // Save the modified assembly to the target path
        tempTargetAssembly.Write(targetAssemblyPath);

        Console.WriteLine($"////-WEAVER-//// SUCCESS: saved the modified assembly to: {targetAssemblyPath}");

        //printCodeOutput(targetAssemblyPath, "SampleScene", "TestMethod");
    }



    static void InjectLogging(string targetAssemblyPath)
    {
        Console.WriteLine($"////-WEAVER-////Weaving assembly: {targetAssemblyPath}");

        // Load the target assembly (the Godot project DLL)
        //var assembly = AssemblyDefinition.ReadAssembly(assemblyPath); // Previous version
        //var assembly = ModuleDefinition.ReadModule(assemblyPath); // New version


        //ProcessCurrentAssembly();//Code to read the current assembly DLL

        // Get the path to the executing assembly (the Weaver DLL)
        string sourceAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Load the assembly using Mono.Cecil
        var sourceAssembly = ModuleDefinition.ReadModule(sourceAssemblyPath);

        var sourceAssemblyTypes = sourceAssembly.Types.FirstOrDefault(t => t.Methods.Any(m => m.Name == "TestMethodSource"));

        Console.WriteLine($"////-WEAVER-//// SOURCE FOUND : {sourceAssemblyTypes.Methods.First(m => m.Name == "TestMethodSource").FullName}");

        var sourceMethod = sourceAssemblyTypes.Methods.First();


        using (var targetAssemblyModule = ModuleDefinition.ReadModule(targetAssemblyPath, new ReaderParameters { ReadWrite = true }))
        {
            foreach (var appModuleType in targetAssemblyModule.Types)
            {
                Console.WriteLine($"////-WEAVER-//// Type _ClassName_ : {appModuleType.FullName}");

                foreach (var methods in appModuleType.Methods)
                {
                    //Console.WriteLine($"////-WEAVER-////  _Methods_List): {methods.FullName}");

                    if (methods.Name == "TestMethod")
                    {
                        Console.WriteLine($"////-WEAVER-//// Method Found: {methods.FullName}");
                    }
                }
            }


            //var targetTypes = assembly.Types.Where(t => t.Methods.Any(m => m.Name == "TestMethod"));
            var targetTypes = targetAssemblyModule.Types.FirstOrDefault(t => t.Methods.Any(m => m.Name == "TestMethod"));

            var targetMethod = targetTypes.Methods.First();

            var processor = targetMethod.Body.GetILProcessor();
            processor.Clear();

            foreach (var instruction in sourceMethod.Body.Instructions)
            {
                processor.Append(instruction);
            }

            // Modify the assembly directly
            targetAssemblyModule.Write(); // Write to the same file that was used to open the file









            //// Generate a temporary path to save the modified module
            //var tempAssemblyPath2 = $"{Path.GetDirectoryName(targetAssemblyPath)}\\Temp_{Path.GetFileName(targetAssemblyPath)}";

            //// Save the modified module to the temporary path
            //targetAssemblyModule.Write(tempAssemblyPath2);



            ////var targetTypes = assembly.Types.Where(t => t.Methods.Any(m => m.Name == "TestMethod"));
            //var targetTypes = targetAssemblyModule.Types.FirstOrDefault(t => t.Methods.Any(m => m.Name == "TestMethod"));

            //var targetMethod = targetTypes.Methods.First();

            //var processor = targetMethod.Body.GetILProcessor();
            //processor.Append(processor.Create(OpCodes.Ldstr, "Hello, World!"));

            //// Modify the assembly directly
            //targetAssemblyModule.Write(); // Write to the same file that was used to open the file
            ////CopyModifiedDll(tempAssemblyPath2, assemblyPath);
        }


        //foreach (var appModuleType in assembly.Types)
        //{
        //    Console.WriteLine($"////-WEAVER-//// Type _ClassName_ : {appModuleType.FullName}");

        //    foreach (var methods in appModuleType.Methods)
        //    {
        //        //Console.WriteLine($"////-WEAVER-////  _Methods_List): {methods.FullName}");

        //        if (methods.Name == "TestMethod")
        //        {
        //            Console.WriteLine($"////-WEAVER-//// Method Found: {methods.FullName}");
        //        }
        //    }
        //}


        ////var targetTypes = assembly.Types.Where(t => t.Methods.Any(m => m.Name == "TestMethod"));
        //var targetTypes = assembly.Types.FirstOrDefault(t => t.Methods.Any(m => m.Name == "TestMethod"));

        //var targetMethod = targetTypes.Methods.First();

        ////var processor = targetMethod.Body.GetILProcessor();
        ////processor.Append(processor.Create(OpCodes.Ldstr, "Hello, World!"));

        //SECTION1
        //// Generate a temporary path to save the modified module
        //var tempAssemblyPath = $"{Path.GetDirectoryName(assemblyPath)}\\Temp_{Path.GetFileName(assemblyPath)}";

        //// Save the modified module to the temporary path
        //assembly.Write(tempAssemblyPath);
        //Console.WriteLine($"////-WEAVER-//// Writting to Temp Path {tempAssemblyPath}");

        //// Now copy the modified assembly to the Godot project
        //CopyModifiedDll(tempAssemblyPath, assemblyPath);

        //SECTION1END

        //// Optionally, copy the modified file back to the original location
        //try
        //{
        //    File.Copy(tempAssemblyPath, assemblyPath, true);
        //    File.Delete(tempAssemblyPath); // Delete temporary file if not needed
        //    Console.WriteLine("////-WEAVER-////Successfully replaced the original assembly with the modified version.");
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"////-WEAVER-////Failed to replace the original assembly: {ex.Message}");
        //}




        //assembly.Write(assemblyPath);


        //Console.WriteLine($"////-WEAVER-//// Target Size: {targetTypes.Count}");

        //foreach (var methods in targetTypes)
        //{
        //    Console.WriteLine($"////-WEAVER-//// Method Found: {methods.FullName}");
        //}

        //    //OLDER VERSION OF CODE///
        //    // Try to resolve the Godot.GD.Print method dynamically
        //    var godotGDPrintMethod = ResolveGodotPrintMethod(assembly);
        //    if (godotGDPrintMethod == null)
        //    {
        //        Console.WriteLine("////-WEAVER-////Failed to resolve Godot.GD.Print method. Is the Godot assembly referenced?");
        //        return;
        //    }

        //    // Iterate through all types and methods in the module
        //    foreach (var type in assembly.Types)
        //    {
        //        foreach (var method in type.Methods)
        //        {
        //            // Only modify _Ready methods
        //            if (method.Name == "_Ready" && method.HasBody)
        //            {
        //                Console.WriteLine($"////-WEAVER-////Injecting into _Ready method in {type.Name}");

        //                var processor = method.Body.GetILProcessor();


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