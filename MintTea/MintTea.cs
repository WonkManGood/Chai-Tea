using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using MintTea;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MintTea {

    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.wellme.MintTea", "Mint tea", "0.1.1")]
    public class MintTea : BaseUnityPlugin {

        private Dictionary<CharacterMotor, MintTeaInfo> info = new Dictionary<CharacterMotor, MintTeaInfo>();

        public void Awake() {
            Reflection.Logger = Logger;
            Configuration.InitConfig(Config);

            On.RoR2.CharacterMotor.PreMove += (orig, self, deltaTime) => {
                Vector3 velocity = self.velocity;
                orig(self, deltaTime);
                if (PlayerCharacterMasterController.instances[0]?.master?.GetBody()?.characterMotor == self) {
                    self.velocity = velocity;
                    PreMove(self, deltaTime);
                }
            };

            if (Configuration.AutoHop.Value) {
                On.EntityStates.GenericCharacterMain.GatherInputs += (orig, self) => {
                    orig(self);
                    if (Reflection.Access<bool>(self, "hasInputBank") && GetMotor(self) is CharacterMotor motor) {
                        GetInfo(motor).GroundedJump = Reflection.AccessProperty<InputBankTest>(self, typeof(EntityState), "inputBank").jump.down;
                    }
                };
            }

            On.EntityStates.GenericCharacterMain.ProcessJump += (orig, self) => {
                CharacterMotor motor = GetMotor(self);
                if ((motor?.isGrounded ?? false) && (GetInfo(motor)?.GroundedJump ?? false)) {
                    Reflection.Set(self, "jumpInputReceived", true);
                }
                orig(self);
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
                if (GetInfo(self).LeniencyFrame || !self.isGrounded) {
                    Quake.AirMovement(self, Configuration.MaxAirAccel.Value, Configuration.AirAccel.Value);
                } else {
                    DefaultMovement.GroundMovement(self, deltaTime);
                }
                if (self.useGravity) {
                    self.velocity.y += Physics.gravity.y * deltaTime;
                    if (self.isGrounded) {
                        self.velocity.y = Mathf.Max(self.velocity.y, 0f);
                    }
                }

                GetInfo(self).LeniencyFrame = !self.isGrounded;
            }
        }


        private MintTeaInfo GetInfo(CharacterMotor motor) {
            if (motor == null)
                return null;
            if (info.TryGetValue(motor, out MintTeaInfo val))
                return val;

            return info[motor] = new MintTeaInfo {
                Motor = motor,
                LeniencyFrame = false
            };
        }

        private static CharacterMotor GetMotor(GenericCharacterMain genericCharacterMain) {
            return Reflection.AccessProperty<CharacterMotor>(genericCharacterMain, typeof(EntityState), "characterMotor");
        }
    }
}