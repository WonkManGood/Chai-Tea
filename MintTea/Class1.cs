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
                Logger.LogInfo(self.moveDirection);
                PreMove(self, deltaTime);
            };
        }
        private void PreMove(CharacterMotor self, float deltaTime) {
            if (self.hasEffectiveAuthority) {
                float num = self.acceleration;
                if (!self.isGrounded) {
                    num *= (self.disableAirControlUntilCollision ? 0f : self.airControl);
                }
                Vector3 a = self.moveDirection;
                if (!self.isFlying) {
                    a.y = 0f;
                }
                if (Access<CharacterBody>(self, "body")?.isSprinting ?? false) {
                    float magnitude = a.magnitude;
                    if (magnitude < 1f && magnitude > 0f) {
                        float d = 1f / a.magnitude;
                        a *= d;
                    }
                }
                Vector3 target = a * self.walkSpeed;
                if (!self.isFlying) {
                    target.y = self.velocity.y;
                }
                self.velocity = Vector3.MoveTowards(self.velocity, target, num * deltaTime);
                if (self.useGravity) {
                    ref float ptr = ref self.velocity.y;
                    ptr += Physics.gravity.y * deltaTime;
                    if (self.isGrounded) {
                        ptr = Mathf.Max(ptr, 0f);
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