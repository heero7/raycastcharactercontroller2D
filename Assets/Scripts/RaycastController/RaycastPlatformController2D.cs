using System.Collections.Generic;
using UnityEngine;

namespace RaycastController
{
    public class RaycastPlatformController2D : RaycastObject2D
    {
        [Header("Platform Variables")]
        [SerializeField] private LayerMask passengerMask;
        [SerializeField] private Vector2 move;

        private void Update()
        {
            UpdateRaycastOrigins();
            
            var velocity = move * Time.deltaTime;
            
            MovePassengers(velocity);
            transform.Translate(velocity);
        }

        private void MovePassengers(Vector2 velocity)
        {
            var movedPassengers = new HashSet<Transform>();
            
            var directionX = (int) Mathf.Sign(velocity.x);
            var directionY = (int) Mathf.Sign(velocity.y);
            
            // Vertically moving platform
            if (velocity.y != 0)
            {
                var rayLength = Mathf.Abs(velocity.y) + SkinWidth;

                for (var i = 0; i < verticalRayCount; i++)
                {
                    var rayOrigin = directionY == -1
                        ? RaycastOrigins.BottomLeft
                        : RaycastOrigins.TopLeft;

                    rayOrigin += Vector2.right * (VerticalRaySpacing * i);
                    var hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                    if (hit)
                    {
                        if (!movedPassengers.Contains(hit.transform))
                        {
                            movedPassengers.Add(hit.transform);
                            var pushY = velocity.y - (hit.distance - SkinWidth) * directionY;
                            var pushX = directionY == 1
                                ? velocity.x
                                : 0;
                        
                            hit.transform.Translate(new Vector3(pushX, pushY));   
                        }
                    }
                }
            }
            
            if (velocity.x != 0)
            {
                var rayLength = Mathf.Abs(velocity.x) + SkinWidth;

                for (var i = 0; i < horizontalRayCount; i++)
                {
                    var rayOrigin = directionX == -1
                        ? RaycastOrigins.BottomLeft
                        : RaycastOrigins.BottomRight;
                    
                    rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
                    var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
                    
                    if (hit)
                    {
                        if (!movedPassengers.Contains(hit.transform))
                        {
                            movedPassengers.Add(hit.transform);
                            var pushY = 0;
                            var pushX = velocity.x - (hit.distance - SkinWidth) * directionX;
                        
                            hit.transform.Translate(new Vector3(pushX, pushY));   
                        }
                    }
                }
            }
            
            // Passenger on a platform moving horizontally or downward moving platform
            if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
            {
                var rayLength = SkinWidth * 2;

                for (var i = 0; i < verticalRayCount; i++)
                {
                    var rayOrigin = RaycastOrigins.TopLeft + Vector2.right * (VerticalRaySpacing * i);
                    var hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                    if (hit)
                    {
                        if (!movedPassengers.Contains(hit.transform))
                        {
                            movedPassengers.Add(hit.transform);
                            var pushY = velocity.y;
                            var pushX = velocity.x;
                        
                            hit.transform.Translate(new Vector3(pushX, pushY));   
                        }
                    }
                }
            }
        }
    }
}