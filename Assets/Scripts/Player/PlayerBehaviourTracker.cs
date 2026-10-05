using UnityEngine;

public class PlayerBehaviorTracker : MonoBehaviour
{
    [Header("Behavior Tracking")]
    [SerializeField] private float closeRangeDistance = 3f;
    [SerializeField] private float farRangeDistance = 6f;
    [Tooltip("Zero samples every frame; larger values lower tracking cost.")]
    [SerializeField] private float behaviorSamplingInterval = 0.2f;
    [SerializeField] private float confidenceBuildTime = 40f;
    [SerializeField] private float expectedDashInterval = 30f;
    [SerializeField] private float expectedAbilityInterval = 45f;

    [Header("Normalized Scores")]
    [Range(0f, 1f)] [SerializeField] private float aggressionScore;
    [Range(0f, 1f)] [SerializeField] private float rangedScore;
    [Range(0f, 1f)] [SerializeField] private float mobilityScore;
    [Range(0f, 1f)] [SerializeField] private float abilityUsageScore;
    [Range(0f, 1f)] [SerializeField] private float stationaryScore;
    [Range(0f, 1f)] [SerializeField] private float dodgeScore;

    private Transform player;
    private PlayerHealth playerHealth;
    private Vector2 previousPosition;
    private Vector2 previousDirection;
    private float previousHealth;
    private float sampleTimer;
    private float trackedTime;
    private float closeTime;
    private float farTime;
    private float nearTime;
    private float movementDistance;
    private float movementTime;
    private float evasiveTime;
    private float repeatedMovementTime;
    private Vector2 recentVelocity;
    private float totalDamageTaken;
    private int shotsFired;
    private int kills;
    private int dashCount;
    private int abilitiesUsed;

    public static PlayerBehaviorTracker ActiveTracker { get; private set; }
    public float BehaviorConfidence => Mathf.Clamp01(
        (nearTime + shotsFired * 0.25f + kills * 0.75f + dashCount * 0.5f + abilitiesUsed * 0.75f) /
        Mathf.Max(1f, confidenceBuildTime)
    );
    public float AggressionScore => aggressionScore;
    public float RangedScore => rangedScore;
    public float MobilityScore => mobilityScore;
    public float AbilityUsageScore => abilityUsageScore;
    public float StationaryScore => stationaryScore;
    public float DodgeScore => dodgeScore;
    public int Kills => kills;
    public int ShotsFired => shotsFired;
    public int DashCount => dashCount;
    public int AbilitiesUsed => abilitiesUsed;
    public float TotalDamageTaken => totalDamageTaken;
    public float AverageMovementSpeed => movementTime > 0f ? movementDistance / movementTime : 0f;
    public float ShotFrequency => trackedTime > 0f ? shotsFired / trackedTime : 0f;
    public float DashFrequency => trackedTime > 0f ? dashCount / trackedTime : 0f;
    public float AbilityFrequency => trackedTime > 0f ? abilitiesUsed / trackedTime : 0f;
    public float PreferredEngagementDistance => trackedTime > 0f ? distanceTimeSum / trackedTime : 0f;

    private float distanceTimeSum;

    private void Awake()
    {
        player = transform;
        playerHealth = GetComponent<PlayerHealth>();
        previousPosition = transform.position;
        previousDirection = Vector2.zero;
        if (playerHealth != null)
            previousHealth = playerHealth.CurrentHealth;
        ActiveTracker = this;
    }

    private void OnDestroy()
    {
        if (ActiveTracker == this)
            ActiveTracker = null;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        float dt = Time.deltaTime;
        trackedTime += dt;
        sampleTimer += dt;
        TrackDamage();

        if (behaviorSamplingInterval <= 0f || sampleTimer >= behaviorSamplingInterval)
        {
            SampleBehavior(sampleTimer);
            sampleTimer = 0f;
        }

        CalculateScores();
    }

    private void TrackDamage()
    {
        if (playerHealth == null)
            return;

        float health = playerHealth.CurrentHealth;
        if (health < previousHealth)
            totalDamageTaken += previousHealth - health;
        previousHealth = health;
    }

