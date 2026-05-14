using UnityEngine;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 4.0f;
        public float RotationSpeed = 2.5f;
        public float SpeedChangeRate = 10.0f;
        public float Gravity = -15.0f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.5f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 90.0f;
        public float BottomClamp = -90.0f;

        private float _cinemachineTargetPitch;
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private CharacterController _controller;
        private const float _threshold = 0.01f;

        // ✅ Input vem do Fusion via PlayerSetup, não mais do teclado direto
        private Vector2 _moveInput;
        private Vector2 _lookInput;

        public void SetInput(Vector2 move, Vector2 look)
        {
            _moveInput = move;
            _lookInput = look;
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (_controller == null) return;
            GroundedCheck();
            ApplyGravity();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void Move()
        {
            float targetSpeed = _moveInput == Vector2.zero ? 0f : MoveSpeed;
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;

            if (Mathf.Abs(currentHorizontalSpeed - targetSpeed) > 0.1f)
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
            else
                _speed = targetSpeed;

            Vector3 inputDirection = Vector3.zero;
            if (_moveInput != Vector2.zero)
                inputDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;

            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
        }

        private void CameraRotation()
        {
            if (_lookInput.sqrMagnitude >= _threshold)
            {
                _cinemachineTargetPitch -= _lookInput.y * RotationSpeed * 0.1f;
                _rotationVelocity = _lookInput.x * RotationSpeed * 0.1f;
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                if (CinemachineCameraTarget != null)
                    CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0f, 0f);

                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void ApplyGravity()
        {
            if (Grounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0f, 1f, 0f, 0.35f);
            Color transparentRed = new Color(1f, 0f, 0f, 0.35f);
            Gizmos.color = Grounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
    }
}