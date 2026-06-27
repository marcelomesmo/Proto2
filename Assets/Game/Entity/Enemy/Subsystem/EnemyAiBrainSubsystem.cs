using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;
using Game.Entity.Enemy.Stats;
using UnityEngine;

namespace Game.Entity.Enemy.Subsystem
{
    [RequireComponent(typeof(EntityMovement))]
    [RequireComponent(typeof(EntityAttackSubsystem))]
    [RequireComponent(typeof(EntityAttackLoadout))]
    [RequireComponent(typeof(EntityTargetingSubsystem))]
    [RequireComponent(typeof(EntityPresentationSubsystem))]
    public sealed class EnemyAiBrainSubsystem : EntityBrainSubsystem
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
        
        private EntityHealth _health;
        private EntityMovement _movement;
        private EntityAttackSubsystem _attackSubsystem;
        private EntityAttackLoadout _attackLoadout;
        private EntityTargetingSubsystem _targeting;
        private EntityPresentationSubsystem _presentationSubsystem;
        
        private EnemyStats _stats;
        
        private float _nextActionTime;
        private float _nextScanTime;
        
        // Offset applied to the movement destination so enemies don't all stack
        // on the exact same position. Sign is set once in SetTarget based on
        // which side of the target this enemy spawned on.
        private float _positionOffset;
        
        protected override void OnInitialize()
        {
            _attackLoadout = GetComponent<EntityAttackLoadout>();
            _movement = GetComponent<EntityMovement>();
            _attackSubsystem = GetComponent<EntityAttackSubsystem>();
            _targeting = GetComponent<EntityTargetingSubsystem>();
            _presentationSubsystem = GetComponent<EntityPresentationSubsystem>();
            _health = Controller.GetComponent<EntityHealth>();
            
            if (Controller.Stats is EnemyStats enemyStats)
                _stats = enemyStats;
            else
                Debug.LogError("[EnemyAI] requires EnemyStats!");
            
            // Sprite is drawn facing left — correct for the (P -------- E) layout.
            _presentationSubsystem.SetFacing(FacingDirection.Left, force: true);

            _health.OnDamageTaken += OnDamageTaken;
            
            _attackLoadout.OnLoadoutChanged += OnLoadoutChanged;
        }
        
