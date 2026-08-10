using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod.DataStructures;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;


namespace CalamityMod.Items.Weapons.Melee
{
    public class FourSeasonsGalaxia : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public Attunement mainAttunement = null;

        //Used for passive effects. On hit proc is never used but its just there so i can pass it as a reference in the passiveeffect function
        public int UseTimer = 0;
        public bool OnHitProc = false;

        #region stats
        public static int BaseDamage = 250;

        public static int PhoenixAttunement_BaseDamage = 300;
        public static int PhoenixAttunement_LocalIFrames = 30; //Remember its got one extra update
        public static float PhoenixAttunement_BoltDamageReduction = 0.5f;
        public static float PhoenixAttunement_BoltThrowDamageMultiplier = 1f;
        public static float PhoenixAttunement_BaseDamageReduction = 0.5f;
        public static float PhoenixAttunement_FullChargeDamageBoost = 2.1f;
        public static float PhoenixAttunement_ThrowDamageBoost = 3.2f;
        public static int PhoenixAttunement_FlamePillarLocalIFrames = 10;

        public static int PolarisAttunement_BaseDamage = 400;
        public static int PolarisAttunement_FullChargeDamage = 630;
        public static int PolarisAttunement_ShredIFrames = 10;
        public static int PolarisAttunement_LocalIFrames = 30; //Be warned its got one extra update so all the iframes should be divided in 2
        public static int PolarisAttunement_LocalIFramesCharged = 16;
        public static float PolarisAttunement_SlashDamageBoost = 6f; //Keep in mind the slice always crits
        public static int PolarisAttunement_SlashIFrames = 20;
        public static float PolarisAttunement_ShotDamageBoost = 0.8f; //The shots fired if the dash connects
        public static float PolarisAttunement_ShredChargeupGain = 1f; //How much charge is gained per second.

        public static int AndromedaAttunement_BaseDamage = 1120;
        public static int AndromedaAttunement_DashHitIFrames = 20;
        public static float AndromedaAttunement_FullChargeMult = 3.5f; //The maxmimum damage multiplier of the lunge. Note that the lunge always crits.
        public static float AndromedaAttunement_StarDamageMultiplier = 1f;
        public static float AndromedaAttunement_ChargeupBoltDamageMultiplier = 0.2f; //Damage of shots fired as it charges

        public static int AriesAttunement_BaseDamage = 375;
        public static int AriesAttunement_LocalIFrames = 10;
        public static int AriesAttunement_Reach = 650;
        public static float AriesAttunement_ChainDamageReduction = 0.2f;
        public static float AriesAttunement_OnHitBoltDamageReduction = 0.5f;
        #endregion

        public override string Texture => "CalamityMod/Items/Weapons/Melee/Galaxia"; //Base sprite for stuff like item browser and shit. yeah

        #region tooltip editing

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            if (list == null)
                return;

            SafeCheckAttunements();

            Player player = Main.LocalPlayer;
            if (player is null)
                return;

            var effectDescTooltip = list.FirstOrDefault(x => x.Text.Contains("[FUNC]") && x.Mod == "Terraria");
            var mainAttunementTooltip = list.FirstOrDefault(x => x.Text.Contains("[ATT]") && x.Mod == "Terraria");

            //Default stuff gets skipped here. MainAttunement is set to true in SafeCheckAttunements() above

            //.. but just in case
            if (mainAttunement == null)
            {
                CalamityMod.Log.Error("No main attunement on galaxia, couldn't edit its tooltip properly. How the hell did that happen.");
                return;
            }

            if (effectDescTooltip != null)
            {
                effectDescTooltip.Text = mainAttunement.FunctionText.ToString();
                effectDescTooltip.Color = mainAttunement.tooltipColor;
            }

