using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuScript : MonoBehaviour
{
    public void LoadSoloScene()
    {
        SceneManager.LoadScene("Solo");
    }

    public void LoadGroupScene()
    {
        SceneManager.LoadScene("Group");
    }
}
