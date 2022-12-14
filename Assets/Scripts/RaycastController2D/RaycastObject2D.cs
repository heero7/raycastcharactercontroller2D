using UnityEngine;

namespace RaycastController2D
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RaycastObject2D : MonoBehaviour
    {
        private const int MINIMUM_RAY_COUNT = 2;
        private const float DISTANCE_BETWEEN_RAYS = 0.25f;
    
        protected const float SkinWidth = 0.015f;

        [Header("Raycast Controller Variables")]
        protected int HorizontalRayCount;
        protected int VerticalRayCount;
    
        [SerializeField]
        [Tooltip("These are the layers the controller will collide with.")]
        protected LayerMask collisionMask;

        [Header("Debug")] 
        [SerializeField] protected bool showDebugLines;

        protected RaycastOrigins RaycastOrigins;
        protected CollisionInfo Collisions;
    
        protected float HorizontalRaySpacing { get; private set; }
        protected float VerticalRaySpacing { get; private set; }

        private BoxCollider2D _boxCollider2D;
    
        protected virtual void Start()
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
        
            CalculateSpaceBetweenRaycasts();
        }
    
        protected void UpdateRaycastOrigins()
        {
            var bounds = _boxCollider2D.bounds;
            bounds.Expand(SkinWidth * -2); // Shrink the bounds of the collider inwards.

            RaycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            RaycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
            RaycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
            RaycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
        }
    
        private void CalculateSpaceBetweenRaycasts()
        {
            // TODO: OnValidate, this should change.
            var bounds = _boxCollider2D.bounds;
            bounds.Expand(SkinWidth * -2); // Shrink the bounds of the collider inwards.

            var boundsWidth = bounds.size.x;
            var boundsHeight = bounds.size.y;

            HorizontalRayCount = Mathf.RoundToInt(boundsHeight / DISTANCE_BETWEEN_RAYS);
            VerticalRayCount = Mathf.RoundToInt(boundsWidth / DISTANCE_BETWEEN_RAYS);
        
            // Ensure that there are at least two rays firing for collision detection.
            HorizontalRayCount = Mathf.Clamp(HorizontalRayCount, MINIMUM_RAY_COUNT, int.MaxValue);
            VerticalRayCount = Mathf.Clamp(VerticalRayCount, MINIMUM_RAY_COUNT, int.MaxValue);

            // Calculate spacing between each ray.
            HorizontalRaySpacing = bounds.size.y / (HorizontalRayCount - 1);
            VerticalRaySpacing = bounds.size.x / (VerticalRayCount - 1);
        }  
    }
}