using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TexelDensityTools
{
    public class TexelInspector : EditorWindow
    {
        private static int _texelsPerUnit = 1024;   // 512;

        private bool _rendererReplaced;
        private static float _metersPerUnit = 1;
        private static int _checkersPerUnit = 4;
        private static Gradient _mipGradient;
        private static float _indicatorScalar = 1;
        private static Color[] _colorSwatches = {
            Color.magenta,
            new Color(178f/255, 0, 1),          // purple
            Color.blue,
            new Color(6f/255, 164f/255, 235f/255),          // another blue
            Color.green,
            new Color(249f/255, 242f/255, 44f/255),         // light yellow
            Color.red,
            Color.white,
            Color.grey,
            new Color(160f/255, 238f/255, 239f/255),        // light blue
        };
    
        private static bool _colorEachMipmap = true;

        private static GUIContent titleText = new GUIContent("Texel Density");
        private static GUIContent baseUnitLabel = new GUIContent("Base Unit in Meters:", 
            "The size of the unit to measure against, in meters. If this is set to 2, and Texels Per Base Unit is set to 1024, every 2 meters will contain" +
            "1024 pixels of texture data. \n\nOnly visible in \"Show Uniform Density\" mode, has no effect in \"Show Actual Density\" mode.");
        private static GUIContent texelsPerUnitLabel = new GUIContent("Texels Per Base Unit:", 
            "The desired texel count across a base unit.");
        private static GUIContent checkersPerUnitLabel = new GUIContent("Checkers Per Base Unit:", 
            "Controls the number of the smaller dark and light checkers per base unit. Can be set to 1 for no checkers.");
        private static GUIContent textureSizeGradient = new GUIContent("Texture Size Gradient:", 
            "This gradient is used for generating the texture-resolution colors used by this tool, from 16px on the left to 8192px on the right. Mipmaps " +
            "can also be colored based on their mipped resolution, not the base resolution of the texture.\n\nThis allows for easy texel density comparison between " +
            "the Uniform Density and Actual Density views. If the colors are similar, the texel density is close.");
        private static GUIContent ColorEachMipmap = new GUIContent("Color Each Mipmap:", 
            "If this option is disabled, the mipmaps of the Actual Density mode will all be colored based on the base texture size. This mode is useful " +
            "for seeing absolute texture sizes across an environment.\n\nIf this option is enabled each mipmap of each texture will be colored according to its " +
            "actual size. E.g., the first mip of a 1024x1024 texture will be colored as a 512x512 texture. This mode is useful for comparing texel densities " +
            "against each other and is the default setting.");
        // TODO: Implement this, figure out how/if it should handle terrain
        /*private static GUIContent overrideMaterials = new GUIContent("Override Materials:", 
            "If this is enabled, the tool will replace all materials with a basic slightly emissive material, rather than just replacing textures in the" +
            " existing materials.\n\nThis can be easier to parse, avoid some visual glitches with certain shaders, and should work for standard shaders or shaders " +
            "that use the default texture scale parameters, but has the downside of not properly representing the texel density if the shader it is replacing is " +
            "using custom texture tiling or different UV sets.");*/
        
        // Cached property IDs
        private static readonly int PixelsPerMeter = Shader.PropertyToID("PixelsPerMeter");
        private static readonly int MetersPerUnit = Shader.PropertyToID("MetersPerUnit");
        private static readonly int WorldGrid = Shader.PropertyToID("WorldGrid");
        private static readonly int UnitCheckers = Shader.PropertyToID("UnitCheckers");
        private static readonly int IndicatorScalar = Shader.PropertyToID("IndicatorScalar");
        //private RenderPipelineEnum _renderPipeline;
        private static OcclusionQueryRunner _runner = new OcclusionQueryRunner();

        private static Gradient MipGradient
        {
            get
            {
                if (_mipGradient != null)
                    return _mipGradient;
                
                _mipGradient = new Gradient();
                GradientColorKey[] colorKeys = {
                    new GradientColorKey(Color.magenta, 0),
                    new GradientColorKey(Color.blue, 0.2f),
                    new GradientColorKey(Color.cyan, 0.4f),
                    new GradientColorKey(Color.green, 0.6f),
                    new GradientColorKey(Color.yellow, 0.8f),
                    new GradientColorKey(Color.red, 1f)
                };

                GradientAlphaKey[] alphaKeys = {
                    new GradientAlphaKey(1, 0), 
                    new GradientAlphaKey(1, 1) 
                };
                _mipGradient.SetKeys(colorKeys, alphaKeys);
                // TransferGradientToSwatches();
                // GenerateTextureUtility.SetMipGradient(_mipGradient, _colorSwatches);
                return _mipGradient;
            }
            set
            {
                _mipGradient = value;
                GenerateTextureUtility.SetMipGradient(_mipGradient, _colorSwatches);
            }
        }

        [MenuItem("Window/Analysis/纹素密度分析(Texel Inspector)", false, 1500)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            TexelInspector window = (TexelInspector) GetWindow(typeof(TexelInspector));
            UpdateShaderGlobals();
            MipGradient = MipGradient;
            // TransferGradientToSwatches();
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = titleText;
        }

        private void OnDestroy()
        {
            DisableRendererOverride();
            RestoreAllMaterials();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            _metersPerUnit = EditorGUILayout.FloatField(baseUnitLabel, _metersPerUnit);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _texelsPerUnit = EditorGUILayout.IntField(texelsPerUnitLabel, _texelsPerUnit);
            EditorGUILayout.EndHorizontal();
            _checkersPerUnit = Mathf.Max(EditorGUILayout.IntField(checkersPerUnitLabel, _checkersPerUnit), 1);
            EditorGUILayout.LabelField(string.Format("Each checker is a {0} x {0} pixel square.", (_texelsPerUnit / Mathf.Max(_checkersPerUnit, 1)).ToString()));
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            MipGradient = EditorGUILayout.GradientField(textureSizeGradient, MipGradient);
            if (GUILayout.Button("Transfer Gradient To Swatches"))
            {
                TransferGradientToSwatches();
                UpdateShaderGlobals();
            }

            // https://answers.unity.com/questions/1443617/custom-editor-guilayoutwidth-isnt-working-when-usi.html
            // Don't control GUI width by variant, use MinWidth() + ExpandWidth() instead
            // int ColorSwatchWidth = (Screen.width - 18) / 5;
            const int ColorSwatchWidth = 20;
            
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < 5; i++)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(string.Format("{0}px", Mathf.Pow(2, i + 4)), GUILayout.MinWidth(ColorSwatchWidth), GUILayout.ExpandWidth(true));
                _colorSwatches[i] = EditorGUILayout.ColorField(GUIContent.none, _colorSwatches[i], false, false, false, GUILayout.MinWidth(ColorSwatchWidth), GUILayout.ExpandWidth(true));
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            for (int i = 5; i < 10; i++)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(string.Format("{0}px", Mathf.Pow(2, i + 4)), GUILayout.MinWidth(ColorSwatchWidth), GUILayout.ExpandWidth(true));
                _colorSwatches[i] = EditorGUILayout.ColorField(GUIContent.none, _colorSwatches[i], false, false, false, GUILayout.MinWidth(ColorSwatchWidth), GUILayout.ExpandWidth(true));
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            _colorEachMipmap = EditorGUILayout.Toggle(ColorEachMipmap, _colorEachMipmap);
            
            //_overrideMaterials = EditorGUILayout.Toggle(overrideMaterials, _overrideMaterials);
            
            if (EditorGUI.EndChangeCheck())
            {
                UpdateShaderGlobals();
                GenerateTextureUtility.ClearGeneratedTextures();
                MaterialModifierUtility.ClearGeneratedMaterials();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Show Uniform Density"))
            {
                _rendererReplaced = true;
                UpdateShaderGlobals();
                SceneView.lastActiveSceneView.SetSceneViewShaderReplace(Shader.Find("Hidden/WorldTexelDensity"), "");
            }
            
            if (GUILayout.Button("Show Actual Density"))
            {
                GenerateTextureUtility.ColorEachMipmap(_colorEachMipmap);
                DisplayInAllScenes();
            }
            if (GUILayout.Button("替换为测量图")) {
                ApplyCalibrationMaterial();
            }
            if (GUILayout.Button("Hide All Displays"))
            {
                DisableRendererOverride();
                RestoreAllMaterials();
            }

            if (GUILayout.Button("测量")) {
                DoQuerySomeGameObject();
            }
            //_renderPipeline = (RenderPipelineEnum)EditorGUILayout.EnumPopup("Render Pipeline", _renderPipeline);
        }

        private void DisplayInAllScenes()
        {
            
            UpdateShaderGlobals();
            DisableRendererOverride();
            RestoreAllMaterials();

            GenerateTextureUtility.ClearGeneratedTextures();
            MaterialModifierUtility.ClearGeneratedMaterials();
            var currentActiveScene = SceneManager.GetActiveScene();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                // 每个scene挂一个storage对象，保存被替换的renderer和原材质关系
                SceneManager.SetActiveScene(SceneManager.GetSceneAt(i));
                GameObject storageObject = new GameObject("TexelDensityDataStorageObject") {tag = "EditorOnly"};
                MaterialReplacementStorage materialReplacementStorage = storageObject.AddComponent<MaterialReplacementStorage>();
                LinkRenderers(materialReplacementStorage, SceneManager.GetSceneAt(i).GetRootGameObjects());
            }

            SceneManager.SetActiveScene(currentActiveScene);
        }

        public static GameObject FindObject(GameObject parent, string name) {
            Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in trs) {
                if (t.name == name) {
                    return t.gameObject;
                }
            }
            return null;
        }

        public static GameObject FindObjectInRootGOs(GameObject[] rootGOs, string name) {
            foreach (var go in rootGOs) {
                if (go.name.Equals(name)) {
                    return go;
                }

                var child = FindObject(go, name);
                if (child != null)
                    return child;
            }

            return null;
        }

        private void DoQuerySomeGameObject() {
            var scene = SceneManager.GetActiveScene();
            var gos = scene.GetRootGameObjects();
            var cameraGO = FindObjectInRootGOs(gos, "OcclusionQueryCamera");
            var rendererGO = FindObjectInRootGOs(gos, "Sphere");
            if (cameraGO == null || rendererGO == null)
                return;

            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/Settings/OcclusionQueryRT.renderTexture");
            if (rt == null)
                return;

            _runner.Setup(cameraGO.GetComponent<Camera>(), rt);
            int result = _runner.QuerySingleRenderer(rendererGO.GetComponent<MeshRenderer>());
            Debug.Log($"{rendererGO.name}使用mipLevle = {result}");
        }

        private void ApplyCalibrationMaterial() {
            UpdateShaderGlobals();
            DisableRendererOverride();
            RestoreAllMaterials();

            GenerateTextureUtility.ClearGeneratedTextures();
            MaterialModifierUtility.ClearGeneratedMaterials();
            var currentActiveScene = SceneManager.GetActiveScene();
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                SceneManager.SetActiveScene(SceneManager.GetSceneAt(i));
                GameObject storageObject = new GameObject("TexelDensityDataStorageObject") { tag = "EditorOnly" };
                MaterialReplacementStorage materialReplacementStorage = storageObject.AddComponent<MaterialReplacementStorage>();
                ReplaceWithCalibrationMaterial(materialReplacementStorage, SceneManager.GetSceneAt(i).GetRootGameObjects());
            }

            SceneManager.SetActiveScene(currentActiveScene);
        }

        private static void TransferGradientToSwatches()
        {
            for (var index = 0; index < _colorSwatches.Length; index++)
            {
                _colorSwatches[index] = MipGradient.Evaluate((float) index / (_colorSwatches.Length - 1));
            }
        }

        private static void UpdateShaderGlobals()
        {
            GenerateTextureUtility.SetMipGradient(MipGradient, _colorSwatches);
            GenerateTextureUtility.ColorEachMipmap(_colorEachMipmap);
            Shader.SetGlobalInt(PixelsPerMeter, _texelsPerUnit);
            Shader.SetGlobalFloat(MetersPerUnit, _metersPerUnit);
            Shader.SetGlobalFloat(IndicatorScalar, _indicatorScalar);
            Shader.SetGlobalInt(UnitCheckers, _checkersPerUnit);
            GenerateAndSetTargetTexelTexture();
            GenerateTextureUtility.GenerateAndSetCalibrationTexture();
        }
        
        private static void GenerateAndSetTargetTexelTexture()
        {
            Shader.SetGlobalFloat(IndicatorScalar, 2);
            Texture worldGridTexture = GenerateTextureUtility.GenerateWorldDensityTexture(_texelsPerUnit, _texelsPerUnit, _texelsPerUnit);
            Shader.SetGlobalTexture(WorldGrid, worldGridTexture);
            Shader.SetGlobalFloat(IndicatorScalar, _indicatorScalar);
        }

        private void DisableRendererOverride()
        {
            if (_rendererReplaced)
            {
                _rendererReplaced = false;
                SceneView.lastActiveSceneView.SetSceneViewShaderReplace(null, "");
            }
        }

        private static MaterialReplacementStorage[] FindStorageContainers()
        {
            return FindObjectsOfType<MaterialReplacementStorage>();
        }

        private static void RestoreAllMaterials()
        {
            var storageContainers = FindStorageContainers();
            if (storageContainers != null && storageContainers.Length > 0)
            {
                foreach (MaterialReplacementStorage storageContainer in storageContainers)
                {
                    storageContainer.UnlinkRenderers();
                    DestroyImmediate(storageContainer.gameObject);
                }
            }
        }
        
        private static void LinkRenderers(MaterialReplacementStorage replacementStorageObject, GameObject[] rootGameObjects)
        {
            try
            {
                for (int i = 0; i < rootGameObjects.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Finding Renderers",
                        string.Format("{0} out of {1} root game objects searched.", i, rootGameObjects.Length), (float)i / rootGameObjects.Length);
                    MeshRenderer[] meshRenderers = rootGameObjects[i].GetComponentsInChildren<MeshRenderer>();
                    for (var index = 0; index < meshRenderers.Length; index++)
                    {
                        replacementStorageObject.LinkRenderer(meshRenderers[index], _texelsPerUnit);
                    }
                    
                    Terrain[] terrains = rootGameObjects[i].GetComponentsInChildren<Terrain>();
                    for (var index = 0; index < terrains.Length; index++)
                    {
                        replacementStorageObject.LinkTerrain(terrains[index], _texelsPerUnit);
                    }
                }
                EditorUtility.ClearProgressBar();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!",string.Format("Something unexpected happened while trying to replace materials. Error message is <{0}> <{1}>", e.Message, e.StackTrace), "Ok");
                EditorUtility.ClearProgressBar();
                throw;
            }
            replacementStorageObject.SerializeMaterialReplacements();
        }

        // 
        private static void ReplaceWithCalibrationMaterial(MaterialReplacementStorage replacementStorageObject, GameObject[] rootGameObjects) {
            try {
                for (int i = 0; i < rootGameObjects.Length; i++) {
                    EditorUtility.DisplayProgressBar("Finding Renderers",
                        string.Format("{0} out of {1} root game objects searched.", i, rootGameObjects.Length), (float)i / rootGameObjects.Length);
                    MeshRenderer[] meshRenderers = rootGameObjects[i].GetComponentsInChildren<MeshRenderer>();
                    for (var index = 0; index < meshRenderers.Length; index++) {
                        replacementStorageObject.ReplaceWithCalibrationMaterial(meshRenderers[index]);
                    }

                    // 不管地形，项目里没有
                    //Terrain[] terrains = rootGameObjects[i].GetComponentsInChildren<Terrain>();
                    //for (var index = 0; index < terrains.Length; index++) {
                    //    replacementStorageObject.LinkTerrain(terrains[index], _texelsPerUnit);
                    //}
                }
                EditorUtility.ClearProgressBar();
            } catch (Exception e) {
                EditorUtility.DisplayDialog("Error!", string.Format("Something unexpected happened while trying to replace materials. Error message is <{0}> <{1}>", e.Message, e.StackTrace), "Ok");
                EditorUtility.ClearProgressBar();
                throw;
            }
            replacementStorageObject.SerializeMaterialReplacements();
        }
    }

    enum RenderPipelineEnum
    {
        Builtin,
        URP,
        HDRP
    }
}
