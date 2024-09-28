using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    public void RestartGameWaitForFourSeconds()
    {
        StartCoroutine("StartGame");
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(4);
        Debug.Log("Restart Game");
        SceneManager.LoadScene("Move");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
