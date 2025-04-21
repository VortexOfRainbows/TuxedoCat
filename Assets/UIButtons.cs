using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIButtons : MonoBehaviour
{
    public GameObject[] lives;
    public GameObject dyingDisclaimer;
    public static bool gamePaused = false;
    public GameObject pauseMenuUI;
    public GameObject dimBG;
    public GameObject settingsMenu;
    public GameObject inventoryMenu;
    public static Vector3 Scale;
    void Start()
    {
        
    }
    void Update()
    {
        Scale = transform.localScale;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePaused)
            {
                Resume();
                if(settingsMenu.activeSelf)
                {
                    settingsMenu.SetActive(false);
                }
                Resume();
                if (inventoryMenu.activeSelf)
                {
                    inventoryMenu.SetActive(false);
                }
            }
            else
            {
                Pause();
            }
        }
        for(int i = 0; i < lives.Length; ++i)
        {
            lives[lives.Length - 1 - i].GetComponent<Image>().enabled = i < Player.Instance.life;
        }
        dyingDisclaimer.SetActive(Player.Instance.life <= 1);
    }
    public void Resume()
    {
        dimBG.SetActive(false);
        pauseMenuUI.SetActive(false);
        //pauseButton.SetActive(true);
        Time.timeScale = 1f;
        gamePaused = false;
    }
    public void Pause()
    {
        dimBG.SetActive(true);
        pauseMenuUI.SetActive(true);
        //pauseButton.SetActive(false);
        Time.timeScale = 0f;
        gamePaused = true;
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
