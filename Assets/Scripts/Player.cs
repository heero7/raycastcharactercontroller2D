using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpVelocity = 10f;
    
    private CharacterController2D _controller;

    private Vector2 _velocity;
    private Vector2 _input;
    private float _gravity = -20f;

    private void Start()
    {
        _controller = GetComponent<CharacterController2D>();
    }

    private void Update()
    {
        if (_controller.Collisions.Above || _controller.Collisions.Below)
        {
            _velocity.y = 0;
        }
        _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && _controller.Collisions.Below)
        {
            _velocity.y = jumpVelocity;
        }
        _velocity.x = _input.x * moveSpeed;
        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}