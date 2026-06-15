using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarVisuals : MonoBehaviour
{
    [Header("Smoke Settings")]
    [SerializeField] ParticleSystem smokeParticle;
    [SerializeField] Vector3 sillOffset;
    [SerializeField] float smokeOutwardBias = 0.5f;
    [SerializeField] float smokeUpwardBias = 0.3f;

    [Header("Skid Settings")]
    [SerializeField] float driftAlignmentThreshold = 0.85f;
    [SerializeField] float minSpeedForDrift = 5f;
    [SerializeField] float impactSpinThreshold = 50f;

    CarController car;
    VisualsConfig visualsConfig => GeneralGameManager.Instance.VisualsConfig;

    SkidmarkEffect activeSkidmark;
    bool isSkidding;

    void Awake()
    {
        car = GetComponent<CarController>();
    }

    void OnEnable()
    {
        car.OnCollided += HandleCollision;
        car.OnMoneyCollected += HandleMoneyCollected;
    }
    void OnDisable()
    {
        car.OnCollided -= HandleCollision;
        car.OnMoneyCollected -= HandleMoneyCollected;
        if (isSkidding) StopSkidding();
    }

    void Update()
    {
        HandleDriftVisuals();
    }

    void HandleCollision(Vector3 contactPoint, float impactForce)
    {
        var sparksObj = ObjectsPool.GetInstance(visualsConfig.CollisionSparksRef, contactPoint, true);       
        ObjectsPool.ReturnToPool(sparksObj, 1f);
    }

    void HandleMoneyCollected(IMoneyCollector collector, int amount)
    {
        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;

        var textObj = ObjectsPool.GetInstance(visualsConfig.FloatingTextRef, spawnPos, true);

        if (textObj.TryGetComponent(out FloatingTextEffect floatingText))
        {
            floatingText.Init(amount);
        }
    }

    void HandleDriftVisuals()
    {
        CarTelemetry telemetry = car.Telemetry;

        bool isTurningSharply = telemetry.SteeringAlignment < driftAlignmentThreshold && telemetry.Speed > minSpeedForDrift;
        bool isSpinningFromImpact = Mathf.Abs(telemetry.AngularVelocity) > impactSpinThreshold;

        bool shouldSkid = isTurningSharply || isSpinningFromImpact;

        if (shouldSkid)
        {
            float turnSign = isSpinningFromImpact ? telemetry.AngularVelocity : telemetry.TurnDirection;
            UpdateSmokeEmitter(turnSign);

            if (!isSkidding) StartSkidding();
        }
        else if (!shouldSkid && isSkidding)
        {
            StopSkidding();
        }

        if (isSkidding)
        {
            UpdateSkidmarks();
        }
    }

    void UpdateSmokeEmitter(float turnSign)
    {
        if (smokeParticle == null) return;

        bool isTurningRight = turnSign > 0;

        Vector3 targetSillOffset = sillOffset;
        if (isTurningRight) targetSillOffset.x = -sillOffset.x;
        smokeParticle.transform.localPosition = targetSillOffset;

        Vector3 smokeDirection = Vector3.back;
        smokeDirection += Vector3.up * smokeUpwardBias;

        if (isTurningRight)
        {
            smokeDirection += Vector3.left * smokeOutwardBias;
        }
        else
        {
            smokeDirection += Vector3.right * smokeOutwardBias;
        }

        smokeParticle.transform.localRotation = Quaternion.LookRotation(smokeDirection.normalized);
    }

    void StartSkidding()
    {
        isSkidding = true;
        if (smokeParticle != null) smokeParticle.Play();

        var lineObj = ObjectsPool.GetInstance(visualsConfig.SkidmarkLineRef, transform.position, true);
        activeSkidmark = lineObj.GetComponent<SkidmarkEffect>();
        activeSkidmark.Init();
    }

    void UpdateSkidmarks()
    {
        if (activeSkidmark != null)
        {
            Vector3 point = transform.position + Vector3.up * 0.05f;
            activeSkidmark.AddPoint(point);
        }
    }

    void StopSkidding()
    {
        isSkidding = false;
        if (smokeParticle != null) smokeParticle.Stop();

        if (activeSkidmark != null)
        {
            activeSkidmark.FadeAndRelease();
            activeSkidmark = null;
        }
    }


}