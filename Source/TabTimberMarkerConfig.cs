using RimWorld;
using UnityEngine;
using Verse;

namespace TimberMarker
{
    // 把原来的 Window 改为 Tab 面板
    public class ITab_TimberMarkerConfig : ITab
    {
        private Vector2 scroll;
        private const float TabWidth = 420f;
        private const float TabHeight = 360f;

        public ITab_TimberMarkerConfig()
        {
            this.size = new Vector2(TabWidth, TabHeight);
            this.labelKey = "TimberMarker.TaskConfig.Label".Translate();
        }

        private CompTimberMarker? SelectedComp
        {
            get
            {
                var twc = this.SelThing as ThingWithComps;
                return twc?.GetComp<CompTimberMarker>();
            }
        }

        public override void FillTab()
        {
            var comp = SelectedComp;
            if (comp == null)
            {
                Widgets.Label(new Rect(0f, 0f, 300f, 30f), "TimberMarker.NoComp".Translate());
                return;
            }

            const float pad = 16f;
            Rect outRect = new Rect(0f, 10f, this.size.x, this.size.y).ContractedBy(pad);
            // 视图高度留足够位置（根据控件多少调整）
            float viewHeight = 260f;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            float rowHeight = 30f; // 每行高度
            float left = 0.4f; // 左侧标签宽度比例
            float right = 0.6f; // 右侧控件宽度比例

            Widgets.BeginScrollView(outRect, ref scroll, viewRect);
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(viewRect);

            // 第 1 行：任务开启
            ls.CheckboxLabeled("TimberMarker.TaskConfigWindow.Enabled.Label".Translate(),
                ref comp.Enabled,
                "TimberMarker.TaskConfigWindow.Enabled.Desc".Translate());
            ls.Gap(6f);

            // 第 2 行：维持库存
            var row2 = ls.GetRect(rowHeight);
            var left2 = new Rect(row2.x, row2.y, row2.width * left, rowHeight);
            var right2 = new Rect(left2.xMax, row2.y, row2.width * right, rowHeight);

            Widgets.Label(left2, "TimberMarker.TaskConfigWindow.MaintainCount.Label".Translate());
            string countText = Widgets.TextField(right2, comp.MaintainCount.ToString());
            if (int.TryParse(countText, out int parsedCount))
            {
                parsedCount = Mathf.Clamp(parsedCount, 0, 9999);
                if (parsedCount != comp.MaintainCount)
                {
                    comp.MaintainCount = parsedCount;
                }
            }
            ls.Gap(6f);

            // 第 3 行：搜索半径
            var row3 = ls.GetRect(rowHeight);
            var left3 = new Rect(row3.x, row3.y, row3.width * left, rowHeight);
            var right3 = new Rect(left3.xMax, row3.y, row3.width * right, rowHeight);

            Widgets.Label(left3, "IngredientSearchRadius".Translate());

            // 初始化滑块值：comp.SearchRadius == 0 表示无限制 -> 使用 101 表示滑块的“无限制”位置
            float sliderValue = comp.SearchRadius == 0 ? 101f : Mathf.Clamp(comp.SearchRadius, 3, 100);

            // 绘制滑块，右端表示“无限制”
            sliderValue = Widgets.HorizontalSlider(
                right3,
                sliderValue,
                3f,
                101f,
                middleAlignment: false,
                label: sliderValue >= 101f ? "无限制" : Mathf.RoundToInt(sliderValue).ToString()
            );

            // 将滑块值写回 comp（滑到 101 则写 0 表示无限制）
            int newRadius = (sliderValue >= 101f) ? 0 : Mathf.RoundToInt(Mathf.Clamp(sliderValue, 3f, 100f));
            if (newRadius != comp.SearchRadius)
            {
                comp.SearchRadius = newRadius;
            }

            ls.Gap(6f);

            // 第 4 行：最小生长程度 滑块
            var row4 = ls.GetRect(rowHeight);
            var left4 = new Rect(row4.x, row4.y, row4.width * left, rowHeight);
            var right4 = new Rect(left4.xMax, row4.y, row4.width * right, rowHeight);

            Widgets.Label(left4, "TimberMarker.TaskConfigWindow.MinGrowth.Label".Translate());
            float oldGrowth = comp.MinGrowth;
            comp.MinGrowth = Widgets.HorizontalSlider(
                right4,
                comp.MinGrowth,
                0f,
                1f,
                middleAlignment: false,
                label: $"{Mathf.RoundToInt(comp.MinGrowth * 100)}%"
            );
            ls.Gap(6f);

            // 应用按钮（仅做提示/反馈）
            //if (ls.ButtonText("TimberMarker.TaskConfigWindow.ApplyButton".Translate()))
            //{
            //    // 触发提示并尝试调用 Comp 的回调（若存在）
            //    var mi = comp.GetType().GetMethod("Notify_SettingsChanged", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            //    mi?.Invoke(comp, null);
            //    Messages.Message("TimberMarker.TaskConfigWindow.AppliedMessage".Translate(), MessageTypeDefOf.PositiveEvent);
            //}

            ls.End();
            Widgets.EndScrollView();
        }
    }
}
