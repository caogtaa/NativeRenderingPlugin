using System.Collections.Generic;
using UnityEngine;

namespace TexelDensityTools
{
    public static class MaterialModifierUtility
    {
        private static readonly Dictionary<Material, Material> GeneratedReplacementMaterials;
        
        private static readonly string[] PreferredStrings = new[] {"albedo", "diffuse", "base", "color", "main", "emission"};
        private static readonly string[] BannedStrings = new[] {"metallic", "ao", "ambient", "bump", "normal", "nrm", "detail"};
        
        private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");
        private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
        private static readonly int Metallic = Shader.PropertyToID("_Metallic");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        static MaterialModifierUtility()
        {
            GeneratedReplacementMaterials = new Dictionary<Material, Material>();
        }
        
        public static Material GetReplacementMaterial(Material sceneMaterial, int texelsPerMeter)
        {
            if (sceneMaterial == null)
                return null;
            
            // Check to see if we've already generated a replacement material for this material and use that one if so.
            Material texelDisplayMaterial;
            if (GeneratedReplacementMaterials.TryGetValue(sceneMaterial, out texelDisplayMaterial))
            {
                return texelDisplayMaterial;
            }

            // 直接复制一份原材质，并拷贝属性
            // 这样可以方便考虑到原材质的tileoffset。在目前项目大量使用lit shader并且没有detail的情况下没有什么必要
            Material newTexelDisplayMaterial = new Material(sceneMaterial.shader) {name = string.Format("{0}_TexelDisplay", sceneMaterial.name)};
            newTexelDisplayMaterial.CopyPropertiesFromMaterial(sceneMaterial);
            // Look for a few default material values we can set to better defaults
            SetDefaultValues(newTexelDisplayMaterial);
            var textureProperties = newTexelDisplayMaterial.GetTexturePropertyNames();
            var texturePropertiesToReplace = GetTexturePropertiesToReplace(sceneMaterial, textureProperties);
            //Early out if we can't find any texture properties, or can't find any texture properties suitable for replacement
            if (textureProperties.Length < 1 || texturePropertiesToReplace.Count < 1)
                return null;
            
            foreach (string textureProperty in textureProperties)
            {
                if (textureProperty.ToLower().Contains("splat"))
                {
                    // Don't replace splat maps at all, they'll be needed
                    continue;
                }
                if (texturePropertiesToReplace.Contains(textureProperty))
                {
                    Texture textureToReplace = newTexelDisplayMaterial.GetTexture(textureProperty);
                    newTexelDisplayMaterial.SetTexture(textureProperty,GenerateTextureUtility.FindOrCreateMatchingDensityTexture(textureToReplace, texelsPerMeter));
                }
                // This section causes materials in the URP and the HDRP to not render correctly unless the material is unfolded in the inspector. Odd.
                /*else
                {
                    // Clear all other textures to make the density texture easier to see
                    //newTexelDisplayMaterial.SetTexture(textureProperty, null);
                }*/
            }
            GeneratedReplacementMaterials.Add(sceneMaterial, newTexelDisplayMaterial);
            return newTexelDisplayMaterial;
        }

        private static void SetDefaultValues(Material newTexelDisplayMaterial)
        {
            if (newTexelDisplayMaterial.HasProperty(ColorId))
                newTexelDisplayMaterial.SetColor(ColorId, Color.white);
            if (newTexelDisplayMaterial.HasProperty(BaseColor))
                newTexelDisplayMaterial.SetColor(BaseColor, Color.white);
            if (newTexelDisplayMaterial.HasProperty(Glossiness))
                newTexelDisplayMaterial.SetFloat(Glossiness, 0);
            if (newTexelDisplayMaterial.HasProperty(Smoothness))
                newTexelDisplayMaterial.SetFloat(Smoothness, 0);
            if (newTexelDisplayMaterial.HasProperty(Metallic))
                newTexelDisplayMaterial.SetFloat(Metallic, 0);
        }

        private static List<string> GetTexturePropertiesToReplace(Material material, IEnumerable<string> propertyNames)
        {
            var propertiesToChange = new List<string>();
            foreach (string propertyName in propertyNames)
            {
                if (PropertyNameHasPreferredString(propertyName) && !PropertyNameHasBannedString(propertyName))
                {
                    Texture textureToCheck = material.GetTexture(propertyName);
                    if (textureToCheck == null || textureToCheck is RenderTexture || textureToCheck is Texture3D)
                        continue;
                    propertiesToChange.Add(propertyName);
                }
            }

            if (propertiesToChange.Count > 0)
                return propertiesToChange;

            foreach (string propertyName in propertyNames)
            {
                if (PropertyNameHasBannedString(propertyName))
                    continue;
                
                Texture textureToCheck = material.GetTexture(propertyName);
                if (textureToCheck == null || textureToCheck is RenderTexture || textureToCheck is Texture3D)
                    continue;
                            
                propertiesToChange.Add(propertyName);
            }
            return propertiesToChange;
        }

        private static bool PropertyNameHasPreferredString(string propertyName)
        {
            foreach (string preferredString in PreferredStrings)
            {
                if (propertyName.ToLower().Contains(preferredString))
                    return true;    
            }
            return false;
        }

        private static bool PropertyNameHasBannedString(string propertyName)
        {
            foreach (string bannedString in BannedStrings)
            {
                if (propertyName.ToLower().Contains(bannedString))
                    return true;    
            }
            return false;
        }
        
        public static void ClearGeneratedMaterials()
        {
            GeneratedReplacementMaterials.Clear();
        }
    }
}