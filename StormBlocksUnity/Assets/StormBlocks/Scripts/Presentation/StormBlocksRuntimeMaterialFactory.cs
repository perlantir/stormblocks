using System;
using UnityEngine;

namespace StormBlocks.Presentation
{
    internal static class StormBlocksRuntimeMaterialFactory
    {
        private static readonly string[] BuiltInMaterialFallbacks =
        {
            "Default-Material.mat",
            "Sprites-Default.mat",
            "Default-Line.mat"
        };

        private static bool _reportedShaderFallback;

        public static Material Create(string materialName, Color color, params string[] shaderNames)
        {
            Shader shader = FindShader(shaderNames);
            Material material = new Material(shader);
            material.name = materialName;
            material.color = color;
            ApplyColorProperties(material, color);
            return material;
        }

        private static Shader FindShader(string[] shaderNames)
        {
            if (shaderNames != null)
            {
                for (int i = 0; i < shaderNames.Length; i++)
                {
                    string shaderName = shaderNames[i];
                    if (string.IsNullOrEmpty(shaderName))
                    {
                        continue;
                    }

                    Shader shader = Shader.Find(shaderName);
                    if (shader != null)
                    {
                        return shader;
                    }
                }
            }

            for (int i = 0; i < BuiltInMaterialFallbacks.Length; i++)
            {
                var builtIn = Resources.GetBuiltinResource<Material>(BuiltInMaterialFallbacks[i]);
                if (builtIn != null && builtIn.shader != null)
                {
                    ReportFallback(shaderNames, builtIn.shader.name);
                    return builtIn.shader;
                }
            }

            throw new InvalidOperationException("Storm Blocks could not find a runtime shader or built-in fallback material.");
        }

        private static void ApplyColorProperties(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private static void ReportFallback(string[] requestedShaders, string fallbackName)
        {
            if (_reportedShaderFallback)
            {
                return;
            }

            _reportedShaderFallback = true;
            string requested = requestedShaders == null ? "none" : string.Join(", ", requestedShaders);
            Debug.LogWarning("Storm Blocks runtime shader fallback used. Requested [" + requested + "], fallback [" + fallbackName + "].");
        }
    }
}
