using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TexelDensityTools
{
    public static class GenerateTextureUtility
    {
        private static readonly List<Texture> GeneratedTextures = new List<Texture>();
        private static readonly Dictionary<Texture, Texture> TextureDictionary = new Dictionary<Texture, Texture>();
        private static readonly int WorldTexelColor = Shader.PropertyToID("WorldTexelColor");
        private static Gradient _mipGradient;
        private static Color[] _swatchColors;
        // TODO: Add controls to adjust these values
        private static float _maxResolution = 8192;
        private static float _minResolution = 16;
        private static bool _colorEachMipmap;
        
        private static Texture _gridTexture;
        private static Texture GridTexture
        {
            get
            {
                if (_gridTexture != null)
                    return _gridTexture;
                
                string sourceTexturePath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("WorldGrid t: Texture2D")[0]);
                _gridTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(sourceTexturePath);
                return _gridTexture;
            }
        }

        private static Material _alphaCopyShader;
        private static Material AlphaCopyShader
        {
            get
            {
                if (_alphaCopyShader != null)
                    return _alphaCopyShader;
                
                string sourceMaterialPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("CopyAlphaMaterial t: Material")[0]);
                _alphaCopyShader = AssetDatabase.LoadAssetAtPath<Material>(sourceMaterialPath);
                return _alphaCopyShader;
            }
        }

        private static Material _texelDisplayMaterial;
        private static readonly int ScaleX = Shader.PropertyToID("_ScaleX");
        private static readonly int ScaleY = Shader.PropertyToID("_ScaleY");
        private static readonly int PixelsX = Shader.PropertyToID("_PixelsX");
        private static readonly int PixelsY = Shader.PropertyToID("_PixelsY");
        private static readonly int MipLevel = Shader.PropertyToID("_MipLevel");
        private static readonly int SourcePixelsX = Shader.PropertyToID("_SourcePixelsX");
        private static readonly int SourcePixelsY = Shader.PropertyToID("_SourcePixelsY");
        private static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        private static readonly int Slice = Shader.PropertyToID("_Slice");
        private static readonly int MainTexArray = Shader.PropertyToID("_MainTexArray");

        private static Material TexelDisplayMaterial
        {
            get
            {
                if (_texelDisplayMaterial != null)
                    return _texelDisplayMaterial;
                
                string sourceMaterialPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("DensityBakeMaterial t: Material")[0]);
                _texelDisplayMaterial = AssetDatabase.LoadAssetAtPath<Material>(sourceMaterialPath);
                return _texelDisplayMaterial;
            }
        }

        private static Texture GenerateDensityTexture(Texture inTexture, int texelDensity)
        {
            int width = inTexture.width;
            int height = inTexture.height;
            Vector2 scale = new Vector2((float)width / texelDensity, (float)height / texelDensity);

            float maxMipLevel = Mathf.Log(Mathf.Max(width, height), 2);
            
            var textureAsArray = inTexture as Texture2DArray;
            if (textureAsArray != null)
            {
                Texture2DArray outputTextureArray = new Texture2DArray(width, height, textureAsArray.depth, TextureFormat.ARGB32, true);
                for (int i = 0; i < textureAsArray.depth; i++)
                {
                    for (int mipLevel = 0; mipLevel <= maxMipLevel; mipLevel++)
                    {
                        int mipDivisor = (int)Mathf.Pow(2, mipLevel);
                        int mipWidth = width / mipDivisor;
                        int mipHeight = height / mipDivisor;
                        mipWidth = Mathf.Max(mipWidth, 1);
                        mipHeight = Mathf.Max(mipHeight, 1);
                        
                        RenderTexture alphaCopy = new RenderTexture(mipWidth, mipHeight, 0);
                        AlphaCopyShader.SetFloat(Slice, i);
                        AlphaCopyShader.SetTexture(MainTexArray, inTexture);
                        Graphics.Blit(Texture2D.blackTexture, alphaCopy, AlphaCopyShader);
                        Graphics.CopyTexture(GenerateSingleTexture(alphaCopy, width, height, mipLevel, scale), 0, 0, outputTextureArray, i, mipLevel);
                    }
                }
                //GeneratedTextures.Add(outputTextureArray);
                TextureDictionary.Add(inTexture, outputTextureArray);
                return outputTextureArray;
            }

            // 非Texture2DArray方式的mipmap
            Texture2D outputTexture = new Texture2D(width, height, TextureFormat.ARGB32, true);
            for (int mipLevel = 0; mipLevel <= maxMipLevel; mipLevel++)
            {
                Graphics.CopyTexture(GenerateSingleTexture(inTexture, width, height, mipLevel, scale), 0, 0, outputTexture, 0, mipLevel);
            }
            //GeneratedTextures.Add(outputTexture);
            TextureDictionary.Add(inTexture, outputTexture);
            return outputTexture;
        }
        
        public static Texture GenerateWorldDensityTexture(int width, int height, int texelDensity)
        {
            Vector2 scale = new Vector2((float)width / texelDensity, (float)height / texelDensity);

            float maxMipLevel = Mathf.Log(Mathf.Max(width, height), 2);
            Texture2D outputTexture = new Texture2D(width, height, TextureFormat.ARGB32, true);
            for (int mipLevel = 0; mipLevel <= maxMipLevel; mipLevel++)
            {
                Graphics.CopyTexture(GenerateSingleTexture(Texture2D.blackTexture, width, height, mipLevel, scale), 0, 0, outputTexture, 0, mipLevel);
            }
            return outputTexture;
        }

        private static RenderTexture GenerateSingleTexture(Texture inTexture, int width, int height, float inMipLevel, Vector2 scale)
        {
            width = Mathf.Max(width, 1);
            height = Mathf.Max(height, 1);
            int mipDivisor = (int)Mathf.Pow(2, inMipLevel);

            // width, height是原图大小，mipmap大小需要除以对应的系数
            int mipWidth = (width / mipDivisor);
            int mipHeight = (height / mipDivisor);
            mipWidth = Mathf.Max(mipWidth, 1);
            mipHeight = Mathf.Max(mipHeight, 1);
            
            RenderTexture tempRenderTexture = new RenderTexture(mipWidth, mipHeight, 0, RenderTextureFormat.ARGB32) {useMipMap = false, wrapMode = TextureWrapMode.Repeat, autoGenerateMips = false};
            TexelDisplayMaterial.SetFloat(ScaleX, scale.x);
            TexelDisplayMaterial.SetFloat(ScaleY, scale.y);
            TexelDisplayMaterial.SetFloat(PixelsX, mipWidth);
            TexelDisplayMaterial.SetFloat(PixelsY, mipHeight);
            TexelDisplayMaterial.SetFloat(MipLevel, inMipLevel);
            TexelDisplayMaterial.SetFloat(SourcePixelsX, width);
            TexelDisplayMaterial.SetFloat(SourcePixelsY, height);
            TexelDisplayMaterial.SetTexture(InputTexture, inTexture);

            float higherResolutionSide;
            if (_colorEachMipmap)
                higherResolutionSide = Mathf.Max(mipWidth / scale.x, mipHeight / scale.y);
            else
                higherResolutionSide = Mathf.Max(width, height);

            float invertedMipLevel = Mathf.Log(higherResolutionSide, 2);
            //float highestResolutionLog2 = Mathf.Log(_maxResolution, 2);
            float lowestResolutionLog2 = Mathf.Log(_minResolution, 2);
            //float gradientValue = Mathf.Max(invertedMipLevel - lowestResolutionLog2, 0) / (highestResolutionLog2 - lowestResolutionLog2);
            Color worldTexelColor = _swatchColors[Mathf.Min(Mathf.Max((int)(invertedMipLevel - lowestResolutionLog2), 0), _swatchColors.Length - 1)];
            //Color worldTexelColor = _mipGradient.Evaluate(gradientValue);
            Shader.SetGlobalColor(WorldTexelColor, worldTexelColor);
            Graphics.Blit(GridTexture, tempRenderTexture, TexelDisplayMaterial);
            return tempRenderTexture;
        }
        
        public static Texture FindOrCreateMatchingDensityTexture(Texture textureToCheck, int texelDensity)
        {
            Texture replacementTexture;
            if (TextureDictionary.TryGetValue(textureToCheck, out replacementTexture))
            {
                return replacementTexture;
            }
            
            /*foreach (Texture generatedTexture in GeneratedTextures)
            {
                if (generatedTexture.width == textureToCheck.width && generatedTexture.height == textureToCheck.height && generatedTexture.dimension == textureToCheck.dimension)
                    return generatedTexture;
            }*/
            return GenerateDensityTexture(textureToCheck, texelDensity);
        }

        public static void SetMipGradient(Gradient inGradient, Color[] inSwatches)
        {
            _mipGradient = inGradient;
            _swatchColors = inSwatches;
        }

        public static void ColorEachMipmap(bool colorEachMipmap)
        {
            _colorEachMipmap = colorEachMipmap;
        }
        
        public static void SetMinMaxResolutionForGradient(int minResolution, int maxResolution)
        {
            _minResolution = minResolution;
            _maxResolution = maxResolution;
        }
        
        public static void ClearGeneratedTextures()
        {
            //GeneratedTextures.Clear();
            TextureDictionary.Clear();
        }
    }
}
