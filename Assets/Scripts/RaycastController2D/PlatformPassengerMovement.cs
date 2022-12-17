using UnityEngine;

namespace RaycastController2D
{
    public struct PlatformPassengerMovement
    {
        public Transform Transform { get; }
        public Vector2 Velocity { get; }
        public bool IsStandingOnPlatform { get; }
        public bool ShouldMoveBeforePlatform { get; }

        public PlatformPassengerMovement(Transform transform, Vector2 velocity, bool isStandingOnPlatform,
            bool shouldMoveBeforePlatform)
        {
            Transform = transform;
            Velocity = velocity;
            IsStandingOnPlatform = isStandingOnPlatform;
            ShouldMoveBeforePlatform = shouldMoveBeforePlatform;
        }
    }
}