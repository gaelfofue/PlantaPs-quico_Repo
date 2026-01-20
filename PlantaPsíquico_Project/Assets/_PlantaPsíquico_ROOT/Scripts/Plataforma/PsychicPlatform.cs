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

    [Header("Impulse Motion")]
    [Tooltip("Curva de aceleración para efecto de impulso")]
    public AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Overshoot: Cuánto sobrepasa el punto antes de regresar")]
    [Range(0f, 0.5f)] public float overshootAmount = 0.1f;

    [Tooltip("Tiempo del overshoot en segundos")]
    [Range(0.1f, 1f)] public float overshootDuration = 0.2f;

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

    private Vector3 _globalPointA;
    private Vector3 _globalPointB;
    private Vector3 _nextPosition;
    private Vector3 _startPosition;
    private bool _isMoving = false;
    private bool _isOvershooting = false;
    private float _waitTimer = 0f;
    private float _moveTimer = 0f;
    private float _totalMoveTime = 1f;
    private Vector3 _previousPosition;
    private Vector3 _velocity;

    private SpriteRenderer _spriteRenderer;
    private bool _isPerfectTimingWindow = false;

    private Rigidbody2D _playerRB;
    private Transform _playerOriginalParent;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (pointA == null || pointB == null)
        {
            Debug.LogError("¡Asigna Point A y Point B en el inspector!");
            enabled = false;
            return;
        }

        // Guardar las posiciones GLOBALES INICIALES
        _globalPointA = pointA.position;
        _globalPointB = pointB.position;

        // Calcular tiempo total de movimiento basado en distancia y velocidad
        float distance = Vector3.Distance(_globalPointA, _globalPointB);
        _totalMoveTime = distance / moveSpeed;

        // Empezar en punto A
        transform.position = _globalPointA;
        _nextPosition = _globalPointB;
        _startPosition = _globalPointA;
        _previousPosition = transform.position;
    }

    private void Start()
    {
        // Registrar esta plataforma en el manager
        if (PsychicPlatformManager.Instance != null)
        {
            PsychicPlatformManager.Instance.RegisterPlatform(this);
        }
    }

    private void Update()
    {
        // Calcular velocidad (antes de mover)
        Vector3 currentPosition = transform.position;
        _velocity = (currentPosition - _previousPosition) / Time.deltaTime;
        _previousPosition = currentPosition;

        // Mantener puntos fijos
        if (pointA != null) pointA.position = _globalPointA;
        if (pointB != null) pointB.position = _globalPointB;

        if (_isMoving)
        {
            _moveTimer += Time.deltaTime;

            if (!_isOvershooting)
            {
                // MOVIMIENTO PRINCIPAL con curva de aceleración
                float t = Mathf.Clamp01(_moveTimer / _totalMoveTime);
                float curvedT = accelerationCurve.Evaluate(t);

                transform.position = Vector3.Lerp(_startPosition, _nextPosition, curvedT);

                // Calcular distancia para ventana perfecta
                float totalDistance = Vector3.Distance(_startPosition, _nextPosition);
                float currentDistance = Vector3.Distance(transform.position, _nextPosition);
                float normalizedDistance = currentDistance / totalDistance;
                _isPerfectTimingWindow = normalizedDistance <= 0.3f;

                // Feedback visual
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = _isPerfectTimingWindow ? readyColor : normalColor;
                }

                // Si llegamos al objetivo, iniciar overshoot o terminar
                if (t >= 1f)
                {
                    if (overshootAmount > 0f)
                    {
                        _isOvershooting = true;
                        _moveTimer = 0f;
                        Vector3 direction = (_nextPosition - _startPosition).normalized;
                        float distanceAB = Vector3.Distance(_globalPointA, _globalPointB);
                        Vector3 overshootTarget = _nextPosition + (direction * overshootAmount * distanceAB);
                        _startPosition = transform.position;
                        _nextPosition = overshootTarget;
                    }
                    else
                    {
                        // Sin overshoot, terminar movimiento
                        _isMoving = false;
                        _waitTimer = waitTime;

                        // Preparar siguiente movimiento
                        _nextPosition = (_nextPosition == _globalPointA) ? _globalPointB : _globalPointA;
                        _startPosition = transform.position;
                    }
                }
            }
            else
            {
                // FASE DE OVERSHOOT (rebote/sobrepaso)
                float t = Mathf.Clamp01(_moveTimer / overshootDuration);

                // Curva suave para el overshoot
                float overshootT = t * t * (3f - 2f * t); // SmoothStep manual

                transform.position = Vector3.Lerp(_startPosition, _nextPosition, overshootT);

                // Si terminó el overshoot, regresar al punto real
                if (t >= 1f)
                {
                    _isOvershooting = false;
                    _isMoving = false;
                    _waitTimer = waitTime;

                    // Determinar punto objetivo real
                    Vector3 realTarget;
                    if (Vector3.Distance(transform.position, _globalPointA) < Vector3.Distance(transform.position, _globalPointB))
                    {
                        realTarget = _globalPointA;
                        _nextPosition = _globalPointB;
                    }
                    else
                    {
                        realTarget = _globalPointB;
                        _nextPosition = _globalPointA;
                    }

                    // Ajustar posición final
                    transform.position = realTarget;
                    _startPosition = realTarget;
                }
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
            _moveTimer = 0f;
            _isOvershooting = false;
            _startPosition = transform.position;
            Debug.Log($"Plataforma activada con impulso!");
        }
    }

    // EXACTAMENTE como el tutorial: SetParent
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Guardar el parent original del jugador
            _playerOriginalParent = collision.gameObject.transform.parent;

            // Hacer hijo SOLO al jugador
            collision.gameObject.transform.SetParent(transform);
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
            // Restaurar el parent original del jugador
            collision.gameObject.transform.SetParent(_playerOriginalParent);
            _playerRB = null;
            Debug.Log("Jugador salió de la plataforma");
        }
    }

    private void ApplyPerfectTimingBoost(Rigidbody2D playerRB)
    {
        // Calcular boost basado en la velocidad ACTUAL de la plataforma
        Vector2 boost = _velocity * momentumMultiplier;

        // EXTRA: Añadir boost basado en la dirección de movimiento
        Vector3 moveDirection = (_nextPosition - _startPosition).normalized;

        // Boost direccional adicional durante el impulso
        if (_isPerfectTimingWindow && _isMoving)
        {
            // Aumentar boost en la dirección del movimiento
            boost.x += moveDirection.x * moveSpeed * 0.3f;
            boost.y += moveDirection.y * moveSpeed * 0.3f;
        }

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
            // Usar las posiciones globales si estamos en play mode
            Vector3 displayPointA = Application.isPlaying && _globalPointA != Vector3.zero ? _globalPointA : pointA.position;
            Vector3 displayPointB = Application.isPlaying && _globalPointB != Vector3.zero ? _globalPointB : pointB.position;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(displayPointA, displayPointB);
            Gizmos.DrawWireSphere(displayPointA, 0.3f);
            Gizmos.DrawWireSphere(displayPointB, 0.3f);

            // Dibujar línea de overshoot si está configurado
            if (overshootAmount > 0f)
            {
                Gizmos.color = Color.yellow;
                Vector3 direction = (displayPointB - displayPointA).normalized;
                float distanceAB = Vector3.Distance(displayPointA, displayPointB);
                Vector3 overshootPoint = displayPointB + (direction * overshootAmount * distanceAB);
                Gizmos.DrawWireSphere(overshootPoint, 0.2f);

                // Dibujar línea punteada manualmente
                DrawDashedLine(displayPointB, overshootPoint, 0.5f);
            }

            // Dibujar la ventana de timing perfecto
            if (Application.isPlaying && _isMoving && _isPerfectTimingWindow)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }

    // Método auxiliar para dibujar líneas discontinuas
    private void DrawDashedLine(Vector3 start, Vector3 end, float dashLength)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        int segments = Mathf.FloorToInt(distance / dashLength);

        for (int i = 0; i < segments; i += 2)
        {
            float startOffset = i * dashLength;
            float endOffset = Mathf.Min((i + 1) * dashLength, distance);

            if (startOffset < distance)
            {
                Vector3 dashStart = start + direction * startOffset;
                Vector3 dashEnd = start + direction * endOffset;
                Gizmos.DrawLine(dashStart, dashEnd);
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
            Vector3 displayPointB = Application.isPlaying && _globalPointB != Vector3.zero ? _globalPointB : pointB.position;
            Gizmos.DrawWireSphere(displayPointB, perfectTimingWindow);
        }
    }
}