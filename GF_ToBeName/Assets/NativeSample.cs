using System.Runtime.InteropServices;
using UnityEngine;

public class NativeSample
{
    private static NativeSample _instance;

    public static NativeSample Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new NativeSample();
            }

            return _instance;
        }
    }

#if UNITY_IPHONE

    [DllImport("__Internal")]
    private static extern void checkCurrentNotificationStatus();

    [DllImport("__Internal")]
    private static extern bool getTestConfig();

    [DllImport("__Internal")]
    private static extern bool AppFlyerEventTrack(float price, string iso, string ID);

    [DllImport("__Internal")]
    private static extern void requestTrackingAuthorizationWithCompletionHandler();

    [DllImport("__Internal")]
    private static extern int getAppTrackingAuthorizationStatus();

    [DllImport("__Internal")]
    private static extern bool isAvailableIOSSysterVersion(float version);

    [DllImport("__Internal")]
    private static extern bool sendRecordImmediately();

    [DllImport("__Internal")]
    private static extern bool showFiveStarComment();

    [DllImport("__Internal")]
    private static extern void Vibrate(int Level);

#endif


    public void recordIfNotificationIsOn()
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        checkCurrentNotificationStatus();
#endif
    }

    public static void Vibrate(long milliseconds, int amplitude = 2)
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        //0,1,2不同强度
        Vibrate(VibrateManager.Instance.iOSAmplitude);
#elif UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            currentActivity.Call("unityVibrate", milliseconds, amplitude);
        }));
#endif
    }

    public void IOSAppFlyerEventTrack(float price, string iso, string ID)
    {
#if UNITY_IOS && !UNITY_EDITOR
        AppFlyerEventTrack(price, iso, ID);
#endif
    }


    public void SendiOSRecordImmediately()
    {
#if UNITY_IOS && !UNITY_EDITOR
          sendRecordImmediately();
#endif
    }

    public bool ShowNativeFiveStarDialog()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return showFiveStarComment();
#endif
        return false;
    }
}