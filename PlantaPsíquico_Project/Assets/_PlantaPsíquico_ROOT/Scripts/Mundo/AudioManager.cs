using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Declaracion de singleton
    public static AudioManager Instance;

    [Header("Audio Source Refrences")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxScource;

    [Header("Audio clip Arrays")]
    public AudioClip[] musiclist;
    public AudioClip[] sfxlist;

    private void Awake()
    {
        //singleton que no se distruye entre escenas
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(int musicIndex)
    {
        musicSource.clip = musiclist[musicIndex];
        musicSource.Play();
    }

    public void PlaySFX(int sfxIndex)
    {
        sfxScource.PlayOneShot(sfxlist[sfxIndex]);
    }
}