    private void SampleBehavior(float dt)
    {
        if (player == null || dt <= 0f)
            return;

        Vector2 position = player.position;
        Vector2 velocity = (position - previousPosition) / dt;
        recentVelocity = velocity;
        float speed = velocity.magnitude;
        movementDistance += Vector2.Distance(position, previousPosition);
        movementTime += dt;

        if (speed > 0.05f)
        {
            Vector2 direction = velocity.normalized;
            if (previousDirection.sqrMagnitude > 0.1f && Vector2.Dot(previousDirection, direction) > 0.96f)
                repeatedMovementTime += dt;
            previousDirection = direction;
        }

        float closestDistance = GetClosestEnemyDistance(position);
        if (closestDistance < 0f)
            closestDistance = farRangeDistance;

        distanceTimeSum += closestDistance * dt;
        if (closestDistance <= closeRangeDistance)
            closeTime += dt;
        if (closestDistance >= farRangeDistance)
            farTime += dt;
        if (closestDistance <= farRangeDistance)
            nearTime += dt;
        if (closestDistance <= farRangeDistance && speed > 2f)
            evasiveTime += dt;

        previousPosition = position;
    }

    private float GetClosestEnemyDistance(Vector2 position)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closest = float.MaxValue;
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;
            float distance = Vector2.Distance(position, enemy.transform.position);
            if (distance < closest)
                closest = distance;
        }
        return closest == float.MaxValue ? -1f : closest;
    }

    private void CalculateScores()
    {
        if (trackedTime <= 0f)
            return;

        float dashExpectation = Mathf.Max(1f, trackedTime / Mathf.Max(0.01f, expectedDashInterval));
        float abilityExpectation = Mathf.Max(1f, trackedTime / Mathf.Max(0.01f, expectedAbilityInterval));
        float shotRate = shotsFired / Mathf.Max(1f, trackedTime);

        aggressionScore = Mathf.Clamp01((closeTime / trackedTime) * 0.75f + Mathf.Clamp01(shotRate / 1.5f) * 0.25f);
        rangedScore = Mathf.Clamp01(farTime / trackedTime);
        mobilityScore = Mathf.Clamp01(dashCount / dashExpectation * 0.6f + (AverageMovementSpeed / 5f) * 0.4f);
        abilityUsageScore = Mathf.Clamp01(abilitiesUsed / abilityExpectation);
        stationaryScore = Mathf.Clamp01(1f - AverageMovementSpeed / 3f);
        dodgeScore = Mathf.Clamp01((nearTime > 0f ? evasiveTime / nearTime : 0f) * 0.7f + (nearTime > 0f ? repeatedMovementTime / nearTime : 0f) * 0.3f);
    }

    public Vector2 GetRecentVelocity() => recentVelocity;
    public Vector2 GetPredictedPosition(float predictionTime, float accuracy = 1f) => (Vector2)transform.position + GetRecentVelocity() * Mathf.Max(0f, predictionTime) * Mathf.Clamp01(accuracy);

    public void RegisterShot() { shotsFired++; }
    public void RegisterKill() { kills++; }
    public void RegisterDash() { dashCount++; }
    public void RegisterAbilityUse(string abilityType) { abilitiesUsed++; }
    public float GetAggressionScore() => aggressionScore;
    public float GetRangedScore() => rangedScore;
    public float GetMobilityScore() => mobilityScore;
    public float GetAbilityUsageScore() => abilityUsageScore;
    public float GetStationaryScore() => stationaryScore;
    public float GetDodgeScore() => dodgeScore;
    public int GetShotsFired() => shotsFired;
    public int GetDashCount() => dashCount;
    public int GetAbilitiesUsed() => abilitiesUsed;
    public int GetKills() => kills;
    public float GetDamageTaken() => totalDamageTaken;

    public void PrintBehaviorReport()
    {
        Debug.Log($"PLAYER BEHAVIOR | Confidence {BehaviorConfidence:0.00} | Aggression {aggressionScore:0.00} | Ranged {rangedScore:0.00} | Mobility {mobilityScore:0.00} | Ability {abilityUsageScore:0.00} | Stationary {stationaryScore:0.00} | Dodge {dodgeScore:0.00} | Kills {kills} | Shots {shotsFired} | Damage {totalDamageTaken:0}");
    }
}
