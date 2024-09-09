using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MicrophoneInput : MonoBehaviour
{
    public Button myButton;
    public float activationThreshold = 0.2f;

    private AudioClip micClip;
    private string micName;
    private bool micInitialized;
    public BattleSystem battleSystem;
  void Start()
    {
        InitializeMicrophone();
        myButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (micInitialized)
        {
            float volume = GetAverageVolume();
            if (volume > activationThreshold)
            {
                ActivateButton();
            }
            else
            {
                DeactivateButton();
            }

            if (volume > 0.1f){
                battleSystem.VoiceCommandReceived();
            }
        }
    }

    void InitializeMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            micClip = Microphone.Start(micName, true, 1, 44100);
            micInitialized = true;
        }
        else
        {
            Debug.LogError("No microphone detected.");
        }
    }

    float GetAverageVolume()
    {
        float[] data = new float[256];
        int micPosition = Microphone.GetPosition(micName) - (256 + 1);
        if (micPosition < 0) return 0;
        micClip.GetData(data, micPosition);

        float sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += Mathf.Abs(data[i]);
        }
        return sum / data.Length;
    }

    void ActivateButton()
    {
        myButton.gameObject.SetActive(true);
        myButton.interactable = true;
    }

    void DeactivateButton()
    {
        myButton.gameObject.SetActive(false);
    }

    void OnApplicationQuit()
    {
        Microphone.End(micName);
    }
}