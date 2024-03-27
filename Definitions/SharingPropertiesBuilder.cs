// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

namespace SharingService
{
    public static class SharingPropertiesBuilder
    {
        public static void CreatePropertiesEnumFromConstants(List<string> customProperties)
        {
            Type sharableStringsType = typeof(SharableStrings);
            var fields = sharableStringsType.GetFields(BindingFlags.Public | BindingFlags.Static);
            
            // Step 2: Create an assembly
            AssemblyName assemblyName = new AssemblyName("DynamicSharingPropertiesEnumAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            EnumBuilder enumBuilder = moduleBuilder.DefineEnum("SharingPropertiesEnum", TypeAttributes.Public, typeof(int));
            
            int value = 0; // Enum values, typically starting at 0
            foreach (var field in fields)
            {
                enumBuilder.DefineLiteral(field.Name, value++);
            }

            foreach (var customProperty in customProperties)
            {
                enumBuilder.DefineLiteral(customProperty, value++);
            }

            // Step 5: Complete the type
            Type enumType = enumBuilder.CreateTypeInfo();
            string enumSourceCode = GenerateEnumSourceCode(enumType.Name, Enum.GetNames(enumType));
            string filePath = Path.Combine(Application.dataPath,"SharingService", enumType.Name + ".cs");

            // Write the source code to the file
            File.WriteAllText(filePath, enumSourceCode);

            // Refresh the AssetDatabase to include the new file in Unity's project structure
            AssetDatabase.Refresh();
        }
        
        private static string GenerateEnumSourceCode(string enumName, string[] names)
        {
            string namespaceName = "SharingService"; // Change this to your actual namespace

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendLine("namespace " + namespaceName);
            builder.AppendLine("{");
            builder.AppendLine("    public enum " + enumName);
            builder.AppendLine("    {");

            for (int i = 0; i < names.Length; i++)
            {
                builder.AppendLine($"        {names[i]} = {i},");
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
#endif
