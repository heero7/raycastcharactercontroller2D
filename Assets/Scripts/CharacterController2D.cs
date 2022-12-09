using UnityEngine;

public struct CollisionInfo
{
    public bool Above;
    public bool Below;
    public bool Left;
    public bool Right;

    public void Reset()
    {
        Above = false;
        Below = false;
        Left = false;
        Right = false;
    }
}

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour
{
    private const float SKIN_WIDTH = 0.015f;
    private const int MIN_RAY_COUNT = 2;

    [Header("Raycast Controller Variables")]
    [SerializeField] private int horizontalRayCount = 4;
    [SerializeField] private int verticalRayCount = 4;

    [SerializeField] private LayerMask collisionMask;

    [Header("Debug")] 
    [SerializeField] private bool showDebugLines;

    private BoxCollider2D _boxCollider2D;
    private RaycastOrigins _raycastOrigins;
    private CollisionInfo _collisions;
    
    private float _horizontalRaySpacing;
    private float _verticalRaySpacing;

    public CollisionInfo Collisions => _collisions;

    private void OnEnable()
    {
        // TODO: How can we set this at this for the project without a manual step?
        // Can we check this at project start?
        // Do we have to do this for each GameObject that has this?
        if (!Physics.autoSyncTransforms)
        {
            Physics.autoSyncTransforms = true;
        }
    }

    public void Move(Vector2 requestedVelocity)
    {
        UpdateRaycastOrigins();
        _collisions.Reset();
        
        if (requestedVelocity.x != 0)
        {
            HandleHorizontalCollisions(ref requestedVelocity);
        }

        if (requestedVelocity.y != 0)
        {
            HandleVerticalCollisions(ref requestedVelocity);
        }
        transform.Translate(requestedVelocity);
    }

    private void Start()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        
        CalculateSpaceBetweenRaycasts();
    }
    
    private void HandleHorizontalCollisions(ref Vector2 velocity)
    {
        var directionX = (int) Mathf.Sign(velocity.x); // 1 = Moving Up, -1 Moving Down.
        var rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

        for (var i = 0; i < horizontalRayCount; i++)
        {
            var rayOrigin = directionX == -1
                ? _raycastOrigins.BottomLeft
                : _raycastOrigins.BottomRight;

            // Project where we WILL be (why we add velocity.x).
            rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
            var raycastHit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (showDebugLines)
            {
                Debug.DrawRay(rayOrigin, Vector3.right * (directionX * rayLength), Color.red);
            }

            if (raycastHit)
            {
                velocity.x = (raycastHit.distance - SKIN_WIDTH) * directionX;
                rayLength = raycastHit.distance;

                _collisions.Left = directionX == -1;
                _collisions.Right = directionX == 1;
            }
        }
    }

    private void HandleVerticalCollisions(ref Vector2 velocity)
    {
        var directionY = (int) Mathf.Sign(velocity.y); // 1 = Moving Up, -1 Moving Down.
        var rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        
        
        
        for (var i = 0; i < verticalRayCount; i++)
        {
            var rayOrigin = directionY == -1
                ? _raycastOrigins.BottomLeft
                : _raycastOrigins.TopLeft;

            // Project where we WILL be (why we add velocity.x).
            rayOrigin += Vector2.right * (_verticalRaySpacing * i + velocity.x);
            var raycastHit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            
            if (showDebugLines)
            {
                Debug.DrawRay(rayOrigin, Vector3.up * (directionY * rayLength), Color.red);
            }

            if (raycastHit)
            {
                velocity.y = (raycastHit.distance - SKIN_WIDTH) * directionY;
                rayLength = raycastHit.distance; 
                
                _collisions.Below = directionY == -1;
                _collisions.Above = directionY == 1;
            }
            
            // TODO: Step through this before part 3.
                // I just have a feeling this should iterate 3 more times.
                // velocity should get updated in those times and rayLength would be 
                // the length of the longest distance between the player and the ground.
        }
    }

    private void UpdateRaycastOrigins()
    {
        var bounds = _boxCollider2D.bounds;
        bounds.Expand(SKIN_WIDTH * -2); // Shrink the bounds of the collider inwards.

        _raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        _raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }
    
    // Only needs to be done once, when vertical/horizontal ray count has changed
    // TODO: OnValidate, this should change.
    private void CalculateSpaceBetweenRaycasts()
    {
        var bounds = _boxCollider2D.bounds;
        bounds.Expand(SKIN_WIDTH * -2); // Shrink the bounds of the collider inwards.
        
        // Ensure that there are at least two rays firing for collision detection.
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, MIN_RAY_COUNT, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, MIN_RAY_COUNT, int.MaxValue);

        // Calculate spacing between each ray.
        _horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
}
