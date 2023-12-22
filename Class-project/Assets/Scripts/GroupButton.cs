using UnityEngine;
using UnityEngine.SceneManagement;

public class GroupButton : MonoBehaviour
{
    public void onClick()
    {
        SceneManager.LoadScene("Group", LoadSceneMode.Single);
    }
}
