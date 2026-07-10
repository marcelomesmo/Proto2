using Core.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    public class EntityLootMagnetSubsystem : BaseSubsystem
    {
        [Header("Settings")]
        [SerializeField] float magnetRadius = 3f;               // TODO: Move this to Stats when expanding to allow for upgrades.
        [SerializeField] float magnetSpeedMultiplier = 1f;      // TODO: Move this to Stats when expanding to allow for upgrades.
        [SerializeField] LayerMask lootMask;
        
        private readonly Collider2D[] _lootBuffer = new Collider2D[16];

        protected override void OnFixedUpdate()
        {
            var count = Physics2D.OverlapCircleNonAlloc(
                transform.position,
                magnetRadius,
                _lootBuffer,
                lootMask
            );

            for (int i = 0; i < count; i++)
            {
                if (_lootBuffer[i].TryGetComponent(out IMagnetizableLoot loot))
                    loot.TryBeginMagnet(transform, magnetSpeedMultiplier);
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, magnetRadius);

            // Optional inner disc for clarity
            //Handles.color = new Color(0.2f, 0.8f, 1f, 0.08f);
            //Handles.DrawSolidDisc(transform.position, Vector3.forward, magnetRadius);
            
            // Label slightly above the circle
            Vector3 labelPos = transform.position + Vector3.up * (magnetRadius + 0.25f);

            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.cyan }
            };
            
            Handles.Label(labelPos, "Loot Area", style);
        }
#endif
    }
}
