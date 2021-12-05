using System;
using UnityEngine;

namespace GameJamTeamOne
{
    [Serializable]
    public class JetpackSettings
    {
        #region Fields

        [Header("Jetpack")]
        public JetpackType JetpackType;
        public float JetpackMaxFlightTime = 5f;
        public float JetpackHoverDownSpeed = 10f;
        public bool HasFlightCeiling = false;
        [SerializeField, Min(0)] private float _flightCeilingHeight;
        [Header("Upward thrust settings")]
        public float JetpackVerticalAcceleration = 5f;
        public float JetpackMaxVerticalSpeed = 10f;
        [SerializeField, Range(0f, 2f)] private float _jetpackHorizAccelProportion;
        [SerializeField, Range(0f, 1f)] private float _jetpackHorizDecelProportion;
        [Header("Freeflight settings")]
        [SerializeField] public float FreeflightJetpackSpeed = 10f;
        [SerializeField] public float FreeflightJetpackAcceleration = 100f;
        [SerializeField] public float FreeflightJetpackDeceleration = 100f;
        [SerializeField] public bool CanAscend = true;

        #endregion


        #region Properties

        public float FlightCeilingHeight
        {
            get => _flightCeilingHeight;
            set => _flightCeilingHeight = Mathf.Abs(value);
        }

        public float JetpackHorizAccelProportion
        {
            get => _jetpackHorizAccelProportion;
            set => _jetpackHorizAccelProportion = Mathf.Clamp(value, 0, 2);
        }

        public float JetpackHorizDecelProportion
        {
            get => _jetpackHorizDecelProportion;
            set => _jetpackHorizDecelProportion = Mathf.Clamp(value, 0, 1);
        }

        #endregion

        #region Methods

        public void SetSettingsFromSource(JetpackSettings settings)
        {
            JetpackType = settings.JetpackType;
        }

        #endregion
    }
}