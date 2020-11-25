using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void MainPlayButton()
    {
        SceneManager.LoadScene("1Level");
    }
}
