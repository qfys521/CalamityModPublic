using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Enums;
using CalamityMod.Pathfinding;
using CalamityMod.Pathfinding.Movements;
using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Utilities.NPCUtils;

namespace CalamityMod.NPCs.SunkenSea
{
    /// <summary>
    /// An abstract class that provides the necessary members for an NPC to function as a Sunken Sea NPC.<br/>
    /// These NPCs can hunt both players and other NPCs, and maintain lists of NPCs they hunt and avoid.
    /// </summary>
    public abstract class SunkenSeaNPC : ModNPC, IPathfinder
    {
        protected PathfindingManager pathfinding = null;

        private NPC _currentPrey;
        private NPC _currentPredator;
        private Player _currentPlayer;

        /// <summary>
        /// A list of NPC IDs that this creature hunts.
        /// </summary>
        protected abstract List<int> PreyIDs { get; }

        /// <summary>
        /// A list of NPC IDs that this creature avoids.
        /// </summary>
        protected abstract List<int> PredatorIDs { get; }

        /// <summary>
        /// The biome flags that define where this Sunken Sea NPC can spawn.
        /// </summary>
        protected abstract SunkenSeaBiomeFlags BiomeDesignation { get; }

        /// <summary>
        /// The current NPC that this creature has identified as prey.
        /// </summary>
        protected NPC CurrentPrey
        {
            get => _currentPrey;
            private set
            {
                if (value != null && (_currentPrey == null || _currentPrey.whoAmI != value.whoAmI))
                    OnPreyDetection(value);
                _currentPrey = value;
            }
        }

        /// <summary>
        /// The current NPC that this creature has identified as a predator.
        /// </summary>
        protected NPC CurrentPredator
        {
            get => _currentPredator;
            private set
            {
                if (value != null && (_currentPredator == null || _currentPredator.whoAmI != value.whoAmI))
                    OnPredatorDetection(value);
                _currentPredator = value;
            }
        }

