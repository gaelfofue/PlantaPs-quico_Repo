using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TextTrans : MonoBehaviour
{
    public TextMeshProUGUI[] texts;
    public int index = 0;
    public string nextScene = "Level1"; // ← ¡CAMBIAR AQUÍ!

    private int clickCounter = 0;
    private const int MAX_CLICKS = 10;

    void Start()
    {
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].gameObject.SetActive(i == 0);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickCounter++;

            // Si ya hicimos 9 clics, cargar el nivel
            if (clickCounter >= MAX_CLICKS)
            {
                SceneManager.LoadScene(nextScene);
                return;
            }

            // Si no, cambiar texto normalmente
            ChangeText();
        }
    }

    void ChangeText()
    {
        texts[index].gameObject.SetActive(false);
        index++;

        if (index >= texts.Length)
        {
            // Si terminamos todos los textos, también cargar el nivel
            SceneManager.LoadScene(nextScene);
            return;
        }

        texts[index].gameObject.SetActive(true);
    }
}
