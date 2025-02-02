﻿using EntityStates;
using RoR2;
using UnityEngine;
using Dancer.Modules.Components;
using UnityEngine.Networking;
namespace Dancer.SkillStates
{
    public class Pull : BaseSkillState
    {
        public Vector3 point;
        private Vector3 direction;
        private float distance;
        private float duration;
        private float speed;
        private float startSpeed;
        private float endSpeed;

        public bool hitWorld;
        private float exitHopVelocity = 15f;

        public static float minDuration = 0.2f;
        public static float maxDuration = 0.8f;
        public static float maxDistance = 80f;
        public static float minVelocity = 0.7f;
        public static float velocityMultiplier = 1.3f;

        private float maxAngle = 60f;
        private Animator animator;
        private float stopwatch;

        private DancerComponent weaponAnimator;

        public override void OnEnter()
        {
            base.OnEnter();

            if (base.characterBody && NetworkServer.active) base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            this.animator = base.GetModelAnimator();
            this.weaponAnimator = base.GetComponent<DancerComponent>();

            this.distance = (base.transform.position - this.point).magnitude;
            this.direction = (this.point - base.transform.position).normalized;
            this.duration = Mathf.Lerp(minDuration, maxDuration, this.distance / maxDistance);
            this.speed = this.distance / this.duration;
            this.startSpeed = this.speed * 2f;
            this.endSpeed = this.speed * 0.0f;

            if (base.GetComponent<KinematicCharacterController.KinematicCharacterMotor>())
            {
                base.GetComponent<KinematicCharacterController.KinematicCharacterMotor>().ForceUnground();
            }

            Vector3 direction = point - base.transform.position;
            this.weaponAnimator.RotationOverride(direction.normalized * 500f + base.transform.position);

            EffectManager.SimpleMuzzleFlash(Modules.Assets.dragonLungePullEffect, base.gameObject, "LanceBase", false);
            base.PlayAnimation("FullBody, Override", "DragonLungePull", "Slash.playbackRate", this.duration * 1f);
        }

        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            }
            this.weaponAnimator.StopRotationOverride();
            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.OnExit();
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.speed = Mathf.Lerp(this.startSpeed, this.endSpeed, base.fixedAge / this.duration);
            base.characterDirection.forward = this.direction;
            base.characterMotor.velocity = this.direction * this.speed;
            if (base.fixedAge >= duration)
            {
                base.characterMotor.velocity = Vector3.zero;
                if(base.inputBank.jump.justPressed)
                {
                    base.PlayAnimation("Body", "Jump");
                    base.SmallHop(base.characterMotor, base.characterBody.jumpPower);
                    this.outer.SetNextStateToMain();
                    return;
                }
                else if(!this.hitWorld)
                    this.outer.SetNextStateToMain();
                else if(!base.inputBank.skill3.down)
                    this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}