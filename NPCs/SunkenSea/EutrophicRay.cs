using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.SunkenSea
{
    public class EutrophicRay : ModNPC
    {
        public bool hasBeenHit = false;

        public static Asset<Texture2D> GlowTexture;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers();
            value.Position.X += 24f;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
            if (!Main.dedServ)
            {
                GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow", AssetRequestMode.AsyncLoad);
            }
        }

        public override void SetDefaults()
        {
            NPC.damage = Main.hardMode ? 50 : 20;
            NPC.width = 116;
            NPC.height = 36;
            NPC.defense = Main.hardMode ? 15 : 5;
            NPC.lifeMax = Main.hardMode ? 500 : 200;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(0, 0, 1, 0);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath55;
            NPC.knockBackResist = 0.5f;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<EutrophicRayBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.EutrophicRay")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.chaseable);
            writer.Write(hasBeenHit);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.chaseable = reader.ReadBoolean();
            hasBeenHit = reader.ReadBoolean();
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            if (NPC.velocity.X > 0.25f)
            {
                NPC.spriteDirection = 1;
            }
            else if (NPC.velocity.X < 0.25f)
            {
                NPC.spriteDirection = -1;
            }
            if (NPC.justHit && !hasBeenHit)
            {
                hasBeenHit = true;
                NPC.damage = NPC.defDamage;
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                if (NPC.Center.X < Main.player[NPC.target].Center.X)
                {
                    NPC.ai[0] = 1f;
                }
                else
                {
                    NPC.ai[0] = 2f;
                }
            }
            NPC.chaseable = hasBeenHit;
            if (hasBeenHit)
            {
                float AccelerationY = Main.hardMode ? 0.4f : 0.2f;
                float MaxSpeedY = Main.hardMode ? 4f : 2.5f;
                float Rotation = 0;
                if ((NPC.Center.Y + 0.4f) > Main.player[NPC.target].Center.Y)
                {
                    NPC.velocity.Y -= AccelerationY;
                    if (NPC.velocity.Y < -MaxSpeedY)
                    {
                        NPC.velocity.Y = -MaxSpeedY;
                    }
                }
                else if ((NPC.Center.Y - 0.4f) < Main.player[NPC.target].Center.Y)
                {
                    NPC.velocity.Y += AccelerationY;
                    if (NPC.velocity.Y > MaxSpeedY)
                    {
                        NPC.velocity.Y = MaxSpeedY;
                    }
                }
                float AccelerationX = Main.hardMode ? 0.4f : 0.25f;
                float MaxSpeedX = Main.hardMode ? 6f : 4f;

                if (NPC.ai[0] == 1f)
                {
                    Rotation = -0.05f;
                    NPC.velocity.X -= AccelerationX;
                    if (NPC.velocity.X < -MaxSpeedX)
                    {
                        NPC.velocity.X = -MaxSpeedX;
                    }

                    if ((NPC.Center.X + 300f) < Main.player[NPC.target].Center.X)
                    {
                        NPC.ai[0] = 2f;
                    }
                }
                else if (NPC.ai[0] == 2f)
                {
                    Rotation = 0.05f;
                    NPC.velocity.X += AccelerationX;
                    if (NPC.velocity.X > MaxSpeedX)
                    {
                        NPC.velocity.X = MaxSpeedX;
                    }

                    if ((NPC.Center.X - 300f) > Main.player[NPC.target].Center.X)
                    {
                        NPC.ai[0] = 1f;
                    }
                }

                NPC.rotation = NPC.velocity.Y * Rotation;
                if (NPC.rotation < -0.1f)
                {
                    NPC.rotation = -0.1f;
                }
                if (NPC.rotation > 0.1f)
                {
                    NPC.rotation = 0.1f;
                    return;
                }
            }
            else
                NPC.damage = 0;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += (hasBeenHit || NPC.IsABestiaryIconDummy) ? 0.15f : 0f;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Vector2 center = new Vector2(NPC.Center.X, NPC.Center.Y);
            Vector2 halfSizeTexture = new Vector2((float)(TextureAssets.Npc[NPC.type].Value.Width / 2), (float)(TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2));
            Vector2 vector = center - screenPos;
            vector -= new Vector2((float)GlowTexture.Value.Width, (float)(GlowTexture.Value.Height / Main.npcFrameCount[NPC.type])) * 1f / 2f;
            vector += halfSizeTexture * 1f + new Vector2(0f, 4f + NPC.gfxOffY);
            Color color = new Color(127 - NPC.alpha, 127 - NPC.alpha, 127 - NPC.alpha, 0).MultiplyRGBA(Microsoft.Xna.Framework.Color.LightBlue);
            Main.spriteBatch.Draw(GlowTexture.Value, vector,
                new Microsoft.Xna.Framework.Rectangle?(NPC.frame), color, NPC.rotation, halfSizeTexture, 1f, spriteEffects, 0f);
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion && !projectile.Calamity().overridesMinionDamagePrevention)
            {
                return hasBeenHit;
            }
            return null;
        }

        public override float SpawnChance(NPC.Spawner spawner)
        {
            if (spawner.Player.Calamity().ZoneSunkenSea && spawner.waterTile && !spawner.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.6f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (Main.netMode != NetmodeID.Server)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RayGore1").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RayGore2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RayGore3").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RayGore4").Type, 1f);
                }
            }
        }
    }
}
