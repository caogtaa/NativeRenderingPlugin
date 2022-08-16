using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace TexelDensityTools
{
    [ExecuteInEditMode]
    public class MaterialReplacementStorage : MonoBehaviour
    {
        [HideInInspector]
        public MaterialRendererLink[] RendererLinks;
        public MaterialTerrainLink[] TerrainLinks;
        private readonly List<MaterialRendererLink> _tempRenderingLinks = new List<MaterialRendererLink>();
        private readonly List<MaterialTerrainLink> _tempTerrainLinks = new List<MaterialTerrainLink>();

        public void OnEnable()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += DestroySelf;
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosing += DestroySelf;
            EditorApplication.playModeStateChanged += DestroySelf;
        }

        public void OnDestroy()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= DestroySelf;
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosing -= DestroySelf;
            EditorApplication.playModeStateChanged -= DestroySelf;
            UnlinkRenderers();
        }
        
        private void DestroySelf(PlayModeStateChange playModeStateChange)
        {
            DestroyImmediate(gameObject);
        }

        private void DestroySelf(Scene scene, bool removingscene)
        {
            DestroyImmediate(gameObject);
        }

        private void DestroySelf(Scene scene, string path)
        {
            DestroyImmediate(gameObject);
        }

        public void UnlinkRenderers()
        {
            if (RendererLinks == null)
                return;
            
            foreach (var rendererLink in RendererLinks)
            {
                if (rendererLink.Renderer == null)
                    continue;
                
                if (rendererLink.Material != null)
                {
                    rendererLink.Renderer.sharedMaterial = rendererLink.Material;
                }
                else if (rendererLink.Materials != null && rendererLink.Materials.Length > 0)
                {
                    var tempMaterialArray = rendererLink.Renderer.sharedMaterials;
                    for (var i = 0; i < tempMaterialArray.Length; i++)
                    {
                        tempMaterialArray[i] = rendererLink.Materials[i];
                    }
                    rendererLink.Renderer.sharedMaterials = tempMaterialArray;
                }
            }
            RendererLinks = null;

            foreach (var terrainLink in TerrainLinks)
            {
                if (terrainLink.Terrain == null)
                    continue;

                if (terrainLink.Material != null)
                {
                    terrainLink.Terrain.materialTemplate = terrainLink.Material;
                }
            }
            TerrainLinks = null;
        }
        
        // 执行材质替换
        public void LinkRenderer(MeshRenderer meshRenderer, int texelsPerMeter)
        {
            if (meshRenderer.sharedMaterials.Length > 1)
            {
                _tempRenderingLinks.Add(new MaterialRendererLink {Renderer = meshRenderer, Materials = meshRenderer.sharedMaterials});
                Material[] tempMaterialArray = meshRenderer.sharedMaterials;
                for (var i = 0; i < meshRenderer.sharedMaterials.Length; i++)
                {
                    //_materialsInScene.Add(meshRenderer.sharedMaterials[i]);
                    Material displayMaterial = MaterialModifierUtility.GetReplacementMaterial(meshRenderer.sharedMaterials[i], texelsPerMeter);
                    if (displayMaterial != null)
                        tempMaterialArray[i] = displayMaterial;
                }
                meshRenderer.sharedMaterials = tempMaterialArray;
            }
            else
            {
                //_materialsInScene.Add(meshRenderer.sharedMaterial);
                _tempRenderingLinks.Add(new MaterialRendererLink {Renderer = meshRenderer, Material = meshRenderer.sharedMaterial});
                Material displayMaterial = MaterialModifierUtility.GetReplacementMaterial(meshRenderer.sharedMaterial, texelsPerMeter);
                if (displayMaterial != null)
                    meshRenderer.sharedMaterial = displayMaterial;
            }
        }

        public void ReplaceWithCalibrationMaterial(MeshRenderer meshRenderer) {
            Material calibrationMaterial = GenerateTextureUtility.CalibrationMaterial;
            if (meshRenderer.sharedMaterials.Length > 1) {
                _tempRenderingLinks.Add(new MaterialRendererLink { Renderer = meshRenderer, Materials = meshRenderer.sharedMaterials });
                Material[] tempMaterialArray = meshRenderer.sharedMaterials;
                for (var i = 0; i < meshRenderer.sharedMaterials.Length; i++) {
                    //_materialsInScene.Add(meshRenderer.sharedMaterials[i]);
                    Material displayMaterial = calibrationMaterial;
                    if (displayMaterial != null)
                        tempMaterialArray[i] = displayMaterial;
                }
                meshRenderer.sharedMaterials = tempMaterialArray;
            } else {
                //_materialsInScene.Add(meshRenderer.sharedMaterial);
                _tempRenderingLinks.Add(new MaterialRendererLink { Renderer = meshRenderer, Material = meshRenderer.sharedMaterial });
                Material displayMaterial = calibrationMaterial;
                if (displayMaterial != null)
                    meshRenderer.sharedMaterial = displayMaterial;
            }
        }

        public void LinkTerrain(Terrain terrain, int texelsPerMeter)
        {
            _tempTerrainLinks.Add(new MaterialTerrainLink() {Terrain = terrain, Material = terrain.materialTemplate});
            Material displayMaterial = MaterialModifierUtility.GetReplacementMaterial(terrain.materialTemplate, texelsPerMeter);
            if (displayMaterial != null)
                terrain.materialTemplate = displayMaterial;
        }

        public void SerializeMaterialReplacements()
        {
            RendererLinks = _tempRenderingLinks.ToArray();
            TerrainLinks = _tempTerrainLinks.ToArray();
        }
    }

    [Serializable]
    public struct MaterialRendererLink
    {
        public Material Material;
        public Material[] Materials;
        public MeshRenderer Renderer;
    }
    
    [Serializable]
    public struct MaterialTerrainLink
    {
        public Material Material;
        public Material[] Materials;
        public Terrain Terrain;
    }
}