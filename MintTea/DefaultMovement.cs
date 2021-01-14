using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MintTea {
    class DefaultMovement {
        public static void AirMovement(CharacterMotor self, float deltaTime) {
            float acceleration = self.acceleration * (self.disableAirControlUntilCollision ? 0f : self.airControl);
            Vector3 direction = self.moveDirection;
            if (!self.isFlying) {
                direction.y = 0f;
            }
            if (Reflection.Access<CharacterBody>(self, "body")?.isSprinting ?? false) {
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
        public static void GroundMovement(CharacterMotor self, float deltaTime) {
            float acceleration = self.acceleration;
            Vector3 direction = self.moveDirection;
            if (!self.isFlying) {
                direction.y = 0f;
            }
            if (Reflection.Access<CharacterBody>(self, "body")?.isSprinting ?? false) {
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
    }
}