            if (mainAttunementTooltip != null)
            {
                mainAttunementTooltip.Text = mainAttunementTooltip.Text.Replace("ATT", mainAttunement.AttunementName.ToString());
                mainAttunementTooltip.Color = Color.Lerp(mainAttunement.tooltipColor, mainAttunement.tooltipColor2, 0.5f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.5f);
            }
        }
        #endregion

        public override void SetDefaults()
        {
            Item.width = Item.height = 128;
            Item.damage = BaseDamage;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = RarityType<Turquoise>();
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 24f;
            Item.reuseDelay = 12;
        }

        #region saving and syncing attunements
        public override ModItem Clone(Item item)
        {
            var clone = base.Clone(item);
            if (Main.mouseItem.type == ItemType<FourSeasonsGalaxia>())
                item.ModItem?.HoldItem(Main.LocalPlayer);

            if (clone is FourSeasonsGalaxia a && item.ModItem is FourSeasonsGalaxia a2)
                a.mainAttunement = a2.mainAttunement;

            return clone;
        }

        public override void SaveData(TagCompound tag)
        {
            int attunement1 = mainAttunement == null ? -1 : (int)mainAttunement.id;
            tag["mainAttunement"] = attunement1;
        }

        public override void LoadData(TagCompound tag)
        {
            int attunement1 = tag.GetInt("mainAttunement");

            mainAttunement = AttunementSystem.FindOrNull(attunement1);
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(mainAttunement != null ? (byte)mainAttunement.id : AttunementSystem.EmptyID);
        }

        public override void NetReceive(BinaryReader reader)
        {
            mainAttunement = AttunementSystem.FindOrNull(reader.ReadInt32());
        }

        #endregion

        // 03FEB2024: Ozzatron: added so the Iban Blades don't break Overhaul compatibility. Weapons are functionally unchanged.
        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (mainAttunement == null || player.altFunctionUse != ItemAlternativeFunctionID.None)
                return false;

            Projectile blade = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            blade.originalDamage = (int)MathF.Round(blade.originalDamage * mainAttunement.DamageMultiplier);
            return false;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            damage *= (mainAttunement?.DamageMultiplier ?? 1f);
        }

        public void SafeCheckAttunements()
        {
            if (mainAttunement == null)
                mainAttunement = AttunementSystem.FindOrNull(AttunementID.Phoenix);

            else
                mainAttunement = AttunementSystem.FindOrNull(ClampAttunementRange((int)mainAttunement.id));
        }

        private static int ClampAttunementRange(int input)
        {
            if (input < (int)AttunementID.Phoenix)
                return (int)AttunementID.Phoenix;

            if (input > (int)AttunementID.Andromeda)
                return (int)AttunementID.Andromeda;

            return input;
        }

        public override void HoldItem(Player player)
        {
            player.Calamity().rightClickListener = true;
            player.Calamity().mouseWorldListener = true;

            if (CanUseItem(player))
            {
                player.Calamity().LungingDown = false;
            }
            else
            {
                UseTimer++;
            }

            SafeCheckAttunements();
            mainAttunement.ApplyStats(Item);

            if (player.Calamity().mouseRight && CanUseItem(player) && player.whoAmI == Main.myPlayer && !Main.mapFullscreen)
            {
                //Don't shoot out a visual blade if you already have one out
                if (Main.projectile.Any(n => n.active && n.type == ProjectileType<GalaxiaHoldout>() && n.owner == player.whoAmI))
                    return;

                var source = player.GetSource_ItemUse(Item);
                Projectile.NewProjectile(source, player.Top, Vector2.Zero, ProjectileType<GalaxiaHoldout>(), 0, 0, player.whoAmI, 0, Math.Sign(player.position.X - Main.MouseWorld.X));
            }
        }

        public override void UpdateInventory(Player player)
        {
            SafeCheckAttunements();
        }

        public override bool CanUseItem(Player player)
        {
            bool isRightClicking = player.altFunctionUse != ItemAlternativeFunctionID.None;
            return !isRightClicking && !Main.projectile.Any(n => n.active && n.owner == player.whoAmI &&
            (n.type == ProjectileType<PhoenixsPride>() ||
             n.type == ProjectileType<AndromedasStride>() ||
             n.type == ProjectileType<PolarisGaze>() ||
             n.type == ProjectileType<AriesWrath>()
            ));
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (mainAttunement == null)
                mainAttunement = AttunementSystem.FindOrNull(AttunementID.Phoenix);

            Texture2D itemTexture = Request<Texture2D>((mainAttunement.id == AttunementID.Polaris || mainAttunement.id == AttunementID.Andromeda) ? "CalamityMod/Items/Weapons/Melee/GalaxiaDusk" : "CalamityMod/Items/Weapons/Melee/GalaxiaDawn").Value;
            Texture2D outlineTexture = Request<Texture2D>((mainAttunement.id == AttunementID.Polaris || mainAttunement.id == AttunementID.Andromeda) ? "CalamityMod/Items/Weapons/Melee/GalaxiaDuskOutline" : "CalamityMod/Items/Weapons/Melee/GalaxiaDawnOutline").Value;

            int currentFrame = ((int)Math.Floor(Main.GlobalTimeWrappedHourly * 15f)) % 7;
            Rectangle animFrame = new Rectangle(0, 128 * currentFrame, 126, 126);

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, Main.UIScaleMatrix);

            spriteBatch.Draw(outlineTexture, position, animFrame, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);


            spriteBatch.Draw(itemTexture, position, animFrame, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (mainAttunement == null)
                mainAttunement = AttunementSystem.FindOrNull(AttunementID.Phoenix);

            Texture2D itemTexture = Request<Texture2D>((mainAttunement.id == AttunementID.Polaris || mainAttunement.id == AttunementID.Andromeda) ? "CalamityMod/Items/Weapons/Melee/GalaxiaDusk" : "CalamityMod/Items/Weapons/Melee/GalaxiaDawn").Value;
            Texture2D outlineTexture = Request<Texture2D>((mainAttunement.id == AttunementID.Polaris || mainAttunement.id == AttunementID.Andromeda) ? "CalamityMod/Items/Weapons/Melee/GalaxiaDuskOutline" : "CalamityMod/Items/Weapons/Melee/GalaxiaDawnOutline").Value;

            int currentFrame = ((int)Math.Floor(Main.GlobalTimeWrappedHourly * 15f)) % 7;
            Rectangle animFrame = new Rectangle(0, 128 * currentFrame, 126, 126);

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, Main.Transform);

            spriteBatch.Draw(outlineTexture, item.Center - Main.screenPosition, animFrame, lightColor, rotation, Item.Size * 0.5f, scale, SpriteEffects.None, 0f);

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.Transform);


            spriteBatch.Draw(itemTexture, item.Center - Main.screenPosition, animFrame, lightColor, rotation, Item.Size * 0.5f, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<OmegaBiomeBlade>().
                AddIngredient<ArmoredShell>().
                AddIngredient<TwistingNether>().
                AddIngredient<DarkPlasma>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
