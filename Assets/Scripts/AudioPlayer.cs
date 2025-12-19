using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    void Awake() {
        if (Instance==null) {
            Instance=this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public void PlayAudio(int clipId, float volume = 1.0f) {
        if (clipId>=0&&clipId<audioClips.Length) {
            audioSource.PlayOneShot(audioClips[clipId], volume);
        }
    }
}
