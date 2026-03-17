package com.tile;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.os.Environment;
import android.provider.Settings;
import android.util.Log;

import com.google.firebase.messaging.MessageForwardingService;
import com.unity3d.player.UnityPlayerActivity;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;
import com.google.android.gms.common.GoogleApiAvailability;

import android.text.TextUtils;
import android.net.Uri;

import android.content.res.AssetManager;
import java.io.IOException;
import java.io.InputStream;


public class LauncherActivity extends UnityPlayerActivity {

    private Vibrator mVibrator;
    protected static AssetManager assetManager;

    @Override
    protected void onNewIntent(Intent intent) {
        Intent message = new Intent(this, MessageForwardingService.class);
        message.setAction(MessageForwardingService.ACTION_REMOTE_INTENT);
        message.putExtras(intent);
        message.setData(intent.getData());
        // For older versions of Firebase C++ SDK (< 7.1.0), use `startService`.
        // startService(message);
        MessageForwardingService.enqueueWork(this, message);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        if (mUnityPlayer != null) {
            mUnityPlayer.quit();
            mUnityPlayer = null;
        }
        super.onCreate(savedInstanceState);
        mVibrator = (Vibrator)this.getSystemService(Context.VIBRATOR_SERVICE);

        assetManager = getAssets();
    }

    public void unityVibrate(long milliseconds, int amplitude)
    {
        try
        {
            Log.v("LauncherActivity", "amplitude=" + amplitude);
            if(mVibrator != null)
            {
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O)
                {
                    mVibrator.vibrate(VibrationEffect.createOneShot(milliseconds, amplitude));
                }
                else
                {
                    mVibrator.vibrate(milliseconds);
                }
            }
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    public void RecordInAppPurchase(String productId, float price, String iso)
    {
//        Map<String, Object> eventValue = new HashMap<>();
//        eventValue.put(AFInAppEventParameterName.REVENUE,price);
//        eventValue.put(AFInAppEventParameterName.CONTENT_ID, productId);
//        eventValue.put(AFInAppEventParameterName.CURRENCY, iso);
//        AppsFlyerLib.getInstance().logEvent(getApplicationContext() , AFInAppEventType.PURCHASE, eventValue);
    }

    public void JumpToGPStore()
    {
        String appPkg = getPackageName();
        final String GOOGLE_PLAY = "com.android.vending";//这里对应的是谷歌商店，跳转别的商店改成对应的即可
        try {
            if (TextUtils.isEmpty(appPkg))
                return;
            Uri uri = Uri.parse("market://details?id=" + appPkg);
            Log.v("LauncherActivity", "appPkg=" + appPkg);
            Intent intent = new Intent(Intent.ACTION_VIEW, uri);
            intent.setPackage(GOOGLE_PLAY);
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            startActivity(intent);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public int isGooglePlayServicesAvailable() {
        return GoogleApiAvailability.getInstance().isGooglePlayServicesAvailable(this);
    }

    public boolean reqPermission() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && false == Environment.isExternalStorageManager()) {
            Uri uri = Uri.parse("package:" + getPackageName());
            startActivity(new Intent(Settings.ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION, uri));
            return false;
        }
        else {
            return true;
        }
    }

       //返回字节数组
    public static byte[] LoadBytes(String path) throws IOException {
        InputStream inputStream = assetManager.open(path);
        try {
            int len = inputStream.available();
            byte buf[] = new byte[len];
            inputStream.read(buf);
            inputStream.close();
            return buf;
        }
        catch (IOException e) {
            Log.v ("unity", e.getMessage());
        }
        return null;
    }
}
