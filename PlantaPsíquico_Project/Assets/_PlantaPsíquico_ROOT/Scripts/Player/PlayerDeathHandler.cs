using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Death Settings")]
    public float respawnDelay = 0.2f;
    public ParticleSystem deathParticles;
    public AudioClip deathSound;
    public Transform currentCheckpoint;

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Vector3 originalSpawn;
    private bool isDead = false;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        originalSpawn = transform.position;
    }

    // Método que llamarán los pinchos
    public void InstantDeath()
    {
        if (isDead) return;

        isDead = true;
        //Create deathParticles

        // Desactivar movimiento
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Detener física
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Hacer invisible
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        // Desactivar collider
        if (playerCollider != null)
            playerCollider.enabled = false;

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        // Respawn después del delay
        Invoke("Respawn", respawnDelay);
    }

    private void Respawn()
    {
        // Posición de respawn
        Vector3 respawnPos = currentCheckpoint != null ?
            currentCheckpoint.position : originalSpawn;

        transform.position = respawnPos;

        // Reactivar todo
        if (rb != null)
            rb.isKinematic = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (playerCollider != null)
            playerCollider.enabled = true;

        if (playerMovement != null)
            playerMovement.enabled = true;

        // Resetear velocidad
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        isDead = false;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
    }

    void CreateDeathParticles()
    {
        deathParticles.Play();
    }

}
