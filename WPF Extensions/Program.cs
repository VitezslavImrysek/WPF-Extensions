using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace WPF_Extensions
{
    class Program
    {
        static void Main(string[] args)
        {
            RoslynGenerateAssembly();
            CecilModifyMethods();
        }

        private static void RoslynGenerateAssembly()
        {
            var metadataReferences = new[]
            {
                typeof(Expression).GetTypeInfo().Assembly,        // WindowsBase.dll
                typeof(UIElement).GetTypeInfo().Assembly,         // PresentationCore.dll
                typeof(FrameworkElement).GetTypeInfo().Assembly,  // PresentationFramework.dll
                typeof(object).GetTypeInfo().Assembly,            // mscorlib.dll
            }.Select(x => MetadataReference.CreateFromFile(x.Location)).ToList();

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).
                WithMetadataImportOptions(MetadataImportOptions.All);

            var topLevelBinderFlagsProperty = typeof(CSharpCompilationOptions).GetProperty("TopLevelBinderFlags", BindingFlags.Instance | BindingFlags.NonPublic);
            topLevelBinderFlagsProperty.SetValue(compilationOptions, (uint)1 << 22);

            var compilation = CSharpCompilation.Create(
                "WPFExtensions",
                GatherCodeCompilationUnits(),
                metadataReferences,
                compilationOptions);

            var cr = compilation.Emit("WPFExtensionsTmp.dll");
        }

        private static void CecilModifyMethods()
        {
            using (Stream stream = File.OpenRead("WPFExtensionsTmp.dll"))
            {
                AssemblyDefinition asmDefinition = AssemblyDefinition.ReadAssembly(stream);
                ModuleDefinition module = asmDefinition.MainModule;
                TypeDefinition expressionBaseType = module.Types.FirstOrDefault(definition => definition.FullName.Contains("ExpressionBase"));

                AssemblyDefinition gacDefinition = GetWindowsBaseDefinition();
                TypeDefinition expressionType = gacDefinition.MainModule.Types.FirstOrDefault(definition => definition.FullName == typeof(Expression).FullName);

                expressionBaseType.BaseType = module.ImportReference(typeof(Expression));

                Collection<MethodDefinition> methods = expressionBaseType.Methods;
                MethodDefinition getSourcesMethod = methods.FirstOrDefault(x => x.Name == "GetSources");
                MethodDefinition copyMethod = methods.FirstOrDefault(x => x.Name == "Copy");
                MethodDefinition getValueMethod = methods.FirstOrDefault(x => x.Name == "GetValue");
                MethodDefinition setValueMethod = methods.FirstOrDefault(x => x.Name == "SetValue");
                MethodDefinition onAttachMethod = methods.FirstOrDefault(x => x.Name == "OnAttach");
                MethodDefinition onDetachMethod = methods.FirstOrDefault(x => x.Name == "OnDetach");
                MethodDefinition onPropertyInvalidationMethod = methods.FirstOrDefault(x => x.Name == "OnPropertyInvalidation");
                MarkMethodAsInternal(getSourcesMethod);
                MarkMethodAsVirtual(getSourcesMethod);
                MarkMethodAsOverride(getSourcesMethod);
                MarkMethodAsOverride(copyMethod);
                MarkMethodAsOverride(getValueMethod);
                MarkMethodAsOverride(setValueMethod);
                MarkMethodAsOverride(onAttachMethod);
                MarkMethodAsOverride(onDetachMethod);
                MarkMethodAsOverride(onPropertyInvalidationMethod);

                asmDefinition.Write("WPFExtensions.dll");
            }

            File.Delete("WPFExtensionsTmp.dll");
        }

        private static SyntaxTree[] GatherCodeCompilationUnits()
        {
            return new SyntaxTree[]
            {
                CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "DpiHelper.cs"))),
                CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "ExpressionBase.cs")))
            };
        }

        private static AssemblyDefinition GetWindowsBaseDefinition()
        {
            AssemblyDefinition asmDefinition;
            string location = typeof(Expression).Assembly.Location;
            using (Stream stream = File.OpenRead(location))
            {
                asmDefinition = AssemblyDefinition.ReadAssembly(stream);
            }

            return asmDefinition;
        }

        private static void MarkMethodAsInternal(MethodDefinition method)
        {
            method.IsPrivate = false;
            method.IsAssembly = true;
        }

        private static void MarkMethodAsVirtual(MethodDefinition method)
        {
            method.IsVirtual = true;
        }

        private static void MarkMethodAsOverride(MethodDefinition method)
        {
            method.IsNewSlot = false;
            method.IsReuseSlot = true;
        }
    }
}
