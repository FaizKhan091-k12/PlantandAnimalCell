using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip right;
    [SerializeField] AudioClip wrong;

    void Awake()
    {
            Instance= this;
        audioSource = GetComponent<AudioSource>();
    }


    public void PlayRightAnswer()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(right);
    }

public void PlayWrongAnswer()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(wrong);
    }
}
