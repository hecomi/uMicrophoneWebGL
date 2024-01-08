using UnityEngine;
using UnityEngine.Events;
using System;
using System.Runtime.InteropServices;

namespace uMicrophoneWebGL
{
    
[Serializable]
public class TimingEvent : UnityEvent
{
}
    
[Serializable]
public class DataEvent : UnityEvent<float[]> 
{
}

public static class Lib
{
    public static bool isInitialized { get; private set; } = false;
    public static bool isReady { get; private set; } = false;
    public static TimingEvent readyEvent { get; } = new();
    public static TimingEvent deviceListEvent { get; } = new();
    public static TimingEvent startEvent { get; } = new();
    public static TimingEvent stopEvent { get; } = new();
    public static DataEvent dataEvent { get; } = new();
    static float[] _dataBuffer = null;
    
    public static void Initialize()
    {
        if (isInitialized) return;
        
        Initialize(OnReady, OnStarted, OnStopped, OnDeviceListUpdated, OnDataReceived);
        isInitialized = true;
    }
    
    [AOT.MonoPInvokeCallback(typeof(Action))]
    static void OnReady()
    {
        isReady = true;
        
        try
        {
            readyEvent.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    
    [AOT.MonoPInvokeCallback(typeof(Action<IntPtr, int>))]
    static void OnDataReceived(IntPtr ptr, int length)
    {
        if (_dataBuffer == null || _dataBuffer.Length != length)
        {
            _dataBuffer = new float[length];
        }
        
        Marshal.Copy(ptr, _dataBuffer, 0, length);
        
        try
        {
            dataEvent.Invoke(_dataBuffer);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    
    [AOT.MonoPInvokeCallback(typeof(Action))]
    static void OnDeviceListUpdated()
    {
        try
        {
            deviceListEvent.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    
    [AOT.MonoPInvokeCallback(typeof(Action))]
    static void OnStarted()
    {
        try
        {
            startEvent.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    
    [AOT.MonoPInvokeCallback(typeof(Action))]
    static void OnStopped()
    {
        try
        {
            stopEvent.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_Initialize")]
    private static extern void Initialize(
        Action readyCallback,
        Action startCallback,
        Action stopCallback,
        Action deviceListCallback,
        Action<IntPtr, int> dataCallback);

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_RefreshDeviceList")]
    public static extern int RefreshDeviceList();

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_GetDeviceCount")]
    public static extern int GetDeviceCount();

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_GetDeviceId")]
    public static extern string GetDeviceId(int index);

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_GetLabel")]
    public static extern string GetLabel(int index);
    
    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_GetSampleRate")]
    public static extern int GetSampleRate(int index);

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_SetDevice")]
    public static extern void SetDevice(int index);

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_Start")]
    public static extern void Start();

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_Stop")]
    public static extern void Stop();

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_IsRecording")]
    public static extern bool IsRecording();
}

}