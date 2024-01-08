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

}