using UnityEngine;

namespace RaycastController2D
{
    public struct CollisionInfo
    {
        public bool Above;
        public bool Below;
        public bool Left;
        public bool Right;

        public float SlopeAngle;
        public float SlopeAnglePreviousFrame;
        
        public bool AscendingSlope;
        public bool DescendingSlope;
        public bool IsFallingThroughPlatform;

        public int FacingDirection; 

        public Vector2 PreviousVelocity;

        public void Reset()
        {
            Above = false;
            Below = false;
            Left = false;
            Right = false;

            AscendingSlope = false;
            DescendingSlope = false;
            SlopeAnglePreviousFrame = SlopeAngle;
            SlopeAngle = 0f;
        }
    }
}