        protected override void OnDeinitialize()
        {
            controlEnabled = false;
            StopAllCoroutines();

            RemoveAppliedPassiveEffects();
            ClearTarget();
            
            _health.OnDamageTaken -= OnDamageTaken;
            _health = null;
            
            _attackLoadout.OnLoadoutChanged -= OnLoadoutChanged;
            _attackLoadout = null;
        }

        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == _stats.spawnFinishedTag)
            {
                controlEnabled = true;
                _nextActionTime = Time.time + _stats.thinkingTime;
                //_nextScanTime = Time.time; // allow immediate scan
            }

            if (tag == _stats.deadTag || tag == _stats.matchEndedTag)
            {
                controlEnabled = false;
                StopAllCoroutines();
                _movement.Stop();
            }
        }

        protected override void OnUpdate()
        {
            if (!controlEnabled)
                return;
            
            if (!HasAggroTarget)
            {
                if (Time.time >= _nextScanTime)
                {
                    _nextScanTime = Time.time + _stats.thinkingTime;
                    TryAcquireAggroTarget();
                }
                return;
            }
            
            ValidateCurrentTarget();
        }
        
        protected override void OnFixedUpdate()
        {
            if (!controlEnabled || Controller.IsStunned) 
            {
                _movement.Stop();
                return; // <--- THIS blocks attack/move before spawn finished!
            }
            
            // A sequence is mid-execution — don't start anything new and don't move.
            if (_attackSubsystem.IsAttackInProgress)
            {
                _movement.Stop();
                return;
            }
            
            // Mental model:
            // a. Can I attack right now from where I am?
            // b. If not, is there a ready attack I could reach by moving?
            // c. Otherwise, all ready attacks are on cooldown — hold position.
            
            // --- ATTACK INTENT ---
            // 1. Attack immediately if the global cooldown has elapsed.
            if (Time.time >= _nextActionTime)
            {
                if (TryExecuteAnyReadyAttack())
                    return;
            }
            
            // --- MOVEMENT INTENT ---
            // 2. Move toward target if a ready attack exists but is out of range.
            if (ShouldMoveTowardAggroTarget())
            {
                FaceTarget(CurrentAggroTarget, CombatTargetType.Enemies);
                _movement.MoveTo(CurrentAggroTarget.transform.position);
                return;
            }
            
            // 3. In range but all attacks on cooldown — hold position.
            _movement.Stop();
        }
        
        //
        //  Damage reaction
        // 
        private void OnDamageTaken(CombatPayload payload)
        {
            if (payload.source.sourceEntity != null)
                SetAggroTarget(payload.source.sourceEntity);
        }
        
        //
        //  Attack selection
        // 
        #region Attack Selection
        
        private bool TryExecuteAnyReadyAttack()
        {
            foreach (var attack in _attackLoadout.Attacks)
            {
                // Shouldn't recast, is manually cast on Initialize and during Loadout change.
                if (attack.isPassive)
                    continue;
                
                // This specific attack's cooldown hasn't elapsed yet.
                if (!_attackSubsystem.CanExecute(attack))
                    continue;

                EntityController target = _targeting.ResolveAttackTarget(attack, requireLos: false);

                if (!target)
                    continue;
                
                // Self-targeted abilities have no positional requirement.
                if (attack.targetType != CombatTargetType.Self)
                {
                    float distance = Vector2.Distance(
                        transform.position,
                        target.transform.position);

                    // Attack isn't in range, skip.
                    if (distance > attack.range)
                        continue;
                }
                
                FaceTarget(target, attack.targetType);
                
                Vector2 attackDir = attack.targetType == CombatTargetType.Self
                    ? (Vector2)transform.right
                    : (target.transform.position - transform.position).normalized;

                bool executed = _attackSubsystem.TryExecute(
                    attack,
                    new AttackContext
                    {
                        Target = target,
                        Direction = attackDir
                    });

                if (executed)
                {
                    _movement.Stop();
                    _nextActionTime = Time.time + _stats.globalCooldown;
                    return true;
                }
            }

            return false;
        }
        
        // Returns true if at least one ready, non-self attack exists whose
        // target is alive but currently out of range — meaning we should chase.
        private bool ShouldMoveTowardAggroTarget()
        {
            if (!HasAggroTarget)
                return false;
            
            foreach (var attack in _attackLoadout.Attacks)
            {
                if (!_attackSubsystem.CanExecute(attack))
                    continue;
                
                if (attack.targetType == CombatTargetType.Self)
                    continue;
                
                // TODO: This will only work for Damage abilities, for Healing abilities the entity will not
                // move towards allies to heal them. i.e. Enemy only move towards the Player to attack but
                // not towards allies to heal, we should change this is a HealerBrain probably.
                
                // Enemy attacks are evaluated against aggro target
                if (attack.targetType == CombatTargetType.Enemies)
                {
                    float distance = Vector2.Distance(
                        transform.position,
                        CurrentAggroTarget.transform.position);
                
                    if (distance > attack.range)
                        return true;
                }
            }

            return false;
        }
        
        #endregion
        
        
        //
        //  Passive skills
        // 
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
        
        //
        //  Targeting — movement target (CurrentTarget)
        //
        //  CurrentTarget drives where this enemy walks.
        //  It is always an enemy-faction entity (i.e. a player character).
        //
        // Attacks resolve their own targets independently using the filter.
        // Movement follows CurrentAggroTarget.
        // Specialized brains may override movement intent.
        //
        #region Targeting for Movement
        
        private void TryAcquireAggroTarget()
        {
            EntityController best = _targeting.FindBestEnemy(
                _stats.aggroRadius,
                TargetSelectionMode.Closest,
                requireLos: true);
            
            if (best)
                SetAggroTarget(best);
        }
        
        private void SetAggroTarget(EntityController target)
        {
            if (target == null || target.IsDead)
                return;
            
            CurrentAggroTarget = target;
            
            // Space enemies out so they don't all converge on the exact same pixel.
            float direction = transform.position.x > CurrentAggroTarget.transform.position.x 
                ? 1f 
                : -1f;
            
            _positionOffset = Random.Range(0.5f, 1f) * direction; // Will space them out based on which side they spawn
        }
        
        private void ClearTarget()
        {
            CurrentAggroTarget = null;
            _positionOffset = 0f;   // This was added now, check if it doesn't break anything. If you see this in the future and movement is fine, remove this comment (leave the code).
        }
        
        private void ValidateCurrentTarget()
        {
            if (CurrentAggroTarget == null || !CurrentAggroTarget.gameObject || CurrentAggroTarget.IsDead)
            {
                ClearTarget();
                return;
            }

            float distSq = (CurrentAggroTarget.transform.position - Controller.transform.position).sqrMagnitude;
            float leashRadius = _stats.aggroRadius + _stats.aggroTolerance; // Lose agroo radius
            
            if (distSq > leashRadius * leashRadius)
                ClearTarget(); // Clearing target because out of range.
        }
        
        private bool HasAggroTarget =>
            CurrentAggroTarget != null &&
            CurrentAggroTarget.gameObject != null &&
            !CurrentAggroTarget.IsDead;
        
        //
        //  Brain gate — should this AI do anything this frame?
        //
        
        // True when at least one meaningful action is possible:
        //   • We have a movement/attack target (enemy), OR
        //   • We have a self-cast ability ready (no target needed).
        // Ally-targeted abilities (heals/buffs) are NOT checked here because
        // the targeting subsystem resolves them fresh inside TryExecuteAnyReadyAttack;
        // we rely on that path rather than scanning for allies every frame.
        private bool HasAnyPossibleActionTarget() =>
            HasAggroTarget || HasSelfActionAvailable();
        
        private bool HasSelfActionAvailable()
        {
            foreach(var attack in _attackLoadout.Attacks)
            {
                if (!_attackSubsystem.CanExecute(attack))
                    continue;

                if (attack.targetType == CombatTargetType.Self)
                    return true;
            }

            return false;
        }
        
        // TODO: We keep this for now because movement is enemy-centric,
        // but later we might want a healer brain that override this. 
        private Vector2 TargetPosition =>
            HasAggroTarget 
                ? new Vector2(
                    CurrentAggroTarget.transform.position.x + _positionOffset, 
                    CurrentAggroTarget.transform.position.y) 
                : Vector2.zero;
        
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
        
        //
        //  Editor
        //
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_stats)
                return;
            
            // Aggro acquire radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _stats.aggroRadius);
            
            // Leash radius (aggro + tolerance) — where the target is dropped
            Gizmos.color = new Color(1f, 0.5f, 0f); // orange
            Gizmos.DrawWireSphere(transform.position, _stats.aggroRadius + _stats.aggroTolerance);
        }
#endif
    }
}