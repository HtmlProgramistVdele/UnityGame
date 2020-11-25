using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class NextLevelLoad : MonoBehaviour
{
    [SerializeField] string m_sceneName;
    private void OnTriggerEnter2D(Collider2D other)
    {
        SceneManager.LoadScene(m_sceneName);
    }
}
