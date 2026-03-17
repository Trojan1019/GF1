#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using System;
using UnityEngine;

public class CompilationListener
{
    private static bool isCompiling = false;
    private static Action customCallback;

    public static void RequestCompilationAndListen(Action completeCallback)
    {
        if (isCompiling)
        {
            Debug.LogWarning("Compilation is already in progress!");
            return;
        }

        customCallback = completeCallback;

        // 标记为正在编译
        isCompiling = true;
        Debug.Log("Requesting script compilation...");

        // 请求脚本编译
        CompilationPipeline.RequestScriptCompilation();

        // 监听编译完成
        CompilationPipeline.compilationFinished += OnCompilationFinished;

        // 可选：通过 EditorApplication.update 检测编译过程
        EditorApplication.update += MonitorCompilationStatus;
    }

    private static void MonitorCompilationStatus()
    {
        if (EditorApplication.isCompiling)
        {
            Debug.Log("Compilation in progress...");
        }
        else if (isCompiling) // 确保监听停止时调用
        {
            Debug.Log("Compilation has completed!");
            isCompiling = false;

            // 停止监听
            EditorApplication.update -= MonitorCompilationStatus;
        }
    }

    private static void OnCompilationFinished(object context)
    {
        Debug.Log("Compilation finished!");

        customCallback?.Invoke();
        customCallback = null;
        // 清理回调
        CompilationPipeline.compilationFinished -= OnCompilationFinished;
        isCompiling = false;
    }
}
#endif
