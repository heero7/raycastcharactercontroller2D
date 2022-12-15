using UnityEngine;

namespace RaycastController2D
{
    public class RaycastCharacterController2D : RaycastObject2D
    {
        [SerializeField] private float maximumAllowedSlopeAngle = 80f;
        [SerializeField] private bool startingFaceDirectionIsRight = true;

        public CollisionInfo CollisionData => Collisions;

        private Vector2 _input;

        private void OnEnable()
        {
            // TODO: How can we set this at this for the project without a manual step?
            // Can we check this at project start?
            // Do we have to do this for each GameObject that has this?
            if (!Physics2D.autoSyncTransforms)
            {
                Physics2D.autoSyncTransforms = true;
            }

            if (!Physics2D.reuseCollisionCallbacks)
            {
                Physics2D.reuseCollisionCallbacks = true;
            }
        }

        protected override void Start()
        {
            base.Start();
            Collisions.FacingDirection = startingFaceDirectionIsRight ? 1 : -1;
        }

        public void Move(Vector2 targetVelocity, bool isStandingOnPlatform = false)
        {
            Move(targetVelocity, Vector2.zero, isStandingOnPlatform);
        }

        public void Move(Vector2 targetVelocity, Vector2 input, bool isStandingOnPlatform = false)
        {
            UpdateRaycastOrigins();
            Collisions.Reset();
            Collisions.PreviousVelocity = targetVelocity;
            _input = input;

            if (targetVelocity.x != 0)
            {
                Collisions.FacingDirection = Mathf.RoundToInt(Mathf.Sign(targetVelocity.x));
            }

            if (targetVelocity.y < 0)
            {
                DescendSlope(ref targetVelocity);
            }
            
            HandleHorizontalCollisions(ref targetVelocity);

            if (targetVelocity.y != 0)
            {
                HandleVerticalCollisions(ref targetVelocity);
            }
            
            transform.Translate(targetVelocity);

            if (isStandingOnPlatform)
            {
                Collisions.Below = true;
            }
        }

        private void AscendSlope(ref Vector2 velocity, float slopeAngle)
        {
            var movementDistance = Mathf.Abs(velocity.x);
            var ascendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * movementDistance;

            if (!(velocity.y <= ascendVelocityY)) return;
        
            Collisions.Below = true;
            Collisions.AscendingSlope = true;
            Collisions.SlopeAngle = slopeAngle;
            velocity.y = ascendVelocityY; 
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * movementDistance * Mathf.Sign(velocity.x);
        }

        private void DescendSlope(ref Vector2 velocity)
        {
            var directionX = Mathf.RoundToInt(Mathf.Sign(velocity.x));
            var rayOrigin = directionX == -1
                ? RaycastOrigins.BottomRight
                : RaycastOrigins.BottomLeft;

            var hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if (hit)
            {
                var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maximumAllowedSlopeAngle)
                {
                    if (Mathf.RoundToInt(Mathf.Sign(hit.normal.x)) == directionX)
                    {
                        if (hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                        {
                            var movementDistance = Mathf.Abs(velocity.x);
                            var descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * movementDistance;
                            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * movementDistance * Mathf.Sign(velocity.x);
                            velocity.y -= descendVelocityY;

                            Collisions.SlopeAngle = slopeAngle;
                            Collisions.Below = true;
                            Collisions.DescendingSlope = true;
                        }
                    }
                }
            }
        }
    
