using RimWorld;
using System;
using Verse;

namespace TimberMarker
{
    // 组件本体：保存/加载数据
    public class CompTimberMarker : ThingComp
    {
        public bool Enabled = true;
        public int MaintainCount = 100;
        public int SearchRadius = 20;
        public float MinGrowth = 1f;

        // ThingComp 推荐重写 PostExposeData 来序列化
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref Enabled, "TM_Enabled");
            Scribe_Values.Look(ref MaintainCount, "TM_MaintainCount");
            Scribe_Values.Look(ref SearchRadius, "TM_SearchRadius");
            Scribe_Values.Look(ref MinGrowth, "TM_MinGrowth", 1f); // 默认值为 1.0
        }

        public override void CompTickLong()
        {
            //Log.Message("[TimberMarker] ticker");
            base.CompTickLong();

            if (!Enabled) return;
            if (parent?.Map == null) return;

            var map = parent.Map;

            if (!TryGetWoodCount(map, out int woodCount)) return;
            if (woodCount >= MaintainCount) return;

            var center = parent.Position;
            var (targetTree, foundDesignated) = FindClosestMatureTreeOrDetectDesignated(map, center, SearchRadius, MinGrowth);

            // 如果在搜索过程中发现有人已标记树为砍伐，按你的需求直接返回
            if (foundDesignated) return;
            if (targetTree == null) return;

            // 再次确认（竞态保护）后添加 designation
            if (map.designationManager.DesignationOn(targetTree) != null) return;

            try
            {
                map.designationManager.AddDesignation(new Designation(targetTree, DesignationDefOf.CutPlant));
                 //Log.Message($"[TimberMarker] 标记砍伐: {targetTree} at {targetTree.Position} by {parent.LabelShort}");
            }
            catch (Exception ex)
            {
                Log.Error($"[TimberMarker] 标记砍伐失败: {ex}");
            }
        }

        private bool TryGetWoodCount(Map map, out int count)
        {
            count = 0;
            if (map?.resourceCounter == null) return false;
            try
            {
                count = map.resourceCounter.GetCount(ThingDefOf.WoodLog);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 在半径内寻找最近且成熟的树；如果在搜索中遇到任意一棵已经被标记为 CutPlant，则返回 foundDesignated=true（caller 会直接 return）。
        /// 返回 (targetThing, foundDesignated)
        /// </summary>
        private (Thing target, bool foundDesignated) FindClosestMatureTreeOrDetectDesignated(Map map, IntVec3 center, int radius, float minGrowth)
        {
            var plants = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant);
            if (plants == null || plants.Count == 0) return (null, false);

            int radiusSq = radius * radius;
            Thing best = null;
            int bestDistSq = int.MaxValue;

            for (int i = 0; i < plants.Count; i++)
            {
                var thing = plants[i];
                if (!(thing is Plant p)) continue;
                if (p.def.plant == null || !p.def.plant.IsTree) continue;

                // 如果发现任何树已被标记为砍伐，直接返回指示 caller 退出
                var des = map.designationManager.DesignationOn(thing);
                if (des != null && des.def == DesignationDefOf.CutPlant)
                {
                    return (null, true);
                }

                if (p.Growth < minGrowth) continue;

                int dx = thing.Position.x - center.x;
                int dz = thing.Position.z - center.z;
                int distSq = dx * dx + dz * dz;
                if (distSq > radiusSq) continue;

                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = thing;
                }
            }

            return (best, false);
        }
    }

}