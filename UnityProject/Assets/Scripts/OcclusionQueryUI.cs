using System.Collections;
using System.Collections.Generic;
using System;
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

    // protected Material _originMaterial;
    // protected Material[] _originMaterials;
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

#if UNITY_EDITOR
    OcclusionQueryRunner _runner = new OcclusionQueryRunner();
#endif


    // Start is called before the first frame update
    //void Start() {

    //}

    //// Update is called once per frame
    //void Update() {
        
    //}

    public void OnQueryBtnClicked() {
        // TODO: 确保材质加载并且测量纹理已经设置到材质
#if UNITY_EDITOR
        _runner.Setup(OcclusionQueryCamera, TempRT);
        _runner.QuerySingleRenderer(TargetObject);
#endif

        // Debug.Log("Start Query");
        // LabelFragPass.text = "1234";
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