        private void HandleHorizontalCollisions(ref Vector2 velocity)
        {
            var directionX = Collisions.FacingDirection;
            var rayLength = Mathf.Abs(velocity.x) + SkinWidth;

            if (Mathf.Abs(velocity.x) < SkinWidth)
            {
                rayLength = 2 * SkinWidth;
            }

            for (var i = 0; i < HorizontalRayCount; i++)
            {
                var rayOrigin = directionX == -1
                    ? RaycastOrigins.BottomLeft
                    : RaycastOrigins.BottomRight;

                // Project where we WILL be (why we add velocity.x).
                rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
                var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (showDebugLines)
                {
                    Debug.DrawRay(rayOrigin, Vector2.right * (directionX), Color.red);
                }

                if (hit)
                {
                    if (hit.distance == 0)
                    {
                        continue;
                    }

                    var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (i == 0 && slopeAngle <= maximumAllowedSlopeAngle)
                    {
                        if (Collisions.DescendingSlope)
                        {
                            Collisions.DescendingSlope = false;
                            velocity = Collisions.PreviousVelocity;
                        }

                        var distanceToBeginningOfTheSlope = 0f;
                        if (slopeAngle != Collisions.SlopeAnglePreviousFrame)
                        {
                            distanceToBeginningOfTheSlope = hit.distance - SkinWidth;
                            velocity.x -= distanceToBeginningOfTheSlope * directionX;
                        }

                        AscendSlope(ref velocity, slopeAngle);
                        velocity.x += distanceToBeginningOfTheSlope * directionX;
                    }

                    if (Collisions.AscendingSlope && !(slopeAngle > maximumAllowedSlopeAngle)) continue;
                    velocity.x = (hit.distance - SkinWidth) * directionX;
                    rayLength = hit.distance;

                    if (Collisions.AscendingSlope)
                    {
                        velocity.y = Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    Collisions.Left = directionX == -1;
                    Collisions.Right = directionX == 1;
                }
            }
        }

        private void HandleVerticalCollisions(ref Vector2 velocity)
        {
            var directionY = Mathf.RoundToInt(Mathf.Sign(velocity.y)); // 1 = Moving Up, -1 Moving Down.
            var rayLength = Mathf.Abs(velocity.y) + SkinWidth;

            for (var i = 0; i < VerticalRayCount; i++)
            {
                var rayOrigin = directionY == -1
                    ? RaycastOrigins.BottomLeft
                    : RaycastOrigins.TopLeft;

                // Project where we WILL be (why we add velocity.x).
                rayOrigin += Vector2.right * (VerticalRaySpacing * i + velocity.x);
                var hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            
                if (showDebugLines)
                {
                    Debug.DrawRay(rayOrigin, Vector2.up * (directionY), Color.red);
                }

                if (hit)
                {
                    if (hit.collider.CompareTag("ThroughPlatform")) // TODO: Editor Script to ensure this created
                    {
                        if (directionY == 1 || hit.distance == 0)
                        {
                            continue;   
                        }

                        if (Collisions.IsFallingThroughPlatform)
                        {
                            continue;
                        }

                        if (Mathf.RoundToInt(_input.y) == -1)
                        {
                            Collisions.IsFallingThroughPlatform = true;
                            Invoke(nameof(ResetFallingThroughPlatform), 0.5f);
                            continue;
                        }
                    }
                    velocity.y = (hit.distance - SkinWidth) * directionY;
                    rayLength = hit.distance;

                    if (Collisions.AscendingSlope)
                    {
                        velocity.x = velocity.y / Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) *
                                     Mathf.Sign(velocity.x);
                    }
                
                    Collisions.Below = directionY == -1;
                    Collisions.Above = directionY == 1;
                }
            
                // TODO: Step through this before part 3.
                // I just have a feeling this should iterate 3 more times.
                // velocity should get updated in those times and rayLength would be 
                // the length of the longest distance between the player and the ground.
            }

            if (Collisions.AscendingSlope)
            {
                var directionX = Mathf.RoundToInt(Mathf.Sign(velocity.x));
                rayLength = Mathf.RoundToInt(Mathf.Abs(velocity.x) + SkinWidth);
                var rayOrigin = (directionX == -1
                    ? RaycastOrigins.BottomLeft
                    : RaycastOrigins.BottomRight) + Vector2.up * velocity.y;

                var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (hit)
                {
                    var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != Collisions.SlopeAngle)
                    {
                        velocity.x = (hit.distance - SkinWidth) * directionX;
                        Collisions.SlopeAngle = slopeAngle;
                    }
                }
            }
        }

        private void ResetFallingThroughPlatform()
        {
            Collisions.IsFallingThroughPlatform = false;
        }
    }
}
