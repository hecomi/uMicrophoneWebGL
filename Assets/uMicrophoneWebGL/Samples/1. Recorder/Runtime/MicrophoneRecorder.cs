using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace uMicrophoneWebGL.Samples
{
    
public class MicrophoneRecorder : MonoBehaviour
{
    [Header("Components")]
    public MicrophoneWebGL microphoneWebGL;
    public AudioSource audioSource;
    
    [Header("UI")]
    public Text toggleButtonText;
    public Button playButton;
    public Text playButtonText;
    public Dropdown deviceDropdown;
        
    [Header("Record")]
    public float maxDuration = 10f;
    
    private float[] _buffer = null;
    private int _bufferSize = 0;
    private AudioClip _clip;
    private bool _isPlaying = false;

    void Update()
    {
        UpdatePlayButtonText();
    }
    
    private void UpdatePlayButtonText()
    {
        if (!audioSource) return;
        if (audioSource.isPlaying == _isPlaying) return;
        
        _isPlaying = audioSource.isPlaying;
        playButtonText.text = _isPlaying ? "Stop" : "Play";
    }

    public void ToggleRecord()
    {
        if (!microphoneWebGL || !microphoneWebGL.isValid) return;

        bool isRecording = microphoneWebGL.isRecording;

        if (!isRecording)
        {
            Begin();
        }
        else
        {
            End();
        }

        isRecording = !isRecording;

        if (toggleButtonText)
        {
            toggleButtonText.text = isRecording ? "Stop" : "Start";
        }

        if (playButton)
        {
            playButton.interactable = !isRecording;
        }
    }
    
    public void TogglePlay()
    {
        if (!audioSource) return;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        else
        {
            audioSource.clip = _clip;
            audioSource.Play();
        }
    }
    
    private void Begin()
    {
        if (deviceDropdown)
        {
            microphoneWebGL.micIndex = deviceDropdown.value;
        }
        
        microphoneWebGL.Begin();
    }
    
    private void End()
    {
        microphoneWebGL.End();
    }

    public void OnBegin()
    {
        int freq = microphoneWebGL.selectedDevice.sampleRate;
        int n = (int)(freq * maxDuration);
        if (_buffer == null || _buffer.Length != n)
        {
            _buffer = new float[n];
        }
        _bufferSize = 0;
    }

    public void OnEnd()
    {
        if (!audioSource) return;
        
        var device = microphoneWebGL.selectedDevice;
        var freq = device.sampleRate;
        var ch = device.channelCount;
        _clip = AudioClip.Create("uMicrophoneWebGL-Recorded", _bufferSize + freq, ch, freq, false);
        var data = new float[_bufferSize];
        System.Array.Copy(_buffer, data, _bufferSize);
        _clip.SetData(data, 0);
    }

    public void OnData(float[] input)
    {
        if (input == null) return;
        int n = input.Length;
        if (_bufferSize + n >= _buffer.Length) return;
        System.Array.Copy(input, 0, _buffer, _bufferSize, n);
        _bufferSize += n;
    }

    public void OnDeviceListUpdated(List<Device> devices)
    {
        if (!deviceDropdown) return;
        
        var options = new List<Dropdown.OptionData>();
        foreach (var device in devices)
        {
            Dropdown.OptionData option = new Dropdown.OptionData()
            {
                text = $"{device.label} (Ch: {device.channelCount})",
            };
            options.Add(option);
        }
        deviceDropdown.options = options;
    }
}

}