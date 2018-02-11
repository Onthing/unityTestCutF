
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStatics : MonoBehaviour
{
    static PlayerCharacter player;
    public static PlayerCharacter Player {
        get {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacter>();
            return player;
        }
    }

    private static AudioSource audioSource2D;

    public static void Play2D(AudioClip clip, float volume)
    {
        if (audioSource2D)
            audioSource2D.PlayOneShot(clip, volume);
    }

    public static void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void Awake()
    {
        audioSource2D = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }


}
