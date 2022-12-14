using UnityEngine;

namespace RaycastController2D
{
    public class RaycastCharacterController2D : RaycastObject2D
    {
        private const float MAXIMUM_ASCEND_SLOPE_ANGLE = 80f;
        private const float MAXIMUM_DESCEND_SLOPE_ANGLE = 75f;

        public CollisionInfo CollisionData => Collisions;

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

        public void Move(Vector2 targetVelocity, bool isStandingOnPlatform = false)
        {
            UpdateRaycastOrigins();
            Collisions.Reset();
            Collisions.PreviousVelocity = targetVelocity;

            if (targetVelocity.y < 0)
            {
                DescendSlope(ref targetVelocity);
            }
        
            if (targetVelocity.x != 0)
            {
                HandleHorizontalCollisions(ref targetVelocity);
            }

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
            var directionX = (int) Mathf.Sign(velocity.x);
            var rayOrigin = directionX == -1
                ? RaycastOrigins.BottomRight
                : RaycastOrigins.BottomLeft;

            var hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if (hit)
            {
                var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= MAXIMUM_DESCEND_SLOPE_ANGLE)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
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
            var directionX = (int) Mathf.Sign(velocity.x);
            var rayLength = Mathf.Abs(velocity.x) + SkinWidth;

            for (var i = 0; i < horizontalRayCount; i++)
            {
                var rayOrigin = directionX == -1
                    ? RaycastOrigins.BottomLeft
                    : RaycastOrigins.BottomRight;

                // Project where we WILL be (why we add velocity.x).
                rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
                var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (showDebugLines)
                {
                    Debug.DrawRay(rayOrigin, Vector3.right * (directionX * rayLength), Color.red);
                }

                if (hit)
                {
                    if (hit.distance == 0)
                    {
                        continue;
                    }
                    
                    var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (i == 0 && slopeAngle <= MAXIMUM_ASCEND_SLOPE_ANGLE)
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

                    if (!Collisions.AscendingSlope || slopeAngle > MAXIMUM_ASCEND_SLOPE_ANGLE)
                    {
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
        }

        private void HandleVerticalCollisions(ref Vector2 velocity)
        {
            var directionY = (int) Mathf.Sign(velocity.y); // 1 = Moving Up, -1 Moving Down.
            var rayLength = Mathf.Abs(velocity.y) + SkinWidth;

            for (var i = 0; i < verticalRayCount; i++)
            {
                var rayOrigin = directionY == -1
                    ? RaycastOrigins.BottomLeft
                    : RaycastOrigins.TopLeft;

                // Project where we WILL be (why we add velocity.x).
                rayOrigin += Vector2.right * (VerticalRaySpacing * i + velocity.x);
                var raycastHit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            
                if (showDebugLines)
                {
                    Debug.DrawRay(rayOrigin, Vector3.up * (directionY * rayLength), Color.red);
                }

                if (raycastHit)
                {
                    velocity.y = (raycastHit.distance - SkinWidth) * directionY;
                    rayLength = raycastHit.distance;

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
                var directionX = (int) Mathf.Sign(velocity.x);
                rayLength = Mathf.Abs(velocity.x) + SkinWidth;
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
    }
}
