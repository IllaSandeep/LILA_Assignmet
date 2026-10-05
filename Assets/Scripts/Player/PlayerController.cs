using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [InspectorName("Max Forward Speed")]
    [Tooltip("Maximum forward speed before permanent upgrades and temporary boosts.")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("Time-based change in forward speed while W is held, in units per second squared.")]
    [SerializeField] private float forwardAcceleration = 8f;
    [Tooltip("How quickly forward motion slows when thrust is released or reversed.")]
    [SerializeField] private float forwardDeceleration = 5f;
    [Tooltip("Reverse top speed as a fraction of forward top speed.")]
    [SerializeField] private float reverseSpeedMultiplier = 0.5f;
    [Tooltip("Time-based change in reverse speed while S is held.")]
    [SerializeField] private float reverseAcceleration = 6f;
    [Tooltip("How quickly reverse motion slows when S is released or W is pressed.")]
    [SerializeField] private float reverseDeceleration = 7f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 240f;
    [SerializeField] private float rotationAcceleration = 1000f;
    [SerializeField] private float rotationDeceleration = 1400f;

    [Header("Speed Boost")]
    [SerializeField] private float speedBoostMultiplier = 1.7f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashCooldown = 2f;

    [Header("Thrust Visual")]
    [Tooltip("Optional child object shown while forward thrust is applied. A ParticleSystem on it or its children has its emission rate adjusted automatically.")]
    [SerializeField] private GameObject thrustEffect;
    [Tooltip("Particle emission rate at low forward speed, in particles per second.")]
    [SerializeField] private float minimumThrust = 2f;
    [Tooltip("Particle emission rate at maximum forward speed, in particles per second.")]
    [SerializeField] private float maximumThrust = 20f;

    [Header("Player Visual")]
    [SerializeField] private Transform playerVisual;

    [Header("Debug")]
    [SerializeField] private bool debugRotation;

    private Rigidbody2D rb;
    private ParticleSystem thrustParticles;
    private ParticleSystem.EmissionModule thrustEmission;
    private LineRenderer placeholderThrustLine;
    private Material placeholderThrustMaterial;

    private float forwardInput;
    private float rotationInput;
    private float currentRotationSpeed;
    private float currentSpeed;

    private float baseMoveSpeed;

    private bool speedBoostActive;
    private bool dashActive;

    private float dashTimer;

    private Quaternion lastExpectedRootRotation;
    private Quaternion lastExpectedVisualRotation;
    private bool rotationDebugLogged;

    private PermanentUpgradeManager permanentUpgradeManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            rb.angularVelocity = 0f;
        }

        baseMoveSpeed = moveSpeed;

        permanentUpgradeManager =
            FindFirstObjectByType<PermanentUpgradeManager>();

        if (playerVisual == null)
        {
            Transform visual =
                transform.Find("PlayerVisual");

            if (visual != null)
                playerVisual = visual;
        }

        InitializeThrustVisual();

        lastExpectedRootRotation = transform.rotation;
        if (playerVisual != null)
            lastExpectedVisualRotation = playerVisual.rotation;
    }

    private void OnDestroy()
    {
        if (placeholderThrustMaterial != null)
            Destroy(placeholderThrustMaterial);
    }

    private void InitializeThrustVisual()
    {
        if (thrustEffect == null && playerVisual != null)
        {
            thrustEffect = new GameObject("Thrust Effect");
            thrustEffect.transform.SetParent(playerVisual, false);

            placeholderThrustLine = thrustEffect.AddComponent<LineRenderer>();
            placeholderThrustLine.useWorldSpace = false;
            placeholderThrustLine.positionCount = 2;
            placeholderThrustLine.SetPosition(0, new Vector3(0f, -0.5f, 0f));
            placeholderThrustLine.SetPosition(1, new Vector3(0f, -0.95f, 0f));
            placeholderThrustLine.startWidth = 0.12f;
            placeholderThrustLine.endWidth = 0.015f;
            placeholderThrustLine.startColor = new Color(1f, 0.55f, 0.15f, 0.8f);
            placeholderThrustLine.endColor = new Color(1f, 0.2f, 0.05f, 0f);
            placeholderThrustLine.numCapVertices = 2;

            SpriteRenderer shipRenderer = playerVisual.GetComponent<SpriteRenderer>();
            if (shipRenderer != null)
            {
                placeholderThrustLine.sortingLayerID = shipRenderer.sortingLayerID;
                placeholderThrustLine.sortingOrder = shipRenderer.sortingOrder + 1;
            }

            Shader defaultSpriteShader = Shader.Find("Sprites/Default");
            if (defaultSpriteShader != null)
            {
                placeholderThrustMaterial = new Material(defaultSpriteShader);
                placeholderThrustLine.material = placeholderThrustMaterial;
            }
        }

        if (thrustEffect == null)
            return;

        if (placeholderThrustLine == null)
            placeholderThrustLine = thrustEffect.GetComponent<LineRenderer>();

        thrustParticles = thrustEffect.GetComponentInChildren<ParticleSystem>(true);
        if (thrustParticles != null)
            thrustEmission = thrustParticles.emission;

        thrustEffect.SetActive(false);
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        if (debugRotation)
        {
            CheckForUnexpectedRotation(transform, lastExpectedRootRotation);
            if (playerVisual != null)
                CheckForUnexpectedRotation(playerVisual, lastExpectedVisualRotation);
        }

        ReadInput();

        RotateVisual();
        if (playerVisual != null)
            lastExpectedVisualRotation = playerVisual.rotation;

        UpdateThrustVisual();

        dashTimer -= Time.deltaTime;

        if (
            dashActive &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            dashTimer <= 0f
        )
        {
            Dash();
        }
    }

    private void LateUpdate()
    {
        if (debugRotation)
        {
            CheckForUnexpectedRotation(transform, lastExpectedRootRotation);
            if (playerVisual != null)
                CheckForUnexpectedRotation(playerVisual, lastExpectedVisualRotation);
        }

        // Keep the monitor quiet while debugging is disabled, and use the
        // latest observed rotations as the baseline for the next frame.
        lastExpectedRootRotation = transform.rotation;
        if (playerVisual != null)
            lastExpectedVisualRotation = playerVisual.rotation;
    }

    private void CheckForUnexpectedRotation(Transform target, Quaternion expectedRotation)
    {
        if (rotationDebugLogged || Quaternion.Angle(expectedRotation, target.rotation) < 0.1f)
            return;

        Debug.LogWarning(
            "Unexpected rotation detected on " + target.name +
            ". PlayerController did not request this change. Attached components to inspect: " +
            GetAttachedComponentNames(target.gameObject),
            target
        );
        rotationDebugLogged = true;
    }

    private string GetAttachedComponentNames(GameObject target)
    {
        Component[] components = target.GetComponents<Component>();
        string names = string.Empty;

        foreach (Component component in components)
        {
            if (component == null || component is Transform || component is PlayerController)
                continue;

            if (names.Length > 0)
                names += ", ";

            names += component.GetType().Name;
        }

        return names.Length > 0 ? names : "none";
    }

    private void ReadInput()
    {
        forwardInput = 0f;
        rotationInput = 0f;

        if (Keyboard.current == null)
            return;

        // Forward / Reverse
        if (Keyboard.current.wKey.isPressed)
            forwardInput += 1f;

        if (Keyboard.current.sKey.isPressed)
            forwardInput -= 1f;

        // Rotate
        if (Keyboard.current.aKey.isPressed)
            rotationInput += 1f;

        if (Keyboard.current.dKey.isPressed)
            rotationInput -= 1f;
    }

    private void RotateVisual()
    {
        if (playerVisual == null)
            return;

        float targetRotationSpeed =
            rotationInput * rotationSpeed;

        float speedChange =
            Mathf.Abs(rotationInput) > 0.01f
                ? rotationAcceleration
                : rotationDeceleration;

        currentRotationSpeed =
            Mathf.MoveTowards(
                currentRotationSpeed,
                targetRotationSpeed,
                speedChange * Time.deltaTime
            );

        if (Mathf.Abs(currentRotationSpeed) < 0.01f)
        {
            currentRotationSpeed = 0f;
            return;
        }

        playerVisual.Rotate(
            0f,
            0f,
            currentRotationSpeed * Time.deltaTime
        );
    }

    private void FixedUpdate()
    {
        if (rb == null || playerVisual == null)
            return;

        float effectiveForwardSpeed = GetCurrentMoveSpeed();
        float targetSpeed = 0f;
        if (forwardInput > 0f)
            targetSpeed = effectiveForwardSpeed;
        else if (forwardInput < 0f)
            targetSpeed = -effectiveForwardSpeed * reverseSpeedMultiplier;

        float speedChange;
        bool reducingSpeed =
            Mathf.Abs(targetSpeed) < 0.01f ||
            (Mathf.Abs(currentSpeed) > 0.01f &&
             Mathf.Abs(targetSpeed) > 0.01f &&
             (Mathf.Sign(currentSpeed) != Mathf.Sign(targetSpeed) ||
              Mathf.Abs(targetSpeed) < Mathf.Abs(currentSpeed)));

        if (reducingSpeed)
            speedChange = currentSpeed < 0f ? reverseDeceleration : forwardDeceleration;
        else
            speedChange = targetSpeed < 0f ? reverseAcceleration : forwardAcceleration;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            speedChange * Time.fixedDeltaTime
        );

        Vector2 movementDelta = playerVisual.up.normalized * currentSpeed * Time.fixedDeltaTime;

        rb.MovePosition(
            rb.position +
            movementDelta
        );
    }

    private void UpdateThrustVisual()
    {
        if (thrustEffect == null)
            return;

        bool thrusting = forwardInput > 0.01f;
        if (thrustEffect.activeSelf != thrusting)
        {
            thrustEffect.SetActive(thrusting);

            if (thrustParticles != null)
            {
                if (thrusting)
                    thrustParticles.Play();
                else
                    thrustParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        if (!thrusting)
            return;

        float speedFraction = Mathf.Clamp01(currentSpeed / Mathf.Max(GetCurrentMoveSpeed(), 0.01f));
        float emissionRate = Mathf.Lerp(minimumThrust, maximumThrust, speedFraction);
        if (thrustParticles != null)
            thrustEmission.rateOverTime = emissionRate;

        if (placeholderThrustLine != null)
        {
            placeholderThrustLine.startWidth = Mathf.Lerp(0.04f, 0.12f, speedFraction);
            placeholderThrustLine.endWidth = Mathf.Lerp(0.008f, 0.025f, speedFraction);
        }
    }

    private float GetCurrentMoveSpeed()
    {
        float speed = baseMoveSpeed;

        if (permanentUpgradeManager != null)
        {
            speed *=
                permanentUpgradeManager
                    .GetMoveSpeedMultiplier();
        }

        if (speedBoostActive)
        {
            speed *= speedBoostMultiplier;
        }

        return speed;
    }

    private void Dash()
    {
        if (rb == null || playerVisual == null)
            return;

        Vector2 dashDirection =
            playerVisual.up.normalized;

        rb.MovePosition(
            rb.position +
            dashDirection *
            dashDistance
        );

        dashTimer =
            dashCooldown;

        PlayerBehaviorTracker behaviorTracker =
            GetComponent<PlayerBehaviorTracker>();

        if (behaviorTracker != null)
        {
            behaviorTracker.RegisterDash();
        }

        Debug.Log("DASH!");
    }

    public void ActivateSpeedBoost()
    {
        if (speedBoostActive)
            return;

        speedBoostActive = true;

        Debug.Log(
            "SPEED BOOST ACTIVATED! Speed: " +
            GetCurrentMoveSpeed()
        );
    }

    public void DeactivateSpeedBoost()
    {
        speedBoostActive = false;

        Debug.Log(
            "SPEED BOOST EXPIRED! Speed: " +
            GetCurrentMoveSpeed()
        );
    }

    public void ActivateDash()
    {
        dashActive = true;
        dashTimer = 0f;

        Debug.Log(
            "DASH ACTIVATED!"
        );
    }

    public void DeactivateDash()
    {
        dashActive = false;

        Debug.Log(
            "DASH EXPIRED!"
        );
    }

    public float GetCurrentMoveSpeedValue()
    {
        return GetCurrentMoveSpeed();
    }

    public bool IsSpeedBoostActive()
    {
        return speedBoostActive;
    }

    public bool IsDashActive()
    {
        return dashActive;
    }
}
