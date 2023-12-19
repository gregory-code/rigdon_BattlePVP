using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class muteButton : MonoBehaviour
{
    [SerializeField] GameObject openMic;
    [SerializeField] GameObject mutedMic;

    [SerializeField] musicSlider[] musicSliders;

    bool isMuted;

    public void ClickMute()
    {
        foreach(musicSlider slider in musicSliders)
        {
            if(isMuted)
            {
                slider.SetMusicLevel(1);
                isMuted = true;
            }
            else
            {
                slider.SetMusicLevel(0.0001f);
                isMuted = false;
            }
        }

        CheckMute();
    }

    public void CheckMute()
    {
        foreach(musicSlider slider in musicSliders)
        {
            if(slider.GetSliderLevel() > 0.0001f)
            {
                SetMute(false);
                return;
            }
        }

        SetMute(true);
    }

    private void SetMute(bool state)
    {
        isMuted = state;
        openMic.SetActive(!state);
        mutedMic.SetActive(state);
    }
}
