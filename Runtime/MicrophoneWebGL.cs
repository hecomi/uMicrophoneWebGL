using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace uMicrophoneWebGL
{
    
[System.Serializable]
public class DeviceListEvent : UnityEvent<List<Device>> 
{
}

public class MicrophoneWebGL : MonoBehaviour
{
    public bool isAutoStart = true;
    public int micIndex = 0;
    public TimingEvent readyEvent = new();
    public TimingEvent startEvent = new();
    public TimingEvent stopEvent = new();
    public DeviceListEvent deviceListEvent = new();
    public DataEvent dataEvent = new();

    public bool isValid => (micIndex >= 0 && micIndex < devices.Count);
    public List<Device> devices { get; } = new();
    public Device selectedDevice => isValid ? devices[micIndex] : Device.invalid;
    public bool isRecording => Lib.IsRecording();

    private bool _isBeginRequested = false;

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
        if (isRecording)
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
        
        RequestStart();
    }

    private void RequestStart()
    {
        Lib.SetDevice(micIndex);
        Lib.Start();
    }

    public void End()
    {
        if (!isRecording)
        {
            return;
        }
        
        Lib.Stop();
    }

    public void RefreshDeviceList()
    {
        Lib.RefreshDeviceList();
    }

    private void OnReady()
    {
        readyEvent.Invoke();

        if (_isBeginRequested)
        {
            _isBeginRequested = false;
            RequestStart();
        }
    }

    private void OnDataReceived(float[] input)
    {
        dataEvent.Invoke(input);
    }

    private void OnDeviceListUpdated()
    {
        devices.Clear();
        
        int n = Lib.GetDeviceCount();
        if (n == 0) return;
        
        for (int i = 0; i < n; ++i)
        {
            var device = new Device()
            {
                index = i,
                deviceId = Lib.GetDeviceId(i),
                label = Lib.GetLabel(i),
                sampleRate = Lib.GetSampleRate(i),
            };
            
            Debug.Log($"Device[{i}]: {device.label} (sampleRate: {device.sampleRate}, ID: {device.deviceId})");
            
            devices.Add(device);
        }
        
        deviceListEvent.Invoke(devices);
    }

    private void OnStarted()
    {
        startEvent.Invoke();
    }
    
    private void OnStopped()
    {
        stopEvent.Invoke();
    }
}

}