using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Death Settings")]
    public float respawnDelay = 0.2f;
    public GameObject deathParticlesPrefab; // Cambié a prefab
    public AudioClip deathSound;
    public Transform currentCheckpoint;

    [Header("Visual Effects")]
    public float flashDuration = 0.1f;
    public Color flashColor = Color.red;
    public int flashCount = 3;

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Vector3 originalSpawn;
    private bool isDead = false;
    private Color originalColor;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        originalSpawn = transform.position;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    // Método que llamarán los pinchos
    public void InstantDeath()
    {
        if (isDead) return;

        Debug.Log("Player died!");
        isDead = true;

        // Sonido de muerte
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(0);
        else if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        // Efecto visual de parpadeo antes de desaparecer
        StartCoroutine(DeathFlash());

        // Crear partículas de muerte
        CreateDeathParticles();

        // Desactivar movimiento
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Detener física
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Desactivar collider
        if (playerCollider != null)
            playerCollider.enabled = false;

        // Respawn después del delay
        Invoke(nameof(Respawn), respawnDelay);
    }

    private System.Collections.IEnumerator DeathFlash()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration / 2);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration / 2);
        }

        // Hacer invisible después del parpadeo
        spriteRenderer.enabled = false;
    }

    private void CreateDeathParticles()
    {
        if (deathParticlesPrefab != null)
        {
            GameObject particles = Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

            // Configurar partículas
            ParticleSystem ps = particles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Asegurar que miren hacia arriba o según necesites
                var main = ps.main;
                main.startRotation = 0f;

                ps.Play();

                // Auto-destrucción
                Destroy(particles, main.duration + main.startLifetime.constantMax);
            }

            Debug.Log("Death particles created at: " + transform.position);
        }
        else
        {
            Debug.LogWarning("No deathParticlesPrefab assigned!");
        }
    }

    private void Respawn()
    {
        Debug.Log("Respawning player...");

        // Posición de respawn
        Vector3 respawnPos = currentCheckpoint != null ?
            currentCheckpoint.position : originalSpawn;

        transform.position = respawnPos;

        // Reactivar todo
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector2.zero;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }

        if (playerCollider != null)
            playerCollider.enabled = true;

        if (playerMovement != null)
            playerMovement.enabled = true;

        isDead = false;

        Debug.Log("Player respawned at: " + respawnPos);
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint != null)
        {
            currentCheckpoint = checkpoint;
            Debug.Log("Checkpoint set to: " + checkpoint.name);
        }
    }

    // Para debugging
    void OnDrawGizmosSelected()
    {
        if (currentCheckpoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentCheckpoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, currentCheckpoint.position);
        }
    }
}