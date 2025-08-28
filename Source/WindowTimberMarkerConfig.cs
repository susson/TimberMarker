using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace TimberMarker
{
    // 自定义对话框
    public class WindowTimberMarkerConfig : Window
    {
        private CompTimberMarker comp;
        // 状态字段（可根据需要改为从组件/ModSettings读取）
        private bool enabled;
        private int maintainCount;
        private int searchRadius;
        private float minGrowth;

        public WindowTimberMarkerConfig(CompTimberMarker comp)
        {
            this.comp = comp ?? throw new ArgumentNullException(nameof(comp));

            enabled = comp.Enabled;
            maintainCount = comp.MaintainCount;
            searchRadius = comp.SearchRadius;
            minGrowth = comp.MinGrowth;

            // 窗口行为
            doCloseX = true;                  // 右上角 X
            doCloseButton = true;             // 底部 Close
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(400f, 300f);

        public override void DoWindowContents(Rect inRect)
        {
            const float pad = 8f;
            var rect = inRect.ContractedBy(pad);

            Listing_Standard ls = new Listing_Standard();
            ls.Begin(rect);

            float rowHeight = 30f; // 每行高度

            // 第 1 行：任务运行中 ✔/❌
            var row1 = ls.GetRect(rowHeight);
            var left1 = new Rect(row1.x, row1.y, row1.width * 0.4f, rowHeight);
            var right1 = new Rect(left1.xMax, row1.y, row1.width * 0.6f, rowHeight);

            // Widgets.Label(left1, "任务运行中:");
            Widgets.Label(left1, "TimberMarker.TaskConfigWindow.Enabled.Label".Translate());
            bool running = enabled;
            Widgets.Checkbox(right1.x, right1.y + 5f, ref running, rowHeight - 10f); // 小偏移让复选框居中
            if (enabled != running)
            {
                enabled = running;
                comp.Enabled = enabled;
            }
            ls.Gap(6f);

            // 第 2 行：维持数量
            var row2 = ls.GetRect(rowHeight);
            var left2 = new Rect(row2.x, row2.y, row2.width * 0.4f, rowHeight);
            var right2 = new Rect(left2.xMax, row2.y, row2.width * 0.6f, rowHeight);

            // Widgets.Label(left2, "维持数量:");
            Widgets.Label(left2, "TimberMarker.TaskConfigWindow.MaintainCount.Label".Translate());
            string countText = Widgets.TextField(right2, maintainCount.ToString());
            if (int.TryParse(countText, out int parsedCount))
            {
                parsedCount = Mathf.Clamp(parsedCount, 0, 9999);
                if (parsedCount != maintainCount)
                {
                    maintainCount = parsedCount;
                    comp.MaintainCount = maintainCount;
                }
            }
            ls.Gap(6f);

            // 第 3 行：搜索半径
            var row3 = ls.GetRect(rowHeight);
            var left3 = new Rect(row3.x, row3.y, row3.width * 0.4f, rowHeight);
            var right3 = new Rect(left3.xMax, row3.y, row3.width * 0.6f, rowHeight);

            Widgets.Label(left3, "IngredientSearchRadius".Translate());
            string radiusText = Widgets.TextField(right3, searchRadius.ToString());
            if (int.TryParse(radiusText, out int parsedRadius))
            {
                parsedRadius = Mathf.Clamp(parsedRadius, 0, 500);
                if (parsedRadius != searchRadius)
                {
                    searchRadius = parsedRadius;
                    comp.SearchRadius = searchRadius;
                }
            }
            ls.Gap(6f);

            // 第 4 行：最小生长程度 滑块
            var row4 = ls.GetRect(rowHeight);
            var left4 = new Rect(row4.x, row4.y, row4.width * 0.4f, rowHeight);
            var right4 = new Rect(left4.xMax, row4.y, row4.width * 0.6f, rowHeight);

            // Widgets.Label(left4, "最小生长程度:");
            Widgets.Label(left4, "TimberMarker.TaskConfigWindow.MinGrowth.Label".Translate());
            float oldGrowth = minGrowth;
            minGrowth = Widgets.HorizontalSlider(
                right4,
                minGrowth,
                0f,
                1f,
                middleAlignment: false,
                label: $"{Mathf.RoundToInt(minGrowth * 100)}%"
            );
            if (!Mathf.Approximately(oldGrowth, minGrowth))
            {
                comp.MinGrowth = minGrowth;
            }
            ls.Gap(10f);

            ls.End();
        }
    }
}