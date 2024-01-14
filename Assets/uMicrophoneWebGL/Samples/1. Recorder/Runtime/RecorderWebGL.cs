using UnityEngine;
using UnityEngine.UI;

namespace uMicrophoneWebGL.Samples
{
    
[RequireComponent(typeof(MicrophoneWebGL))]
public class RecorderWebGL : MonoBehaviour
{
    MicrophoneWebGL _mic = null;
    
    public float duration = 3f;
    public Text toggleButtonText;
    public Button playButton;

    void OnEnable()
    {
        _mic = GetComponent<MicrophoneWebGL>();
        if (!_mic) return;
        
        _mic.dataEvent.AddListener(OnData);
    }

    void OnDisable()
    {
        if (!_mic) return;
        
        _mic.dataEvent.RemoveListener(OnData);
    }

    public void ToggleRecord()
    {
        if (!_mic || !_mic.isValid) return;

        bool isRecording = _mic.isRecording;

        if (!isRecording)
        {
            Debug.Log("Begin");
            _mic.Begin();
        }
        else
        {
            Debug.Log("End");
            _mic.End();
            CreateClip();
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
    
    public void Play()
    {
        Debug.Log("Play");
    }

    private void CreateClip()
    {
        Debug.Log("Create Clip");
    }

    private void OnData(float[] input)
    {
        Debug.Log(input.Length);
    }
}

}