using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Gravity")]
    [HideInInspector] public float gravityStrength;
    [HideInInspector] public float gravityScale;
    [Space(5)]
    public float fallGravityMult = 1.5f;
    public float maxFallSpeed = 20f;
    [Space(5)]
    public float fastFallGravityMult = 2f;
    public float maxFastFallSpeed = 30f;

    [Space(20)]

    [Header("Run")]
    public float runMaxSpeed = 10f;
    public float runAcceleration = 10f;
    [HideInInspector] public float runAccelAmount;
    public float runDecceleration = 10f;
    [HideInInspector] public float runDeccelAmount;
    [Space(5)]
    [Range(0f, 1)] public float accelInAir = 0.65f;
    [Range(0f, 1)] public float deccelInAir = 0.65f;
    [Space(5)]
    public bool doConserveMomentum = true;

    [Space(20)]

    [Header("Jump")]
    public float jumpHeight = 4f;
    public float jumpTimeToApex = 0.4f;
    [HideInInspector] public float jumpForce;

    [Header("Both Jumps")]
    public float jumpCutGravityMult = 2f;
    [Range(0f, 1)] public float jumpHangGravityMult = 0.5f;
    public float jumpHangTimeThreshold = 2f;
    [Space(0.5f)]
    public float jumpHangAccelerationMult = 1.1f;
    public float jumpHangMaxSpeedMult = 1.3f;

    [Header("Wall Jump")]
    public Vector2 wallJumpForce = new Vector2(15f, 20f);
    [Space(5)]
    [Range(0f, 1f)] public float wallJumpRunLerp = 0.5f;
    [Range(0f, 1.5f)] public float wallJumpTime = 0.15f;
    public bool doTurnOnWallJump = true;

    [Space(20)]

    [Header("Slide")]
    public float slideSpeed = -5f;
    public float slideAccel = 25f;

    [Header("Assists")]
    [Range(0.01f, 0.5f)] public float coyoteTime = 0.1f;
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime = 0.1f;

    [Space(20)]

    [Header("Psychic Power")]
    [Tooltip("Tiempo de recarga del poder psíquico al tocar el suelo")]
    public float psychicRechargeTime = 0.1f;
    [Tooltip("Grace period después de presionar el botón de poder psíquico")]
    [Range(0.01f, 0.5f)] public float psychicInputBufferTime = 0.1f;

    private void OnValidate()
    {
        // Calcular gravedad
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
        gravityScale = gravityStrength / Physics2D.gravity.y;

        // Calcular aceleración
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        // Calcular fuerza de salto
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        // Clamps
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
    }
}