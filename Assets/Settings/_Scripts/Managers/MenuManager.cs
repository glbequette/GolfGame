using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // This function now takes a "string" (text) as a parameter.
    // Whatever word the button sends to this function, Unity will load that scene!
    public void LoadSpecificScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}