#region Copyright & License Information
/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Dr.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Dr.Orders
{
	// Copied from PlaceBuildingOrderGenerator
	public class BuilderUnitBuildingOrderGenerator : IOrderGenerator
	{
		[Flags]
		enum CellType { Valid = 0, Invalid = 1, LineBuild = 2 }

		readonly BuilderUnit queue;
		readonly ActorInfo actorInfo;
		readonly BuildingInfo buildingInfo;
		readonly PlaceBuildingInfo placeBuildingInfo;
		readonly FootprintPlaceBuildingPreviewInfo footprintPlaceBuildingPreviewInfo;
		readonly string faction;
		readonly Sprite buildOk;
		readonly Sprite buildBlocked;
		readonly Viewport viewport;
		readonly WVec centerOffset;
		readonly int2 topLeftScreenOffset;
		IActorPreview[] preview;

		bool initialized;

		public BuilderUnitBuildingOrderGenerator(BuilderUnit queue, string name, WorldRenderer worldRenderer)
		{
			var world = queue.Actor.World;
			this.queue = queue;
			viewport = worldRenderer.Viewport;
			placeBuildingInfo = queue.Actor.Owner.PlayerActor.Info.TraitInfo<PlaceBuildingInfo>();

			// Clear selection if using Left-Click Orders
			if (Game.Settings.Game.UseClassicMouseStyle)
				world.Selection.Clear();

			var map = world.Map;
			var tileset = world.Map.Tileset.ToLowerInvariant();

			actorInfo = map.Rules.Actors[name];
			footprintPlaceBuildingPreviewInfo = actorInfo.TraitInfo<FootprintPlaceBuildingPreviewInfo>();
			buildingInfo = actorInfo.TraitInfo<BuildingInfo>();
			centerOffset = buildingInfo.CenterOffset(world);
			topLeftScreenOffset = -worldRenderer.ScreenPxOffset(centerOffset);

			var buildableInfo = actorInfo.TraitInfo<BuildableInfo>();
			var mostLikelyProducer = queue.MostLikelyProducer();
			faction = buildableInfo.ForceFaction
				?? (mostLikelyProducer.Trait != null ? mostLikelyProducer.Trait.Faction : queue.Actor.Owner.Faction.InternalName);

			if (map.Rules.Sequences.HasSequence("overlay", "build-valid-{0}".F(tileset)))
				buildOk = map.Rules.Sequences.GetSequence("overlay", "build-valid-{0}".F(tileset)).GetSprite(0);
			else
				buildOk = map.Rules.Sequences.GetSequence("overlay", "build-valid").GetSprite(0);
			buildBlocked = map.Rules.Sequences.GetSequence("overlay", "build-invalid").GetSprite(0);
		}

		CellType MakeCellType(bool valid, bool lineBuild = false)
		{
			var cell = valid ? CellType.Valid : CellType.Invalid;
			if (lineBuild)
				cell |= CellType.LineBuild;

			return cell;
		}

		public IEnumerable<Order> Order(World world, CPos cell, int2 worldPixel, MouseInput mi)
		{
			if (mi.Button == MouseButton.Right)
				world.CancelInputMode();

			var ret = InnerOrder(world, cell, mi).ToArray();

			// If there was a successful placement order
			if (ret.Any(o => o.OrderString == "BuildUnitPlaceBuilding"))
				world.CancelInputMode();

			return ret;
		}

		IEnumerable<Order> InnerOrder(World world, CPos cell, MouseInput mi)
		{
			if (world.Paused)
				yield break;

			var owner = queue.Actor.Owner;
			if (mi.Button == MouseButton.Left)
			{
				var orderType = "BuildUnitPlaceBuilding";
				var topLeft = viewport.ViewToWorld(Viewport.LastMousePos + topLeftScreenOffset);

				if (!world.CanPlaceBuilding(topLeft, actorInfo, buildingInfo, queue.Actor))
				{
					foreach (var order in ClearBlockersOrders(world, topLeft))
						yield return order;

					Game.Sound.PlayNotification(world.Map.Rules, owner, "Speech", "BuildingCannotPlaceAudio", owner.Faction.InternalName);
					yield break;
				}

				var target = topLeft + (buildingInfo.Dimensions / 2);

				yield return new Order(orderType, owner.PlayerActor, Target.FromCell(world, target), false)
				{
					TargetString = actorInfo.Name,
					ExtraLocation = topLeft,
					ExtraData = queue.Actor.ActorID,
					SuppressVisualFeedback = true
				};
			}
		}

		public void Tick(World world)
		{
			if (queue == null)
				world.CancelInputMode();

			if (preview == null)
				return;

			foreach (var p in preview)
				p.Tick();
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr, World world) { yield break; }
		public IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr, World world)
		{
			var topLeft = viewport.ViewToWorld(Viewport.LastMousePos + topLeftScreenOffset);
			var centerPosition = world.Map.CenterOfCell(topLeft) + centerOffset;
			var rules = world.Map.Rules;

			foreach (var dec in actorInfo.TraitInfos<IPlaceBuildingDecorationInfo>())
				foreach (var r in dec.RenderAnnotations(wr, world, actorInfo, centerPosition))
					yield return r;

			var cells = new Dictionary<CPos, CellType>();

			if (!initialized)
			{
				var td = new TypeDictionary()
				{
					new FactionInit(faction),
					new OwnerInit(queue.Actor.Owner),
				};

				foreach (var api in actorInfo.TraitInfos<IActorPreviewInitInfo>())
					foreach (var o in api.ActorPreviewInits(actorInfo, ActorPreviewType.PlaceBuilding))
						td.Add(o);

				var init = new ActorPreviewInitializer(actorInfo, wr, td);
				preview = actorInfo.TraitInfos<IRenderActorPreviewInfo>()
					.SelectMany(rpi => rpi.RenderPreview(init))
					.ToArray();

				initialized = true;
			}

			var previewRenderables = preview
				.SelectMany(p => p.Render(wr, centerPosition))
				.OrderBy(WorldRenderer.RenderableScreenZPositionComparisonKey);

			foreach (var r in previewRenderables)
				yield return r;

			var res = world.WorldActor.TraitOrDefault<ResourceLayer>();
			var isCloseEnough = buildingInfo.IsCloseEnoughToBase(world, world.LocalPlayer, actorInfo, topLeft);
			foreach (var t in buildingInfo.Tiles(topLeft))
				cells.Add(t, MakeCellType(isCloseEnough && world.IsCellBuildable(t, actorInfo, buildingInfo) && (res == null || res.GetResource(t).Density == 0)));

			var cellPalette = wr.Palette(footprintPlaceBuildingPreviewInfo.Palette);
			var linePalette = wr.Palette(footprintPlaceBuildingPreviewInfo.LineBuildSegmentPalette);
			var topLeftPos = world.Map.CenterOfCell(topLeft);
			foreach (var c in cells)
			{
				var tile = !c.Value.HasFlag(CellType.Invalid) ? buildOk : buildBlocked;
				var pal = c.Value.HasFlag(CellType.LineBuild) ? linePalette : cellPalette;
				var pos = world.Map.CenterOfCell(c.Key);
				yield return new SpriteRenderable(tile, pos, new WVec(0, 0, topLeftPos.Z - pos.Z),
					-511, pal, 1f, true);
			}
		}

		public IEnumerable<IRenderable> RenderAnnotations(WorldRenderer wr, World world)
		{
			return null;
		}

		public string GetCursor(World world, CPos cell, int2 worldPixel, MouseInput mi) { return "default"; }

		// Copied from PlaceBuildingOrderGenerator, triplicated in BuildOnSite and BuilderUnitBuildingOrderGenerator
		IEnumerable<Order> ClearBlockersOrders(World world, CPos topLeft)
		{
			var allTiles = buildingInfo.Tiles(topLeft).ToArray();
			var neightborTiles = Common.Util.ExpandFootprint(allTiles, true).Except(allTiles)
				.Where(world.Map.Contains).ToList();

			var blockers = allTiles.SelectMany(world.ActorMap.GetActorsAt)
				.Where(a => a.Owner == queue.Actor.Owner && a.IsIdle)
				.Select(a => new TraitPair<Mobile>(a, a.TraitOrDefault<Mobile>()));

			foreach (var blocker in blockers.Where(x => x.Trait != null))
			{
				var availableCells = neightborTiles.Where(t => blocker.Trait.CanEnterCell(t)).ToList();
				if (availableCells.Count == 0)
					continue;

				yield return new Order("Move", blocker.Actor, Target.FromCell(world, blocker.Actor.ClosestCell(availableCells)), false)
				{
					SuppressVisualFeedback = true
				};
			}
		}

		public void Deactivate()
		{
			// throw new NotImplementedException();
		}

		public bool HandleKeyPress(KeyInput e)
		{
			// throw new NotImplementedException();
			return true;
		}
	}
}
