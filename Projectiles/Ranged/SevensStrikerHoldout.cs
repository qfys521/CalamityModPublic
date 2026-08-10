using CalamityMod.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class SevensStrikerHoldout : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<TheSevensStriker>();
        public bool rolling = true; // If the slot machine is currently rolling
        public bool shotonce = false; // So that the first shot doesn't consume two ammo
        public int shottimer = 0; // Solely exists so that the Platinum shots aren't instantaneous
        public int rolltimer = 60; // Cooldown for the slot machine so that it doesn't instantly role
        public int soundtimer = 0; // Counts how long the slot machine has been spinning + the cooldown

        public SlotId RouletteSoundSlot;
        public SlotId JingleSoundSlot;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 19;
        }

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 playerpos = player.RotatedRelativePoint(player.MountedCenter, true);
            bool shouldBeHeld = !player.CantUseHoldout();

            int shot;
            float scaleFactor = 14f;
            int weaponDamage = player.GetWeaponDamage(player.HeldItem);
            float weaponKnockback = player.HeldItem.knockBack;

            // Consumes a coin, stores it, then calculates what effect will be executed
            if (Projectile.ai[1] == 0)
            {
                // Ensure the method doesn't consumes ammo on first shot to avoid consuming two coins
                if (player.PickAmmo(player.HeldItem, out shot, out scaleFactor, out weaponDamage, out weaponKnockback, out _, !shotonce))
                {
                    Projectile.ai[0] = shot;
                    Projectile.ai[1] = CalculateOutcome();
                }
                else
                {
                    Projectile.Kill();
                }
                shotonce = true;
            }

            rolltimer--;
            soundtimer++;

            // While the slot machine is rolling, play the animation
            if (rolling)
            {
                Projectile.frameCounter++;
            }
            // Make sure that it defaults at frame 1
            else
            {
                Projectile.frame = 0;
            }
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            // Once the animation is finished, stop rolling, set the extra timer to 16 frames, and reset the sprite to frame 0
            if (Projectile.frame >= Main.projFrames[Type])
            {
                rolling = false;
                rolltimer = 16;
                Projectile.frame = 0;
            }

            if (Main.myPlayer == Projectile.owner)
            {
                if (shouldBeHeld && Projectile.ai[1] != 0)
                {
                    // Holdout stuff
                    float holdscale = player.HeldItem.shootSpeed * Projectile.scale;
                    Vector2 playerpos2 = playerpos;
                    Vector2 going = Main.screenPosition + new Vector2((float)Main.mouseX, (float)Main.mouseY) - playerpos2;
                    if (player.gravDir == -1f)
                    {
                        going.Y = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - playerpos2.Y;
                    }
                    Vector2 normalizedgoing = Vector2.Normalize(going);
                    if (float.IsNaN(normalizedgoing.X) || float.IsNaN(normalizedgoing.Y))
                    {
                        normalizedgoing = -Vector2.UnitY;
                    }
                    normalizedgoing *= holdscale;
                    if (normalizedgoing.X != Projectile.velocity.X || normalizedgoing.Y != Projectile.velocity.Y)
                    {
                        Projectile.netUpdate = true;
                    }
                    Projectile.velocity = normalizedgoing * 0.55f;

                    // If the animation isn't playing and the cooldown timer is at or below 0
                    if (!rolling && rolltimer <= 0)
                    {
                        // Jackpot gets special benefits since it shoots multiple rounds
                        if (Projectile.ai[1] == 4)
                        {
                            shottimer++;
                            // Play a sound and display the jackpot text
                            if (shottimer == 1)
                            {
                                JingleSoundSlot = SoundEngine.PlaySound(Main.zenithWorld ? TheSevensStriker.JackpotGFB : TheSevensStriker.JackpotSound, player.Center);
                                CombatText.NewText(player.getRect(), Color.Gold, CalamityUtils.GetTextValue("Misc.SevensJackpot"), true);
                            }
                            // Every 7 frames, shoot 7 coins. The first 7 frames are excluded for timing purposes
                            if (shottimer % 7 == 0 && shottimer > 7 && shottimer <= 56)
                            {
                                int jackpotDamage = (int)(weaponDamage * (Main.zenithWorld ? TheSevensStriker.JackpotMultiplierGFB : TheSevensStriker.JackpotMultiplier));
                                Shoot(7, ModContent.ProjectileType<SevensStrikerPlatinumCoin>(), jackpotDamage, weaponKnockback, (int)scaleFactor * 2f, 0.2f);
                                SoundEngine.PlaySound(TheSevensStriker.CoinSound, Projectile.Center);
                            }
                            // After 7 waves have been shot, reset the gun and roll again
                            if (shottimer > (Main.zenithWorld ? 88 : 56))
                            {
                                soundtimer = 0;
                                rolling = true;
                                Projectile.ai[1] = 0;
                                shottimer = 0;
                            }
                        }
                        else
                        {
                            shottimer++;

                            if (shottimer == 1)
                            {
                                // The other three outcomes
                                switch (Projectile.ai[1])
                                {
                                    // A single brick with 100% damage
                                    case 1:
                                        Shoot(1, ModContent.ProjectileType<SevensStrikerBrick>(), weaponDamage, 0, 2f, 0);
                                        CombatText.NewText(player.getRect(), Color.Gray, CalamityUtils.GetTextValue("Misc.SevensBust"), true);
                                        JingleSoundSlot = SoundEngine.PlaySound(Main.zenithWorld ? TheSevensStriker.BustGFB : TheSevensStriker.BustSound, player.Center);
                                        break;
                                    // 7 exploding oranges with 100% damage
                                    case 2:
                                        int doublesDamage = (int)(weaponDamage * TheSevensStriker.DoublesMultiplier);
                                        Shoot(7, ModContent.ProjectileType<SevensStrikerOrange>(), doublesDamage, weaponKnockback, 2f, 0.1f);
                                        CombatText.NewText(player.getRect(), Color.Orange, CalamityUtils.GetTextValue("Misc.SevensDoubles"), true);
                                        JingleSoundSlot = SoundEngine.PlaySound(TheSevensStriker.DoublesSound, player.Center);
                                        break;
                                    // 7 piercing grapes with X% damage
                                    // Also fires 7 splitting cherries in a tighter spread with Y% damage
                                    case 3:
                                        int cherryDamage = (int)(weaponDamage * TheSevensStriker.TriplesCherryMultiplier);
                                        int grapeDamage = (int)(weaponDamage * TheSevensStriker.TriplesGrapeMultiplier);
                                        Shoot(7, ModContent.ProjectileType<SevensStrikerCherry>(), cherryDamage, weaponKnockback, 1.5f, 0.1f);
                                        Shoot(7, ModContent.ProjectileType<SevensStrikerGrape>(), grapeDamage, weaponKnockback, 2f, 0.2f);
                                        CombatText.NewText(player.getRect(), Color.Red, CalamityUtils.GetTextValue("Misc.SevensTriples"), true);
                                        JingleSoundSlot = SoundEngine.PlaySound(TheSevensStriker.TriplesSound, player.Center);
                                        break;
                                }
                            }

                            // Reset the gun and roll again
                            if (shottimer > (Main.zenithWorld ? 56 : 16))
                            {
                                soundtimer = 0;
                                rolling = true;
                                Projectile.ai[1] = 0;
                                shottimer = 0;
                            }
                        }
                    }
                    // Update the roll and jingle sound positions, if applicable
                    if (SoundEngine.TryGetActiveSound(RouletteSoundSlot, out var rouletteSound) && rouletteSound.IsPlaying)
                        rouletteSound.Position = Projectile.Center;

                    if (SoundEngine.TryGetActiveSound(JingleSoundSlot, out var jingle) && jingle.IsPlaying)
                        jingle.Position = player.Center;
                }
                // If the player can't use the gun, KILL it
                else
                {
                    Projectile.Kill();
                }
            }

            // Sounds
            // Crank & new casino
            if (Projectile.frameCounter == 0 && Projectile.frame == 2 * (Main.projFrames[Type] / 19))
            {
                SoundEngine.PlaySound(SoundID.Item108 with { Volume = SoundID.Item108.Volume * 0.9f }, Projectile.Center);
                RouletteSoundSlot = SoundEngine.PlaySound(TheSevensStriker.RouletteSound, Projectile.Center);
            }
            // Clicks for when each slot is finished
            if (soundtimer == 92 || soundtimer == 108 || soundtimer == 124)
            {
                SoundEngine.PlaySound(TheSevensStriker.RouletteTickSound, Projectile.Center);
            }

            // Holdout stuff
            Projectile.position = (player.RotatedRelativePoint(player.MountedCenter, true) - Projectile.Size / 2f) + Projectile.velocity * 95;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        // Calculates which attack will occur based on coin.
        public int CalculateOutcome()
        {
            if (Main.zenithWorld)
            {
                int roll = Main.rand.Next(100);
                if (roll < 20)
                    return 4;
                else
                    return 1;
            }
            else
            {
                switch (Projectile.ai[0])
                {
                    // Copper Coins have:
                    // 50% chance for a brick
                    // 30% chance for oranges
                    // 15% chance for grapes and cherries
                    // 5% chance for a jackpot
                    case ProjectileID.CopperCoin:
                        {
                            int roll = Main.rand.Next(100);
                            if (roll <= 50)
                                return 1;
                            else if (roll > 50 && roll <= 80)
                                return 2;
                            else if (roll > 80 && roll <= 95)
                                return 3;
                            else
                                return 4;
                        }
                    // Silver Coins have:
                    // 20% chance for a brick
                    // 50% chance for oranges
                    // 20% chance for grapes and cherries
                    // 10% chance for a jackpot
                    case ProjectileID.SilverCoin:
                        {
                            int roll = Main.rand.Next(100);
                            if (roll <= 20)
                                return 1;
                            else if (roll > 20 && roll <= 70)
                                return 2;
                            else if (roll > 70 && roll <= 90)
                                return 3;
                            else
                                return 4;
                        }
                    // Gold Coins have:
                    // 5% chance for a brick
                    // 30% chance for oranges
                    // 50% chance for grapes and cherries
                    // 15% chance for a jackpot
                    case ProjectileID.GoldCoin:
                        {
                            int roll = Main.rand.Next(100);
                            if (roll <= 5)
                                return 1;
                            else if (roll > 5 && roll <= 35)
                                return 2;
                            else if (roll > 35 && roll <= 85)
                                return 3;
                            else
                                return 4;
                        }
                    // Platinum Coins are a guaranteed jackpot
                    case ProjectileID.PlatinumCoin:
                        return 4;
                }
            }
            // This should never be returned
            return 1;
        }

        // Where the shooting takes place (wow!)
        public void Shoot(int projcount, int type, int damage, float kb, float scaleFactor, float spreadfactor)
        {
            Player player = Main.player[Projectile.owner];
            Vector2 armPosition = player.RotatedRelativePoint(player.MountedCenter, true);
            armPosition += Projectile.velocity.SafeNormalize(player.direction * Vector2.UnitX) * 32f;
            armPosition.Y -= 20f;
            Vector2 gunTip = armPosition + Projectile.velocity.SafeNormalize(player.direction * Vector2.UnitX) * player.HeldItem.scale * 70f;
            for (int i = 0; i < projcount; ++i)
            {
                Vector2 perturbedSpeed = Projectile.velocity.RotatedBy(MathHelper.Lerp(-spreadfactor, spreadfactor, i / 7f)) * scaleFactor;
                int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), gunTip, perturbedSpeed, type, damage, kb, Main.player[Projectile.owner].whoAmI);
                if (Main.projectile.IndexInRange(p))
                    Main.projectile[p].originalDamage = damage;

            }
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Player Owner = Main.player[Projectile.owner];
            Texture2D gun = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/SevensStrikerHoldout").Value;

            SpriteEffects flip = Projectile.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float drawAngle = Projectile.rotation + (Owner.direction < 0 ? MathHelper.Pi : 0);
            Vector2 drawOrigin = new Vector2(Owner.direction < 0 ? gun.Width - 33f : 33f, 33f);
            Vector2 drawOffset = Owner.MountedCenter + Projectile.rotation.ToRotationVector2() - Main.screenPosition;
            drawOffset.Y -= 10;
            int indframeheight = gun.Height / Main.projFrames[Type];
            int currentframe = indframeheight * Projectile.frame;
            Rectangle frame = new Rectangle(0, currentframe, gun.Width, indframeheight);

            // This is full brightness but it's better than random flickering for whatever reason
            Main.EntitySpriteDraw(gun, drawOffset, frame, Color.White, drawAngle, drawOrigin, Projectile.scale, flip, 0);

            return false;
        }

        // When the gun disappears, stop any in-progress slot or jingle sounds and set a cooldown of 12 frames.
        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(RouletteSoundSlot, out var dringdring))
                dringdring.Stop();

            if (SoundEngine.TryGetActiveSound(JingleSoundSlot, out var jingle))
                jingle.Stop();

            Main.player[Projectile.owner].SetDummyItemTime(12);
        }

        // This gun does not deal melee damage, thanks.
        public override bool? CanDamage() => false;

        // Has velocity but updates positions manually
        public override bool ShouldUpdatePosition() => false;
    }
}
