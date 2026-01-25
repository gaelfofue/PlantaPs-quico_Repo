using UnityEngine;
using System.Collections;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Death Settings")]
    public float respawnDelay = 0.2f;
    public GameObject deathParticlesPrefab;
    public AudioClip deathSound;
    public Transform currentCheckpoint;

    [Header("Visual Effects")]
    public float flashDuration = 0.1f;
    public Color flashColor = Color.red;
    public int flashCount = 3;
    public float deathAnimationDuration = 0.5f; // Duración de la animación de muerte

    [Header("Animation")]
    public string deathAnimationName = "Death"; // Nombre de la animación en el Animator

    // Componentes
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Animator animator;

    // Variables de estado
    private Vector3 originalSpawn;
    private bool isDead = false;
    private Color originalColor;
    private string currentAnimationState; // Para guardar la animación actual

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>(); // Obtener el Animator
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

        // Guardar la animación actual antes de morir
        if (animator != null)
        {
            // Esto es útil si quieres volver a la animación anterior después del respawn
            // currentAnimationState = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        }

        // Reproducir animación de muerte
        PlayDeathAnimation();

        // Efecto visual de parpadeo durante la muerte
        StartCoroutine(DeathSequence());

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

    private void PlayDeathAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(deathAnimationName))
        {
            // Método 1: Play directamente la animación por nombre
            animator.Play(deathAnimationName, 0, 0f);

            // Método 2: Si prefieres usar un trigger (si lo tienes en el Animator)
            // animator.SetTrigger("Death");

            Debug.Log("Playing death animation: " + deathAnimationName);
        }
        else
        {
            Debug.LogWarning("Animator o nombre de animación no configurado!");
        }
    }

    private IEnumerator DeathSequence()
    {
        // Crear partículas de muerte inmediatamente
        CreateDeathParticles();

        // Parpadeo durante la animación de muerte
        if (spriteRenderer != null)
        {
            for (int i = 0; i < flashCount; i++)
            {
                spriteRenderer.color = flashColor;
                yield return new WaitForSeconds(flashDuration / 2);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashDuration / 2);
            }
        }

        // Esperar el resto de la animación de muerte
        yield return new WaitForSeconds(deathAnimationDuration - (flashCount * flashDuration));

        // Hacer invisible al final de la animación
        if (spriteRenderer != null)
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
                // Hacer que las partículas salgan en todas direcciones
                var main = ps.main;
                main.startRotation = 0f;
                main.startSpeed = 5f;

                // Añadir algo de aleatoriedad
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.Local;

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

        // Hacer visible de nuevo
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }

        // Reactivar física
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector2.zero;
        }

        // Reactivar collider
        if (playerCollider != null)
            playerCollider.enabled = true;

        // Reactivar movimiento
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Resetear animaciones (opcional)
        if (animator != null)
        {
            // Método 1: Volver a la animación por defecto
            animator.Play("Idle", 0, 0f);

            // Método 2: Resetear todos los parámetros
            // animator.Rebind();
            // animator.Update(0f);
        }

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

    // Versión alternativa si quieres que la animación controle todo
    private IEnumerator DeathSequenceWithAnimation()
    {
        // Crear partículas
        CreateDeathParticles();

        // Esperar un poco para sincronizar con la animación
        yield return new WaitForSeconds(0.1f);

        // Parpadeo sincronizado con la animación
        if (spriteRenderer != null)
        {
            float elapsedTime = 0f;
            bool isFlashing = false;

            while (elapsedTime < deathAnimationDuration)
            {
                if (elapsedTime % (flashDuration * 2) < flashDuration)
                {
                    if (!isFlashing)
                    {
                        spriteRenderer.color = flashColor;
                        isFlashing = true;
                    }
                }
                else
                {
                    if (isFlashing)
                    {
                        spriteRenderer.color = originalColor;
                        isFlashing = false;
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Asegurar que vuelva al color original
            spriteRenderer.color = originalColor;
        }

        // Hacer invisible
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
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

    // Método para testing
    void Update()
    {
        // Solo para testing - remover después
        if (Input.GetKeyDown(KeyCode.T) && !isDead)
        {
            InstantDeath();
        }
    }
}