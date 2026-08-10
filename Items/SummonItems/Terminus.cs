using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    [LegacyName("BossRush")]
    public class Terminus : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public override void SetDefaults()
        {
            Item.width = Main.zenithWorld ? 54 : 70;
            Item.height = Main.zenithWorld ? 78 : 80;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<TerminusHoldout>();
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override void UpdateInventory(Player player)
        {
            if (Main.zenithWorld)
                Item.SetNameOverride(this.GetLocalizedValue("GFBName"));
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            // The wiki classifies Boss Rush as an event
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.EventItem;
        }

        #region Sprite Drawing
        public static void DrawTerminusGlow(SpriteBatch spriteBatch, Vector2 baseDrawPosition, Rectangle frame, float rotation, float baseScale)
        {
            Texture2D glowTexture = ModContent.Request<Texture2D>("CalamityMod/Items/SummonItems/TerminusGlow").Value;
            Vector2 origin = frame.Size() * 0.5f;

            float pulseRate = Main.GlobalTimeWrappedHourly % 5f;
            float slowingRate = Main.GlobalTimeWrappedHourly * 0.4f;
            float orbitRadius = baseScale != 1f ? 4f : 7f;
            float glowCount = 3f;

            pulseRate /= 2.5f;
            if (pulseRate >= 1f)
                pulseRate = 2f - pulseRate;
            pulseRate *= 0.6f;

            for (int i = 0; i < glowCount; i++)
            {
                float progress = i / glowCount;
                float pulseRotation = (progress + slowingRate) * MathHelper.TwoPi;

                Vector2 offset = Vector2.UnitY * orbitRadius;
                offset = offset.RotatedBy(pulseRotation) * pulseRate;

                spriteBatch.Draw(glowTexture, baseDrawPosition + offset, frame, new Color(56, 12, 115, 33), rotation, origin, baseScale, 0, 0);
            }

            spriteBatch.Draw(glowTexture, baseDrawPosition, frame, Color.White, rotation, origin, baseScale, 0, 0);
        }

        public static void DrawGlowInWorld(SpriteBatch spriteBatch, Vector2 baseDrawPosition, float rotation, float baseScale)
        {
            Texture2D glowTexture = ModContent.Request<Texture2D>("CalamityMod/Items/SummonItems/TerminusGlow").Value;
            Rectangle frame = glowTexture.Bounds;
            DrawTerminusGlow(spriteBatch, baseDrawPosition + new Vector2(0, -1), frame, rotation, baseScale);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture;
            float invScale = scale * 1.4f;

            if (Main.zenithWorld)
            {
                texture = ModContent.Request<Texture2D>("CalamityMod/Items/SummonItems/Terminus_GFB").Value;
                spriteBatch.Draw(texture, position - new Vector2(-4, -1), null, Color.White, 0f, origin, invScale - 0.04f, 0, 0);
            }
            else
            {
                texture = ModContent.Request<Texture2D>("CalamityMod/Items/SummonItems/Terminus").Value;
                spriteBatch.Draw(texture, position, null, Color.White, 0f, origin, invScale, 0, 0);
                DrawTerminusGlow(spriteBatch, position, frame, 0f, invScale);
            }

            return false;
        }

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (Main.zenithWorld)
            {
                Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/SummonItems/Terminus_GFB").Value;
                spriteBatch.Draw(texture, item.position - Main.screenPosition, null, lightColor, rotation, Vector2.Zero, scale, 0, 0);
                return false;
            }
            else
                return true;
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (!Main.zenithWorld)
            {
                DrawGlowInWorld(spriteBatch, item.Center - Main.screenPosition, rotation, scale);
            }
        }
        #endregion
    }
}
