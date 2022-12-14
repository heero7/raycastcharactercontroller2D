using UnityEngine;

namespace RaycastController
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RaycastObject2D : MonoBehaviour
    {
        private const int MINIMUM_RAY_COUNT = 2;
    
        protected const float SkinWidth = 0.015f;

        [Header("Raycast Controller Variables")]
        [SerializeField] protected int horizontalRayCount = 4;
        [SerializeField] protected int verticalRayCount = 4;
    
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
        
            // Ensure that there are at least two rays firing for collision detection.
            horizontalRayCount = Mathf.Clamp(horizontalRayCount, MINIMUM_RAY_COUNT, int.MaxValue);
            verticalRayCount = Mathf.Clamp(verticalRayCount, MINIMUM_RAY_COUNT, int.MaxValue);

            // Calculate spacing between each ray.
            HorizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            VerticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }  
    }
}