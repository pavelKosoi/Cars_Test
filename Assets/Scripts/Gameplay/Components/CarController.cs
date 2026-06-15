using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CarController : MonoBehaviour, IVehicle, IPoolable, IMoneyCollector
{
    [Header("Vehicle Settings")]
    [SerializeField] float moveSpeed;
    [SerializeField] float turnSpeed;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float deceleration = 20f;
    [SerializeField] float collisionSpinMultiplier;

    [Header("Physics Impulses")]
    [SerializeField] float knockbackForce;
    [SerializeField] float impactDamping;

    [Header("Other")]
    [SerializeField] float fineCooldown = 1.5f;

    float currentSpeed;
    Vector3 externalImpactVelocity;
    float currentAngularVelocity;
    float currentAlignment = 1f;
    float currentTurnDirection;



    bool isPenaltyOnCooldown;
    CancellationTokenSource fineCooldownCts;

    MathCollider[] mathColliders;

    public event Action OnReturnedToPool;
    public event Action OnHitBounds;
    public event Action<IMoneyCollector, int> OnMoneyCollected;
    public event Action<Vector3, float> OnCollided;

    public IVehicleControlStrategy ActiveStrategy { get; set; }
    public bool Stop { get; set; }


    public CarTelemetry Telemetry => new CarTelemetry(currentSpeed, currentAngularVelocity,
        transform.forward * currentSpeed + externalImpactVelocity, currentAlignment, currentTurnDirection);

    VehicleStrategyFactory strategyFactory => GeneralGameManager.Instance.CarsManager.VehicleStrategyFactory;
    GamePropertiesConfig gamePropertiesConfig => GeneralGameManager.Instance.GamePropertiesConfig;
    IPlaygroundBounds bounds => GeneralGameManager.Instance.Playground;

    void Awake()
    {
        mathColliders = GetComponentsInChildren<MathCollider>();

        foreach (var collider in mathColliders)
        {
            collider.OnCollisionEnter += HandleCollision;
            collider.OnResolvePenetration += ResolvePenetration;
        }

        fineCooldownCts = new CancellationTokenSource();
    }

    public void InjectControlStrategy<T>() where T : IVehicleControlStrategy
    {
        Type strategyType = typeof(T);

        if (ActiveStrategy != null && strategyType == ActiveStrategy.GetType()) return;
        var strategy = strategyFactory.Create<T>(this);
        ActiveStrategy = strategy;
    }

   
    public void SetCollisionProfile(CollisionLayer layer, CollisionLayer collidesWith)
    {
        foreach (var collider in mathColliders)
        {
            collider.Layer = layer;
            collider.CollidesWith = collidesWith;
        }
    }

    public void MoveTo(Vector3 targetDirection)
    {    
        bool hasTarget = targetDirection.sqrMagnitude > 0.001f;
        float targetSpeed = 0f;

        if (hasTarget)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            Vector3 normalizedDir = targetDirection.normalized; 
            currentAlignment = Vector3.Dot(transform.forward, normalizedDir);
            currentTurnDirection = Vector3.Cross(transform.forward, normalizedDir).y;

            float speedMultiplier = Mathf.Clamp(currentAlignment, 0.2f, 1f);
            targetSpeed = moveSpeed * speedMultiplier;
        }
        else
        {
            currentAlignment = 1f;
            currentTurnDirection = 0f;
        }

        float currentAccelRate = hasTarget ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, currentAccelRate * Time.deltaTime);
        Vector3 engineMove = transform.forward * currentSpeed;

        externalImpactVelocity = Vector3.Lerp(externalImpactVelocity, Vector3.zero, impactDamping * Time.deltaTime);
        currentAngularVelocity = Mathf.Lerp(currentAngularVelocity, 0f, impactDamping * Time.deltaTime);

        if (Mathf.Abs(currentAngularVelocity) > 0.1f)
        {
            transform.Rotate(0, currentAngularVelocity * Time.deltaTime, 0, Space.World);
        }

        Vector3 step = (engineMove + externalImpactVelocity) * Time.deltaTime;
        Vector3 nextPosition = transform.position + step;

        if (ActiveStrategy != null && ActiveStrategy.IsConstrainedByBounds)
        {
            bool hitWall = false;

            foreach (var collider in mathColliders)
            {
                Vector3 colliderOffset = collider.transform.position - transform.position;
                Vector3 futureColliderPos = nextPosition + colliderOffset;
                Vector3 constrainedPos = bounds.ConstrainPoint(futureColliderPos, collider.Radius);

                if ((constrainedPos - futureColliderPos).sqrMagnitude > 0.0001f)
                {
                    nextPosition += (constrainedPos - futureColliderPos);
                    hitWall = true;
                }
            }

            if (hitWall)
            {
                externalImpactVelocity = Vector3.zero;
                currentAngularVelocity = 0f;
                OnHitBounds?.Invoke();
            }
        }

        transform.position = nextPosition;
    }

    void HandleCollision(MathCollider other)
    {
        if (Stop) return;
        switch (other.Layer)
        {
            case CollisionLayer.Note:
                if (other.TryGetComponent(out NoteController note))
                {
                    note.ReturnToPool();
                    OnMoneyCollected?.Invoke(this, note.Denomination);
                }
                break;

            case CollisionLayer.Npc:
                if (isPenaltyOnCooldown) break; 

                OnMoneyCollected?.Invoke(this, -gamePropertiesConfig.NpcCollisionFine);

                ProcessPenaltyCooldownAsync(fineCooldownCts.Token).Forget();
                break;
        }
    }

    async UniTaskVoid ProcessPenaltyCooldownAsync(CancellationToken token)
    {
        isPenaltyOnCooldown = true;
      
        bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(fineCooldown), 
            cancellationToken: token).SuppressCancellationThrow();

        if (!isCancelled)
        {
            isPenaltyOnCooldown = false;
        }
    }

    void ResolvePenetration(MathCollider myCollider, MathCollider hitCollider, Vector3 correction)
    {
        correction.y = 0;
        transform.position += correction;

        if (correction.sqrMagnitude > 0.0001f)
        {
            Vector3 pushDir = correction.normalized;
            externalImpactVelocity = pushDir * knockbackForce;

            Vector3 contactPoint = myCollider.GetContactPoint(hitCollider);

            OnCollided?.Invoke(contactPoint, externalImpactVelocity.magnitude);

            Vector3 leverArm = contactPoint - transform.position;
            Vector3 torque = Vector3.Cross(leverArm, pushDir);

            currentAngularVelocity += torque.y * collisionSpinMultiplier;
            currentAngularVelocity = Mathf.Clamp(currentAngularVelocity, -1000f, 1000f);
        }
    }

    void Update()
    {
        Vector3 direction = (ActiveStrategy != null && !Stop) ? ActiveStrategy.GetMoveDirection() : Vector3.zero;
        MoveTo(direction);
    
    }

    public void ReturnToPool()
    {
        if (ObjectsPool.ReturnToPool(gameObject))
        {
            ActiveStrategy = null;
            OnReturnedToPool?.Invoke();

            OnReturnedToPool = null;
            OnHitBounds = null;
            OnMoneyCollected = null;

            currentSpeed = 0f;
            externalImpactVelocity = Vector3.zero;
            currentAngularVelocity = 0f;

            isPenaltyOnCooldown = false;
            fineCooldownCts?.Cancel();
            fineCooldownCts?.Dispose();
            fineCooldownCts = new CancellationTokenSource();

            Stop = false;

        }
    }

    void OnDestroy()
    {
        OnReturnedToPool = null;
        OnHitBounds = null;
        OnMoneyCollected = null;

        foreach (var collider in mathColliders)
        {
            collider.OnCollisionEnter -= HandleCollision;
            collider.OnResolvePenetration -= ResolvePenetration;
        }

        fineCooldownCts?.Cancel();
        fineCooldownCts?.Dispose();
    }
}