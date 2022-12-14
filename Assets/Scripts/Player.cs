using RaycastController;
using UnityEngine;

[RequireComponent(typeof(RaycastCharacterController2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump Variables")] 
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float timeToJumpApex = 0.25f;
    
    private float _gravity;
    private float _jumpVelocity;
    
    private RaycastCharacterController2D _controller;

    private Vector2 _velocity;
    private Vector2 _input;

    private void Start()
    {
        _controller = GetComponent<RaycastCharacterController2D>();

        _gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        _jumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
    }

    private void Update()
    {
        if (_controller.CollisionData.Above || _controller.CollisionData.Below)
        {
            _velocity.y = 0;
        }
        _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && _controller.CollisionData.Below)
        {
            _velocity.y = _jumpVelocity;
        }
        _velocity.x = _input.x * moveSpeed;
        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}