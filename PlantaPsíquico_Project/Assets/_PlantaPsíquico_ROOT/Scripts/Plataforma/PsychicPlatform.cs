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

    private Vector3 _positionA;
    private Vector3 _positionB;
    private Vector3 _targetPosition;
    private Vector3 _startPosition;
    private bool _isMoving = false;
    private bool _movingToB = true;
    private float _waitTimer = 0f;
    private Vector3 _previousPosition;
    private Vector3 _velocity;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private bool _isPerfectTimingWindow = false;

    // Para transferir momentum automáticamente
    private bool _playerOnPlatform = false;
    private Rigidbody2D _playerRB;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (pointA == null || pointB == null)
        {
            Debug.LogError("¡Asigna Point A y Point B en el inspector!");
            enabled = false;
            return;
        }

        // Configurar Rigidbody2D si existe
        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // GUARDAMOS LAS POSICIONES WORLD de A y B al inicio
        _positionA = pointA.position;
        _positionB = pointB.position;

        // Posicionar la plataforma en el punto A al inicio
        transform.position = _positionA;
        _startPosition = _positionA;
        _targetPosition = _positionB;
        _previousPosition = transform.position;
    }

    private void Start()
    {
        // Registrar esta plataforma en el manager
        PsychicPlatformManager.Instance?.RegisterPlatform(this);
    }

    private void Update()
    {
        if (!_isMoving)
        {
            // Resetear color cuando no se mueve
            if (_spriteRenderer != null)
                _spriteRenderer.color = normalColor;
            return;
        }

        // Calcular distancias
        float distanceToTarget = Vector3.Distance(transform.position, _targetPosition);
        float totalDistance = Vector3.Distance(_startPosition, _targetPosition);

        // Calcular si estamos en la ventana de timing perfecto
        // Ventana más generosa: basada en distancia absoluta
        _isPerfectTimingWindow = distanceToTarget <= perfectTimingWindow;

        // Feedback visual
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _isPerfectTimingWindow ? readyColor : normalColor;
        }

        // Debug visual en la consola
        if (_isPerfectTimingWindow && !_playerOnPlatform)
        {
            Debug.Log($"VENTANA PERFECTA ACTIVA - Distancia al objetivo: {distanceToTarget:F2}");
        }
    }

    private void FixedUpdate()
    {
        // Calcular velocidad ANTES de mover
        _velocity = (transform.position - _previousPosition) / Time.fixedDeltaTime;

        if (_isMoving)
        {
            // Mover la plataforma
            Vector3 newPosition = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.fixedDeltaTime);

            // Usar Rigidbody si existe, sino transform
            if (_rb != null)
            {
                _rb.MovePosition(newPosition);
            }
            else
            {
                transform.position = newPosition;
            }

            // Verificar si llegamos al objetivo
            if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
            {
                _isMoving = false;
                _waitTimer = waitTime;

                // Cambiar dirección
                if (_movingToB)
                {
                    _startPosition = _positionB;
                    _targetPosition = _positionA;
                }
                else
                {
                    _startPosition = _positionA;
                    _targetPosition = _positionB;
                }
                _movingToB = !_movingToB;
            }
        }
        else if (_waitTimer > 0)
        {
            // Manejar tiempo de espera
            _waitTimer -= Time.fixedDeltaTime;
        }

        // Transferir momentum automáticamente al jugador si está sobre la plataforma
        if (_playerOnPlatform && _playerRB != null && _isMoving)
        {
            TransferMomentumToPlayer();
        }

        _previousPosition = transform.position;
    }

    private void TransferMomentumToPlayer()
    {
        // Transferir velocidad de la plataforma al jugador (especialmente horizontal)
        float momentumTransfer = 0.8f; // Qué tanto momentum se transfiere

        if (Mathf.Abs(_velocity.x) > 0.1f)
        {
            _playerRB.linearVelocity = new Vector2(
                Mathf.Lerp(_playerRB.linearVelocity.x, _velocity.x * momentumTransfer, 0.5f),
                _playerRB.linearVelocity.y
            );
        }
    }

    // Método llamado por el PsychicPlatformManager
    public void Activate()
    {
        if (!_isMoving && _waitTimer <= 0)
        {
            _isMoving = true;
            Debug.Log($"Plataforma activada! Moviéndose hacia {(_movingToB ? "B" : "A")}");
        }
    }

    // Detectar cuando el jugador está sobre la plataforma
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerRB = collision.gameObject.GetComponent<Rigidbody2D>();
            _playerOnPlatform = true;
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

            // Si salta durante la ventana perfecta y la plataforma se está moviendo
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
            _playerOnPlatform = false;
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

            if (_movingToB == false)
            {
                Gizmos.DrawWireSphere(pointA.position, perfectTimingWindow);
            }
        }
    }
}