using System.Collections.Generic;
using UnityEngine;

namespace RaycastController2D
{
    public class RaycastPlatformController2D : RaycastObject2D
    {
        [Header("Platform Variables")]
        [SerializeField] private LayerMask passengerMask;
        [SerializeField] private float platformSpeed = 1.5f;
        [SerializeField] private float waitTime = 0.5f;
        [SerializeField] [Range(0,2)]private float easeAmount = 0.5f;
        [SerializeField] private bool cyclic;
        [SerializeField] private Vector2[] localWayPoints;

        private Vector2[] _globalWayPoints;
        private readonly List<PassengerMovement> _passengerMovements = new();
        private readonly Dictionary<Transform, RaycastCharacterController2D> _raycastControllerCache = new();
        
        private int _fromWaypointIndex;
        private float _percentageBetweenWaypoints;
        private float _nextTimePlatformCanMove;

        protected override void Start()
        {
            base.Start();

            _globalWayPoints = new Vector2[localWayPoints.Length];
            for (var i = 0; i < localWayPoints.Length; i++)
            {
                _globalWayPoints[i] = localWayPoints[i] + (Vector2) transform.position;
            }
        }

        private void Update()
        {
            UpdateRaycastOrigins();
            
            var velocity = CalculatePlatformMovement();
            
            CalculatePassengerMovement(velocity);
            
            MovePassengers(true);
            transform.Translate(velocity);
            MovePassengers(false);
        }

        private Vector2 CalculatePlatformMovement()
        {
            if (Time.time < _nextTimePlatformCanMove)
            {
                return Vector2.zero;
            }
            
            _fromWaypointIndex %= _globalWayPoints.Length;
            
            var toWayPointIndex = (_fromWaypointIndex + 1) % _globalWayPoints.Length;
            var distanceBetweenWaypoints =
                Vector2.Distance(_globalWayPoints[_fromWaypointIndex], _globalWayPoints[toWayPointIndex]);
            _percentageBetweenWaypoints += Time.deltaTime * platformSpeed / distanceBetweenWaypoints;
            _percentageBetweenWaypoints = Mathf.Clamp01(_percentageBetweenWaypoints);

            var easedPercentBetweenWaypoints = Ease(_percentageBetweenWaypoints);

            var newPosition = Vector2.Lerp(_globalWayPoints[_fromWaypointIndex], _globalWayPoints[toWayPointIndex], easedPercentBetweenWaypoints);

            if (_percentageBetweenWaypoints >= 1)
            {
                _percentageBetweenWaypoints = 0f;
                _fromWaypointIndex++;

                if (!cyclic)
                {
                    if (_fromWaypointIndex >= _globalWayPoints.Length - 1)
                    {
                        _fromWaypointIndex = 0;
                        System.Array.Reverse(_globalWayPoints);
                    }
                }

                _nextTimePlatformCanMove = Time.time + waitTime;
            }
            return newPosition - (Vector2) transform.position;
        }

        private void MovePassengers(bool shouldMoveBeforeMovePlatform)
        {
            foreach (var passengerMovement in _passengerMovements)
            {
                if (!_raycastControllerCache.ContainsKey(passengerMovement.Transform))
                {
                    _raycastControllerCache.Add(passengerMovement.Transform, passengerMovement.Transform.GetComponent<RaycastCharacterController2D>());
                }
                
                if (passengerMovement.ShouldMoveBeforePlatform == shouldMoveBeforeMovePlatform)
                {
                    _raycastControllerCache[passengerMovement.Transform].Move(passengerMovement.Velocity, passengerMovement.IsStandingOnPlatform);
                }
            }
        }

        private void CalculatePassengerMovement(Vector2 velocity)
        {
            var movedPassengers = new HashSet<Transform>();
            _passengerMovements.Clear(); // TODO: List<T>.Clear() might be more efficient.
            
            var directionX = Mathf.RoundToInt(Mathf.Sign(velocity.x));
            var directionY = Mathf.RoundToInt(Mathf.Sign(velocity.y));
            
            // Vertically moving platform.
            if (velocity.y != 0)
            {
                var rayLength = Mathf.Abs(velocity.y) + SkinWidth;

                for (var i = 0; i < VerticalRayCount; i++)
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

                            var passengerVelocity = new Vector2(pushX, pushY);
                            var isStandingOnPlatform = directionY == 1;
                            _passengerMovements.Add(new PassengerMovement(hit.transform, passengerVelocity, isStandingOnPlatform, true));
                        }
                    }
                }
            }
            
            // Horizontally moving platform.
            if (velocity.x != 0)
            {
                var rayLength = Mathf.Abs(velocity.x) + SkinWidth;

                for (var i = 0; i < HorizontalRayCount; i++)
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
                            const float SMALL_DOWNWARD_FORCE_TO_ALLOW_JUMPING = -SkinWidth;
                            var pushX = velocity.x - (hit.distance - SkinWidth) * directionX;
                        
                            var passengerVelocity = new Vector2(pushX, SMALL_DOWNWARD_FORCE_TO_ALLOW_JUMPING);
                            _passengerMovements.Add(new PassengerMovement(hit.transform, passengerVelocity, false, true));   
                        }
                    }
                }
            }
            
            // Passenger on a platform moving horizontally or downward moving platform
            if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
            {
                var rayLength = SkinWidth * 2;

                for (var i = 0; i < VerticalRayCount; i++)
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
                        
                            var passengerVelocity = new Vector2(pushX, pushY);
                            _passengerMovements.Add(new PassengerMovement(hit.transform, passengerVelocity, true, false));   
                        }
                    }
                }
            }
        }

        private float Ease(float x)
        {
            var a = easeAmount + 1;
            return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1-x, a));
        }

        private void OnDrawGizmos()
        {
            if (localWayPoints == null) return;
            Gizmos.color = Color.red;
            const float MARKER_LENGTH = 0.3f;

            for (var i = 0; i < localWayPoints.Length; i++)
            {
                var globalWayPointPosition = Application.isPlaying 
                    ? (Vector3) _globalWayPoints[i]
                    : (Vector3) localWayPoints[i] + transform.position;
                Gizmos.DrawLine(globalWayPointPosition - Vector3.up * MARKER_LENGTH, globalWayPointPosition + Vector3.up * MARKER_LENGTH);
                Gizmos.DrawLine(globalWayPointPosition - Vector3.left * MARKER_LENGTH, globalWayPointPosition + Vector3.left * MARKER_LENGTH);
            }
        }
    }
}