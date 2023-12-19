using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class loadingScript : MonoBehaviour
{
    private bool bShowing;

    [SerializeField] GameObject[] scenes;
    [SerializeField] Image currentBackground;
    [SerializeField] GameObject currentScene;

    public void show()
    {
        if (currentBackground.fillAmount < 0.96f) 
            GetNewScene();

        SetBackgroundState(true);
    }

    public void hide()
    {
        SetBackgroundState(false);
    }

    private void SetBackgroundState(bool state)
    {
        bShowing = state;
        currentBackground.raycastTarget = state;
        currentBackground.fillOrigin = (state) ? 0 : 1;
    }

    private void GetNewScene()
    {
        if (currentScene != null)
            currentScene.SetActive(false);

        int random = Random.Range(0, scenes.Length);
        currentScene = scenes[random];
        currentScene.SetActive(true);

        currentBackground = currentScene.GetComponent<Image>();
    }

    void Update()
    {
        if (currentBackground == null) return;

        int targetValue = (bShowing) ? 1 : 0;
        currentBackground.fillAmount = Mathf.Lerp(currentBackground.fillAmount, targetValue, 9 * Time.deltaTime);

        float transparency = Mathf.Lerp(currentBackground.color.a, targetValue, 9 * Time.deltaTime);
        currentBackground.color = new Color(1, 1, 1, transparency);
    }
}
