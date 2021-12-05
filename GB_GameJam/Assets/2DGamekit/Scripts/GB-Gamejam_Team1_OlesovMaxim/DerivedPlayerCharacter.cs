using System.Collections;
using UnityEngine;
using Gamekit2D;

namespace GameJamTeamOne
{
    public sealed class DerivedPlayerCharacter : PlayerCharacter, IDataPersister
    {
        #region Fields

        [Header("Jetpack")]
        [SerializeField] private JetpackSettings _jetpackSettings;
        [Space]
        [SerializeField] private string _jetpackItemName = "Jetpack";
        [Space]
        [SerializeField] private ParticleSystem _jetStreamFX;
        [SerializeField] private Transform _facingLeftJetStreamPoint;
        [SerializeField] private Transform _facingRightJetStreamnPoint;
        [Header("Jetstream damager")]
        [SerializeField] private Damager _jetstreamDamager;
        [SerializeField, Min(0.1f)] private float _damageTicksPerSecond = 0.1f;
        [Space]
        [SerializeField] private DataSettings _dataSettings;

        private float _remainingFlightTime;
        private readonly int _hashJetpacking = Animator.StringToHash("IsJetpacking");
        private bool _isJetpacking;
        private float _currentFlightCeiling;

        private Coroutine _nextJetstreamDamageTickEnabler;

        #endregion


        #region Fields

        public JetpackType CurrentJetpackType => _jetpackSettings.JetpackType;

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
                _remainingFlightTime = _jetpackSettings.JetpackMaxFlightTime;
                _isJetpacking = true;
                _jetStreamFX.gameObject.SetActive(true);
                _currentFlightCeiling = transform.position.y + _jetpackSettings.FlightCeilingHeight;

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
                _jetpackSettings.HasFlightCeiling && transform.position.y >= _currentFlightCeiling)
                SetVerticalMovement(0f);
            else
            {
                m_MoveVector.y = Mathf.Min(m_MoveVector.y + _jetpackSettings.JetpackVerticalAcceleration * Time.deltaTime, _jetpackSettings.JetpackMaxVerticalSpeed);
            }
        }

        public void JetpackHorizontalMovement()
        {
            float desiredSpeed = PlayerInput.Instance.Horizontal.Value * maxSpeed;

            float acceleration;

            if (PlayerInput.Instance.Horizontal.ReceivingInput)
                acceleration = groundAcceleration * _jetpackSettings.JetpackHorizAccelProportion;
            else
                acceleration = groundDeceleration * _jetpackSettings.JetpackHorizDecelProportion;

            m_MoveVector.x = Mathf.MoveTowards(m_MoveVector.x, desiredSpeed, acceleration * Time.deltaTime);
        }

        public void JetpackFreeflightMovement()
        {
            UpdateFlightTime();

            Vector2 desiredMovementVector;
            desiredMovementVector.x = PlayerInput.Instance.Horizontal.Value;
            desiredMovementVector.y = PlayerInput.Instance.Vertical.Value;

            var desiredHorizontalSpeed = desiredMovementVector.normalized.x * _jetpackSettings.FreeflightJetpackSpeed;
            var desiredVerticakSpeed = desiredMovementVector.normalized.y * _jetpackSettings.FreeflightJetpackSpeed;

            if ((!_jetpackSettings.CanAscend || _jetpackSettings.HasFlightCeiling && transform.position.y >= _currentFlightCeiling)
                && desiredVerticakSpeed > 0f)
                desiredVerticakSpeed = 0f;

            float acceleration;

            if (PlayerInput.Instance.Horizontal.ReceivingInput)
                acceleration = _jetpackSettings.FreeflightJetpackAcceleration;
            else
                acceleration = _jetpackSettings.FreeflightJetpackDeceleration;

            m_MoveVector.x = Mathf.MoveTowards(m_MoveVector.x, desiredHorizontalSpeed, acceleration * Time.deltaTime);

            if (_remainingFlightTime > 0)
            {
                if (PlayerInput.Instance.Vertical.ReceivingInput)
                    acceleration = _jetpackSettings.FreeflightJetpackAcceleration;
                else
                    acceleration = _jetpackSettings.FreeflightJetpackDeceleration;

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

        private void FloatDownWithJetpack() => m_MoveVector.y -= _jetpackSettings.JetpackHoverDownSpeed * Time.deltaTime;

        public void UpdateJetstreamPosition() => 
            _jetStreamFX.transform.position = spriteRenderer.flipX ? _facingRightJetStreamnPoint.position : _facingLeftJetStreamPoint.position;

        public void SetFreeflightAscend(bool canAscend) => _jetpackSettings.CanAscend = canAscend;

        public void SetJetpackType(JetpackType type) => _jetpackSettings.JetpackType = type;

        public void SetFlightCeilingEnabled(bool isEnabled) => _jetpackSettings.HasFlightCeiling = isEnabled;

        public void SetFlightCeilingHeight(float height) => _jetpackSettings.FlightCeilingHeight = height;

        public void ChangeFlightCeilingHeightByValue(float value) => _jetpackSettings.FlightCeilingHeight += value;

        public void ChangeJetpackType()
        {
            if (_jetpackSettings.JetpackType == JetpackType.Freeflight)
                SetJetpackType(JetpackType.UpwardThrust);
            else
                SetJetpackType(JetpackType.Freeflight);
        }

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


        #region UnityMethods

        void OnEnable()
        {
            PersistentDataManager.RegisterPersister(this);
        }

        void OnDisable()
        {
            PersistentDataManager.UnregisterPersister(this);
        }

        #endregion


        #region IDataPersister

        public DataSettings GetDataSettings() => _dataSettings;

        public Data SaveData() { Debug.Log("KBA " + name); return new Data<JetpackSettings>(_jetpackSettings); }

        public void LoadData(Data data)
        {
            Debug.Log("KPR " + name);
            var newData = data as Data<JetpackSettings>;
            _jetpackSettings = newData.value;
        }

        public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
        {
            _dataSettings.dataTag = dataTag;
            _dataSettings.persistenceType = persistenceType;
        }

        #endregion
    }
}
