using UnityEngine;
using UnityEngine.SceneManagement;

public class SoloButton : MonoBehaviour
{
    public void onClick()
    {
        SceneManager.LoadScene("Solo", LoadSceneMode.Single);
    }
}
