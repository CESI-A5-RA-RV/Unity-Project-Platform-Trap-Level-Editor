using UnityEngine;

public class QuitApplication : MonoBehaviour
{
    public void QuitApp()
    {
        Debug.Log("Application is quitting...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
