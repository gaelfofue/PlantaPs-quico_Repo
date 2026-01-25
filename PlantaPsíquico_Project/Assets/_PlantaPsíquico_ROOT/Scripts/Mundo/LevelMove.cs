using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMove : MonoBehaviour
{
    public int sceneBuilIndex;

    private void OnTriggerEnter2D (Collider2D other)
    {
        print("Trigger Entered");

        if(other.tag == "Player") {
            print("Switching Scene to" + sceneBuilIndex);
            SceneManager.LoadScene(sceneBuilIndex, LoadSceneMode.Single);
        }
    }
}
