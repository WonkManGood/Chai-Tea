using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using MintTea;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MintTea {
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.wellme.MintTea", "Mint tea", "0.0.0")]
    public class MintTea : BaseUnityPlugin {

        private Dictionary<CharacterMotor, bool> leniencyFrames = new Dictionary<CharacterMotor, bool>();

        public void Awake() {
            Configuration.InitConfig(Config);

            On.RoR2.CharacterMotor.PreMove += (orig, self, deltaTime) => {
                PreMove(self, deltaTime);
            };
            On.EntityStates.GenericCharacterMain.GatherInputs += (orig, self) => {
                orig(self);
                if(Access<bool>(self, "hasInputBank")) {
                    Set(self, "jumpInputReceived", AccessProperty<InputBankTest>(self, typeof(EntityState), "inputBank").jump.down);
                }
            };
            On.EntityStates.GenericCharacterMain.ApplyJumpVelocity += (orig, characterMotor, characterBody, horizontalBonus, verticalBonus, vault) => {
                Vector3 velocity = characterMotor.velocity;
                orig(characterMotor, characterBody, horizontalBonus, verticalBonus, vault);
                Vector3 horizontalBonusVelocity = characterMotor.moveDirection * characterBody.moveSpeed * horizontalBonus;
                if (characterMotor.velocity.sqrMagnitude < horizontalBonusVelocity.sqrMagnitude) {
                    horizontalBonusVelocity.y = characterMotor.velocity.y;
                    characterMotor.velocity = horizontalBonusVelocity;
                } else {
                    velocity.y = characterMotor.velocity.y;
                    characterMotor.velocity = velocity;
                }
            };
        }
        private void PreMove(CharacterMotor self, float deltaTime) {
            if (self.hasEffectiveAuthority) {
                if (leniencyFrames.TryGetValue(self, out bool val) && val || !self.isGrounded) {
                    Squake.Shmove(self, Configuration.MaxAirAccel.Value, Configuration.AirAccel.Value);
                } else {
                    float acceleration = self.acceleration;
                    Vector3 direction = self.moveDirection;
                    if (!self.isFlying) {
                        direction.y = 0f;
                    }
                    if (Access<CharacterBody>(self, "body")?.isSprinting ?? false) {
                        if (0f < direction.magnitude && direction.magnitude < 1f) {
                            direction /= direction.magnitude;
                        }
                    }
                    Vector3 target = direction * self.walkSpeed;
                    if (!self.isFlying) {
                        target.y = self.velocity.y;
                    }
                    self.velocity = Vector3.MoveTowards(self.velocity, target, acceleration * deltaTime);
                }
                if (self.useGravity) {
                    self.velocity.y += Physics.gravity.y * deltaTime;
                    if (self.isGrounded) {
                        self.velocity.y = Mathf.Max(self.velocity.y, 0f);
                    }
                }
                leniencyFrames[self] = !self.isGrounded;
            }
        }
        
        private T Access<T>(object o, string field) {
            FieldInfo fieldInfo = o.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (fieldInfo == null)
                Logger.LogError("Unrecognized field " + field + " in class " + o.GetType().FullName);
            T value = (T) fieldInfo.GetValue(o);
            if (value == null) {
                Logger.LogWarning("Value was null for field " + field + " in class " + o.GetType().FullName);
            }
            return value;
        }
        private T AccessProperty<T>(object o, Type type, string field) {
            PropertyInfo propertyInfo = type.GetProperty(field, BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);
            if (propertyInfo == null)
                Logger.LogError("Unrecognized property " + field + " in class " + type.FullName);
            T value = (T) propertyInfo.GetValue(o);
            if (value == null) {
                Logger.LogWarning("Value was null for property " + field + " in class " + type.FullName);
            }
            return value;
        }

        private void Set(object o, string field, object value) {
            FieldInfo fieldInfo = o.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            fieldInfo.SetValue(o, value);
        }
    }
}