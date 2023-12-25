using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StartButton : MonoBehaviour
{
    void Start()
    {
        var button = GetComponent<Button>();

        button.onClick.AddListener(ChangeTitle);
    }

    void ChangeTitle()
    {
        SceneManager.LoadScene("MainScene");
    }
}
