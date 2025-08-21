using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TimberMarker
{
    public class BuildingTimberMarkerSpot : Building
    {
        // 保存和加载数据
        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos()) yield return g;

            // 下方按钮栏中添加一个新的 Gizmo(按钮)，用于打开清单面板
            yield return new Command_Action
            {
                //defaultLabel = "任务设置",
                defaultLabel = "TimberMarker.TaskConfig.Label".Translate(),
                defaultDesc = "TimberMarker.TaskConfig.Desc".Translate(),
                icon = TexCommand.Replant, // 可替换为更合适的图标
                action = () =>
                {
                    var comp = this.GetComp<CompTimberMarker>();
                    if (comp == null)
                    {
                        Log.Error("[TimberMarker] Building missing CompTimberMarker!");
                        return;
                    }

                    var w = new WindowTimberMarkerConfig(comp);
                    Find.WindowStack.Add(w);
                }
            };
        }
    }
}