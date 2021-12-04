using UnityEngine;
using Gamekit2D;

namespace GameJamTeamOne
{
    public sealed class ModifiedPlayerCharacter : PlayerCharacter
    {
        #region Fields

        [Header("Jetpack")]
        [SerializeField] private JetpackType _jetpackType;
        [SerializeField] private float _jetpackMaxFlightTime = 5f;
        [SerializeField] private float _jetpaHoverDownSpeed = 10f;
        [Header("Upward thrust settings")]
        [SerializeField] private float _jetpackVerticalAcceleration = 5f;
        [SerializeField] private float _jetpackMaxVerticalSpeed = 10f;
        [SerializeField, Range(0f, 2f)] public float _jetpackHorizAccelProportion;
        [SerializeField, Range(0f, 1f)] public float _jetpackHorizDecelProportion;
        [Header("Freeflight settings")]
        [SerializeField] private float _freeflightJetpackSpeed = 10f;
        [SerializeField] private float _freeflightJetpackAcceleration = 100f;
        [SerializeField] private float _freeflightJetpackDeceleration = 100f;
        [Space]
        [SerializeField] private string _jetpackItemName = "Jetpack";
        [Space]
        [SerializeField] private ParticleSystem _jetStreamFX;
        [SerializeField] private Transform _facingLeftJetStreamPoint;
        [SerializeField] private Transform _facingRightJetStreamnPoint;

        private float _remainingFlightTime;
        private readonly int _hashJetpacking = Animator.StringToHash("IsJetpacking");
        private bool _isJetpacking;

        #endregion


        #region Fields

        public JetpackType JetpackType => _jetpackType;

        #endregion


        #region Methods

        public void CheckForJetpack()
        {
            if (inventoryController.HasItem(_jetpackItemName))
                SetJetpacking(CheckForJumpInput());
        }

        public void SetJetpacking(bool isJetpacking)
        {
            m_Animator.SetBool(_hashJetpacking, isJetpacking);
        }

        public void StartJetpacking()
        {
            if (!_isJetpacking)
            {
                _remainingFlightTime = _jetpackMaxFlightTime;
                _isJetpacking = true;
                _jetStreamFX.gameObject.SetActive(true);
            }
        }

        public void StopJetpacking()
        {
            SetJetpacking(false);
            _isJetpacking = false;
            _jetStreamFX.gameObject.SetActive(false);
        }

        public bool CheckForGroundedWithJetpack()
        {
            var isGrounded = CheckForGrounded();

            if (isGrounded)
                StopJetpacking();

            return isGrounded;
        }

        public bool CheckIfJetpackIsInUse()
        {
            return PlayerInput.Instance.Jump.Held;
        }

        public void JetpackVerticalMovement()
        {
            UpdateFlightTime();

            if (_remainingFlightTime <= 0)
                FloatDownWithJetpack();
            else if (PlayerInput.Instance.Vertical.Value < 0f || m_CharacterController2D.IsCeilinged && m_MoveVector.y > 0f)
                SetVerticalMovement(0f);
            else
                m_MoveVector.y = Mathf.Min(m_MoveVector.y + _jetpackVerticalAcceleration * Time.deltaTime, _jetpackMaxVerticalSpeed);
        }

        private void FloatDownWithJetpack() => m_MoveVector.y -= _jetpaHoverDownSpeed * Time.deltaTime;
        

        private void UpdateFlightTime()
        {
            if (_remainingFlightTime > 0)
                _remainingFlightTime -= Time.deltaTime;
        }

        public void UpdateJetstreamPosition()
        {
            _jetStreamFX.transform.position = spriteRenderer.flipX ? _facingRightJetStreamnPoint.position : _facingLeftJetStreamPoint.position;
        }

        public void JetpackHorizontalMovement()
        {
            float desiredSpeed = PlayerInput.Instance.Horizontal.Value * maxSpeed;

            float acceleration;

            if (PlayerInput.Instance.Horizontal.ReceivingInput)
                acceleration = groundAcceleration * _jetpackHorizAccelProportion;
            else
                acceleration = groundDeceleration * _jetpackHorizDecelProportion;

            m_MoveVector.x = Mathf.MoveTowards(m_MoveVector.x, desiredSpeed, acceleration * Time.deltaTime);
        }

        public void JetpackFreeflightMovement()
        {
            UpdateFlightTime();

            Vector2 desiredVector;
            desiredVector.x = PlayerInput.Instance.Horizontal.Value;
            desiredVector.y = PlayerInput.Instance.Vertical.Value;

            var desiredHorizontalSpeed = desiredVector.normalized.x * _freeflightJetpackSpeed;
            var desiredVerticakSpeed = desiredVector.normalized.y * _freeflightJetpackSpeed;

            float acceleration;

            if (PlayerInput.Instance.Horizontal.ReceivingInput)
                acceleration = _freeflightJetpackAcceleration;
            else
                acceleration = _freeflightJetpackDeceleration;

            m_MoveVector.x = Mathf.MoveTowards(m_MoveVector.x, desiredHorizontalSpeed, acceleration * Time.deltaTime);

            if (_remainingFlightTime > 0)
            {
                if (PlayerInput.Instance.Vertical.ReceivingInput)
                    acceleration = _freeflightJetpackAcceleration;
                else
                    acceleration = _freeflightJetpackDeceleration;

                m_MoveVector.y = Mathf.MoveTowards(m_MoveVector.y, desiredVerticakSpeed, acceleration * Time.deltaTime);
            }
            else
                FloatDownWithJetpack();
        }

        #endregion

        private void LateUpdate()
        {
            Debug.Log(_remainingFlightTime);
        }
    }
}
