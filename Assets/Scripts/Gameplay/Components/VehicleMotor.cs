using UnityEngine;

public class VehicleMotor : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [SerializeField] float moveSpeed = 15f;
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float deceleration = 20f;
    [SerializeField] float collisionSpinMultiplier = 50f;

    [Header("Physics Impulses")]
    [SerializeField] float knockbackForce = 15f;
    [SerializeField] float impactDamping = 5f;

    float currentSpeed;
    Vector3 externalImpactVelocity;
    float currentAngularVelocity;
    float currentAlignment = 1f;
    float currentTurnDirection;


    public CarTelemetry Telemetry => new CarTelemetry(currentSpeed, currentAngularVelocity,
        transform.forward * currentSpeed + externalImpactVelocity, currentAlignment, currentTurnDirection);

    public void Move(Vector3 targetDirection, bool isConstrained, MathCollider[] colliders, IPlaygroundBounds bounds)
    {       
        targetDirection.y = 0;

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
      
        engineMove.y = 0;

        externalImpactVelocity = Vector3.Lerp(externalImpactVelocity, Vector3.zero, impactDamping * Time.deltaTime);
        currentAngularVelocity = Mathf.Lerp(currentAngularVelocity, 0f, impactDamping * Time.deltaTime);

        if (Mathf.Abs(currentAngularVelocity) > 0.1f)
        {
            transform.Rotate(0, currentAngularVelocity * Time.deltaTime, 0, Space.World);
        }

        Vector3 step = (engineMove + externalImpactVelocity) * Time.deltaTime;
        Vector3 nextPosition = transform.position + step;

        if (isConstrained && bounds != null && colliders != null)
        {
            foreach (var collider in colliders)
            {
                Vector3 colliderOffset = collider.transform.position - transform.position;

                colliderOffset.y = 0;

                Vector3 futureColliderPos = nextPosition + colliderOffset;
                Vector3 constrainedPos = bounds.ConstrainPoint(futureColliderPos, collider.Radius);

                if ((constrainedPos - futureColliderPos).sqrMagnitude > 0.0001f)
                {
                    nextPosition += (constrainedPos - futureColliderPos);
                    externalImpactVelocity = Vector3.zero;
                    currentAngularVelocity = 0f;
                }
            }
        }

        nextPosition.y = transform.position.y;
        transform.position = nextPosition;
        
        Vector3 currentEuler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0, currentEuler.y, 0);
    }

    public void ApplyImpact(Vector3 correction, Vector3 contactPoint)
    {
        correction.y = 0;
        transform.position += correction;

        if (correction.sqrMagnitude > 0.0001f)
        {
            Vector3 pushDir = correction.normalized;
            externalImpactVelocity = pushDir * knockbackForce;

            Vector3 leverArm = contactPoint - transform.position;
            Vector3 torque = Vector3.Cross(leverArm, pushDir);

            currentAngularVelocity += torque.y * collisionSpinMultiplier;
            currentAngularVelocity = Mathf.Clamp(currentAngularVelocity, -1000f, 1000f);
        }
    }

    public void ResetState()
    {
        currentSpeed = 0f;
        externalImpactVelocity = Vector3.zero;
        currentAngularVelocity = 0f;
    }
}