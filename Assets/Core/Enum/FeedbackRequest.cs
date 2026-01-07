using UnityEngine;

namespace Core.Enum
{
    public enum FeedbackScope
    {
        Global,
        Player,
        Entity
    }

    [System.Serializable]
    public struct FeedbackRequest
    {
        public FeedbackScope Scope;
        public FeedbackType Type;
        public Vector3 WorldPosition;
        public float Intensity;     // 0–1 normalized
    }

    public enum FeedbackType
    {
        // ─────────────────────────
        // Player / Entity Actions
        // ─────────────────────────
        Footstep,
        Jump,
        Land,
        Dash,
        SprintStart,
        SprintStop,

        // ─────────────────────────
        // Combat
        // ─────────────────────────
        LightHit,
        HeavyHit,
        CriticalHit,
        Explosion,
        ProjectileImpact,

        // ─────────────────────────
        // World / Interaction
        // ─────────────────────────
        InteractionStart,
        InteractionTick,
        InteractionComplete,
        ExtractionStart,
        ExtractionPulse,
        ExtractionComplete,

        // ─────────────────────────
        // Damage / Status
        // ─────────────────────────
        DamageTaken,
        Heal,
        ShieldBreak,
        Stun,

        // ─────────────────────────
        // Camera-specific
        // ─────────────────────────
        CameraShake_Small,
        CameraShake_Medium,
        CameraShake_Large,
        CameraImpulse,

        // ─────────────────────────
        // UI / Meta
        // ─────────────────────────
        Warning,
        Error,
        Confirmation
    }
}