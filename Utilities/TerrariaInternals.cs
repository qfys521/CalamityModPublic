using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace CalamityMod;

internal static class TerrariaInternals
{
    internal static float MeleeUseTimeMultiplier(Player player) => 1f / player.GetTotalAttackSpeed(DamageClass.Melee);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "scAdj")]
    private static extern ref float MainScreenAdjustmentField(Main main);

    internal static float MainScreenAdjustment => MainScreenAdjustmentField(Main.instance);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_workingLightMap")]
    internal static extern ref LightMap WorkingLightMap(LightingEngine lightingEngine);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "UpdateTime_SpawnTownNPCs")]
    private static extern void UpdateTimeSpawnTownNPCsAccessor(Main _, bool forceUpdate);

    internal static void UpdateTimeSpawnTownNPCs(bool forceUpdate) => UpdateTimeSpawnTownNPCsAccessor(null, forceUpdate);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_currentPriceAdjustment")]
    internal static extern ref float ShopPriceAdjustment(ShopHelper shopHelper);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_currentHappiness")]
    internal static extern ref string ShopHappiness(ShopHelper shopHelper);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "LimitAndRoundMultiplier")]
    internal static extern float LimitAndRoundShopPrice(ShopHelper shopHelper, float priceAdjustment);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_damage")]
    internal static extern ref int HurtInfoDamage(ref Player.HurtInfo hurtInfo);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetPickaxeDamage")]
    internal static extern int GetPickaxeDamage(Player player, int x, int y, int pickPower, int hitBufferIndex, Tile tileTarget);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DoesPickTargetTransformOnKill")]
    internal static extern bool DoesPickTargetTransformOnKill(Player player, HitTile hitCounter, int damage, int x, int y, int pickPower, int bufferIndex, Tile tileTarget);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ClearMiningCacheAt")]
    internal static extern void ClearMiningCacheAt(Player player, int x, int y, int hitTileCacheType);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "UpdatePortableStoolUsage")]
    internal static extern void UpdatePortableStoolUsage(Player player);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Fishing_GetBait")]
    internal static extern void GetFishingBait(Player player, out Item bait);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_sliderCurrentValueCache")]
    internal static extern ref float DifficultySliderValue(CreativePowers.ASharedSliderPower power);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "GetExclusions")]
    private static extern HashSet<int> GetBestiaryExclusionsAccessor(BestiaryDatabaseNPCsPopulator _);

    internal static HashSet<int> GetBestiaryExclusions() => GetBestiaryExclusionsAccessor(null);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_currentDatabase")]
    private static extern ref BestiaryDatabase CurrentBestiaryDatabaseField(BestiaryDatabaseNPCsPopulator _);

    internal static BestiaryDatabase CurrentBestiaryDatabase => CurrentBestiaryDatabaseField(null);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Register")]
    internal static extern BestiaryEntry RegisterBestiaryEntry(BestiaryDatabaseNPCsPopulator populator, BestiaryEntry entry);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_wasSeenNearPlayerByNetId")]
    internal static extern ref List<int> SeenNearbyNpcNetIds(NPCWasNearPlayerTracker tracker);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_playerHitboxesForBestiary")]
    internal static extern ref List<Rectangle> BestiaryPlayerHitboxes(NPCWasNearPlayerTracker tracker);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_highQualityLightingRequirement")]
    internal static extern ref Color HighQualityLightingRequirement(TileDrawing tileDrawing);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mediumQualityLightingRequirement")]
    internal static extern ref Color MediumQualityLightingRequirement(TileDrawing tileDrawing);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "waterfallTexture")]
    internal static extern ref Asset<Texture2D>[] WaterfallTextures(WaterfallManager waterfallManager);

    internal static Dictionary<string, CustomSky> SkyEffects(EffectManager<CustomSky> skyManager) => EffectManagerAccessors<CustomSky>.Effects(skyManager);

    private static class EffectManagerAccessors<T> where T : GameEffect
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_effects")]
        internal static extern ref Dictionary<string, T> Effects(EffectManager<T> effectManager);
    }
}
