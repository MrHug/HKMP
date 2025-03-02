﻿using Hkmp.Util;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Hkmp.Animation.Effects {
    public class DashSlash : DamageAnimationEffect {
        public override void Play(GameObject playerObject, bool[] effectInfo) {
            // Obtain the Nail Arts FSM from the Hero Controller
            var nailArts = HeroController.instance.gameObject.LocateMyFSM("Nail Arts");

            // Get an audio source relative to the player
            var audioObject = AudioUtil.GetAudioSourceObject(playerObject);
            var audioSource = audioObject.GetComponent<AudioSource>();

            // Get the audio clip of the Great Slash
            var dashSlashClip = (AudioClip) nailArts.GetAction<AudioPlay>("Dash Slash", 1).oneShotClip.Value;
            audioSource.PlayOneShot(dashSlashClip);

            Object.Destroy(audioObject, dashSlashClip.length);

            // Get the attacks gameObject from the player object
            var localPlayerAttacks = HeroController.instance.gameObject.FindGameObjectInChildren("Attacks");

            // Get the prefab for the Dash Slash and instantiate it relative to the remote player object
            var dashSlashObject = localPlayerAttacks.FindGameObjectInChildren("Dash Slash");
            var dashSlash = Object.Instantiate(
                dashSlashObject,
                playerObject.transform.parent
            );

            // Since we anchor the dash slash on the player container instead of the player object
            // (to prevent it from flipping when the knight turns around) we need to adjust the scale based
            // on which direction the knight is facing
            var dashSlashTransform = dashSlash.transform;
            var dashSlashScale = dashSlashTransform.localScale;
            dashSlashTransform.localScale = new Vector3(
                dashSlashScale.x * playerObject.transform.localScale.x,
                dashSlashScale.y,
                dashSlashScale.z
            );

            dashSlash.layer = 22;

            ChangeAttackTypeOfFsm(dashSlash);

            dashSlash.SetActive(true);

            // Remove audio source component that exists on the dash slash object
            Object.Destroy(dashSlash.GetComponent<AudioSource>());

            // Set the newly instantiate collider to state Init, to reset it
            // in case the local player was already performing it
            dashSlash.LocateMyFSM("Control Collider").SetState("Init");

            var damage = GameSettings.DashSlashDamage;
            if (GameSettings.IsPvpEnabled && ShouldDoDamage && damage != 0) {
                // Somehow adding a DamageHero component simply to the dash slash object doesn't work,
                // so we create a separate object for it
                var dashSlashCollider = Object.Instantiate(
                    new GameObject(
                        "DashSlashCollider",
                        typeof(PolygonCollider2D),
                        typeof(DamageHero)
                    ),
                    dashSlash.transform
                );
                dashSlashCollider.SetActive(true);
                dashSlashCollider.layer = 22;

                // Copy over the polygon collider points
                dashSlashCollider.GetComponent<PolygonCollider2D>().points =
                    dashSlash.GetComponent<PolygonCollider2D>().points;

                dashSlashCollider.GetComponent<DamageHero>().damageDealt = damage;
            }

            // Get the animator, figure out the duration of the animation and destroy the object accordingly afterwards
            var dashSlashAnimator = dashSlash.GetComponent<tk2dSpriteAnimator>();
            var dashSlashAnimationDuration = dashSlashAnimator.DefaultClip.frames.Length / dashSlashAnimator.ClipFps;

            Object.Destroy(dashSlash, dashSlashAnimationDuration);
        }

        public override bool[] GetEffectInfo() {
            return null;
        }
    }
}