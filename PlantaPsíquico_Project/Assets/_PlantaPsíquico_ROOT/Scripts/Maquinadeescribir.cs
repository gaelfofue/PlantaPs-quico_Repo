using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    public float velocidad = 0.07f;
    private TextMeshProUGUI textoTMP;
    private string textoCompleto;

    void Awake()
    {
        textoTMP = GetComponent<TextMeshProUGUI>();
        textoCompleto = textoTMP.text;
        textoTMP.text = "";
    }

    void Start()
    {
        StartCoroutine(EscribirTexto());
    }

    IEnumerator EscribirTexto()
    {
        foreach (char letra in textoCompleto)
        {
            textoTMP.text += letra;
            yield return new WaitForSeconds(velocidad);
        }
    }
}

