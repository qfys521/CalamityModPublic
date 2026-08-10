using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Items.Armor.Auric;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Empyrean;
using CalamityMod.Items.Armor.Fearmonger;
using CalamityMod.Items.Armor.GemTech;
using CalamityMod.Items.Armor.GodSlayer;
using CalamityMod.Items.Armor.Hydrothermic;
using CalamityMod.Items.Armor.OmegaBlue;
using CalamityMod.Items.Armor.Prismatic;
using CalamityMod.Items.Armor.Silva;
using CalamityMod.Items.Armor.Tarragon;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Accessories.Wings
{
    // The equip sprite is actually blank as a custom draw layer is used to draw the real sprites without any base sprites conflicting

    [AutoloadEquip(EquipType.Wings)]
    public class TiredTail : BaseWings
    {
        public override float BonusAscentWhileFalling => 0.95f;
        public override float BonusAscentWhileRising => 0.15f;
        public override float RisingSpeedThreshold => 1f;
        public override float MaxAscentSpeed => 4.5f;
        public override float BaseAscent => 0.1f;

        float BoostPower => 3f;

        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(210, 8.5f, 2.88f, true, 16, 10f);
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 22;
            Item.height = 20;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (!hideVisual)
                player.GetModPlayer<TiredTailPlayer>().tiredTailDraw = true;
            if (player.velocity.Y != 0)
            {
                if (player.dashDelay != -1) //This differs from Starboard - Starboard checks for 1 full second passing after a dash begun, while this happens as soon as the dash ends
                    player.runSlowdown *= 6;
            }


        }

        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<TiredTailPlayer>().tiredTailDraw = true;
        }

        public override bool WingUpdate(Player player, bool inUse) => true;

        public override void AdditionalFlightMovement(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            if (player.TryingToHoverDown && player.controlJump && player.wingTime > 0f && !player.merman)
            {
                player.wingTime += 0.5f;
                player.velocity.Y *= 0.8f;
                if (player.velocity.Y > -2f && player.velocity.Y < 1f)
                    player.velocity.Y = 1E-05f;

                ascentWhenFalling *= 0f;
                ascentWhenRising *= 0f;
                constantAscend *= 0f;
            }
            if (player.TryingToHoverUp)
            {
                ascentWhenFalling *= BoostPower;
                ascentWhenRising *= BoostPower;
                maxCanAscendMultiplier /= 1.5f;
                maxAscentMultiplier /= 1.5f;
                constantAscend *= BoostPower;
                player.wingTime -= 1;
            }
        }

        public override void AddRecipes()
        {

            CreateRecipe().
                AddIngredient<ArmoredShell>(3).
                AddIngredient(ItemID.Sapphire).
                AddTile(TileID.DemonAltar).
                Register();
        }
    }

    public class TiredTailPlayer : ModPlayer
    {
        public bool tiredTailDraw = false;
        public List<(Vector2 pos, float rot)> tailPos;
        public override void ResetEffects()
        {
            //Handled here to prevent issues with instancing of chat tags
            if (TiredTailTextEffects.displayTimer > 0)
            {
                TiredTailTextEffects.expansionFactor += 0.033f;

                TiredTailTextEffects.displayTimer -= 1;
            }
            else
            {
                TiredTailTextEffects.expansionFactor = 0f;
            }

            tiredTailDraw = false;
        }
        public override void PostUpdate()
        {
            if (tiredTailDraw)
            {
                if (tailPos == null)
                {
                    tailPos = new();
                    for (var i = 0; i < 3; i++)
                    {

                        tailPos.Add(new(Player.Center, 0));
                    }
                }
                for (var i = 0; i < tailPos.Count; i++)
                {
                    Vector2 nextCent = Vector2.Zero;
                    float nextRot = 0;
                    if (i == 0)
                    {
                        nextCent = Player.Center + new Vector2(0, 6);
                        nextRot = new Vector2(Player.direction, -0.325f).ToRotation();
                    }
                    else
                    {
                        nextCent = tailPos[i - 1].Item1;
                        nextRot = tailPos[i - 1].Item2;
                    }

                    Vector2 destinationOffset = nextCent - tailPos[i].Item1;
                    if (nextRot != tailPos[i].Item2)
                    {
                        float angle = MathHelper.WrapAngle(nextRot - tailPos[i].Item2);
                        destinationOffset = destinationOffset.RotatedBy(angle * 0.1f);
                    }
                    var rotation = destinationOffset.ToRotation();

                    var center = nextCent - destinationOffset.SafeNormalize(Vector2.Zero) * (14 * 1);
                    if (i == tailPos.Count - 1)
                    {
                        center = nextCent - destinationOffset.SafeNormalize(Vector2.Zero) * (21 * 1);
                    }
                    tailPos[i] = new(center, rotation);
                }
            }
            else
            {
                tailPos = null;
            }
        }
    }
    public class TailDraw : PlayerDrawLayer
    {
        List<Vector4[]> ColorPallettes = new();
        public override void SetStaticDefaults()
        {
            if (Main.netMode != NetmodeID.Server)
            Main.QueueMainThreadAction(() =>
            {
                //Load palettes automatically
                var texture = ModContent.Request<Texture2D>("CalamityMod/Items/Accessories/Wings/TiredTailPallette", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                var BaseArray = new Color[texture.Width * texture.Height];
                texture.GetData(BaseArray);
                List<Vector4> pal = new();
                for (var i = 0; i < BaseArray.Length; i++)
                {
                    if (i % texture.Width == 0)
                    {
                        if (pal.Count > 0)
                            ColorPallettes.Add(pal.ToArray());
                        pal = new();
                    }
                    pal.Add(BaseArray[i].ToVector4());
                }
                ColorPallettes.Add(pal.ToArray());
                ArmorPallettes =
                [
                    //silva 
                    ([ItemType<SilvaHeadMagic>(),ItemType<SilvaHeadSummon>()],
                    [ItemType<SilvaArmor>()],
                    [ItemType<SilvaLeggings>()],
                    ColorPallettes[2]),
                    //god slayer
                    ([ItemType<GodSlayerHeadMelee>(),ItemType<GodSlayerHeadRanged>(),ItemType<GodSlayerHeadRogue>()],
                    [ItemType<GodSlayerChestplate>()],
                    [ItemType<GodSlayerLeggings>()],
                    ColorPallettes[3]),
                    //hydro
                    ([ItemType<HydrothermicHeadMelee>(),ItemType<HydrothermicHeadRanged>(),ItemType<HydrothermicHeadMagic>(),ItemType<HydrothermicHeadSummon>(),ItemType<HydrothermicHeadRogue>()],
                    [ItemType<HydrothermicArmor>()],
                    [ItemType<HydrothermicSubligar>()],
                    ColorPallettes[4]),
                    //auric
                    ([ItemType<AuricTeslaHeadMelee>(),ItemType<AuricTeslaHeadRanged>(),ItemType<AuricTeslaHeadMagic>(),ItemType<AuricTeslaHeadSummon>(),ItemType<AuricTeslaHeadRogue>()],
                    [ItemType<AuricTeslaBodyArmor>()],
                    [ItemType<AuricTeslaCuisses>()],
                    ColorPallettes[5]),
                    //fearmonger
                    ([ItemType<FearmongerGreathelm>()],
                    [ItemType<FearmongerPlateMail>()],
                    [ItemType<FearmongerGreaves>()],
                    ColorPallettes[6]),
                    //Bloodlfare
                    ([ItemType<BloodflareHeadMelee>(),ItemType<BloodflareHeadRanged>(),ItemType<BloodflareHeadMagic>(),ItemType<BloodflareHeadSummon>(),ItemType<BloodflareHeadRogue>()],
                    [ItemType<BloodflareBodyArmor>()],
                    [ItemType<BloodflareCuisses>()],
                    ColorPallettes[7]),
                    //Tarragon
                    ([ItemType<TarragonHeadMelee>(),ItemType<TarragonHeadRanged>(),ItemType<TarragonHeadMagic>(),ItemType<TarragonHeadSummon>(),ItemType<TarragonHeadRogue>()],
                    [ItemType<TarragonBreastplate>()],
                    [ItemType<TarragonLeggings>()],
                    ColorPallettes[8]),
                    //Omega Blue
                    ([ItemType<OmegaBlueHelmet>()],
                    [ItemType<OmegaBlueChestplate>()],
                    [ItemType<OmegaBlueTentacles>()],
                    ColorPallettes[9]),
                    //Gem Tech
                    ([ItemType<GemTechHeadgear>()],
                    [ItemType<GemTechBodyArmor>()],
                    [ItemType<GemTechSchynbaulds>()],
                    ColorPallettes[10]),
                    //Demonshade
                    ([ItemType<DemonshadeHelm>()],
                    [ItemType<DemonshadeBreastplate>()],
                    [ItemType<DemonshadeGreaves>()],
                    ColorPallettes[11]),
                    //Prismatic
                    ([ItemType<PrismaticHelmet>()],
                    [ItemType<PrismaticRegalia>()],
                    [ItemType<PrismaticGreaves>()],
                    ColorPallettes[12]),
                    //Empyrean
                    ([ItemType<EmpyreanMask>()],
                    [ItemType<EmpyreanCloak>()],
                    [ItemType<EmpyreanCuisses>()],
                    ColorPallettes[13]),
                    //Solar
                    ([ItemID.SolarFlareHelmet],
                    [ItemID.SolarFlareBreastplate],
                    [ItemID.SolarFlareLeggings],
                    ColorPallettes[14]),
                    //Vortex
                    ([ItemID.VortexHelmet],
                    [ItemID.VortexBreastplate],
                    [ItemID.VortexLeggings],
                    ColorPallettes[15]),
                    //Nebula
                    ([ItemID.NebulaHelmet],
                    [ItemID.NebulaBreastplate],
                    [ItemID.NebulaLeggings],
                    ColorPallettes[16]),
                    //Stardust
                    ([ItemID.StardustHelmet],
                    [ItemID.StardustBreastplate],
                    [ItemID.StardustLeggings],
                    ColorPallettes[17]),
                ];
            });

        }

        List<(List<int> heads, List<int> bodies, List<int> legs, Vector4[] pallette)> ArmorPallettes = [];
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => true;

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Wings);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;
            var modplayer = drawInfo.drawPlayer.GetModPlayer<TiredTailPlayer>();
            var tex1 = Request<Texture2D>("CalamityMod/Items/Accessories/Wings/TiredTailSegment").Value;
            var tex2 = Request<Texture2D>("CalamityMod/Items/Accessories/Wings/TiredTailTail").Value;
            if (modplayer.tailPos != null)
            {
                var TargetPallette = ColorPallettes[1];
                foreach (var item in ArmorPallettes)
                {
                    if ((item.heads.Count == 0 || item.heads.Any(x => ContentSamples.ItemsByType[x].headSlot == player.head))
                        && (item.bodies.Count == 0 || item.bodies.Any(x => ContentSamples.ItemsByType[x].bodySlot == player.body))
                        && (item.legs.Count == 0 || item.legs.Any(x => ContentSamples.ItemsByType[x].legSlot == player.legs)))
                    {
                        TargetPallette = item.pallette;
                        break;
                    }
                }
                //manual override for Empyrean's transform to work
                if (player.Calamity().meldTransformationPower || player.Calamity().meldTransformationForce)
                {
                    TargetPallette = ArmorPallettes.First(x => x.heads.Contains(ItemType<EmpyreanMask>())).pallette;
                }
                using var SegmentLease = RenderTargetPool.Shared.Rent(Main.instance.GraphicsDevice, tex1.Width, tex1.Height);
                using var TailLease = RenderTargetPool.Shared.Rent(Main.instance.GraphicsDevice, tex2.Width, tex2.Height);
                using (Main.spriteBatch.Scope())
                {
                    using (SegmentLease.Scope(clearColor: Color.Transparent))
                    {
                        var shaderSnap = new SpriteBatchSnapshot();
                        shaderSnap.SortMode = SpriteSortMode.Immediate;
                        shaderSnap.SamplerState = SamplerState.PointClamp;
                        shaderSnap.TransformMatrix = Matrix.Identity;
                        Main.spriteBatch.Begin(shaderSnap);

                        MiscShaderData paletteSwap = GameShaders.Misc["CalamityMod:PaletteSwap"];
                        paletteSwap.Shader.Parameters["sourcePalette"].SetValue(ColorPallettes[0]);
                        paletteSwap.Shader.Parameters["paletteSize"].SetValue(ColorPallettes[0].Length);
                        paletteSwap.Shader.Parameters["targetPalette"].SetValue(TargetPallette);
                        paletteSwap.Shader.Parameters["matchThreshold"].SetValue(0.0001f);
                        paletteSwap.Apply();
                        Main.spriteBatch.Draw(tex1, Vector2.Zero, Color.White);
                        Main.spriteBatch.End();
                    }
                    using (TailLease.Scope(clearColor: Color.Transparent))
                    {
                        var shaderSnap = new SpriteBatchSnapshot();
                        shaderSnap.SortMode = SpriteSortMode.Immediate;
                        shaderSnap.SamplerState = SamplerState.PointClamp;
                        shaderSnap.TransformMatrix = Matrix.Identity;
                        Main.spriteBatch.Begin(shaderSnap);

                        MiscShaderData paletteSwap = GameShaders.Misc["CalamityMod:PaletteSwap"];
                        paletteSwap.Shader.Parameters["sourcePalette"].SetValue(ColorPallettes[0]);
                        paletteSwap.Shader.Parameters["paletteSize"].SetValue(ColorPallettes[0].Length);
                        paletteSwap.Shader.Parameters["targetPalette"].SetValue(TargetPallette);
                        paletteSwap.Shader.Parameters["matchThreshold"].SetValue(0.01f);
                        paletteSwap.Apply();
                        Main.spriteBatch.Draw(tex2, Vector2.Zero, Color.White);
                        Main.spriteBatch.End();
                    }
                }
                var calamityPlayer = drawInfo.drawPlayer.Calamity();
                //Copied from the rogue steallth visuals
                // TODO -- rogue stealth visuals are an utter catastrophe and should be fully destroyed on next stealth rework
                float r = 1;
                float g = 1;
                float b = 1;
                float a = 1;
                if (calamityPlayer.rogueStealth > 0f && calamityPlayer.rogueStealthMax > 0f && drawInfo.drawPlayer.townNPCs < 3f && CalamityClientConfig.Instance.StealthInvisibility)
                {
                    // A translucent orchid color, the rogue class color
                    float colorValue = calamityPlayer.rogueStealth / calamityPlayer.rogueStealthMax * 0.9f; //0 to 0.9
                    r = 1f - (colorValue * 0.89f); //255 to 50
                    g = 1f - colorValue; //255 to 25
                    b = 1f - (colorValue * 0.89f); //255 to 50
                    a = 1f - colorValue; //255 to 25
                }

                for (var i = modplayer.tailPos.Count - 1; i >= 0; i--)
                {
                    var color2 = Lighting.GetColor(modplayer.tailPos[i].pos.ToTileCoordinates());
                    var color = new Color((int)(color2.R * r), (int)(color2.G * g), (int)(color2.B * b), (int)(color2.A * a));
                    if (i == modplayer.tailPos.Count - 1)
                    {
                        drawInfo.DrawDataCache.Add(new(TailLease.Target, modplayer.tailPos[i].pos - Main.screenPosition, null, color, modplayer.tailPos[i].rot + (float)Math.PI, tex2.Size() / 2f, 1f, SpriteEffects.None, 0));
                    }
                    else
                    {
                        drawInfo.DrawDataCache.Add(new(SegmentLease.Target, modplayer.tailPos[i].pos - Main.screenPosition, null, color, modplayer.tailPos[i].rot + (float)Math.PI, tex1.Size() / 2f, 1f, SpriteEffects.None, 0));
                    }
                }
            }

        }
    }

    public sealed class TiredTailTextEffects(string text) : TextSnippet
    {
        //Handled by TiredTailPlayer as this effect is for TiredTail
        public static float expansionFactor = 0;
        public static int displayTimer = 0;
        public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = new Vector2(), Color color = new Color(), float scale = 1)
        {
            displayTimer = 10;
            //size = new Vector2(GetStringLength(FontAssets.MouseText.Value), FontAssets.MouseText.Value.MeasureString(" ").Y * scale);

            if (color == default || color == Main.MouseTextColorReal)
            {
                color = Colors.AlphaDarken(HotPink.TextColor);
            }
            var textarray = text.ToArray();
            for (var i = 0; i < textarray.Length; i++)
            {
                if (expansionFactor - 10 > i)
                {
                    textarray[i] = (i == 0 ? 'ɔ' : '»');
                }
            }
            var textToDraw = new string(textarray);

            size = FontAssets.MouseText.Value.MeasureString(textToDraw) * scale;

            if (!justCheckingString && (color.R != 0 || color.G != 0 || color.B != 0))
            {
                var pos = position;
                using var lease = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice);
                string txt = "";
                var matrix = spriteBatch.transformMatrix;
                using (spriteBatch.Scope())
                {
                    using (lease.Scope(clearColor: Color.Transparent))
                    {
                        var max = FontAssets.MouseText.Value.MeasureString(text) * Math.Min(1f, expansionFactor);
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, matrix);

                        foreach (var item in textarray)
                        {//new(92,83,117)
                            pos = position;
                            pos.X += Math.Min(FontAssets.MouseText.Value.MeasureString(txt).X, max.X + 9999);
                            float sin = (MathF.Sin(pos.X * 0.02f + Main.GlobalTimeWrappedHourly * -1.5f) + 1) * 0.5f;
                            float sin2 = (MathF.Sin(pos.X * 0.02f + Main.GlobalTimeWrappedHourly * -0.9f) + 1) * 0.5f;
                            float sin3 = MathF.Sin(pos.X * 0.02f + Main.GlobalTimeWrappedHourly * -1.5f + MathHelper.PiOver2);
                            var c = new Color(171, 153, 204);
                            if (txt.Length == 0 || txt.Length == text.Length - 1)
                                c = Color.Cyan;
                            else if (txt.Length % 4 == 3)
                            {
                                c = Color.HotPink;
                            }
                            c = Color.Lerp(Colors.AlphaDarken(new Color(0, 255, 200)), c, MathHelper.Clamp(expansionFactor - 2, 0, 1));
                            float posMult = Math.Max(MathHelper.Clamp((expansionFactor - 10) * 0.5f, 0, 3), MathHelper.Clamp((expansionFactor - 2) * 0.5f, 0, 1));
                            var origin = FontAssets.MouseText.Value.MeasureString(item.ToString()) * 0.5f;
                            ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, item.ToString(), origin + pos + new Vector2(0, item == 'ɔ' ? -1 : 0) + new Vector2((-2f + 4 * sin2) * posMult, (-2 + sin * 4) * posMult), c, sin3 * posMult * 0.1f, origin, new Vector2(scale));
                            txt += item;
                        }
                        spriteBatch.End();
                    }

                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
                    foreach (var item in ChatManager.ShadowDirections)
                    {
                        spriteBatch.Draw(lease.Target, Vector2.Zero + Vector2.TransformNormal(item * 2, matrix), null, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                    spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    spriteBatch.End();
                }
            }
            return true;
        }

    }
}
