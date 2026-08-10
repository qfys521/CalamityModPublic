using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("Scorpion")]
    public class Scorpio : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        #region Other stats

        // Weapon stats.
        public static int OriginalUseTime = 30;
        public static int TimeBetweenBursts = 8;
        public static int ProjectilesPerBurst = 8;

        // Small rocket stats.
        public static float EnemyDetectionDistance = 2000f;
        public static float TrackingSpeed = 0.06f; // VERY DELICATE VALUE, CHANGE SLOWLY.

        // Large rocket stats. 
        public static float NukeEnemyDistanceDetection = 300f;
        public static float NukeRequiredRotationProximity = 0.96f;
        public static float NukeTrackingSpeed = 0.0095f; // VERY DELICATE VALUE, CHANGE SLOWLY.

        #endregion

        public static readonly SoundStyle RocketShoot = new("CalamityMod/Sounds/Item/ScorpioShot") { Volume = 0.45f };
        public static readonly SoundStyle RocketHit = new("CalamityMod/Sounds/Item/ScorpioHit") { Volume = 0.35f };
        public static readonly SoundStyle NukeHit = new("CalamityMod/Sounds/Item/ScorpioNukeHit") { Volume = 0.6f };

        public override void SetDefaults()
        {
            Item.damage = 33;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = Item.useTime = OriginalUseTime;
            Item.shoot = ProjectileType<ScorpioHoldout>();
            Item.shootSpeed = 15f;
            Item.knockBack = 6.5f;

            Item.width = 96;
            Item.height = 42;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.useAmmo = AmmoID.Rocket;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = new SoundStyle("CalamityMod/Sounds/Item/DudFire") with { Volume = .4f, Pitch = -.9f, PitchVariance = 0.1f };
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;

        // Spawning the holdout won't consume ammo.
        public override bool CanConsumeAmmo(Item ammo, Player player) => player.ownedProjectileCounts[Item.shoot] != 0;

        // Makes the rotation of the mouse around the player sync in multiplayer.
        public override void HoldItem(Player player) => player.Calamity().mouseRotationListener = true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile holdout = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, ProjectileType<ScorpioHoldout>(), 0, 0f, player.whoAmI, -30);

            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            // We set the rotation to the direction to the mouse so the first frame doesn't appear bugged out.
            holdout.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);

            return false;
        }

        public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/Scorpio_Glow").Value);

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.SnowmanCannon).
                AddIngredient(ItemID.FragmentNebula, 6).
                AddIngredient(ItemID.Nanites, 100).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
