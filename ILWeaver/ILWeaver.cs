using System;
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
        // Get the path to the executing assembly (the Weaver DLL)
        string sourceAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        string targetAssemblyPath = args[0];
        
        //InjectLogging(assemblyPath);
        InjectLogging2(targetAssemblyPath, sourceAssemblyPath);
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


    static void InjectLogging2(string targetAssemblyPath, string sourceAssemblyPath)
    {
        // Load the source and target assemblies
        var sourceAssembly = ModuleDefinition.ReadModule(sourceAssemblyPath);
        var targetAssembly = ModuleDefinition.ReadModule(targetAssemblyPath);

        // Get the source method (e.g., _Notification from CodeToCopy)
        var sourceAssemblyTypes = sourceAssembly.Types.FirstOrDefault(t => t.Methods.Any(m => m.Name == "TestMethodSource"));
        var sourceMethod = sourceAssemblyTypes.Methods.First();
        Console.WriteLine($"////-WEAVER-//// SOURCE FOUND : {sourceAssemblyTypes.Methods.First(m => m.Name == "TestMethodSource").FullName}");

        // Import the source method into the target module
        var importedMethod = targetAssembly.ImportReference(sourceMethod);

        // Find the target class to add the method to
        var targetTypes = targetAssembly.Types.FirstOrDefault(t => t.Methods.Any(m => m.Name == "TestMethod"));
        var targetMethod = targetTypes.Methods.First();
  
        if (targetMethod == null)
        {
            throw new Exception("Target class not found in the target assembly.");
            Console.WriteLine($"////-WEAVER-//// Target class not found in the target assembly.");
        }

        // Create a new method in the target type
        var newMethod = new MethodDefinition(
            sourceMethod.Name,
            sourceMethod.Attributes,
            targetAssembly.ImportReference(sourceMethod.ReturnType)
        );

        // Import parameters
        foreach (var param in sourceMethod.Parameters)
        {
            newMethod.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, targetAssembly.ImportReference(param.ParameterType)));
        }

        // Copy the IL body of the source method
        //foreach (var instruction in sourceMethod.Body.Instructions)
        //{
        //    if (instruction.Operand is IMetadataTokenProvider metadataTokenProvider)
        //    {
        //        newMethod.Body.Instructions.Add(Instruction.Create(instruction.OpCode, targetAssembly.ImportReference((TypeReference)metadataTokenProvider)));
        //    }
        //    else
        //    {
        //        newMethod.Body.Instructions.Add(instruction);
        //    }
        //}


        // Add the new method to the target class
        //targetType.Methods.Add(newMethod);
        var processor = targetMethod.Body.GetILProcessor();
        processor.Clear();

        foreach (var instruction in sourceMethod.Body.Instructions)
        {
            processor.Append(instruction);
            Console.WriteLine($"////-WEAVER-//// Instruction beind added: {instruction.ToString()}");
        }

        // Save the modified target assembly
        targetAssembly.Write(targetAssemblyPath);

        Console.WriteLine($"Successfully copied Sucessfull to {targetAssemblyPath}");
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

        //                // Inject: GD.Print("_Ready called in <TypeName>")
        //                processor.InsertBefore(method.Body.Instructions.First(),
        //                    processor.Create(OpCodes.Ldstr, $"IL Weaver _Ready called in {type.Name}"));
        //                processor.InsertAfter(method.Body.Instructions.First(),
        //                    processor.Create(OpCodes.Call, godotGDPrintMethod));
        //            }
        //        }
        //    }

        //    // Save the modified module
        //    assembly.Write(assemblyPath);
        //    Console.WriteLine("////-WEAVER-////Weaving completed successfully.");
        //}

        //static MethodReference? ResolveGodotPrintMethod(ModuleDefinition module)
        //{
        //    // Find the Godot.GD type in the target module's references
        //    var godotGDType = module.AssemblyReferences
        //        .Select(reference => module.AssemblyResolver.Resolve(reference))
        //        .SelectMany(resolvedAssembly => resolvedAssembly.MainModule.Types)
        //        .FirstOrDefault(type => type.FullName == "Godot.GD");

        //    if (godotGDType == null)
        //        return null;

        //    // Find the "Print" method within Godot.GD
        //    var printMethod = godotGDType.Methods.FirstOrDefault(m => m.Name == "Print");
        //    if (printMethod == null)
        //        return null;

        //    // Import the method into the target module
        //    return module.ImportReference(printMethod);
    }
}