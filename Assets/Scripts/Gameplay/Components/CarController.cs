using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(VehicleMotor))]
public class CarController : MonoBehaviour, IVehicle, IPoolable, IMoneyCollector
{
    [Header("Gameplay Settings")]
    [SerializeField] float fineCooldown = 1.5f;

    private VehicleMotor motor;
    private MathCollider[] mathColliders;
    private CancellationTokenSource fineCooldownCts;
    private bool isPenaltyOnCooldown;

    public event Action OnReturnedToPool;
    public event Action<IMoneyCollector, int> OnMoneyCollected;
    public event Action<Vector3, float> OnCollided;

    public bool Stop { get; set; }
    public IVehicleControlStrategy ActiveStrategy { get; private set; }
    
    public CarTelemetry Telemetry => motor.Telemetry;  

    VehicleStrategyFactory strategyFactory => ServiceLocator.Get<CarsManager>().VehicleStrategyFactory;
    GamePropertiesConfig gameProperties => ServiceLocator.Get<GamePropertiesConfig>();
    IPlaygroundBounds bounds => ServiceLocator.Get<Playground>();

    void Awake()
    {
        motor = GetComponent<VehicleMotor>();
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
        ActiveStrategy = strategyFactory.Create<T>(this);
    }

    public void SetCollisionProfile(CollisionLayer layer, CollisionLayer collidesWith)
    {
        foreach (var collider in mathColliders)
        {
            collider.Layer = layer;
            collider.CollidesWith = collidesWith;
        }
    }

    void Update()
    {
        Vector3 direction = (ActiveStrategy != null && !Stop) ? ActiveStrategy.GetMoveDirection() : Vector3.zero;
        MoveTo(direction);
    }

    public void MoveTo(Vector3 targetDirection)
    {
        bool isConstrained = ActiveStrategy != null && ActiveStrategy.IsConstrainedByBounds;

        motor.Move(targetDirection, isConstrained, mathColliders, bounds);
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
                OnMoneyCollected?.Invoke(this, -gameProperties.NpcCollisionFine);
                ProcessPenaltyCooldownAsync(fineCooldownCts.Token).Forget();
                break;
        }
    }

    void ResolvePenetration(MathCollider myCollider, MathCollider hitCollider, Vector3 correction)
    {
        Vector3 contactPoint = myCollider.GetContactPoint(hitCollider);
        motor.ApplyImpact(correction, contactPoint);

        if (correction.sqrMagnitude > 0.0001f)
        {
            OnCollided?.Invoke(contactPoint, motor.Telemetry.Velocity.magnitude);
        }
    }

    async UniTaskVoid ProcessPenaltyCooldownAsync(CancellationToken token)
    {
        isPenaltyOnCooldown = true;
        bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(fineCooldown), cancellationToken: token).SuppressCancellationThrow();
        if (!isCancelled) isPenaltyOnCooldown = false;
    }

    public void ReturnToPool()
    {
        if (ObjectsPool.ReturnToPool(gameObject))
        {
            ActiveStrategy = null;
            OnReturnedToPool?.Invoke();

            OnReturnedToPool = null;
            OnMoneyCollected = null;
            Stop = false;

            motor.ResetState();

            isPenaltyOnCooldown = false;
            fineCooldownCts?.Cancel();
            fineCooldownCts?.Dispose();
            fineCooldownCts = new CancellationTokenSource();
           
        }
    }

    void OnDestroy()
    {
        OnReturnedToPool = null;
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