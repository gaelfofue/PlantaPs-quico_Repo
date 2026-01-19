using System.Collections.Generic;
using UnityEngine;

public class PsychicPlatformManager : MonoBehaviour
{
    public static PsychicPlatformManager Instance { get; private set; }

    private List<PsychicPlatform> _platforms = new List<PsychicPlatform>();

    [Header("Settings")]
    [Tooltip("Efecto visual/partículas al activar (opcional)")]
    public GameObject activationEffect;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterPlatform(PsychicPlatform platform)
    {
        if (!_platforms.Contains(platform))
        {
            _platforms.Add(platform);
            Debug.Log($"Plataforma registrada. Total: {_platforms.Count}");
        }
    }

    public void UnregisterPlatform(PsychicPlatform platform)
    {
        if (_platforms.Contains(platform))
        {
            _platforms.Remove(platform);
        }
    }

    public void ActivateAllPlatforms()
    {
        Debug.Log($"Activando {_platforms.Count} plataformas");

        foreach (PsychicPlatform platform in _platforms)
        {
            if (platform != null)
            {
                platform.Activate();
            }
        }

        // Efecto visual opcional
        if (activationEffect != null)
        {
            // Puedes instanciar partículas o efectos aquí
            // Instantiate(activationEffect, player.position, Quaternion.identity);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}