        /// <summary>
        /// The current player that this creature has detected.
        /// </summary>
        protected Player CurrentPlayer
        {
            get => _currentPlayer;
            private set
            {
                if (value != null && (_currentPlayer == null || _currentPlayer.whoAmI != value.whoAmI))
                    OnPlayerDetection(value);
                _currentPlayer = value;
            }
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.UsesNewTargeting[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            AIType = -1;

            SpawnModBiomes = Enum.GetValues<SunkenSeaBiomeFlags>()
                .Where(flag => flag != SunkenSeaBiomeFlags.None &&
                               flag != SunkenSeaBiomeFlags.UndergroundDesert &&
                               BiomeDesignation.HasFlag(flag))
                .Select(flag => SunkenSeaBiomeCorrespondentDict.Dict[flag].BiomeType)
                .ToArray();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
            => bestiaryEntry.Info.AddRange([new FlavorTextBestiaryInfoElement($"Mods.CalamityMod.Bestiary.{Name}")]);

        public override bool PreAI()
        {
            UpdateTargets();
            return true;
        }

        /// <summary>
        /// Called when this NPC is hit by another NPC. Override this method to define custom behavior when attacked.
        /// </summary>
        /// <param name="attacker">The NPC that attacked this creature.</param>
        public virtual void OnHitByNPC(NPC attacker) { }

        /// <summary>
        /// Called when this creature detects a prey NPC. Override this method to define custom behavior upon detecting prey.
        /// </summary>
        /// <param name="prey">The NPC that has been detected as prey.</param>
        protected virtual void OnPreyDetection(NPC prey) { }

        /// <summary>
        /// Called when this creature detects a predator NPC. Override this method to define custom behavior upon detecting a predator.
        /// </summary>
        /// <param name="predator">The NPC that has been detected as a predator.</param>
        protected virtual void OnPredatorDetection(NPC predator) { }

        /// <summary>
        /// Called when this creature detects a player. Override this method to define custom behavior upon detecting a player.
        /// </summary>
        /// <param name="player">The player that has been detected.</param>
        protected virtual void OnPlayerDetection(Player player) { }

        /// <summary>
        /// Determines whether a detected player is a valid target based on specific conditions.
        /// </summary>
        /// <param name="p">The player to evaluate.</param>
        /// <returns><see langword="true"/> if the NPC is a valid target; otherwise, <see langword="false"/>.</returns>
        protected virtual bool PlayerSearchFilter(Player p) => NPC.HasSight(p.Center) && Vector2.DistanceSquared(NPC.Center, p.Center) < 72900f;

        /// <summary>
        /// Determines whether a detected NPC is a valid target based on specific conditions.
        /// </summary>
        /// <param name="n">The NPC to evaluate.</param>
        /// <returns><see langword="true"/> if the NPC is a valid target; otherwise, <see langword="false"/>.</returns>
        protected virtual bool NPCSearchFilter(NPC n) => NPC.HasSight(n.Center) && Vector2.DistanceSquared(NPC.Center, n.Center) < 72900f && (PreyIDs.Contains(n.type) || PredatorIDs.Contains(n.type));

        /// <summary>
        /// Updates the current targets of this creature, including prey, predators, and players, based on detection logic.
        /// </summary>
        protected void UpdateTargets()
        {
            var searchResults = SearchForTarget(NPC, playerFilter: PlayerSearchFilter, npcFilter: NPCSearchFilter);

            if (!searchResults.FoundTarget)
            {
                (CurrentPredator, CurrentPrey, CurrentPlayer) = (null, null, null);
                return;
            }

            CurrentPlayer = searchResults.NearestTankOwner;

            if (!searchResults.FoundNPC)
            {
                (CurrentPredator, CurrentPrey) = (null, null);
                return;
            }

            var nearestNPC = searchResults.NearestNPC;
            CurrentPredator = PredatorIDs.Contains(nearestNPC.type) ? nearestNPC : null;
            CurrentPrey = PreyIDs.Contains(nearestNPC.type) ? nearestNPC : null;
        }

        /// <summary>
        /// Checks whether a specific tile is valid for this NPC, considering water level and entity size.
        /// </summary>
        /// <param name="point">The tile location to check.</param>
        /// <returns><see langword="true"/> if the tile is valid; otherwise, <see langword="false"/>.</returns>
        protected bool SunkenSeaTileValidity(Point point)
        {
            return SunkenSeaTileValidity(NPC, point);
        }

        /// <summary>
        /// Checks whether a specific tile is valid for this NPC, considering water level.
        /// </summary>
        /// <param name="point">The tile location to check.</param>
        /// <returns><see langword="true"/> if the tile is valid; otherwise, <see langword="false"/>.</returns>
        protected bool SunkenSeaTileValiditySizeless(Point point)
        {
            return SunkenSeaTileValidity(NPC, point, false);
        }

        /// <summary>
        /// Checks whether a specific tile is valid for an NPC, considering water level and entity size if given.
        /// </summary>
        /// <param name="npc">The npc to use for the check.</param>
        /// <param name="point">The tile location to check.</param>
        /// <returns><see langword="true"/> if the tile is valid; otherwise, <see langword="false"/>.</returns>
        public static bool SunkenSeaTileValidity(NPC npc, Point point, bool accountForSize = true)
        {
            Point actualFuckingPoint = new Point(point.X * 16, point.Y * 16);

            if (accountForSize)
            {
                return npc.Hitbox.Contains(actualFuckingPoint)
                    || !npc.GetIntersectingHitboxPoints(
                        actualFuckingPoint, 10, 10).Any(a => Main.tile[a].IsTileSolidGround() || Main.tile[a].LiquidAmount < 255 || Main.tile[a].LiquidType != LiquidID.Water);
            }
            else
            {
                return !(Main.tile[point].IsTileSolidGround() || Main.tile[point].LiquidAmount < 255 || Main.tile[point].LiquidType != LiquidID.Water);
            }
        }

        /// <summary>
        /// Checks whether a specific tile is valid for a lava NPC, considering water level and entity size if given.
        /// </summary>
        /// <param name="point">The tile location to check.</param>
        /// <returns><see langword="true"/> if the tile is valid; otherwise, <see langword="false"/>.</returns>
        public bool LavaTileValidity(Point point)
        {
            Point actualFuckingPoint = new Point(point.X * 16, point.Y * 16);

            return NPC.Hitbox.Contains(actualFuckingPoint)
                || !NPC.GetIntersectingHitboxPoints(
                    actualFuckingPoint, 10, 10).Any(a => Main.tile[a].IsTileSolidGround() || Main.tile[a].LiquidAmount < 255 || Main.tile[a].LiquidType != LiquidID.Lava);
        }

        #region IPathfinder Implementation

        /// <summary>
        /// The acceleration this PathfindingManager will impart to its Entity when making it follow a found path.<br />
        /// This has no impact on the Entity's other behaviors, AI, etc.
        /// </summary>
        public float Acceleration { get; set; } = 0.2f;

        /// <summary>
        /// The maximum speed this PathfindingManager allow its Entity to move at when making it follow a found path.<br />
        /// This has no impact on the Entity's other behaviors, AI, etc.
        /// </summary>
        public float MaxSpeed { get; set; } = 4f;

        /// <summary>
        /// The minimum distance this PathdingManager requires its Entity to reach from its target point before the point is marked as "reached".<br />
        /// This has no impact on the Entity's other behaviors, AI, etc.
        /// </summary>
        public float MinimumPointDistance { get; set; } = 48f;

        public virtual IEnumerable<IMovement> Movements => [new SunkenSeaSwimMovement(NPC)];

        public virtual void AwaitingPathBehavior() => NPC.velocity *= 0.95f;

        #endregion
    }
}
