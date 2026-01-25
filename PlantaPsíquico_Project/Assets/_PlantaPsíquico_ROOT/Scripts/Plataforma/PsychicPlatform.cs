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

    [Header("Animation")]
    [Tooltip("Curva de aceleraci�n para efecto de impulso (Ease In/Out)")]
    public AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Overshoot: Cu�nto sobrepasa el punto antes de regresar (da sensaci�n de peso)")]
    [Range(0f, 0.5f)] public float overshootAmount = 0.1f;

    [Tooltip("Tiempo del overshoot en segundos")]
    [Range(0.1f, 1f)] public float overshootDuration = 0.2f;

    [Header("Momentum Settings")]
    [Tooltip("Multiplicador de momentum transferido al jugador")]
    [Range(0.5f, 3f)] public float momentumMultiplier = 1.5f;

    [Tooltip("Ventana de tiempo (en segundos) para obtener el boost perfecto")]
    [Range(0.1f, 3f)] public float perfectTimingWindow = 0.8f;

    [Tooltip("Boost vertical extra cuando la plataforma sube")]
    [Range(1f, 3f)] public float verticalBoostMultiplier = 2f;

    [Header("Visual Feedback")]
    [Tooltip("Color cuando est� lista para dar boost")]
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
            Debug.LogError("�Asigna Point A y Point B en el inspector!");
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

        if (_isMoving)
        {
            _moveTimer += Time.deltaTime;

            if (!_isOvershooting)
            {
                // === MOVIMIENTO PRINCIPAL con curva de aceleraci�n ===
                float t = Mathf.Clamp01(_moveTimer / _totalMoveTime);
                float curvedT = accelerationCurve.Evaluate(t);

                transform.position = Vector3.Lerp(_startPosition, _nextPosition, curvedT);

                // Calcular ventana perfecta (ANTES de llegar al punto)
                float currentDistance = Vector3.Distance(transform.position, _nextPosition);
                _isPerfectTimingWindow = currentDistance <= perfectTimingWindow;

                // Feedback visual
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = _isPerfectTimingWindow ? readyColor : normalColor;
                }

                // Si llegamos al objetivo
                if (t >= 1f)
                {
                    if (overshootAmount > 0f)
                    {
                        // === INICIAR OVERSHOOT ===
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
                        // Sin overshoot, terminar
                        FinishMovement();
                    }
                }
            }
            else
            {
                // === FASE DE OVERSHOOT (rebote) ===
                float t = Mathf.Clamp01(_moveTimer / overshootDuration);

                // Curva suave para el overshoot (smoothstep)
                float overshootT = t * t * (3f - 2f * t);

                transform.position = Vector3.Lerp(_startPosition, _nextPosition, overshootT);

                // IMPORTANTE: La ventana perfecta TAMBI�N aplica durante el overshoot
                // Esto permite saltar justo DESPU�S de llegar al punto (como Celeste)
                _isPerfectTimingWindow = true; // Durante overshoot siempre es ventana perfecta

                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = readyColor; // Cyan durante overshoot
                }

                // Si termin� el overshoot
                if (t >= 1f)
                {
                    _isOvershooting = false;
                    FinishMovement();
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

    private void FinishMovement()
    {
        _isMoving = false;
        _waitTimer = waitTime;
        _velocity = Vector3.zero;

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

        // Ajustar posici�n final
        transform.position = realTarget;
        _startPosition = realTarget;
    }

    // M�todo llamado por el PsychicPlatformManager
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
            Debug.Log("Jugador subi� a la plataforma");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Verificar si el jugador est� saltando
            bool isJumping = Input.GetKeyDown(KeyCode.Space) ||
                           Input.GetKeyDown(KeyCode.C) ||
                           Input.GetKeyDown(KeyCode.J);

            // SIEMPRE dar boost al saltar desde plataforma en movimiento (como Celeste)
            if (isJumping && _isMoving && _playerRB != null)
            {
                // Si est�s en la ventana perfecta, dar boost completo
                if (_isPerfectTimingWindow)
                {
                    ApplyPerfectTimingBoost(_playerRB);
                }
                // Si no, dar boost reducido (50% del normal)
                else
                {
                    ApplyReducedBoost(_playerRB);
                }
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
            Debug.Log("Jugador sali� de la plataforma");
        }
    }

    private void ApplyPerfectTimingBoost(Rigidbody2D playerRB)
    {
        // Sistema SIMPLE y CONSISTENTE como Celeste
        Vector2 platformVel = _velocity;
        Vector2 currentPlayerVel = playerRB.linearVelocity;

        Vector2 boost = Vector2.zero;

        // HORIZONTAL: Transferir velocidad horizontal de la plataforma
        if (Mathf.Abs(platformVel.x) > 0.1f)
        {
            boost.x = platformVel.x * momentumMultiplier;
            Debug.Log($"Boost Horizontal: {boost.x:F2}");
        }

        // VERTICAL: Solo dar boost si la plataforma va hacia ARRIBA
        if (platformVel.y > 0.1f)
        {
            boost.y = platformVel.y * verticalBoostMultiplier;
            Debug.Log($"Boost Vertical: {boost.y:F2}");
        }

        // APLICAR el boost (reemplazar horizontal, a�adir vertical)
        playerRB.linearVelocity = new Vector2(
            boost.x != 0 ? boost.x : currentPlayerVel.x,
            currentPlayerVel.y + boost.y
        );

        Debug.Log($"�PERFECT TIMING! Velocidad final: {playerRB.linearVelocity}");
    }

    private void ApplyReducedBoost(Rigidbody2D playerRB)
    {
        // Boost reducido cuando saltas FUERA de la ventana perfecta
        Vector2 platformVel = _velocity;
        Vector2 currentPlayerVel = playerRB.linearVelocity;

        float reducedMultiplier = 0.5f; // 50% del boost normal
        Vector2 boost = Vector2.zero;

        // HORIZONTAL
        if (Mathf.Abs(platformVel.x) > 0.1f)
        {
            boost.x = platformVel.x * momentumMultiplier * reducedMultiplier;
        }

        // VERTICAL (solo si va hacia arriba)
        if (platformVel.y > 0.1f)
        {
            boost.y = platformVel.y * verticalBoostMultiplier * reducedMultiplier;
        }

        // APLICAR
        playerRB.linearVelocity = new Vector2(
            boost.x != 0 ? boost.x : currentPlayerVel.x,
            currentPlayerVel.y + boost.y
        );

        Debug.Log($"Boost reducido aplicado: {playerRB.linearVelocity}");
    }

    // Visualizaci�n en el editor
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

            // Dibujar l�nea de overshoot si est� configurado
            if (overshootAmount > 0f)
            {
                Gizmos.color = Color.yellow;
                Vector3 direction = (displayPointB - displayPointA).normalized;
                float distanceAB = Vector3.Distance(displayPointA, displayPointB);
                Vector3 overshootPoint = displayPointB + (direction * overshootAmount * distanceAB);
                Gizmos.DrawWireSphere(overshootPoint, 0.2f);
            }

            // Dibujar la ventana de timing perfecto
            if (Application.isPlaying && _isMoving && _isPerfectTimingWindow)
            {
                Gizmos.color = Color.cyan;
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
            Vector3 displayPointB = Application.isPlaying && _globalPointB != Vector3.zero ? _globalPointB : pointB.position;
            Gizmos.DrawWireSphere(displayPointB, perfectTimingWindow);
        }
    }
}