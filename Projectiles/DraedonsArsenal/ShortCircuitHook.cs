using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Cooldowns;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    [PierceResistException]
    public class ShortCircuitHook : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public static readonly SoundStyle Explode = new("CalamityMod/Sounds/Item/ElectricBurst") { Volume = 0.8f };
        public bool giveCooldown = false;
        public bool onSpawn = true;
        public enum TaserAIState
        {
            Firing,
            Electrocuting,
            ReelingBack
        }

        public TaserAIState AIState
        {
            get => (TaserAIState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public float Time
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public int ElectrocutionTarget
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public const float ReelbackSpeed = 40f;
        public Color hookColor = Color.SlateGray;
        public SlotId Hum { get; set; }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.extraUpdates = 8;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.ArmorPenetration = 10;
        }

        public override void AI()
        {
            if (onSpawn)
            {
                for (int i = 0; i < 15; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 30, Effects.ArsenalEffects.ArsenalElectricDust);
                    dust.scale = Main.rand.NextBool(7) ? 1.5f : 0.9f;
                    dust.noGravity = true;
                    dust.fadeIn = 2;
                    dust.color = Effects.ArsenalEffects.ArsenalElectricColor;
                    dust.velocity = Projectile.velocity.RotatedByRandom(0.7f) * Main.rand.NextFloat(0.9f, 2.8f);
                }
                onSpawn = false;
            }
            if (AIState != TaserAIState.Firing)
                Time++;
            Player player = Main.player[Projectile.owner];
            hookColor = Color.Lerp(hookColor, Color.SlateGray, 0.25f);

            float distanceFromPlayer = Projectile.Distance(player.Center);
            switch (AIState)
            {
                case TaserAIState.Firing:
                    if (distanceFromPlayer > 800f || Time >= 90f)
                        GoToAIState(TaserAIState.ReelingBack);
                    break;
                case TaserAIState.Electrocuting:
                    if (distanceFromPlayer > 2000f)
                        GoToAIState(TaserAIState.ReelingBack);

                    if (SoundEngine.TryGetActiveSound(Hum, out var hum) && hum.IsPlaying)
                    {
                        hum.Position = player.Center;
                        hum.Pitch = MathHelper.Lerp(0f, 1f, Utils.GetLerpValue(0f, 150, Time, true));
                    }
                    // electric explosion;
                    if (Time == 150 || !Main.npc[ElectrocutionTarget].active)
                    {
                        if (!Main.npc[ElectrocutionTarget].active)
                            giveCooldown = false;
                        SoundEngine.PlaySound(Explode, Projectile.Center);
                        Projectile.localNPCHitCooldown = 15;

                        for (int i = 0; i < 25; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(Projectile.Center, Effects.ArsenalEffects.ArsenalElectricDust, new Vector2(9, 9).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f));
                            dust.scale = Main.rand.NextFloat(0.9f, 1.4f);
                            dust.noGravity = false;
                            dust.color = Effects.ArsenalEffects.ArsenalElectricColor;

                            Dust dust2 = Dust.NewDustPerfect(Projectile.Center, Effects.ArsenalEffects.ArsenalElectricDust, new Vector2(15, 15).RotatedByRandom(100) * Main.rand.NextFloat(0.75f, 1f));
                            dust2.scale = Main.rand.NextFloat(1.7f, 2.1f);
                            dust2.noGravity = true;
                            dust2.color = Effects.ArsenalEffects.ArsenalElectricColor;
                        }

                        DirectionalPulseRing pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Effects.ArsenalEffects.ArsenalElectricColor * 0.5f, new Vector2(1f, 1), 0, 0.5f, 3f, 18);
                        GeneralParticleHandler.SpawnParticle(pulse);
                        Particle bloom = new CustomSpark(Projectile.Center, Vector2.Zero, "CalamityMod/Particles/BloomCircle", false, 10, 1.3f, Effects.ArsenalEffects.ArsenalElectricColor, Vector2.One, true, true);
                        GeneralParticleHandler.SpawnParticle(bloom);

                        Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ShortCircuitExplosion>(), Projectile.damage * 8, 0, Projectile.owner, 0);
                        Time = 0;
                        Projectile.velocity = Projectile.SafeDirectionTo(player.Center);

                        GoToAIState(TaserAIState.ReelingBack);
                        return;
                    }
                    if (Main.npc[ElectrocutionTarget].active)
                        Projectile.Center = Main.npc[ElectrocutionTarget].Center;
                    break;
                case TaserAIState.ReelingBack:
                    // Kill the gun and the hook if the hook has returned to the gun.
                    if (SoundEngine.TryGetActiveSound(Hum, out var hum2) && hum2.IsPlaying)
                    {
                        hum2?.Stop();
                    }
                    if (Projectile.Hitbox.Intersects(player.Hitbox))
                    {
                        if (giveCooldown)
                        {
                            player.Calamity().arsenalCooldown = 300;
                            player.AddCooldown(ArsenalPower.ID, 300);
                        }
                        SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/DudFire") with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
                        Projectile.Kill();
                        return;
                    }
                    Projectile.tileCollide = false;
                    Projectile.extraUpdates = 8;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(player.Center) * (ReelbackSpeed / Projectile.extraUpdates), 0.02f);
                    break;
            }

            Projectile.rotation = Projectile.AngleFrom(player.Center);

            ManipulatePlayerItemValues(player);
        }


        public void ManipulatePlayerItemValues(Player player)
        {
            player.ChangeDir((player.Center.X - Projectile.Center.X < 0).ToDirectionInt());
            player.itemRotation = CalamityUtils.WrapAngle90Degrees(Projectile.rotation);
            player.itemTime = 4;
            player.itemAnimation = 4;
        }

        public void GoToAIState(TaserAIState newAIState)
        {
            // Don't waste the resources changing the AI state if the projectile is already in said state.
            if (AIState == newAIState)
                return;

            Projectile.penetrate = -1;
            AIState = newAIState;
            Projectile.netUpdate = true;
        }

        public override bool PreDraw(Player renderingPlayer, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Player player = Main.player[Projectile.owner];
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D lineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomLineSoftEdge").Value;
            Vector2 altPos = player.Center - Vector2.UnitY * 3;
            float distance = Utils.Distance(altPos, Projectile.Center);
            int drawSeperation = 10;
            int startSeperation = drawSeperation;
            Vector2 toPoint = Utils.DirectionTo(altPos, Projectile.Center);
            for (int i = startSeperation; i < distance; i += drawSeperation)
            {
                Main.EntitySpriteDraw(lineTex, Projectile.Center - Main.screenPosition - toPoint * i, null, hookColor with { A = 0 }, toPoint.ToRotation() + MathHelper.PiOver2, lineTex.Size() * 0.5f, new Vector2(1, 1.3f) * Projectile.scale * 0.01f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.tileCollide = false;
            if (Projectile.localNPCHitCooldown > 7 && target == Main.npc[ElectrocutionTarget])
            Projectile.localNPCHitCooldown -= 1;
            hookColor = Color.Cyan;
            target.AddBuff(ModContent.BuffType<StaticDischarge>(), 120);

            if (AIState == TaserAIState.Firing)
            {
                giveCooldown = true;
                Projectile.Center = target.Center;
                Projectile.extraUpdates = 1;
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        Dust dust2 = Dust.NewDustPerfect(Projectile.Center, Effects.ArsenalEffects.ArsenalElectricDust, new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.85f, 0.9f));
                        dust2.scale = Main.rand.NextFloat(0.9f, 1.2f);
                        dust2.noGravity = true;
                        dust2.color = Effects.ArsenalEffects.ArsenalElectricColor;
                    }
                }
                ElectrocutionTarget = target.whoAmI;
                Time = 0f;

                SoundStyle charge = new("CalamityMod/Sounds/Item/LowHum");
                Hum = SoundEngine.PlaySound(charge with { Volume = 1.6f, IsLooped = true }, Projectile.Center);

                GoToAIState(TaserAIState.Electrocuting);
            }

            // If it kills an enemy, don't give cooldown
            if (giveCooldown && (target.life <= 0 && target.realLife == -1))
                giveCooldown = false;
        }
        public override bool? CanHitNPC(NPC target) => (Projectile.numHits == 0 || target == Main.npc[ElectrocutionTarget]) ? null : false;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            GoToAIState(TaserAIState.ReelingBack);
            return false;
        }
    }
}
