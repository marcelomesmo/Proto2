using System.Collections.Generic;
using Core.Gameplay.Combat.Attack;
using Core.Interfaces;
using Core.Services;
using UnityEngine;

namespace Core.Gameplay.Combat.ChainAttack
{
    public static class ChainAttackResolver
    {
        public static void ResolveChain(
            AttackInstance attack,
            CombatPayload initialPayload,
            ICombatReceiver firstTarget)
        {
            if (!attack.Data.chainData)
                return;

            var chain = attack.Data.chainData;

            var hitTargets = new HashSet<ICombatReceiver>();
            hitTargets.Add(firstTarget);

            var currentPayload = initialPayload;
            var currentTarget = firstTarget;

            var effectiveMaxBounces = attack.GetResolvedChainBounces();

            for (int i = 0; i < effectiveMaxBounces; i++)
            {
                var nextTarget = FindNextTarget(
                    currentTarget,
                    hitTargets,     // exclude previous targets
                    chain.chainRange,
                    initialPayload);

                if (nextTarget == null)
                    break;

                hitTargets.Add(nextTarget);

                currentPayload = ModifyPayloadForBounce(
                    attack,
                    currentPayload,
                    i + 1);

                SpawnChainVfx(
                    attack.Data,
                    ((Component)currentTarget).transform,
                    ((Component)nextTarget).transform,
                    i
                );
                
                CombatExecutionPipeline.Execute(
                    nextTarget,
                    currentPayload
                );
                currentTarget = nextTarget;
            }
        }
        
        private static readonly List<Collider2D> _chainHits = new();
        
        private static ICombatReceiver FindNextTarget(
            ICombatReceiver from,
            HashSet<ICombatReceiver> excluded,
            float range,
            CombatPayload payload)
        {
            var origin = ((Component)from).transform.position;
            
            _chainHits.Clear();
            
            Physics2D.OverlapCircle(
                origin,
                range,
                payload.source.targetFilter.ToContactFilter(),
                _chainHits
            );

            foreach (var hit in _chainHits)
            {
                if (!hit)
                    continue;

                if (!payload.source.targetFilter.CanHit(hit))
                    continue;
                
                if (!hit.TryGetComponent<ICombatReceiver>(out var receiver))
                    continue;

                if (excluded.Contains(receiver))      // this needs to change later if we want it to bounce to same target.
                    continue;

                if (!receiver.CanReceiveCombat(payload))
                    continue;

                return receiver;
            }

            return null;
        }
        
        private static CombatPayload ModifyPayloadForBounce(
            AttackInstance attack,
            CombatPayload previous,
            int bounceIndex)
        {
            float multiplier = Mathf.Pow(
                attack.GetResolvedChainAmountMultiplier(),     // multiply damage
                bounceIndex);
            
            return CombatPayloadFactory.CreateChain(
                previous,
                multiplier,
                attack.Data.chainData.applyEffectsOnEveryBounce
            );
        }
        
        private static void SpawnChainVfx(
            AttackData attackData,
            Transform from,
            Transform to,
            int bounceIndex)
        {
            var chainData = attackData.chainData;
            if (!chainData || !chainData.chainVfxPrefab)
                return;

            CoroutineRunner.Instance.StartCoroutine(
                SpawnAfterDelay(
                    chainData,
                    from,
                    to,
                    chainData.hopVfxDelay * bounceIndex
                )
            );
        }
        
        private static System.Collections.IEnumerator SpawnAfterDelay(
            ChainAttackData data,
            Transform from,
            Transform to,
            float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            if (!from || !to)
                yield break;

            var vfx = Object.Instantiate(
                data.chainVfxPrefab,
                from.position,
                Quaternion.identity
            );

            if (vfx.TryGetComponent<IChainVfx>(out var chainVfx))
            {
                chainVfx.Initialize(from, to);
            }

            if (data.vfxLifetime > 0f)
            {
                Object.Destroy(vfx, data.vfxLifetime);
            }
        }
    }
}
