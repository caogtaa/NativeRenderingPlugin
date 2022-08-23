using TMPro;
using UnityEngine;

public class OcclusionQueryUI : MonoBehaviour
{
    public TMP_Text LabelFragPass;
    public Camera OcclusionQueryCamera;
    public MeshRenderer TargetObject;
    public RenderTexture TempRT;

#if UNITY_EDITOR
    OcclusionQueryRunner _runner = new OcclusionQueryRunner();
#endif


    public void OnQueryBtnClicked() {
        // TODO: 确保材质加载并且测量纹理已经设置到材质
#if UNITY_EDITOR
        _runner.Setup(OcclusionQueryCamera, TempRT);
        int result = _runner.QuerySingleRenderer(TargetObject);
        LabelFragPass.text = result.ToString();
#endif
    }

    public void OnFetchResultBtnClicked() {
        int result = _runner.GetPluginLastQueryResult();
        LabelFragPass.text = result.ToString();
    }

    // TODO: update和start 2选1
    // Update方式在帧前执行
    // Start方式在帧尾执行
    //private void Update() {
    //    QuerySingleRenderer(TargetObject);
    //}
    //IEnumerator Start() {
    //    yield return StartCoroutine("CallPluginAtEndOfFrames");
    //}

    //private IEnumerator CallPluginAtEndOfFrames() {
    //    _runner.Setup(OcclusionQueryCamera, TempRT);
    //    while (true) {
    //        // Wait until all frame rendering is done
    //        yield return new WaitForEndOfFrame();
    //        _runner.QuerySingleRenderer(TargetObject);
    //    }
    //}
}
