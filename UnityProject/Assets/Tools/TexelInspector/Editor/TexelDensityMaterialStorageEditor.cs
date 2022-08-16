using UnityEditor;

namespace TexelDensityTools
{
    [CustomEditor(typeof(MaterialReplacementStorage))]
    public class MaterialReplacementStorageEditor : Editor
    {
        private SerializedProperty _rendererLinks;
    
        void OnEnable()
        {
            _rendererLinks = serializedObject.FindProperty("RendererLinks");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField(string.Format("Storing replacement material links for {0} mesh renderer(s).", _rendererLinks.arraySize.ToString()));
            EditorGUILayout.LabelField("Delete this game object or reload the scene to restore original materials.");
        }
    }
}
