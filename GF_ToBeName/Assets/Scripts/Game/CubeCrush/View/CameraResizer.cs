using UnityEngine;

public class CameraResizer : MonoBehaviour
{
    private const float BASE_CONSTANT = 6.84f;
    private void Start()
    {
        ResizeCamera();
    }

    private void ResizeCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // 获取屏幕的宽高比
        float screenRatio = (float)Screen.width / Screen.height;

        // 计算 Camera Size
        float calculatedSize = BASE_CONSTANT / screenRatio;

        // 应用到摄像机
        cam.orthographicSize = calculatedSize;
    }
}