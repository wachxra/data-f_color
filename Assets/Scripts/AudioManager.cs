using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SoundEffect
{
    public string keyId;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(-3f, 3f)]
    public float pitch = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Sound Effects List")]
    public List<SoundEffect> sfxList = new List<SoundEffect>();
    private Dictionary<string, SoundEffect> sfxDict = new Dictionary<string, SoundEffect>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (var sfx in sfxList)
        {
            if (!sfxDict.ContainsKey(sfx.keyId))
                sfxDict.Add(sfx.keyId, sfx);
            else
                Debug.LogWarning($"Duplicate SFX keyId: {sfx.keyId}");
        }
    }

    public void PlaySFX(string keyId)
    {
        if (sfxSource == null) return;

        if (sfxDict.TryGetValue(keyId, out SoundEffect sfx))
        {
            sfxSource.pitch = sfx.pitch;
            sfxSource.PlayOneShot(sfx.clip, sfx.volume);
        }
        else
        {
            Debug.LogWarning($"SFX Key not found: {keyId}");
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
    }
}