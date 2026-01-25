using UnityEngine;
using TMPro;

public class TextTrans : MonoBehaviour
{
    public TextMeshProUGUI[] texts;
    public int index = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].gameObject.SetActive(i == 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ChangeText();
        }
    }

    void ChangeText()
    {
        texts[index].gameObject.SetActive(false);

        index++;

        if (index >= texts.Length)
        {
            index = 0; // vuelve al inicio (opcional)
        }

        texts[index].gameObject.SetActive(true);
    }
}
