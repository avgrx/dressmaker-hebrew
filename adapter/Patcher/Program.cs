using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

var resolver = new DefaultAssemblyResolver();
resolver.AddSearchDirectory(args[0]);
resolver.AddSearchDirectory(System.IO.Path.GetDirectoryName(args[1]));
var settings = new ReaderParameters { AssemblyResolver = resolver };
using var tmp = AssemblyDefinition.ReadAssembly(args[3], settings);
using var helper = AssemblyDefinition.ReadAssembly(args[1], settings);
if (tmp.MainModule.AssemblyReferences.Any(x => x.Name == "DressmakerHebrew")) throw new Exception("Already patched");
var method = tmp.MainModule.Types.Single(t => t.FullName == "TMPro.TMP_Text").Methods.Single(m =>
    m.Name == "PopulateTextBackingArray" && m.Parameters.Count == 3 && m.Parameters[0].ParameterType.FullName == "System.String");
var prepare = tmp.MainModule.ImportReference(helper.MainModule.Types.Single(t => t.Name == "HebrewRuntime").Methods.Single(m => m.Name == "Prepare"));
var il = method.Body.GetILProcessor();
var first = method.Body.Instructions[0];
foreach (var instruction in new[] {
    il.Create(OpCodes.Ldarg_0), il.Create(OpCodes.Ldarg_1),
    il.Create(OpCodes.Ldarga, method.Parameters[1]), il.Create(OpCodes.Ldarga, method.Parameters[2]),
    il.Create(OpCodes.Call, prepare), il.Create(OpCodes.Starg, method.Parameters[0])
}) il.InsertBefore(first, instruction);
tmp.Write(args[2]);
using var check = AssemblyDefinition.ReadAssembly(args[2], settings);
if (!check.MainModule.AssemblyReferences.Any(x => x.Name == "DressmakerHebrew")) throw new Exception("Patch verification failed");
Console.WriteLine("Patched TMP string ingestion; original game DLL untouched.");
