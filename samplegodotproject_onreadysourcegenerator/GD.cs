using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Godot;

public static class GD
{
    private static bool IsVisualStudio2022()
    {
        if (Debugger.IsAttached)
        {
            // Attempt to detect Visual Studio 2022 through process or environment checks
            var ideProcess = Process.GetProcessesByName("devenv").FirstOrDefault();
            return ideProcess != null && ideProcess.MainModule?.FileVersionInfo.ProductVersion?.StartsWith("17.") == true;
        }
        return false;
    }
    public static Variant BytesToVar(Span<byte> bytes) => Godot.GD.BytesToVar(bytes);
    public static Variant BytesToVarWithObjects(Span<byte> bytes) => Godot.GD.BytesToVarWithObjects(bytes);
    public static Variant Convert(Variant what, Variant.Type type) => Godot.GD.Convert(what, type);

    public static int Hash(Variant var) => Godot.GD.Hash(var);

    public static Resource Load(string path) => Godot.GD.Load(path);
    public static T Load<T>(string path) where T : class => Godot.GD.Load<T>(path);

    // Parallelized processing for parameter concatenation
    private static string AppendPrintParams(object[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return "null";

        return string.Join(string.Empty, parameters.AsParallel().Select(p => p?.ToString() ?? "null"));
    }

    private static string AppendPrintParams(char separator, object[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return "null";

        return string.Join(separator.ToString(), parameters.AsParallel().Select(p => p?.ToString() ?? "null"));
    }

    // Asynchronous print handling to prevent blocking
    private static async Task PrintAsync(string what)
    {
        await Task.Run(() =>
        {
            Godot.GD.Print(what);

            if (!IsVisualStudio2022()) return; // Skip if debugging in VS2022

            Debugger.Log(2, "inf", "GD.Log: Info: " + what + "\r\n");
        });
    }

    public static void Print(string what)
    {
        _ = PrintAsync(what); // Fire-and-forget to avoid blocking
    }

    public static void Print(params object[] what)
    {
        Print(AppendPrintParams(what));
    }

    private static async Task PrintRichAsync(string what)
    {
        await Task.Run(() =>
        {
            Godot.GD.PrintRich(what);


            if (!IsVisualStudio2022()) return; // Skip if debugging in VS2022
            Debugger.Log(2, "inf", "GD.Log: Info: " + what + "\r\n");

        });
    }

    public static void PrintRich(string what)
    {

        _ = PrintRichAsync(what); // Fire-and-forget
    }

    public static void PrintRich(params object[] what)
    {
        PrintRich(AppendPrintParams(what));
    }

    private static async Task PrintErrAsync(string what, string err, string error)
    {
        await Task.Run(() =>
        {

            Godot.GD.PrintErr(what);

            if (!IsVisualStudio2022()) return; // Skip if debugging in VS2022
            Debugger.Log(0, err + ": ", error + ": " + what + "\r\n");

        });
    }

    private static async Task PrintTAsync(string what, string err, string error)
    {
        await Task.Run(() =>
        {
            Godot.GD.PrintT(what);

            if (!IsVisualStudio2022()) return; // Skip if debugging in VS2022
            Debugger.Log(0, err + ": ", error + ": " + what + "\r\n");


        });
    }

    public static void PrintErr(string what, string err, string error)
    {
        _ = PrintErrAsync(what, err, error); // Fire-and-forget
    }

    public static void PrintErr(params object[] what)
    {
        PrintErr(AppendPrintParams(what));
    }

    private static async Task PrintRawAsync(string what)
    {
        await Task.Run(() =>
        {
            Godot.GD.PrintRaw(what);

            if (!IsVisualStudio2022()) return; // Skip if debugging in VS2022
            Debugger.Log(2, "inf", what);



        });
    }

    public static void PrintRaw(string what)
    {
        _ = PrintRawAsync(what); // Fire-and-forget
    }

    public static void PrintRaw(params object[] what)
    {
        PrintRaw(AppendPrintParams(what));
    }

    public static void PrintS(params object[] what)
    {
        Godot.GD.PrintS(what);

        if (!IsVisualStudio2022()) return; // Skip if debugging in VS2022
        string message = AppendPrintParams(' ', what);
        PrintErr(message); // Logs both to stderr and console

    }

    public static void PrintT(params object[] what)
    {
        Godot.GD.PrintT(what);

        if (!IsVisualStudio2022()) return; // Skip if debugging in VS2022
        string message = AppendPrintParams('\t', what);
        _ = PrintTAsync(message, "inf", "GD.Log: Info"); // Logs both to stderr and console

    }

    public static void PushError(string message)
    {
        Debugger.Log(0, "err", "Error: " + message);

        if (!IsVisualStudio2022()) return; // Skip if debugging in VS2022
        Godot.GD.PushError(message);
    }

    public static void PushError(params object[] what)
    {
        PushError(AppendPrintParams(what));
    }

    public static void PushWarning(string message)
    {
        Godot.GD.PushWarning(message);

        if (!IsVisualStudio2022()) return; // Skip if debugging in VS2022
        Debugger.Log(1, "wrn", "GD.Log: Warning: " + message + "\r\n");

    }

    public static void PushWarning(params object[] what)
    {
        PushWarning(AppendPrintParams(what));
    }

    public static float Randf() => Godot.GD.Randf();

    public static double Randfn(double mean, double deviation) => Godot.GD.Randfn(mean, deviation);

    public static uint Randi() => Godot.GD.Randi();

    public static void Randomize() => Godot.GD.Randomize();

    public static double RandRange(double from, double to) => Godot.GD.RandRange(from, to);

    public static int RandRange(int from, int to) => Godot.GD.RandRange(from, to);

    public static uint RandFromSeed(ref ulong seed) => Godot.GD.RandFromSeed(ref seed);

    public static IEnumerable<int> Range(int end) => Godot.GD.Range(end);

    public static IEnumerable<int> Range(int start, int end) => Godot.GD.Range(start, end);

    public static IEnumerable<int> Range(int start, int end, int step) => Godot.GD.Range(start, end, step);

    public static void Seed(ulong seed) => Godot.GD.Seed(seed);

    public static Variant StrToVar(string str) => Godot.GD.StrToVar(str);

    public static byte[] VarToBytes(Variant var) => Godot.GD.VarToBytes(var);

    public static byte[] VarToBytesWithObjects(Variant var) => Godot.GD.VarToBytesWithObjects(var);

    public static string VarToStr(Variant var) => Godot.GD.VarToStr(var);

    public static Variant.Type TypeToVariantType(Type type) => Godot.GD.TypeToVariantType(type);
}
