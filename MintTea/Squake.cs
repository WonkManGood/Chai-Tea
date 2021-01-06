using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MintTea {
    class Squake {

        public static void Shmove(CharacterMotor characterMotor, float maxAirAccel, float airAccel) {
            float wishSpeed = characterMotor.moveDirection.sqrMagnitude > 1e-4 ? quake_getMoveSpeed(characterMotor) : 0;

            if (wishSpeed > maxAirAccel)
                wishSpeed = maxAirAccel;

            Vector3 velocity = characterMotor.velocity;
            velocity.y = 0;

            float currentSpeed = Vector3.Dot(velocity, characterMotor.moveDirection.normalized);
            float addSpeed = wishSpeed - currentSpeed;

            if (!(addSpeed > 0))
                return;

            float accelSpeed = airAccel * wishSpeed;
            if (accelSpeed > addSpeed)
                accelSpeed = addSpeed;

            characterMotor.velocity += characterMotor.moveDirection.normalized * accelSpeed;
        }

        private static float quake_getMoveSpeed(CharacterMotor characterMotor) {
            float baseSpeed = characterMotor.walkSpeed;
            return baseSpeed;
        }
    }
}
