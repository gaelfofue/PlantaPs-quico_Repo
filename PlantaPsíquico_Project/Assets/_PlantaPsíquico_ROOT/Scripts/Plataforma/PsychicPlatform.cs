using UnityEngine;

public class PsychicPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Punto inicial de la plataforma")]
    public Transform pointA;

    [Tooltip("Punto final de la plataforma")]
    public Transform pointB;

    [Tooltip("Velocidad de movimiento de la plataforma")]
    public float moveSpeed = 10f;

    [Tooltip("Tiempo que espera en cada punto antes de regresar")]
    public float waitTime = 0.5f;

    [Header("Momentum Settings")]
    [Tooltip("Multiplicador de momentum transferido al jugador")]
    [Range(0.5f, 3f)] public float momentumMultiplier = 1.5f;

    [Tooltip("Ventana de tiempo (en segundos) para obtener el boost perfecto")]
    [Range(0.1f, 1f)] public float perfectTimingWindow = 0.3f;

    [Tooltip("Boost vertical extra cuando la plataforma sube")]
    [Range(1f, 3f)] public float verticalBoostMultiplier = 2f;

    [Header("Visual Feedback")]
    [Tooltip("Color cuando está lista para dar boost")]
    public Color readyColor = Color.cyan;

    [Tooltip("Color normal")]
    public Color normalColor = Color.white;

    private Vector3 nextPosition;
    private bool _isMoving = false;
    private float _waitTimer = 0f;
    private Vector3 _previousPosition;
    private Vector3 _velocity;

    private SpriteRenderer _spriteRenderer;
    private bool _isPerfectTimingWindow = false;

    private Rigidbody2D _playerRB;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (pointA == null || pointB == null)
        {
            Debug.LogError("¡Asigna Point A y Point B en el inspector!");
            enabled = false;
            return;
        }

        // Empezar en punto A
        transform.position = pointA.position;
        nextPosition = pointB.position;
        _previousPosition = transform.position;
    }

    private void Start()
    {
        // Registrar esta plataforma en el manager
        PsychicPlatformManager.Instance?.RegisterPlatform(this);
    }

    private void Update()
    {
        // Calcular velocidad
        _velocity = (transform.position - _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;

        if (_isMoving)
        {
            // MOVER LA PLATAFORMA (igual que el tutorial)
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

            // Calcular si estamos en la ventana de timing perfecto
            float distanceToTarget = Vector3.Distance(transform.position, nextPosition);
            _isPerfectTimingWindow = distanceToTarget <= perfectTimingWindow;

            // Feedback visual
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _isPerfectTimingWindow ? readyColor : normalColor;
            }

            // Si llegamos al objetivo
            if (transform.position == nextPosition)
            {
                _isMoving = false;
                _waitTimer = waitTime;

                // Alternar entre A y B
                nextPosition = (nextPosition == pointA.position) ? pointB.position : pointA.position;
            }
        }
        else
        {
            // Resetear color cuando no se mueve
            if (_spriteRenderer != null)
                _spriteRenderer.color = normalColor;

            // Countdown del timer
            if (_waitTimer > 0)
            {
                _waitTimer -= Time.deltaTime;
            }
        }
    }

    // Método llamado por el PsychicPlatformManager
    public void Activate()
    {
        if (!_isMoving && _waitTimer <= 0)
        {
            _isMoving = true;
            Debug.Log($"Plataforma activada!");
        }
    }

    // EXACTAMENTE como el tutorial: SetParent
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.parent = transform;
            _playerRB = collision.gameObject.GetComponent<Rigidbody2D>();
            Debug.Log("Jugador subió a la plataforma");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Verificar si el jugador está saltando
            bool isJumping = Input.GetKeyDown(KeyCode.Space) ||
                           Input.GetKeyDown(KeyCode.C) ||
                           Input.GetKeyDown(KeyCode.J);

            // Si salta durante la ventana perfecta
            if (isJumping && _isPerfectTimingWindow && _isMoving && _playerRB != null)
            {
                ApplyPerfectTimingBoost(_playerRB);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.parent = null;
            _playerRB = null;
            Debug.Log("Jugador salió de la plataforma");
        }
    }

    private void ApplyPerfectTimingBoost(Rigidbody2D playerRB)
    {
        // Calcular boost basado en la velocidad de la plataforma
        Vector2 boost = _velocity * momentumMultiplier;

        // Si la plataforma se mueve hacia arriba, dar boost vertical EXTRA
        if (_velocity.y > 0.5f)
        {
            boost.y *= verticalBoostMultiplier;
            Debug.Log($"¡PERFECT TIMING BOOST VERTICAL! Boost: {boost}");
        }
        // Si se mueve horizontal, dar boost horizontal
        else if (Mathf.Abs(_velocity.x) > 0.5f)
        {
            boost.x *= 1.3f;
            Debug.Log($"¡PERFECT TIMING BOOST HORIZONTAL! Boost: {boost}");
        }
        else
        {
            Debug.Log($"¡PERFECT TIMING BOOST! Boost: {boost}");
        }

        // AGREGAR el boost a la velocidad actual (no reemplazar)
        playerRB.linearVelocity += boost;
    }

    // Visualización en el editor
    private void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);

            // Dibujar la ventana de timing perfecto
            if (Application.isPlaying && _isMoving)
            {
                Gizmos.color = _isPerfectTimingWindow ? Color.cyan : Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Mostrar el radio de la ventana perfecta
        if (pointA != null && pointB != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan transparente

            // Dibujar esfera en el punto B mostrando la ventana
            Gizmos.DrawWireSphere(pointB.position, perfectTimingWindow);
        }
    }
}