using UnityEngine;

namespace uMicrophoneWebGL.Samples
{
    
public class MicrophoneWaveform : MonoBehaviour
{
    [Header("Components")]
    public LineRenderer lineRenderer;
    
    [Header("Line")]
    public float width = 10f;
    public float height = 1f;

    public void OnData(float[] input)
    {
        if (!lineRenderer) return;
        
        var pos = transform.localPosition;
        int n = input.Length;
        var dx = new Vector3(0f, 0f, width / n);
        
        var list = new Vector3[n];
        
        for (int i = 0; i < n; ++i)
        {
            var val = input[i];
            var dy = new Vector3(0f, val * height, 0f);
            list[i] = pos + dy;
            pos += dx;
        }
        
        lineRenderer.positionCount = n;
        lineRenderer.SetPositions(list);
    }
}

}