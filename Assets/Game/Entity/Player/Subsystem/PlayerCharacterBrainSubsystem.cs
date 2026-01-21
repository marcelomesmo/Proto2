using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;
using Game.Entity.Player.Stats;
using UnityEngine;

namespace Game.Entity.Player.Subsystem
{
    [RequireComponent(typeof(EntityAttackSubsystem))]
    [RequireComponent(typeof(EntityAttackLoadout))]
    public class PlayerCharacterBrainSubsystem : EntityBrainSubsystem
    {
        private EntityAttackSubsystem _attackSubsystem;
        private EntityAttackLoadout _attackLoadout;
        
        private CharacterStats _stats;
        
        private float _nextActionTime;
        private float _nextScanTime;
        
        protected override void OnInitialize()
        {
            _attackLoadout = GetComponent<EntityAttackLoadout>();
            _attackSubsystem = GetComponent<EntityAttackSubsystem>();
            
            if (Controller.Stats is CharacterStats characterStats)
                _stats = characterStats;
            else
                Debug.LogError("[PlayerCharacterBrainSubsystem] requires CharacterStats!");
            
            _targetFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = _stats.targetEntityLayer,
                useTriggers = true
            };
        }
        
        protected override void OnDeinitialize()
        {
            controlEnabled = false;
            StopAllCoroutines();

            ClearTarget();
        }
        
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == _stats.spawnFinishedTag)
            {
                controlEnabled = true;
                _nextActionTime = Time.time + _stats.globalCooldown;
            }

            if (tag == _stats.deadTag)
            {
                controlEnabled = false;
                
                StopAllCoroutines();
            }
        }
        
        protected override void OnUpdate()
        {
            if (!controlEnabled)
                return;
            
            if (!HasTarget)
                TryAcquireTarget();
            
            ValidateCurrentTarget();
        }

        protected override void OnFixedUpdate()
        {
            if (!controlEnabled || Controller.IsStunned) 
                return; // <--- THIS blocks attack/move before spawn finished!
           
            if (!HasTarget)
            {
                TryAcquireTarget();
                return;
            }
            
            if (CurrentTarget.IsDead)
            {
                ClearTarget();
                TryAcquireTarget();
                return;
            }

            // --- ATTACK INTENT ---
            if (Time.time < _nextActionTime)
                return;
            
            // Try all attacks, first valid wins
            foreach (var attack in _attackLoadout.Attacks)
            {
                float distance = Vector2.Distance(
                    transform.position,
                    CurrentTarget.transform.position
                );
                
                if (!IsAttackAppropriate(attack, distance))
                    continue;
                        
                Vector2 direction = 
                    (CurrentTarget.transform.position - transform.position).normalized;
                
                if (_attackSubsystem.TryExecute(
                    attack,
                    new AttackContext
                    {
                        Target = CurrentTarget,
                        Direction = direction
                    }))
                {
                    _nextActionTime = Time.time + _stats.globalCooldown;
                    return;
                }
            }
            
            // No attack available → do nothing, wait for cooldowns
        }
        
        #region Attack Selection
        
        private IEnumerable<AttackData> GetCandidateAttacks(EntityController target)
        {
            float distance =
                Vector2.Distance(transform.position, target.transform.position);

            foreach (var attack in _attackLoadout.Attacks)
            {
                if (IsAttackAppropriate(attack, distance))
                    yield return attack;
            }
        }
        
        private bool IsAttackAppropriate(AttackData attack, float distance)
        {
            // High-level AI intent check ONLY
            // Exact range validation happens inside AttackSubsystem
            return distance <= attack.range;
        }
        
        #endregion
        
        // TODO: This could be added to a TargetingSubsystem.
        #region Targeting
        
        private readonly List<Collider2D> _overlapResults = new();
        private ContactFilter2D _targetFilter;
        
        //
        //  Player-to-Enemy checks
        //
        private void TryAcquireTarget()
        {
            _overlapResults.Clear();
            
            Physics2D.OverlapCircle(
                Controller.transform.position,
                25f,    // todo: parametize this in stats
                _targetFilter,
                _overlapResults
            );
            
            // TODO: get the closest target, not the first one.

            EntityController bestTarget = null;
            float bestDistSq = float.MaxValue;
            Vector2 origin = Controller.transform.position;
            
            foreach (var col in _overlapResults)
            {
                if (!col.TryGetComponent(out EntityController candidate))
                    continue;

                if (candidate.IsDead)
                    continue;

                if (candidate.Stats.faction != Faction.Enemy)  // TODO: This will ignore Ally layer. Should this be any opponent Layer?
                    continue;

                Vector2 toTarget = candidate.transform.position - (Vector3)origin;
                float distSq = toTarget.sqrMagnitude;
                
                // Early distance reject
                if (distSq >= bestDistSq)
                    continue;

                bestDistSq = distSq;
                bestTarget = candidate;
            }

            if (bestTarget)
                SetTarget(bestTarget);
        }
        
        private void SetTarget(EntityController target)
        {
            if (target == null || target.IsDead)
                return;
            
            CurrentTarget = target;
        }
        
        private void ClearTarget()
        {
            CurrentTarget = null;
        }
        
        private void ValidateCurrentTarget()
        {
            if (CurrentTarget && !CurrentTarget.IsDead)
                return;
            
            ClearTarget();
            TryAcquireTarget(); // no idle frame after a kill
        }
        
        private bool HasTarget => CurrentTarget && !CurrentTarget.IsDead;

        #endregion
    }
}
