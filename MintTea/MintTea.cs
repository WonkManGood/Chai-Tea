using BepInEx;
using RoR2;
using System.Reflection;
using UnityEngine;

namespace MyUserName {
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.wellme.MintTea", "Mint tea", "0.0.0")]
    public class MintTea : BaseUnityPlugin {
        public void Awake() {
            On.RoR2.CharacterMotor.PreMove += (orig, self, deltaTime) => {
                PreMove(self, deltaTime);
            };
            On.EntityStates.GenericCharacterMain.ApplyJumpVelocity += (orig, characterMotor, characterBody, horizontalBonus, verticalBonus, vault) => {
                Vector3 velocity = characterMotor.velocity;
                orig(characterMotor, characterBody, horizontalBonus, verticalBonus, vault);
                Vector3 horizontalBonusVelocity = characterMotor.moveDirection * characterBody.moveSpeed * horizontalBonus;
                if (characterMotor.velocity.sqrMagnitude > horizontalBonusVelocity.sqrMagnitude) {
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
                float acceleration = self.acceleration;
                if (!self.isGrounded) {
                    acceleration *= (self.disableAirControlUntilCollision ? 0f : self.airControl);
                }
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
                if (self.useGravity) {
                    self.velocity.y += Physics.gravity.y * deltaTime;
                    if (self.isGrounded) {
                        self.velocity.y = Mathf.Max(self.velocity.y, 0f);
                    }
                }
            }
        }
        
        private T Access<T>(object o, string field) {
            FieldInfo fieldInfo = o.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            T value = (T) fieldInfo.GetValue(o);
            if (value == null) {
                Logger.LogWarning("Value was null for field " + field);
            }
            return value;
        }
    }
}