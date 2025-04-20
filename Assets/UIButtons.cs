using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UIButtons : MonoBehaviour
{
    public static bool gamePaused = false;
    public GameObject pauseMenuUI;
    public GameObject dimBG;
    public GameObject settingsMenu;
    public GameObject inventoryMenu;
    //public GameObject pauseButton;
    public static Vector3 Scale;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
