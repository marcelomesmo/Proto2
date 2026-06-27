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
    [RequireComponent(typeof(EntityTargetingSubsystem))]
    [RequireComponent(typeof(EntityPresentationSubsystem))]
    public class PlayerCharacterBrainSubsystem : EntityBrainSubsystem
    {
        private readonly struct AppliedPassiveEffect
        {
            public readonly EntityController Target;
            public readonly StatusEffectData Effect;

            public AppliedPassiveEffect(
                EntityController target,
                StatusEffectData effect)
            {
                Target = target;
                Effect = effect;
            }
        }
        private readonly List<AppliedPassiveEffect> _appliedPassiveEffects = new();
        
        private EntityAttackSubsystem _attackSubsystem;
        private EntityAttackLoadout _attackLoadout;
        private EntityTargetingSubsystem _targeting;
        private EntityPresentationSubsystem _presentationSubsystem;
        
        private CharacterStats _stats;
        
        private float _nextActionTime;
        
        protected override void OnInitialize()
        {
            _attackLoadout = GetComponent<EntityAttackLoadout>();
            _attackSubsystem = GetComponent<EntityAttackSubsystem>();
            _targeting = GetComponent<EntityTargetingSubsystem>();
            _presentationSubsystem = GetComponent<EntityPresentationSubsystem>();
            
            if (Controller.Stats is CharacterStats characterStats)
                _stats = characterStats;
            else
                Debug.LogError("[PlayerCharacterBrainSubsystem] requires CharacterStats!");
            
            // Sprite is drawn facing right — correct for the (P -------- E) layout.
            _presentationSubsystem.SetFacing(FacingDirection.Right, force: true);
            
            _attackLoadout.OnLoadoutChanged += OnLoadoutChanged;
        }
        
        protected override void OnDeinitialize()
        {
            controlEnabled = false;
            StopAllCoroutines();
            
            RemoveAppliedPassiveEffects();
            ClearTarget();
            
            _attackLoadout.OnLoadoutChanged -= OnLoadoutChanged;
            _attackLoadout = null;
        }
        
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == _stats.spawnFinishedTag)
            {
                controlEnabled = true;
                _nextActionTime = Time.time + _stats.globalCooldown;
            }

            if (tag == _stats.deadTag || tag == _stats.matchEndedTag)
            {
                controlEnabled = false;
                StopAllCoroutines();
                // if we ever do player character move, also add here: _movement.Stop();
            }
        }
        
        protected override void OnUpdate()
        {
            if (!controlEnabled)
                return;
            
            // ValidateCurrentTarget handles both the no-target and dead-target
            // cases and re-acquires immediately, so no separate HasTarget check needed.
            ValidateCurrentTarget();
        }

        protected override void OnFixedUpdate()
        {
            if (!controlEnabled || Controller.IsStunned) 
                return; // <--- THIS blocks attack/move before spawn finished!
            
            // A sequence is mid-execution — don't start anything new.
            if (_attackSubsystem.IsAttackInProgress)
                return;
            
            if (!HasTarget)
            {
                TryAcquireMovementTarget();
                return;
            }

            if (Time.time < _nextActionTime)
                return;

            TryExecuteAnyReadyAttack();
        }

        private bool TryExecuteAnyReadyAttack()
        {
            // Try every attack in loadout order; first valid one fires.
            foreach (var attack in _attackLoadout.Attacks)
            {
                // Shouldn't recast, is manually cast on Initialize and during Loadout change.
                if (attack.isPassive)
                    continue;
                
                // This specific attack's cooldown hasn't elapsed yet.
                if (!_attackSubsystem.CanExecute(attack))
                    continue;
                
                EntityController target = _targeting.ResolveAttackTarget(attack);

                if (!target)
                    continue;
                
                // Self-targeted abilities have no positional requirement.
                if (attack.targetType != CombatTargetType.Self)
                {
                    float distance = Vector2.Distance(
                        transform.position,
                        target.transform.position);
 
                    if (distance > attack.range)
                        continue;
                }
                
                FaceTarget(target, attack.targetType);
                
                Vector2 attackDir = attack.targetType == CombatTargetType.Self
                    ? (Vector2)transform.right   // keep current facing for self-casts
                    : (target.transform.position - transform.position).normalized;

                bool executed = _attackSubsystem.TryExecute(
                    attack,
                    new AttackContext
                    {
                        Target = target,
                        Direction = attackDir
                    });
                
                if(executed)
                {
                    _nextActionTime = Time.time + _stats.globalCooldown;
                    return true;
                }
            }

            return false;
        }
        
        //
        //  Passive skills
        // 
        #region Passive skill helpers
        
        private void OnLoadoutChanged()
        {
            // Avoid reapplying the same passives.
            RemoveAppliedPassiveEffects();
            
            // On evolution, re-cast passives for the new loadout.
            // Active StatModifierEffects from the old passive will OnRemove naturally
            // when their StatusEffect entry in EntityStatusEffectSubsystem expires or
            // is cleared — that subsystem should clear on loadout change too.
            CastPassives();
        }

        private void CastPassives()
        {
            if (_attackLoadout == null || _attackSubsystem == null)
                return;

            foreach (var attack in _attackLoadout.Attacks)
            {
                if (!attack.isPassive)
                    continue;
                
                EntityController target = ResolvePassiveTarget(attack);

                if (!target)
                    continue;
                
                bool executed = _attackSubsystem.TryExecute(
                    attack,
                    new AttackContext
                    {
                        Target = Controller,
                        Direction = Vector2.right
                    });

                if (!executed)
                    continue;

                TrackAppliedPassiveEffects(attack, target);
            }
        }
        
        private EntityController ResolvePassiveTarget(AttackData attack)
        {
            if (attack.targetType == CombatTargetType.Self)
                return Controller;

            return _targeting.ResolveAttackTarget(attack);
        }

        private void TrackAppliedPassiveEffects(
            AttackData attack,
            EntityController target)
        {
            var effects =
                _attackSubsystem.GetCombinedEffects(attack);

            if (effects == null)
                return;

            foreach (var effect in effects)
            {
                if (!effect)
                    continue;

                _appliedPassiveEffects.Add(
                    new AppliedPassiveEffect(
                        target,
                        effect));
            }
        }

        private void RemoveAppliedPassiveEffects()
        {
            for (int i = _appliedPassiveEffects.Count - 1; i >= 0; i--)
            {
                var applied = _appliedPassiveEffects[i];

                if (!applied.Target || !applied.Effect)
                    continue;

                if (!applied.Target.TryGetComponent(
                        out EntityStatusEffectSubsystem statusEffects))
                {
                    continue;
                }

                statusEffects.RemoveEffect(applied.Effect);
            }

            _appliedPassiveEffects.Clear();
        }
        
        #endregion
        
        //
        //  Targeting — movement target (CurrentTarget)
        //
        //  CurrentTarget tracks the closest enemy for facing/intention.
        //
        //  Attack targets are resolved fresh each frame by the targeting
        //  subsystem and may differ (e.g. ally heal, self buff).
        //
        #region Targeting for Movement
        
        private void TryAcquireMovementTarget()
        {
            // Player characters don't use LOS for target acquisition —
            // they auto-target the closest visible enemy in a wide radius.
            EntityController best = _targeting.FindBestEnemy(
                radius: _stats.aggroRadius,
                selectionMode: TargetSelectionMode.Closest,
                requireLos: false);
 
            if (best)
                SetTarget(best);
        }
        
        private void SetTarget(EntityController target)
        {
            if (target == null || target.IsDead)
                return;
            
            CurrentAggroTarget = target;

            FaceTarget(CurrentAggroTarget, CombatTargetType.Enemies);
        }
        
        private void ClearTarget()
        {
            CurrentAggroTarget = null;
        }
        
        private void ValidateCurrentTarget()
        {
            if (CurrentAggroTarget && !CurrentAggroTarget.IsDead)
                return;
            
            ClearTarget();
            TryAcquireMovementTarget(); // no idle frame after a kill
        }
        
        private bool HasTarget => CurrentAggroTarget && !CurrentAggroTarget.IsDead;

        #endregion
        
        //
        //  Presentation helpers
        //
        #region Presentation helpers
        
        // Encapsulates the self-cast special case so no call site
        // has to reason about it. Self-casts preserve the current facing.
        private void FaceTarget(EntityController target, CombatTargetType targetType)
        {
            if (!target)
                return;
            
            if (targetType == CombatTargetType.Self)
                return; // preserve current facing

            _presentationSubsystem.FaceDirection(
                (target.transform.position - transform.position).normalized
            );
        }
        
        #endregion
    }
}
