using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

namespace GameJamTeamOne
{
    public sealed class DerivedPlayerCharacter : PlayerCharacter
    {
        #region Fields

        [Header("Jetpack")]
        [SerializeField] private JetpackType _jetpackType;
        [SerializeField] private float _jetpackMaxFlightTime = 5f;
        [SerializeField] private float _jetpaHoverDownSpeed = 10f;
        [SerializeField] private bool _hasFlightCeiling = false;
        [SerializeField, Min(0)] private float _flightCeilingHeight;
        [Header("Upward thrust settings")]
        [SerializeField] private float _jetpackVerticalAcceleration = 5f;
        [SerializeField] private float _jetpackMaxVerticalSpeed = 10f;
        [SerializeField, Range(0f, 2f)] public float _jetpackHorizAccelProportion;
        [SerializeField, Range(0f, 1f)] public float _jetpackHorizDecelProportion;
        [Header("Freeflight settings")]
        [SerializeField] private float _freeflightJetpackSpeed = 10f;
        [SerializeField] private float _freeflightJetpackAcceleration = 100f;
        [SerializeField] private float _freeflightJetpackDeceleration = 100f;
        [SerializeField] private bool _canAscend = true;
        [Space]
        [SerializeField] private string _jetpackItemName = "Jetpack";
        [Space]
        [SerializeField] private ParticleSystem _jetStreamFX;
        [SerializeField] private Transform _facingLeftJetStreamPoint;
        [SerializeField] private Transform _facingRightJetStreamnPoint;
        [Header("Jetstream damager")]
        [SerializeField] private Damager _jetstreamDamager;
        [SerializeField, Min(0.1f)] private float _damageTicksPerSecond = 0.1f;

        private float _remainingFlightTime;
        private readonly int _hashJetpacking = Animator.StringToHash("IsJetpacking");
        private bool _isJetpacking;
        private float _currentFlightCeiling;

        private Coroutine _nextJetstreamDamageTickEnabler;

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

        public bool CheckIfJetpackIsInUse() => PlayerInput.Instance.Jump.Held;

        public void SetJetpacking(bool isJetpacking) => m_Animator.SetBool(_hashJetpacking, isJetpacking);

        public void StartJetpacking()
        {
            if (!_isJetpacking)
            {
                _remainingFlightTime = _jetpackMaxFlightTime;
                _isJetpacking = true;
                _jetStreamFX.gameObject.SetActive(true);
                _currentFlightCeiling = transform.position.y + _flightCeilingHeight;

                if(_jetstreamDamager)
                {
                    _jetstreamDamager.EnableDamage();
                    _jetstreamDamager.disableDamageAfterHit = true;
                }
            }
        }

        public void StopJetpacking()
        {
            _jetStreamFX.gameObject.SetActive(false);
            SetJetpacking(false);
            _isJetpacking = false;
            StopDamageCooldownCoroutine();
        }

        public bool CheckForGroundedWithJetpack()
        {
            var isGrounded = CheckForGrounded();

            if (isGrounded)
                StopJetpacking();

            return isGrounded;
        }

        public void JetpackVerticalMovement()
        {
            UpdateFlightTime();

            if (_remainingFlightTime <= 0)
                FloatDownWithJetpack();
            else if (PlayerInput.Instance.Vertical.Value < 0f || m_CharacterController2D.IsCeilinged && m_MoveVector.y > 0f || 
                _hasFlightCeiling && transform.position.y >= _currentFlightCeiling)
                SetVerticalMovement(0f);
            else
            {
                m_MoveVector.y = Mathf.Min(m_MoveVector.y + _jetpackVerticalAcceleration * Time.deltaTime, _jetpackMaxVerticalSpeed);
            }
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

            Vector2 desiredMovementVector;
            desiredMovementVector.x = PlayerInput.Instance.Horizontal.Value;
            desiredMovementVector.y = PlayerInput.Instance.Vertical.Value;

            var desiredHorizontalSpeed = desiredMovementVector.normalized.x * _freeflightJetpackSpeed;
            var desiredVerticakSpeed = desiredMovementVector.normalized.y * _freeflightJetpackSpeed;

            if ((!_canAscend || _hasFlightCeiling && transform.position.y >= _currentFlightCeiling)
                && desiredVerticakSpeed > 0f)
                desiredVerticakSpeed = 0f;

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

        private void UpdateFlightTime()
        {
            if (_remainingFlightTime > 0)
                _remainingFlightTime -= Time.deltaTime;
        }

        private void FloatDownWithJetpack() => m_MoveVector.y -= _jetpaHoverDownSpeed * Time.deltaTime;

        public void UpdateJetstreamPosition() => 
            _jetStreamFX.transform.position = spriteRenderer.flipX ? _facingRightJetStreamnPoint.position : _facingLeftJetStreamPoint.position;

        public void SetFreeflightAscend(bool canAscend) => _canAscend = canAscend;

        public void SetJetpackType(JetpackType type) => _jetpackType = type;

        public void SetFlightCeilingEnabled(bool isEnabled) => _hasFlightCeiling = isEnabled;

        public void SetFlightCeilingHeight(float height) => _flightCeilingHeight = height;

        public void ChangeFlightCeilingHeightByValue(float value) => _flightCeilingHeight += value;

        public void BeginDamageCooldown()
        {
            StopDamageCooldownCoroutine();

            _nextJetstreamDamageTickEnabler = StartCoroutine(EnableNextDamageTick());
        }

        private void StopDamageCooldownCoroutine()
        {
            if (_nextJetstreamDamageTickEnabler != null)
            {
                StopCoroutine(_nextJetstreamDamageTickEnabler);
                _nextJetstreamDamageTickEnabler = null;
            }
        }

        IEnumerator EnableNextDamageTick()
        {
            yield return new WaitForSeconds(1f / _damageTicksPerSecond);
            _jetstreamDamager.EnableDamage();
            _nextJetstreamDamageTickEnabler = null;
        }

        #endregion
    }
}
