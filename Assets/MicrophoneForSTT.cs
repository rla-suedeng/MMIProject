using UnityEngine;
using System.Collections;

public class MicrophoneForSTT : MonoBehaviour
{
    private AudioClip audioClip;
    private GoogleSTTService speechToText;

    void Start()
    {
        speechToText = GetComponent<GoogleSTTService>();
        StartCoroutine(StartRecording());
    }

    private IEnumerator StartRecording()
    {
        // Start recording from the microphone
        audioClip = Microphone.Start(null, true, 10, 16000);
        yield return new WaitForSeconds(10);
        Microphone.End(null);

        // Get audio data
        float[] samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0);
        byte[] audioData = ConvertAudioClipToByteArray(samples);

        // Send audio data for speech recognition
        speechToText.RecognizeSpeech(audioData);
    }

    private byte[] ConvertAudioClipToByteArray(float[] samples)
    {
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];
        int rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = System.BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        return bytesData;
    }
}
