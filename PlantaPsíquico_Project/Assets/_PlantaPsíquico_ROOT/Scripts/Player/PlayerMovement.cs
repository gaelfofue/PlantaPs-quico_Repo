using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public PlayerData Data;

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    public Animator Animator { get; private set; }
    #endregion

    #region STATE PARAMETERS
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsSliding { get; private set; }

    // Timers
    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    // Jump
    private bool _isJumpCut;
    private bool _isJumpFalling;

    // Wall Jump
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    // Psychic Power (reemplaza al Dash)
    private bool _hasPsychicCharge;
    private bool _psychicRecharging;

    // Animation States
    private string _currentState;
    private bool _isLanding;
    #endregion

    #region INPUT PARAMETERS
    private Vector2 _moveInput;
    public float LastPressedJumpTime { get; private set; }
    public float LastPressedPsychicTime { get; private set; }
    #endregion

    #region CHECK PARAMETERS
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    // Constantes para nombres de animaciones
    private const string IDLE_ANIM = "Idle";
    private const string RUN_ANIM = "Run";
    private const string JUMP_ANIM = "Jump";
    private const string FALL_ANIM = "Fall";
    private const string LAND_ANIM = "Land";
    private const string WALL_SLIDE_ANIM = "WallSlide";
    private const string PSYCHIC_ANIM = "PsychicPower";

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
    }

    private void Start()
    {
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
        _hasPsychicCharge = true;
        _currentState = IDLE_ANIM;
    }

    private void Update()
    {
        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
        LastPressedPsychicTime -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.x != 0)
            CheckDirectionToFace(_moveInput.x > 0);

        // Jump Input
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J))
        {
            OnJumpInput();
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J))
        {
            OnJumpUpInput();
        }

        // Psychic Power Input
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K))
        {
            OnPsychicInput();
        }
        #endregion

        #region COLLISION CHECKS
        if (!IsJumping)
        {
            // Ground Check
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
            {
                if (LastOnGroundTime < -0.1f && !_isLanding)
                {
                    // Aterrizaje
                    _isLanding = true;
                    ChangeAnimationState(LAND_ANIM);
                    StartCoroutine(ResetLanding());
                }
                LastOnGroundTime = Data.coyoteTime;
            }

            // Wall Checks
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
                LastOnWallRightTime = Data.coyoteTime;

            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
                LastOnWallLeftTime = Data.coyoteTime;

            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        #endregion

        #region JUMP CHECKS
        if (IsJumping && RB.linearVelocity.y < 0)
        {
            IsJumping = false;
            _isJumpFalling = true;
        }

        if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
        {
            IsWallJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
            _isJumpCut = false;
            _isJumpFalling = false;
        }

        // Jump
        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            IsWallJumping = false;
            _isJumpCut = false;
            _isJumpFalling = false;
            Jump();
            ChangeAnimationState(JUMP_ANIM);
        }
        // Wall Jump
        else if (CanWallJump() && LastPressedJumpTime > 0)
        {
            IsWallJumping = true;
            IsJumping = false;
            _isJumpCut = false;
            _isJumpFalling = false;
            _wallJumpStartTime = Time.time;
            _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;
            WallJump(_lastWallJumpDir);
            ChangeAnimationState(JUMP_ANIM);
        }
        #endregion

        #region PSYCHIC POWER CHECKS
        if (CanUsePsychicPower() && LastPressedPsychicTime > 0)
        {
            UsePsychicPower();
            ChangeAnimationState(PSYCHIC_ANIM);
        }
        #endregion

        #region SLIDE CHECKS
        if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
            IsSliding = true;
        else
            IsSliding = false;
        #endregion

        #region ANIMATION UPDATE
        UpdateAnimationState();
        #endregion

        #region GRAVITY
        if (IsSliding)
        {
            SetGravityScale(0);
        }
        else if (RB.linearVelocity.y < 0 && _moveInput.y < 0)
        {
            SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
            RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFastFallSpeed));
        }
        else if (_isJumpCut)
        {
            SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
            RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFallSpeed));
        }
        else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < Data.jumpHangTimeThreshold)
        {
            SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
        }
        else if (RB.linearVelocity.y < 0)
        {
            SetGravityScale(Data.gravityScale * Data.fallGravityMult);
            RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFallSpeed));
        }
        else
        {
            SetGravityScale(Data.gravityScale);
        }
        #endregion
    }

    private void UpdateAnimationState()
    {
        // Si estamos en una animación que no debe ser interrumpida, salir
        if (_currentState == LAND_ANIM || _currentState == PSYCHIC_ANIM)
            return;

        // Determinamos la animación basada en el estado actual
        string newState = IDLE_ANIM;

        if (IsSliding)
        {
            newState = WALL_SLIDE_ANIM;
        }
        else if (LastOnGroundTime > 0)
        {
            // En el suelo
            if (Mathf.Abs(_moveInput.x) > 0.1f)
                newState = RUN_ANIM;
            else
                newState = IDLE_ANIM;
        }
        else
        {
            // En el aire
            if (RB.linearVelocity.y > 0.1f)
                newState = JUMP_ANIM;
            else if (RB.linearVelocity.y < -0.1f)
                newState = FALL_ANIM;
        }

        // Cambiar animación si es diferente a la actual
        ChangeAnimationState(newState);
    }

    private void ChangeAnimationState(string newState)
    {
        // Evita que la misma animación se interrumpa a sí misma
        if (_currentState == newState) return;

        // Reproduce la animación
        Animator.Play(newState);

        // Reasigna el estado actual
        _currentState = newState;
    }

    private IEnumerator ResetLanding()
    {
        yield return new WaitForSeconds(0.1f); // Tiempo de animación de aterrizaje
        _isLanding = false;
    }

    private void FixedUpdate()
    {
        // Movimiento
        if (IsWallJumping)
            Run(Data.wallJumpRunLerp);
        else
            Run(1);

        // Deslizamiento en pared
        if (IsSliding)
            Slide();
    }

    #region INPUT CALLBACKS
    public void OnJumpInput()
    {
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            _isJumpCut = true;
    }

    public void OnPsychicInput()
    {
        LastPressedPsychicTime = Data.psychicInputBufferTime;
    }
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }
    #endregion

    #region RUN METHODS
    private void Run(float lerpAmount)
    {
        float targetSpeed = _moveInput.x * Data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(RB.linearVelocity.x, targetSpeed, lerpAmount);

        float accelRate;
        if (LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;

        if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            targetSpeed *= Data.jumpHangMaxSpeedMult;
        }

        if (Data.doConserveMomentum && Mathf.Abs(RB.linearVelocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.linearVelocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            accelRate = 0;
        }

        float speedDif = targetSpeed - RB.linearVelocity.x;
        float movement = speedDif * accelRate;
        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region JUMP METHODS
    private void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        float force = Data.jumpForce;
        if (RB.linearVelocity.y < 0)
            force -= RB.linearVelocity.y;

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void WallJump(int dir)
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
        force.x *= dir;

        if (Mathf.Sign(RB.linearVelocity.x) != Mathf.Sign(force.x))
            force.x -= RB.linearVelocity.x;

        if (RB.linearVelocity.y < 0)
            force.y -= RB.linearVelocity.y;

        RB.AddForce(force, ForceMode2D.Impulse);
    }
    #endregion

    #region PSYCHIC POWER METHODS
    private void UsePsychicPower()
    {
        LastPressedPsychicTime = 0;
        _hasPsychicCharge = false;

        if (PsychicPlatformManager.Instance != null)
        {
            PsychicPlatformManager.Instance.ActivateAllPlatforms();
        }
        else
        {
            Debug.LogWarning("No hay PsychicPlatformManager en la escena!");
        }

        Debug.Log("¡Poder Psíquico Activado!");

        // Iniciar recarga después de usar el poder
        StartCoroutine(ResetPsychicAnimation());
    }

    private IEnumerator ResetPsychicAnimation()
    {
        yield return new WaitForSeconds(0.5f); // Duración aproximada de la animación
        _currentState = ""; // Resetear para permitir transiciones
    }
    #endregion

    #region SLIDE METHODS
    private void Slide()
    {
        if (RB.linearVelocity.y > 0)
        {
            RB.AddForce(-RB.linearVelocity.y * Vector2.up, ForceMode2D.Impulse);
        }

        float speedDif = Data.slideSpeed - RB.linearVelocity.y;
        float movement = speedDif * Data.slideAccel;
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));
        RB.AddForce(movement * Vector2.up);
    }
    #endregion

    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }

    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
             (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.linearVelocity.y > 0;
    }

    private bool CanWallJumpCut()
    {
        return IsWallJumping && RB.linearVelocity.y > 0;
    }

    private bool CanUsePsychicPower()
    {
        if (!_hasPsychicCharge && LastOnGroundTime > 0 && !_psychicRecharging)
        {
            StartCoroutine(RechargePsychicPower());
        }

        return _hasPsychicCharge;
    }

    private IEnumerator RechargePsychicPower()
    {
        _psychicRecharging = true;
        yield return new WaitForSeconds(Data.psychicRechargeTime);
        _hasPsychicCharge = true;
        _psychicRecharging = false;
        Debug.Log("¡Poder Psíquico Recargado!");
    }

    public bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && LastOnGroundTime <= 0)
            return true;
        else
            return false;
    }
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        if (_groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        }

        if (_frontWallCheckPoint != null && _backWallCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
            Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
        }
    }
    #endregion
}