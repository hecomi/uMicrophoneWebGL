#if UNITY_EDITOR

using UnityEngine;

namespace uMicrophoneWebGL
{
    
[RequireComponent(typeof(AudioSource))]
public class EditorMicrophoneDataRetriever : MonoBehaviour
{
    public DataEvent dataEvent { get; } = new DataEvent();
    public bool playMicrophoneSound { get; set; } = false;
    
    private AudioSource _source;
    private AudioClip _clip;
    private bool _isRecording = false;
    private string _deviceName = "";
    private float[] _buffer = null;
    private float[] _data = null;
    private int _bufferSize = 0;
    private Object _lock = new Object();

    void OnEnable()
    {
        _source = GetComponent<AudioSource>();
        _buffer = new float[AudioSettings.outputSampleRate];
    }

    void Update()
    {
        if (_bufferSize == 0) return;

        lock (_lock)
        {
            if (_data == null || _data.Length != _bufferSize)
            {
                _data = new float[_bufferSize];
            }
            System.Array.Copy(_buffer, _data, _bufferSize);
            _bufferSize = 0;
        }
        
        dataEvent.Invoke(_data);
    }

    public void Begin(string deviceName, int freq)
    {
        if (!_source || _isRecording) return;

        _deviceName = deviceName;
        _clip = Microphone.Start(_deviceName, true, 10, freq);

        int retryCount = 0;
        while (Microphone.GetPosition(_deviceName) <= 0)
        {
            if (++retryCount >= 1000)
            {
                Debug.LogError("Failed to get microphone.");
                return;
            }
            System.Threading.Thread.Sleep(1);
        }

        _source.loop = true;
        _source.clip = _clip;
        _source.Play();

        _isRecording = true;
    }

    public void End()
    {
        if (!_source || !_isRecording) return;

        if (_source.isPlaying)
        {
            _source.Stop();
        }

        Microphone.End(_deviceName);

        _isRecording = false;
    }

    void OnAudioFilterRead(float[] input, int channels)
    {
        int n = input.Length;

        lock (_lock)
        {
            if (_bufferSize + n >= _buffer.Length) return;
            System.Array.Copy(input, 0, _buffer, _bufferSize, n);
            _bufferSize += n;
        }

        if (!playMicrophoneSound)
        {
            System.Array.Clear(input, 0, n);
        }
    }
}

}

#endif