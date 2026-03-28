using UnityEngine;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Son par défaut")]
    public AudioSource defaultAudioSource;

    [Header("Transition")]
    [Range(0.1f, 5f)]
    public float fadeSpeed = 1.5f;

    [Header("Volume cible des zones")]
    [Range(0f, 1f)]
    public float zoneTargetVolume = 1f;

    private AudioSource[] _currentZoneAudios = null;
    private bool _inZone = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        // Fade du son par défaut
        if (_inZone)
        {
            // Fade out default
            defaultAudioSource.volume = Mathf.MoveTowards(
                defaultAudioSource.volume, 0f, fadeSpeed * Time.deltaTime);
        }
        else
        {
            // Fade in default
            defaultAudioSource.volume = Mathf.MoveTowards(
                defaultAudioSource.volume, 1f, fadeSpeed * Time.deltaTime);
        }

        // Fade in des sons de zone
        if (_currentZoneAudios != null)
        {
            foreach (var source in _currentZoneAudios)
            {
                source.volume = Mathf.MoveTowards(
                    source.volume, zoneTargetVolume, fadeSpeed * Time.deltaTime);
            }
        }
    }

    public void EnterZone(AudioSource[] zoneAudios)
    {
        // Stoppe ET remet le volume à 0 pour les anciens sons
        if (_currentZoneAudios != null)
        {
            foreach (var source in _currentZoneAudios)
            {
                source.Stop();
                source.volume = 0f;
            }
        }

        _currentZoneAudios = zoneAudios;
        _inZone = true;

        // Démarre les nouveaux sons à volume 0 → fade in géré dans Update
        foreach (var source in _currentZoneAudios)
        {
            source.volume = 0f;
            if (!source.isPlaying)
                source.Play();
        }

        Debug.Log($"EnterZone : {zoneAudios[0].gameObject.name}");
    }

    public void ExitZone(AudioSource[] zoneAudios)
    {
        // Vérifie que c'est bien la zone courante qui se termine
        if (_currentZoneAudios == null) return;
        if (!_currentZoneAudios.SequenceEqual(zoneAudios)) return;

        foreach (var source in _currentZoneAudios)
        {
            source.Stop();
            source.volume = 0f;
        }

        _currentZoneAudios = null;
        _inZone = false;

        Debug.Log("ExitZone → retour son par défaut");
    }
}