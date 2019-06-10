using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityUtility;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private GameObject startPanel;
    [SerializeField]
    private Button exitButtion;
    [SerializeField]
    private GameObject pausePanel;
    [SerializeField]
    private Button pauseExitButton;
    [SerializeField]
    private BoolEvent onPause;
    private bool isPause = false;


    private void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClick);
        exitButtion.onClick.AddListener(OnExitButtionClick);
    }

    private void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            UsePausePanel();
        }
    }

    private void OnStartButtonClick()
    {
        SceneManager.LoadScene(1);
    }

    private void OnExitButtionClick()
    {
        Application.Quit();
    }

    public void UsePausePanel()
    {
        isPause = !isPause;
        pausePanel.SetActive(isPause);
        Time.timeScale = isPause ? 0 : 1;
        onPause.Invoke(isPause);
    }


}
