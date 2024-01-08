using UnityEngine;
using System.Collections.Generic;

namespace uMicrophoneWebGL
{

public class Device
{
    public int index;
    public string deviceId;
    public string label;
    public int sampleRate;

    public static Device invalid = new Device()
    {
        index = -1,
        deviceId = "",
        label = "",
        sampleRate = 0,
    };
}

public class MicrophoneWebGL : MonoBehaviour
{
    public bool isAutoStart = true;
    public int micIndex = 0;
    public TimingEvent readyEvent = new();
    public TimingEvent startEvent = new();
    public TimingEvent stopEvent = new();
    public TimingEvent deviceListEvent = new();
    public DataEvent dataEvent = new();

    public bool isValid => (micIndex >= 0 && micIndex < devices.Count);
    public List<Device> devices { get; } = new();
    public Device selectedDevice => isValid ? devices[micIndex] : Device.invalid;
    
    bool _isBeginRequested = false;

    void OnEnable()
    {
        Lib.readyEvent.AddListener(OnReady);
        Lib.startEvent.AddListener(OnStarted);
        Lib.stopEvent.AddListener(OnStopped);
        Lib.deviceListEvent.AddListener(OnDeviceListUpdated);
        Lib.dataEvent.AddListener(OnDataReceived);
        
        Lib.Initialize();
        
        if (isAutoStart)
        {
            Begin();
        }
    }

    void OnDisable()
    {
        End();
        
        Lib.readyEvent.RemoveListener(OnReady);
        Lib.startEvent.RemoveListener(OnStarted);
        Lib.stopEvent.RemoveListener(OnStopped);
        Lib.deviceListEvent.RemoveListener(OnDeviceListUpdated);
        Lib.dataEvent.RemoveListener(OnDataReceived);
    }
    
    public void Begin()
    {
        if (Lib.IsRecording())
        {
            Debug.LogError(
                "Another component has already started recording. " +
                "Currently, only one device is permitted to record at a time.");
            return;
        }
        
        if (!Lib.isReady)
        {
            _isBeginRequested = true;
            return;
        }
        
        BeginInternal();
    }

    void BeginInternal()
    {
        Lib.SetDevice(micIndex);
        Lib.dataEvent.AddListener(OnDataReceived);
        Lib.Start();
    }

    public void End()
    {
        Lib.Stop();
    }

    void OnReady()
    {
        readyEvent.Invoke();

        if (_isBeginRequested)
        {
            _isBeginRequested = false;
            BeginInternal();
        }
    }

    void OnDataReceived(float[] input)
    {
        dataEvent.Invoke(input);
    }

    void OnDeviceListUpdated()
    {
        devices.Clear();
        
        int n = Lib.GetDeviceCount();
        if (n == 0) return;
        
        for (int i = 0; i < n; ++i)
        {
            var id = Lib.GetDeviceId(i);
            var label = Lib.GetLabel(i);
            var sampleRate = Lib.GetSampleRate(i);
            var device = new Device()
            {
                index = i,
                deviceId = id,
                label = label,
                sampleRate = sampleRate,
            };
            devices.Add(device);
            Debug.Log($"Device[{i}]: {label} (sampleRate: {sampleRate}, ID: {id})");
        }
        
        deviceListEvent.Invoke();
    }

    void OnStarted()
    {
        startEvent.Invoke();
    }
    
    void OnStopped()
    {
        stopEvent.Invoke();
    }
}

}