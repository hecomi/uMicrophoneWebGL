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
    private static float[] _dataBuffer = null;
    
    public static void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;
        Initialize(OnReady, OnStarted, OnStopped, OnDeviceListUpdated, OnDataReceived);
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

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_Initialize")]
    private static extern void Initialize(
        Action readyCallback,
        Action startCallback,
        Action stopCallback,
        Action deviceListCallback,
        Action<IntPtr, int> dataCallback);
#else
    private static void Initialize(
        Action readyCallback,
        Action startCallback,
        Action stopCallback,
        Action deviceListCallback,
        Action<IntPtr, int> dataCallback)
    {
        deviceListEvent.Invoke();
        readyEvent.Invoke();
        isReady = true;
    }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_RefreshDeviceList")]
    public static extern void RefreshDeviceList();
#else
    public static void RefreshDeviceList()
    {
        deviceListEvent.Invoke();
    }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_GetDeviceCount")]
    public static extern int GetDeviceCount();
#else
    public static int GetDeviceCount() => Microphone.devices.Length;
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_GetDeviceId")]
    public static extern string GetDeviceId(int index);
#else
    public static string GetDeviceId(int index) => 
        index >= 0 && index < Microphone.devices.Length ?
            Microphone.devices[index] : 
            "";
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_GetLabel")]
    public static extern string GetLabel(int index);
#else
    public static string GetLabel(int index) => GetDeviceId(index);
#endif
    
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_GetSampleRate")]
    public static extern int GetSampleRate(int index);
#else
    public static int GetSampleRate(int index)
    {
        var name = GetDeviceId(index);
        if (string.IsNullOrEmpty(name)) return -1;
        Microphone.GetDeviceCaps(name, out var minFreq, out var maxFreq);
        return maxFreq > 0 ? maxFreq : 48000;
    }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_SetDevice")]
    public static extern void SetDevice(int index);
#else
    private static int _deviceIndex = 0;

    public static void SetDevice(int index)
    {
        _deviceIndex = index;
    }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_Start")]
    public static extern void Start();

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_Stop")]
    public static extern void Stop();

    [DllImport("__Internal", EntryPoint = "uMicrophoneWebGL_IsRecording")]
    public static extern bool IsRecording();
#else
    private static GameObject _recordGameObj = null;
    private static EditorMicrophoneDataRetriever _micInputRetriever = null;
    
    public static void Start()
    {
        if (_recordGameObj) return;
        
        _recordGameObj = new GameObject("[uMicrophoneWebGL.EditorMicrophoneInputRetriever]");
        
        _micInputRetriever = _recordGameObj.AddComponent<EditorMicrophoneDataRetriever>();
        _micInputRetriever.dataEvent.AddListener(x => dataEvent.Invoke(x));
        var deviceId = GetDeviceId(_deviceIndex);
        var freq = GetSampleRate(_deviceIndex);
        _micInputRetriever.Begin(deviceId, freq);
        
        startEvent.Invoke();
    }
    
    public static void Stop()
    {
        if (!_recordGameObj) return;
        
        _micInputRetriever.End();
        UnityEngine.Object.Destroy(_recordGameObj);
        stopEvent.Invoke();
    }

    public static bool IsRecording() => _recordGameObj;
#endif
}

}