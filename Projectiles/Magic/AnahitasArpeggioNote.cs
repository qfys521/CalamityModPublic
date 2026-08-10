using CalamityMod.Items.Weapons.Magic;
using CalamityMod.NPCs;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    [PierceResistException(onlyForSingleHitbox: true)]
    public class AnahitasArpeggioNote : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";

        public ref float Timer => ref Projectile.ai[0];
        public ref float AIState => ref Projectile.ai[1];
        public ref float NoteSequence => ref Projectile.ai[2];
        public int LingeringTime = 300;
        public int FadeOutTime = 20;
        public bool HasSetFadeOutVelocity = false;
        public Vector2 ReleaseCenterPoint = Vector2.Zero;
        public float _randomReleaseRotationOffset;
        public SlotId StupidEasterEggSlot;

        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 16;
        }

        public override void AI()
        {            
            Timer++;

            // Slight size oscillation
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.scale += 0.02f;
                if (Projectile.scale >= 1.15f)
                    Projectile.localAI[0] = 1f;
            }
            else if (Projectile.localAI[0] == 1f)
            {
                Projectile.scale -= 0.02f;
                if (Projectile.scale <= 0.85f)
                    Projectile.localAI[0] = 0f;
            }

            // Sound business
            if (Timer == 1f)
            {
                if (Main.zenithWorld)
                {
                    if (NoteSequence == 0f)
                        StupidEasterEggSlot = SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/GFB/SevenTrebleClefSouls"), Owner.Center);
                }
                else
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/HarpLV" + Math.Clamp((int)NoteSequence + 1, 1, 6)) with { Volume = 0.8f }, Owner.Center);
            }
            // They did not obey me
            // Big shoutouts to NotRyo for making this Finale remix out of harp notes
            if (Main.zenithWorld && NoteSequence == 0f && Timer % 2428f == 0f && AIState == 0f)
                StupidEasterEggSlot = SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/GFB/SevenTrebleClefSouls"), Owner.Center);
            if (SoundEngine.TryGetActiveSound(StupidEasterEggSlot, out var TrebleSoul) && TrebleSoul.IsPlaying)
                TrebleSoul.Position = Owner.Center;

            if (Main.zenithWorld)
                Lighting.AddLight(Projectile.Center, 1.25f, 1.25f, 1.25f);
            else
                Lighting.AddLight(Projectile.Center, 0f, 0f, 1.25f);

            if (AIState == 0f) // Orbiting the player
            {
                // Keeps the projectile alive for as long as the weapon is being channeled
                Projectile.timeLeft = LingeringTime + FadeOutTime;

                // Makes the music notes appear to orbit around the player
                float baseRotationSpeed = Main.zenithWorld ? 0.857142f : 1f;
                float rotationSpeed = baseRotationSpeed * (60f / Owner.HeldItem.useTime);
                Projectile.Center = Owner.Center + new Vector2(80, 0).RotatedBy(MathHelper.ToRadians(Timer * rotationSpeed));

                // If the player stops using the weapon or does not have enough mana, switch to fade away mode
                if (Owner.releaseUseItem || !Owner.CheckMana(Owner.HeldItem.mana))
                {
                    Owner.Calamity().arpeggioCooldown = 45;
                    AIState = 1f;
                    Projectile.netUpdate = true;
                }
            }
            else if (AIState == 1f) // Brief fade away
            {
                Vector2 playerDirection = Projectile.Center - Owner.Center;
                if (!HasSetFadeOutVelocity)
                {
                    // Music notes move away from the player
                    // Ensure this velocity is only set once so that their positions don't become desynced while moving
                    // Vector2 playerDirection = Projectile.Center - Owner.Center;
                    playerDirection.Normalize();
                    playerDirection *= 5.5f;
                    Projectile.velocity = playerDirection;
                    HasSetFadeOutVelocity = true;
                }

                // Fade away; once it becomes completely invisible, teleport to around the cursor's position and switch to linger mode
                Projectile.alpha += (int)Math.Ceiling(255f / FadeOutTime);
                if (Projectile.alpha >= 255)
                {
                    Projectile.alpha = 255;

                    float degreesAmt = Main.zenithWorld ? 51.428f : 60f;
                    Vector2 musicNoteRotationOffset = Vector2.UnitY.RotatedBy(MathHelper.ToRadians(degreesAmt * NoteSequence) + _randomReleaseRotationOffset);

                    Vector2 mouse = Owner.ClampedMouseWorld();
                    Projectile.Center = mouse + musicNoteRotationOffset * 220f;
                    ReleaseCenterPoint = mouse;
                    playerDirection = Projectile.Center - mouse;
                    playerDirection.Normalize();
                    playerDirection *= -13f;
                    Projectile.velocity = playerDirection;

                    if (Main.zenithWorld)
                    {
                        if (SoundEngine.TryGetActiveSound(StupidEasterEggSlot, out var Flowey))
                            Flowey?.Stop();
                    }
                    else
                        SoundEngine.PlaySound(AnahitasArpeggio.EndSound with { Volume = 0.8f }, Projectile.Center);
                    AIState = 2f;
                }
            }
            else if (AIState == 2f) // The actual attack state
            {
                // Quickly fade back in
                // Fade out at the end of its lifetime
                if (Projectile.timeLeft > 30)
                {
                    Projectile.alpha -= 17;
                    if (Projectile.alpha < 0)
                        Projectile.alpha = 0;
                }
                else
                {
                    Projectile.alpha += 9;
                    if (Projectile.alpha > 255)
                        Projectile.alpha = 255;
                }

                // Slow down quickly
                if (Projectile.velocity.Length() > 0.5f)
                    Projectile.velocity *= 0.925f;
                else
                {
                    Projectile.velocity = Vector2.Zero;

                    // Makes the notes slowly follow the mouse and rotate
                    Vector2 centerPointDirection = (Owner.Calamity().mouseWorld - ReleaseCenterPoint).SafeNormalize(Vector2.Zero);
                    float distToMove = MathF.Min(5.75f, Vector2.Distance(Owner.Calamity().mouseWorld, ReleaseCenterPoint)); // The constant value is the maximum chase speed
                    ReleaseCenterPoint += centerPointDirection * distToMove;
                    Projectile.Center += centerPointDirection * distToMove;
                    Projectile.Center = ReleaseCenterPoint + Utils.DirectionTo(ReleaseCenterPoint, Projectile.Center).RotatedBy(MathHelper.Pi * 0.01f) * Vector2.Distance(ReleaseCenterPoint, Projectile.Center);
                }
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (Main.zenithWorld)
            {
                Color stupidEasterEggColor = default;
                switch (NoteSequence)
                {
                    case 0:
                        stupidEasterEggColor = new Color(255, 0, 0);
                        break;
                    case 1:
                        stupidEasterEggColor = new Color(255, 128, 0);
                        break;
                    case 2:
                        stupidEasterEggColor = new Color(255, 255, 0);
                        break;
                    case 3:
                        stupidEasterEggColor = new Color(0, 255, 0);
                        break;
                    case 4:
                        stupidEasterEggColor = new Color(0, 255, 255);
                        break;
                    case 5:
                        stupidEasterEggColor = new Color(0, 0, 255);
                        break;
                    case 6:
                        stupidEasterEggColor = new Color(128, 0, 255);
                        break;
                    default:
                        break;
                }
                return stupidEasterEggColor;
            }
            else
                return base.GetAlpha(lightColor);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override bool? CanDamage() => AIState == 2f;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Wet, 900);
            target.AddBuff(BuffID.Confused, 300);
            if (!SoundEngine.TryGetActiveSound(SingularSoundInstanceSystem.SoundSlot, out var activeSound))
                SingularSoundInstanceSystem.PlaySingleInstance(AnahitasArpeggio.HitSound, 60, 60, Owner);
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(_randomReleaseRotationOffset);
        public override void ReceiveExtraAI(BinaryReader reader) => _randomReleaseRotationOffset = reader.ReadSingle();
    }
}
