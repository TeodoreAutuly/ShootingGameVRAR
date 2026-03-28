using UnityEngine;

public class AudioZone : MonoBehaviour
{
    [Header("Tag du joueur")]
    public string playerTag = "Player";

    private AudioSource[] _audioSources;

    void Awake()
    {
        _audioSources = GetComponents<AudioSource>();

        foreach (var source in _audioSources)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0f;
        }

        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"TriggerEnter : {gameObject.name}");
            AudioManager.Instance?.EnterZone(_audioSources);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"TriggerExit : {gameObject.name}");
            AudioManager.Instance?.ExitZone(_audioSources);
        }
    }
}