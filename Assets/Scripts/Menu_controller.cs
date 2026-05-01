using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Menu_controller : MonoBehaviour
{
    public void onstartclick()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void onexitclick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
