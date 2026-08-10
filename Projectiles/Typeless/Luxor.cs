using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Items.Accessories;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class Luxor : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public Player Owner => Main.player[Projectile.owner];
        public CalamityPlayer moddedOwner => Owner.Calamity();
        public Color usedColor = Color.White;
        public DamageClass lastDamageClass = null;
        public ref float time => ref Projectile.ai[0];
        public ref float attackTimer => ref Projectile.ai[1];
        public ref float idleTimer => ref Projectile.localAI[0];
        public int classType = 0; // 0 = Classless, 1 = Melee, 2 = Ranged, 3 = Mage, 4 = Summoner, 5 = Rogue
        public Vector2 tipPosition => Projectile.Center + (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 15;

        public float fxFade = 0; // The glow visuals multiplier
        public float postFireBoost = 0; // The glow increases right when you fire, then quickly fades
        public float followSpeed = 6; // The speed Luxor follows you, lower is faster
        public bool rogueChain = true; // Used for rogue's double projectile shot
        public override void SetDefaults()
        {
            Projectile.width = 114;
            Projectile.height = 38;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Color classColor = Color.White;
            DamageClass itemClass = Owner.HeldItem.DamageType;
            // Held item class is not updated when you hold any tools
            if ((Owner.HeldItem.axe > 0 || Owner.HeldItem.hammer > 0 || Owner.HeldItem.pick > 0) && lastDamageClass != null)
                itemClass = lastDamageClass;
            if (itemClass == null)
            {
                Projectile.Kill();
                return;
            }
            else if (itemClass.CountsAsClass<MeleeDamageClass>())
            {  classColor = Color.Red; classType = 1; }
            else if (itemClass.CountsAsClass<RangedDamageClass>())
            { classColor = Color.Cyan; classType = 2; }
            else if (itemClass.CountsAsClass<MagicDamageClass>())
            { classColor = Color.Gold; classType = 3; }
            else if (itemClass.CountsAsClass<SummonDamageClass>())
            { classColor = Color.Lime; classType = 4; }
            else if (itemClass.CountsAsClass<RogueDamageClass>())
            { classColor = Color.Magenta; classType = 5; }
            else
            { classColor = Color.Gray; classType = 0; };

            if (time == 0 || itemClass != lastDamageClass)
            {
                Projectile.netUpdate = true;
                attackTimer = 60; // 60 frame attack delay on spawn or swapping class weapons 
            }
            lastDamageClass = itemClass;

            float rate = Main.GlobalTimeWrappedHourly * 7;
            List<Color> eColors = new List<Color>()
            {
                Color.Lerp(classColor, Color.White, 0.15f),
                Color.Lerp(classColor, Color.Black, 0.15f)
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            usedColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            Projectile.velocity = Utils.DirectionTo(Projectile.Center, Owner.Calamity().mouseWorld);

            Vector2 destination = Vector2.Lerp(Owner.Center - Projectile.velocity * (70 + 100 * Utils.GetLerpValue(0, 110, attackTimer, true)), Owner.Center + Vector2.UnitY * -40, Utils.GetLerpValue(280, 300, idleTimer, true));
            Projectile.velocity = (destination - Projectile.Center) / followSpeed;

            Projectile.rotation = Utils.DirectionTo(Projectile.Center, Owner.Calamity().mouseWorld).ToRotation() + MathHelper.PiOver2;
            
            fxFade = moddedOwner.luxorsGiftVanity ? 1 : (float)Math.Pow(Utils.GetLerpValue(30, 0, attackTimer, true), 3) + postFireBoost;

            if (idleTimer >= 280)
            {
                if (moddedOwner.luxorHit)
                {
                    if (idleTimer > 320)
                        idleTimer = 320;
                    idleTimer -= 1;
                }
                else
                {
                    fxFade = 1;
                    attackTimer = 40;
                }
            }
            else
            {

            }

            if (!moddedOwner.luxorsGiftVanity)
            {
                if (attackTimer == 0 && moddedOwner.luxorHit)
                {
                    int attackSpeed = 0;
                    int attackDamage = 0;
                    int projType = 0;
                    switch (classType)
                    {
                        case 0: attackSpeed = LuxorsGift.classlessAttackSpeed; attackDamage = LuxorsGift.classlessDamage; projType = ModContent.ProjectileType<LuxorsGiftClassless>(); break;
                        case 1: attackSpeed = LuxorsGift.meleeAttackSpeed; attackDamage = LuxorsGift.meleeDamage; projType = ModContent.ProjectileType<LuxorsGiftMelee>(); break;
                        case 2: attackSpeed = LuxorsGift.rangedAttackSpeed; attackDamage = LuxorsGift.rangedDamage; projType = ModContent.ProjectileType<LuxorsGiftRanged>(); break;
                        case 3: attackSpeed = LuxorsGift.magicAttackSpeed; attackDamage = LuxorsGift.magicDamage; projType = ModContent.ProjectileType<LuxorsGiftMagic>(); break;
                        case 4: attackSpeed = LuxorsGift.summonerAttackSpeed; attackDamage = LuxorsGift.summonerDamage; projType = ModContent.ProjectileType<LuxorsGiftSummon>(); break;
                        case 5: attackSpeed = LuxorsGift.rogueAttackSpeed; attackDamage = LuxorsGift.rogueDamage; projType = ModContent.ProjectileType<LuxorsGiftRogue>(); break;
                    }
                    float powerMult = Utils.GetLerpValue(-120, 140, attackSpeed, true); // An intensity multiplier based on the fire rate of the mode

                    for (int i = 0; i < (int)(20 * powerMult); i++) // Firing dust
                    {
                        Vector2 dustVel = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(0.5f) * 12 * powerMult;
                        Dust dust = Dust.NewDustPerfect(tipPosition, ModContent.DustType<LightDust>(), dustVel * Main.rand.NextFloat(0.5f, 1.2f));
                        dust.noGravity = !Main.rand.NextBool(3);
                        dust.scale = Main.rand.NextFloat(0.75f, 1.4f) * powerMult;
                        dust.color = usedColor;
                        dust.noLightEmittance = true;
                    }
                    if (classType == 1) // Melee shotgun
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, Utils.DirectionTo(tipPosition, Owner.Calamity().mouseWorld).RotatedBy(-0.12f * i * Main.rand.NextFloat(0.7f, 1f)) * (12 - Main.rand.NextFloat(0f, 0.6f) - Math.Abs(i)), projType, (int)(Owner.GetDamage(itemClass).ApplyTo(attackDamage)), 0f, Owner.whoAmI);
                            proj.ArmorPenetration = LuxorsGift.luxArmorPen;
                        }
                        for (int i = 0; i < 5; i++) // These are visual
                        {
                            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, Utils.DirectionTo(tipPosition, Owner.Calamity().mouseWorld).RotatedByRandom(0.4f) * (12 - Main.rand.NextFloat(2f, 5f)), projType, (int)(Owner.GetDamage(itemClass).ApplyTo(attackDamage)), 0f, Owner.whoAmI, 5);
                            proj.timeLeft = 95;
                        }
                    }
                    else // Everything else
                    {
                        Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, Utils.DirectionTo(tipPosition, Owner.Calamity().mouseWorld) * 12, projType, (int)(Owner.GetDamage(itemClass).ApplyTo(attackDamage)), 0f, Owner.whoAmI);
                        proj.ArmorPenetration = LuxorsGift.luxArmorPen;
                        if (classType == 5) // Rogue has gravity so velocity is adjusted a bit
                            proj.velocity.Y -= 1.5f;
                    }

                    SoundStyle blam = new("CalamityMod/Sounds/Item/GunShotSmall");
                    SoundEngine.PlaySound(blam with { Volume = 0.35f, Pitch = Main.rand.NextFloat(0.6f, 0.8f) }, tipPosition);
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.7f, Pitch = 0.2f }, tipPosition);

                    if (classType == 5 && rogueChain)
                    {
                        rogueChain = false;
                        attackTimer = attackSpeed / 3;
                    }
                    else
                    {
                        attackTimer = attackSpeed;
                        moddedOwner.luxorHit = false;
                        if (!CalamityClientConfig.Instance.Photosensitivity)
                            postFireBoost = 1.7f;
                        idleTimer = 0;
                        rogueChain = true;
                    }
                    Projectile.netUpdate = true;
                }
                if (attackTimer > 0)
                    attackTimer--;
                if (postFireBoost > 0) // Quickly fade out the increased poost fire visuals
                    postFireBoost -= 0.2f;
            }

            if (time % 2 == 0 && false)
            {
                Vector2 dustVel = new Vector2(10, 10).RotatedByRandom(100);
                Dust dust = Dust.NewDustPerfect(Owner.Center + dustVel, ModContent.DustType<LightDust>(), dustVel * Main.rand.NextFloat(0.1f, 0.4f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.5f, 0.7f);
                dust.color = usedColor;
                dust.noLightEmittance = true;
            }

            float idleFade = Utils.GetLerpValue(280, 300, idleTimer, true);
            Lighting.AddLight(tipPosition, Color.Lerp(usedColor, Color.Lerp(usedColor, Color.White, 0.5f), idleFade).ToVector3() * (0.5f * MathHelper.Lerp(fxFade, 1, 0.55f) + 0.35f * idleFade));

            if (moddedOwner.luxorsGift || moddedOwner.luxorsGiftVanity)
                Projectile.timeLeft++;
            else
                Projectile.Kill();
            if (Owner.dead)
                Projectile.Kill();

            time++;
            if (!moddedOwner.luxorHit && !moddedOwner.luxorsGiftVanity)
                idleTimer++;
            else if (moddedOwner.luxorsGiftVanity)
                idleTimer = 0;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Texture2D bTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Texture2D cTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/Light").Value;
            float idleFade = moddedOwner.luxorsGiftVanity ? 0 : (float)Math.Pow(Utils.GetLerpValue(280, 300, idleTimer, true), 5);
            Color drawColor = usedColor;
            Color bodyColor = Color.Lerp(lightColor, Color.Gold with { A = 0 }, idleFade);
            float drawMult = Math.Max(idleFade, fxFade);

            if (idleFade == 0)
            {
                for (int i = 0; i < 18; i++) // Backglow
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 18f).ToRotationVector2() * 6 * drawMult;
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + drawOffset, null, drawColor with { A = 0 } * 0.2f * drawMult, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
                }
            }
            // Main body
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, bodyColor * (1 - idleFade), Projectile.rotation, tex.Size() * 0.5f, new Vector2(1 - 0.85f * idleFade, 1f) * Projectile.scale, SpriteEffects.None);
            for (int i = 0; i < 4; i++) // Glow orb
            {
                Main.EntitySpriteDraw(bTexture, Vector2.Lerp(tipPosition, Projectile.Center, idleFade) - Main.screenPosition, null, drawColor with { A = 0 } * 0.35f * drawMult, time * 0.13f * (i + 1), bTexture.Size() * 0.5f, new Vector2(1 + i * 0.15f, i - 0.3f * i) * Projectile.scale * MathHelper.Lerp(0.33f, 0.22f, idleFade) * drawMult, SpriteEffects.None);
            }
            // Glow orb center
            Main.EntitySpriteDraw(cTexture, Vector2.Lerp(tipPosition, Projectile.Center, idleFade) - Main.screenPosition, null, Color.White with { A = 0 } * 0.75f * drawMult, Projectile.rotation, cTexture.Size() * 0.5f, Projectile.scale * MathHelper.Lerp(0.4f, 0.3f, idleFade) * drawMult, SpriteEffects.None);
            return false;
        }
        public override bool? CanDamage() => false;
    }
}
