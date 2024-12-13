using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZombieMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private float baseSpeedOffset;
    [SerializeField] private float speedSmoothTime = 1.0f;
    private Vector3 lastPosition;
    private Vector3 currentVelocity = new Vector3(0, 0, 0);
    private Vector3 moveInput;

    [Header("Seperation")]
    [SerializeField] private float seperationSpeed = 1;
    [DisplayColor(0, 0, 1), SerializeField] private float seperationRadius;

    [Header("Grouping")]
    [SerializeField] private float groupCenterSpeed = 1;
    [SerializeField] private float groupingRangeThreshold;

    private CachedZombieData cachedZombieData;

    private void Start()
    {
        cachedZombieData = GetComponent<CachedZombieData>();
        baseMoveSpeed -= Random.Range(-baseSpeedOffset, baseSpeedOffset);
    }

    private void Update()
    {
        MoveZombie();
    }

    private void LateUpdate()
    {
        if (cachedZombieData.Health.isDead)
            return;

        MoveAnimationLateUpdate();
    }

    void MoveZombie()
    {
        Vector3 moveDirection = (moveInput * baseMoveSpeed) + (SeparationForce() * seperationSpeed);

        float distanceToGroupCenter = (GetGroupCenter() - transform.position).magnitude;

        if (distanceToGroupCenter > groupingRangeThreshold)
        {
            moveDirection += (GetGroupCenter() - transform.position).normalized * groupCenterSpeed;
        }

        transform.position = Vector3.SmoothDamp(transform.position, transform.position + moveDirection.normalized, ref currentVelocity, speedSmoothTime);
    }

    public void OnMove(InputValue inputValue)
    {
        if (cachedZombieData.AutoAttack.isAttacking || cachedZombieData.Health.isDead)
        {
            moveInput = Vector3.zero;
        }
        else
        {
            moveInput = inputValue.Get<Vector2>().normalized;
        }
    }

    Vector3 SeparationForce()
    {
        Collider2D[] nearbyZombies = Physics2D.OverlapCircleAll(transform.position, seperationRadius, 1 << gameObject.layer);

        Vector3 _separationForce = Vector3.zero;

        // Ignore separation if there's no other zombie nearby and compare to one because overlapCircle always hits itself
        if (nearbyZombies.Length <= 1)
        {
            return _separationForce;
        }

        foreach (Collider2D zombie in nearbyZombies)
        {
            if (zombie == GetComponent<Collider2D>())
                continue; 

            Vector3 oppositeDirectionToNearZombie = transform.position - zombie.transform.position;

            // Compare to more than 0 to avoid division by 0
            if (oppositeDirectionToNearZombie.magnitude > 0) 
            {
                _separationForce += oppositeDirectionToNearZombie / oppositeDirectionToNearZombie.magnitude; // Stronger repulsion when closer
            }
        }

        return _separationForce;
    }

    Vector3 GetGroupCenter()
    {
        if(cachedZombieData.ZombiePlayerHordeRegistry.Zombies.Count <= 1)
        {
            return transform.position;
        }

        List<float> xPositions = new List<float>();
        List<float> yPositions = new List<float>();

        foreach (GameObject zombie in cachedZombieData.ZombiePlayerHordeRegistry.Zombies)
        {
            xPositions.Add(zombie.transform.position.x);
            yPositions.Add(zombie.transform.position.y);
        }

        xPositions.Sort();
        yPositions.Sort();

        float medianX = (xPositions.Count % 2 == 1) ? xPositions[xPositions.Count / 2] : (xPositions[xPositions.Count / 2 - 1] + xPositions[xPositions.Count / 2]) / 2;

        float medianY = (yPositions.Count % 2 == 1) ? yPositions[yPositions.Count / 2] : (yPositions[yPositions.Count / 2 - 1] + yPositions[yPositions.Count / 2]) / 2;


        return new Vector3(medianX, medianY, transform.position.z);
    }

    void MoveAnimationLateUpdate()
    {
        var currentSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;  

        lastPosition = transform.position;

        cachedZombieData.Animator.SetFloat("moveSpeed", currentSpeed);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, seperationRadius);
    }
}
