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
            AttackData attackData,
            DamagePayload initialPayload,
            IDamageable firstTarget)
        {
            if (!attackData.chainData)
                return;

            var chain = attackData.chainData;

            var hitTargets = new HashSet<IDamageable>();
            hitTargets.Add(firstTarget);

            var currentPayload = initialPayload;
            var currentTarget = firstTarget;

            for (int i = 0; i < chain.maxBounces; i++)
            {
                var nextTarget = FindNextTarget(
                    currentTarget,
                    hitTargets,     // exclude previous targets
                    chain.chainRange);

                if (nextTarget == null)
                    break;

                hitTargets.Add(nextTarget);

                currentPayload = ModifyPayloadForBounce(
                    attackData,
                    currentPayload,
                    i + 1);

                SpawnChainVfx(
                    attackData,
                    ((Component)currentTarget).transform,
                    ((Component)nextTarget).transform,
                    i
                );
                
                nextTarget.TakeDamage(currentPayload);
                currentTarget = nextTarget;
            }
        }
        
        private static readonly List<Collider2D> _chainHits = new();
        
        private static IDamageable FindNextTarget(
            IDamageable from,
            HashSet<IDamageable> excluded,
            float range)
        {
            var origin = ((Component)from).transform.position;
            
            _chainHits.Clear();
            
            Physics2D.OverlapCircle(
                origin,
                range,
                new ContactFilter2D { useTriggers = true },
                _chainHits
            );

            foreach (var hit in _chainHits)
            {
                if (!hit)
                    continue;
                
                if (!hit.TryGetComponent<IDamageable>(out var damageable))
                    continue;

                if (excluded.Contains(damageable))      // this needs to change later if we want it to bounce to same target.
                    continue;

                if (!damageable.CanBeDamaged())
                    continue;

                return damageable;
            }

            return null;
        }
        
        private static DamagePayload ModifyPayloadForBounce(
            AttackData attackData,
            DamagePayload previous,
            int bounceIndex)
        {
            float multiplier = Mathf.Pow(
                attackData.chainData.damageMultiplierPerBounce,     // multiply damage
                bounceIndex);
            
            return new DamagePayload(
                hitData: previous.hitData,
                baseDamage: Mathf.RoundToInt(previous.baseDamage * multiplier),
                modifiers: previous.modifiers,
                effects: attackData.chainData.applyEffectsOnEveryBounce     // we can either apply effects on every bounce or not
                    ? previous.effects
                    : null,
                hitPoint: previous.hitPoint,
                source: previous.source,
                chainDepth: previous.chainDepth + 1
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
