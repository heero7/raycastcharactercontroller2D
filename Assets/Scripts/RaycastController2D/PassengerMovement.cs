using UnityEngine;

namespace RaycastController2D
{
    public struct PassengerMovement
    {
        public Transform Transform { get; }
        public Vector2 Velocity { get; }
        public bool IsStandingOnPlatform { get; }
        public bool ShouldMoveBeforePlatform { get; }

        public PassengerMovement(Transform transform, Vector2 velocity, bool isStandingOnPlatform,
            bool shouldMoveBeforePlatform)
        {
            Transform = transform;
            Velocity = velocity;
            IsStandingOnPlatform = isStandingOnPlatform;
            ShouldMoveBeforePlatform = shouldMoveBeforePlatform;
        }
    }
}