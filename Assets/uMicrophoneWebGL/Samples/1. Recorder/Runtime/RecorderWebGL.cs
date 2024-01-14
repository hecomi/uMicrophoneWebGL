using UnityEngine;
using UnityEngine.UI;

namespace uMicrophoneWebGL.Samples
{
    
public class RecorderWebGL : MonoBehaviour
{
    public MicrophoneWebGL microphoneWebGL;
    public AudioSource audioSource;
    public float duration = 10f;
    public Text toggleButtonText;
    public Button playButton;
    public Text playButtonText;
    
    private float[] _buffer = null;
    private int _bufferSize = 0;
    private AudioClip _clip;

    void OnEnable()
    {
        if (!microphoneWebGL) return;
        
        microphoneWebGL.dataEvent.AddListener(OnData);
    }

    void OnDisable()
    {
        if (!microphoneWebGL) return;
        
        microphoneWebGL.dataEvent.RemoveListener(OnData);
    }

    void Update()
    {
        if (audioSource && audioSource.isPlaying)
        {
            playButtonText.text = "Stop";
        }
        else
        {
            playButtonText.text = "Play";
        }
    }

    public void ToggleRecord()
    {
        if (!microphoneWebGL || !microphoneWebGL.isValid) return;

        bool isRecording = microphoneWebGL.isRecording;

        if (!isRecording)
        {
            OnBegin();
        }
        else
        {
            OnEnd();
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

    private void OnBegin()
    {
        microphoneWebGL.Begin();
        
        int n = (int)(AudioSettings.outputSampleRate * duration);
        if (_buffer == null || _buffer.Length != n)
        {
            _buffer = new float[n];
        }
        _bufferSize = 0;
    }

    private void OnEnd()
    {
        microphoneWebGL.End();
        
        CreateClip();
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

    private void CreateClip()
    {
        if (!audioSource) return;
        
        var freq = microphoneWebGL.selectedDevice.sampleRate;
        _clip = AudioClip.Create("uMicrophoneWebGL-Recorded", _bufferSize, 1, freq, false);
        var data = new float[_bufferSize];
        System.Array.Copy(_buffer, data, _bufferSize);
        _clip.SetData(data, 0);
    }

    private void OnData(float[] input)
    {
        if (input == null) return;
        int n = input.Length;
        if (_bufferSize + n >= _buffer.Length) return;
        System.Array.Copy(input, 0, _buffer, _bufferSize, n);
        _bufferSize += n;
    }
}

}