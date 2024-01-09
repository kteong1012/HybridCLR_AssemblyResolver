using dnlib.DotNet;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AssemblyResolverNS
{
    public class Resolver
    {
        private string _aotDir;
        private string[] _hotFiles;

        private Dictionary<string, ModuleDefMD> _aotModules;

        public Resolver(string aotDir, string[] hotFiles)
        {
            _aotDir = aotDir;
            _hotFiles = hotFiles;
        }

        public void Resolve()
        {
            Console.WriteLine($"======Starting Resolving assemblies");
            Console.WriteLine($"AOT dir: {_aotDir}");
            Console.WriteLine($"Hot files:\n{string.Join("\n", _hotFiles)}");

            var modContext = ModuleDef.CreateModuleContext();

            // 1. Load all dlls in aot dir but ones equal to hot files
            _aotModules = new Dictionary<string, ModuleDefMD>();
            var aotDir = new System.IO.DirectoryInfo(_aotDir);
            var aotFiles = aotDir.GetFiles("*.dll");
            Console.WriteLine($"——————Loading AOT dlls from '{_aotDir}'");
            foreach (var aotFile in aotFiles)
            {
                if (IsHotFile(aotFile.Name))
                {
                    continue;
                }

                var aotModule = ModuleDefMD.Load(aotFile.FullName, modContext);
                _aotModules.Add(aotModule.Name, aotModule);
                Console.WriteLine($"Loaded '{aotModule.Name}'");
            }

            // 2. Load all hot files and cache them
            Console.WriteLine($"——————Loading hot files");
            var hotModules = new System.Collections.Generic.List<ModuleDefMD>();
            foreach (var hotFile in _hotFiles)
            {
                var hotModule = ModuleDefMD.Load(hotFile, modContext);
                hotModules.Add(hotModule);
                Console.WriteLine($"Loaded '{hotModule.Name}'");
            }

            // 3. Resolve all hot modules
            foreach (var hotModule in hotModules)
            {
                ResolveTypeRefs(hotModule);
                ResolveTypeMembers(hotModule);
            }

            Console.WriteLine($"======Finished Resolving assemblies");
        }

        private void ResolveTypeRefs(ModuleDefMD hotModule)
        {
            Console.WriteLine($"——————Resolving '{hotModule.Name}'");
            var typeRefs = hotModule.GetTypeRefs();
            foreach (var typeRef in typeRefs)
            {
                if (_aotModules.TryGetValue(typeRef.DefinitionAssembly.Name + ".dll", out var aotModule))
                {
                    var aotTypeDef = aotModule.Find(typeRef.FullName, false);
                    if (aotTypeDef != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Resolved '{typeRef.FullName}' => {typeRef.DefinitionAssembly.Name}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Failed to resolve '{typeRef.FullName}' => {typeRef.DefinitionAssembly.Name}");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.WriteLine($"No need to resolve '{typeRef.FullName}' => {typeRef.DefinitionAssembly.Name}");
                }
            }
        }

        private void ResolveTypeMembers(ModuleDefMD hotModule)
        {
            Console.WriteLine($"——————Resolving '{hotModule.Name}'");
            var memberRefs = hotModule.GetMemberRefs();
            foreach (var memberRef in memberRefs)
            {
                if (!memberRef.IsMethodRef)
                {
                    continue;
                }
                if (_aotModules.TryGetValue(memberRef.DeclaringType.DefinitionAssembly.Name + ".dll", out var aotModule))
                {
                    var aotTypeDef = aotModule.Find(memberRef.DeclaringType.FullName, false);
                    if (aotTypeDef != null)
                    {
                        var aotMethodDef = aotTypeDef.FindMethod(memberRef.Name, memberRef.MethodSig);
                        if (aotMethodDef != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Resolved '{memberRef.FullName}' => {memberRef.DeclaringType.DefinitionAssembly.Name}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Failed to resolve '{memberRef.FullName}' => {memberRef.DeclaringType.DefinitionAssembly.Name}");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Failed to resolve '{memberRef.FullName}' => {memberRef.DeclaringType.DefinitionAssembly.Name}");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.WriteLine($"No need to resolve '{memberRef.FullName}' => {memberRef.DeclaringType.DefinitionAssembly.Name}");
                }
            }
        }

        private bool IsHotFile(string fileName)
        {
            foreach (var hotFile in _hotFiles)
            {
                if (Path.GetFileName(hotFile) == fileName)
                    return true;
            }

            return false;
        }
    }
}
