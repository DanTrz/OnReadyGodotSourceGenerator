using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;


// class ILWeaver
// {
//     static void Main(string[] args)
//     {
//         if (args.Length == 0)
//         {
//             Console.WriteLine("Usage: ILWeaver <path_to_target_assembly>");
//             return;
//         }

//         string assemblyPath = args[0];
//         InjectLogging(assemblyPath);
//     }

//     static void InjectLogging(string assemblyPath)
//     {
//         Console.WriteLine($"Weaving assembly: {assemblyPath}");

//         // Load the target assembly (the Godot project DLL)
//         var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);

//         // Try to resolve the Godot.GD.Print method dynamically
//         var godotGDPrintMethod = ResolveGodotPrintMethod(assembly);
//         if (godotGDPrintMethod == null)
//         {
//             Console.WriteLine("Failed to resolve Godot.GD.Print method. Is the Godot assembly referenced?");
//             return;
//         }

//         // Iterate through all types and methods in the assembly
//         foreach (var type in assembly.MainModule.Types)
//         {
//             foreach (var method in type.Methods)
//             {
//                 // Only modify _Ready methods
//                 if (method.Name == "_Ready" && method.HasBody)
//                 {
//                     Console.WriteLine($"Injecting into _Ready method in {type.Name}");

//                     var processor = method.Body.GetILProcessor();

//                     // Inject: GD.Print("_Ready called in <TypeName>")
//                     processor.InsertBefore(method.Body.Instructions.First(),
//                         processor.Create(OpCodes.Ldstr, $"_Ready called in {type.Name}"));
//                     processor.InsertAfter(method.Body.Instructions.First(),
//                         processor.Create(OpCodes.Call, godotGDPrintMethod));
//                 }
//             }
//         }

//         // Save the modified assembly
//         assembly.Write(assemblyPath);
//         Console.WriteLine("Weaving completed successfully.");
//     }

//     static MethodReference? ResolveGodotPrintMethod(AssemblyDefinition assembly)
//     {
//         // Find the Godot.GD type in the target assembly's references
//         var godotGDType = assembly.MainModule.AssemblyReferences
//             .Select(reference => assembly.MainModule.AssemblyResolver.Resolve(reference))
//             .SelectMany(resolvedAssembly => resolvedAssembly.MainModule.Types)
//             .FirstOrDefault(type => type.FullName == "Godot.GD");

//         if (godotGDType == null)
//             return null;

//         // Find the "Print" method within Godot.GD
//         var printMethod = godotGDType.Methods.FirstOrDefault(m => m.Name == "Print");
//         if (printMethod == null)
//             return null;

//         // Import the method into the target assembly
//         return assembly.MainModule.ImportReference(printMethod);
//     }
// }