using System.Collections;
using System.Collections.Generic;
using System;
using TexelDensityTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class OcclusionQueryUI : MonoBehaviour
{
    public TMP_Text LabelFragPass;
    public Camera OcclusionQueryCamera;
    public MeshRenderer TargetObject;
    public RenderTexture TempRT;

    protected Material _originMaterial;
    protected Material[] _originMaterials;
    // protected int _originLayer;


#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("RenderingPlugin")]
#endif
	private static extern IntPtr GetBeginQueryEventFunc();

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("RenderingPlugin")]
#endif
	private static extern IntPtr GetEndQueryEventFunc();


    // Start is called before the first frame update
    //void Start() {

    //}

    //// Update is called once per frame
    //void Update() {
        
    //}

    public void OnQueryBtnClicked() {
        // TODO: 确保材质加载并且测量纹理已经设置到材质

        // Debug.Log("Start Query");
        // LabelFragPass.text = "1234";
        GenerateTextureUtility.GenerateAndSetCalibrationTexture();
        QuerySingleRenderer(TargetObject);  // TODO: 后续替换为逐对象
    }

    private Matrix4x4 GetMVPMatrix(GameObject go, Camera camera) {
        // bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
        Matrix4x4 M = go.transform.localToWorldMatrix;
        Matrix4x4 V = camera.worldToCameraMatrix;
        // 根据当前图形API调整P矩阵
        // TODO: 是否渲染到RT，这里可能需要调整，即使不调整，上下颠倒应该也不影响count
        Matrix4x4 P = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
        // Matrix4x4 P = camera.projectionMatrix;
        // if (d3d) {
        //     // Invert Y for rendering to a render texture
        //     for (int i = 0; i < 4; i++) {
        //         P[1, i] = -P[1, i];
        //     }
        //     // Scale and bias from OpenGL -> D3D depth range
        //     for (int i = 0; i < 4; i++) {
        //         P[2, i] = P[2, i] * 0.5f + P[3, i] * 0.5f;
        //     }
        // }
        return P * V * M;
    }

    static float[] _alphaThresholds = new float[] {
        1, 0.75f, 0.5f, 0.25f
    };
    void QuerySingleRenderer(MeshRenderer renderer) {
        // TODO: 将相机平移到renderer正对面
        // 基于策划配置的原始相机位置，X平移到物体世界坐标，Z根据关卡配置测试上下边界
        var calibrationMaterial = GenerateTextureUtility.CalibrationMaterial;   // TODO: 确保测量纹理已经加载到材质

        try {
            // _originLayer = renderer.gameObject.layer;
            // 替换材质
            // ReplaceWithCalibrationMaterial(renderer, calibrationMaterial);
            // renderer.gameObject.layer = TempLayer.value;
            if (TempRT)
                Graphics.SetRenderTarget(TempRT);

            // GL.Clear(true, true, Color.black);
            GL.PushMatrix();
            var projectionMatrix = GL.GetGPUProjectionMatrix(OcclusionQueryCamera.projectionMatrix, false);
            GL.LoadProjectionMatrix(projectionMatrix);

            var MVP = GetMVPMatrix(renderer.gameObject, OcclusionQueryCamera);
            
            // TODO: 逐步调整
            // test total fragment count without alpha clip
            int total = CountFragmentWithAlphaThreshold(renderer, 0, calibrationMaterial, MVP);
            if (total <= 0) {
                // 当前renderer没有在摄像机里，不做剔除
                // TODO: 还是记录一下
                return;
            }

            // TODO: open
            return;

            int i = 0;
            for (; i < _alphaThresholds.Length; ++i) {
                float alpha = _alphaThresholds[i] - 0.001f;      // 0.001f避免比较相等时的浮点误差，TODO: 后面改用区间噪声alpha后就不需要0.001f了
                int fragPass = CountFragmentWithAlphaThreshold(renderer, alpha, calibrationMaterial, MVP);
                if (fragPass >= total * 0.15) {
                    // 当前mip超过15%可见，保留。该纹理从2048开始剔除i层mipmap，即缩小比例为pow(4, -i)
                    // TODO: 记录当前renderer需要使用mipLevel = i
                    break;
                }
            }

            if (i >= _alphaThresholds.Length) {
                // TODO: 物体太小了，记录当前renderer使用mipLevel = 4。可以和上面break合并掉
            }
        } finally {
            // 回滚材质
            GL.PopMatrix();
            // renderer.gameObject.layer = _originLayer;
            // RestoreMaterial(renderer);
        }
    }

    int CountFragmentWithAlphaThreshold(MeshRenderer renderer, float alphaThreshold, Material calibrationMaterial, Matrix4x4 MVP) {
        if (alphaThreshold < 0.001) {
            calibrationMaterial.SetFloat("_AlphaClip", 0);
            calibrationMaterial.DisableKeyword("_ALPHATEST_ON");
        } else {
            calibrationMaterial.SetFloat("_AlphaClip", 1);
            calibrationMaterial.SetFloat("_Cutoff", 1);
            calibrationMaterial.EnableKeyword("_ALPHATEST_ON");
        }

        var meshFilter = renderer.GetComponent<MeshFilter>();
        if (!meshFilter)
            return 0;

        var mesh = meshFilter.sharedMesh;
        var matrixMV = OcclusionQueryCamera.worldToCameraMatrix * renderer.transform.localToWorldMatrix;
        // TODO: start query, call native plugin
        GL.IssuePluginEvent(GetBeginQueryEventFunc(), 1);
        if (mesh != null && calibrationMaterial.SetPass(0)) {
            // 经过大量测试，DrawMeshNow需要的矩阵是MV矩阵
            // P矩阵通过GL.LoadProjectionMatrix()进行设置
            Graphics.DrawMeshNow(mesh, matrixMV);

            // DrawMesh会推迟到forward pass里执行，这里不适用
            // Graphics.DrawMesh(mesh, renderer.transform.localToWorldMatrix, calibrationMaterial, _originLayer);
        }
        //OcclusionQueryCamera.Render();
        // TODO: end query
        GL.IssuePluginEvent(GetEndQueryEventFunc(), 1);
        

        return 0;
    }

    void ReplaceWithCalibrationMaterial(MeshRenderer renderer, Material calibrationMaterial) {
        _originMaterial = null;
        _originMaterials = null;
        if (renderer.sharedMaterials.Length > 1) {
            _originMaterials = renderer.sharedMaterials;
            var newMaterials = (Material[])_originMaterials.Clone();
            for (int i = 0; i < newMaterials.Length; ++i) {
                newMaterials[i] = calibrationMaterial;
            }
            renderer.sharedMaterials = newMaterials;
        } else {
            _originMaterial = renderer.sharedMaterial;
            renderer.material = calibrationMaterial;
        }
    }

    void RestoreMaterial(MeshRenderer renderer) {
        if (_originMaterials != null) {
            renderer.sharedMaterials = _originMaterials;
        } else if (_originMaterial != null) {
            renderer.sharedMaterial = _originMaterial;
        }

        _originMaterial = null;
        _originMaterials = null;
    }

    // TODO: update和start 2选1
    // Update方式在帧前执行
    // Start方式在帧尾执行
    //private void Update() {
    //    QuerySingleRenderer(TargetObject);
    //}
    // IEnumerator Start() {
    //     yield return StartCoroutine("CallPluginAtEndOfFrames");
    // }

    // private IEnumerator CallPluginAtEndOfFrames() {
    //     while (true) {
    //         // Wait until all frame rendering is done
    //         yield return new WaitForEndOfFrame();
    //         QuerySingleRenderer(TargetObject);  // TODO: 后续替换为逐对象
    //     }
    // }
}
