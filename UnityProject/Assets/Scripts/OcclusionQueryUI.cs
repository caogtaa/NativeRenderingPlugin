using System.Collections;
using System.Collections.Generic;
using System;
using TexelDensityTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class OcclusionQueryUI : MonoBehaviour
{
    public TMP_Text LabelFragPass;
    public Camera OcclusionQueryCamera;
    public MeshRenderer TargetObject;
    public LayerMask TempLayer;         // 临时layer，将需要渲染的物体临时切换到这个layer，数值需要等于camera.cullingMask

    protected Material _originMaterial;
    protected Material[] _originMaterials;
    protected int _originLayer;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void OnQueryBtnClicked() {
        // TODO: 确保材质加载并且测量纹理已经设置到材质

        // Debug.Log("Start Query");
        // LabelFragPass.text = "1234";
        QuerySingleRenderer(TargetObject);  // TODO: 后续替换为逐对象
    }

    static float[] _alphaThresholds = new float[] {
        1, 0.75f, 0.5f, 0.25f
    };
    void QuerySingleRenderer(MeshRenderer renderer) {
        // TODO: 将相机平移到renderer正对面
        // 基于策划配置的原始相机位置，X平移到物体世界坐标，Z根据关卡配置测试上下边界
        var calibrationMaterial = GenerateTextureUtility.CalibrationMaterial;   // TODO: 确保测量纹理已经加载到材质

        try {
            // 替换材质
            ReplaceWithCalibrationMaterial(renderer, calibrationMaterial);
            _originLayer = renderer.gameObject.layer;
            renderer.gameObject.layer = TempLayer.value;
            // TODO: 逐步调整
            // test total fragment count without alpha clip
            int total = CountFragmentWithAlphaThreshold(renderer, 0, calibrationMaterial);
            if (total <= 0) {
                // 当前renderer没有在摄像机里，不做剔除
                // TODO: 还是记录一下
                return;
            }

            int i = 0;
            for (; i < _alphaThresholds.Length; ++i) {
                float alpha = _alphaThresholds[i] - 0.001f;      // 0.001f避免比较相等时的浮点误差，TODO: 后面改用区间噪声alpha后就不需要0.001f了
                int fragPass = CountFragmentWithAlphaThreshold(renderer, alpha, calibrationMaterial);
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
            renderer.gameObject.layer = _originLayer;
            RestoreMaterial(renderer);
        }
    }

    int CountFragmentWithAlphaThreshold(MeshRenderer renderer, float alphaThreshold, Material calibrationMaterial) {
        if (alphaThreshold < 0.001) {
            calibrationMaterial.SetFloat("_AlphaClip", 0);
            calibrationMaterial.DisableKeyword("_ALPHATEST_ON");
        } else {
            calibrationMaterial.SetFloat("_AlphaClip", 1);
            calibrationMaterial.SetFloat("_Cutoff", 1);
            calibrationMaterial.EnableKeyword("_ALPHATEST_ON");
        }

        // TODO: start query, call native plugin
        OcclusionQueryCamera.Render();
        // TODO: end query

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
}
