using System;
using UnityEngine;

namespace PantheonsHitCounter.UI
{
    public class CounterUI
    {
        private static CanvasPanel _panel;
        private static bool _compactMode;
        // Default
        private const string EmptyImage = "Empty";
        private const string BackgroundTopImage = "BackgroundTop";
        private const string BackgroundBossImage = "BackgroundBoss";
        private const string BackgroundBottomImage = "BackgroundBottom";
        private const string SelectedBossImage = "SelectedBoss";
        private const string SplitImage = "Split";
        private const float BackgroundWidth = 360f;
        private const float TitleHeight = 60f;
        private const float BossHeight = 80f;
        private const float BossImageSize = 55f;
        private const float BossHitsWidth = 120f;
        private const float Margin = 25f;
        private const float Center = BossHeight - 1.7f * Margin;
        private const int FontSizeNormal = 20;
        private const int FontSizeSmall = 16;
        private const int FontSizeMini = 14;
        // Compact mode
        private const string SelectedBossCompactImage = "SelectedBossCompact";
        private const string BackgroundTopCompactImage = "BackgroundTopCompact";
        private const string BackgroundBossCompactImage = "BackgroundBossCompact";
        private const string BackgroundBottomCompactImage = "BackgroundBottomCompact";
        private const string SplitCompactImage = "SplitCompact";
        private const float TitleHeightCompact = 60f;
        private const float BossHeightCompact = 30f;
        private const float CenterCompact = 0.32f * BossHeightCompact;
        
        public static void BuildMenu(GameObject canvas, Pantheon pantheon)
        {
            if (PantheonsHitCounter.instance.globalData.compactMode) BuildCompactUI(canvas, pantheon);
            else BuildDefaultUI(canvas, pantheon);

            _panel.FixRenderOrder();
            UpdateUI(pantheon);
        }

        private static void BuildDefaultUI(GameObject canvas, Pantheon pantheon)
        {
            // Background
            var backgroundTop = ResourcesLoader.Instance.images[BackgroundTopImage];
            var backgroundBoss = ResourcesLoader.Instance.images[BackgroundBossImage];
            var backgroundBottom = ResourcesLoader.Instance.images[BackgroundBottomImage];
            _panel = new CanvasPanel(canvas, backgroundTop, new Vector2(1920 - backgroundTop.width, 0), Vector2.zero, new Rect(0, 0, backgroundTop.width, backgroundTop.height));
            // Title
            _panel.AddPanel("PHC-Title", null, new Vector2(0, TitleHeight), Vector2.zero, new Rect(0, 0, BackgroundWidth, TitleHeight));
            var titlePanel = _panel.GetPanel("PHC-Title");
            titlePanel.AddText("PantheonName", "-", new Vector2(1.2f * Margin, 0), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeNormal);
            titlePanel.AddText("PantheonBoss", "-/-", new Vector2(1.2f * Margin, TitleHeight - 1.2f * Margin), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeNormal);
            // Bosss Boss
            var bossImg = ResourcesLoader.Instance.images[SplitImage];
            var selectedBossImg = ResourcesLoader.Instance.images[SelectedBossImage];
            var emptyImg = ResourcesLoader.Instance.images[EmptyImage];
            var max = Math.Min(PantheonsHitCounter.instance.globalData.compactMode ? PantheonsHitCounter.instance.globalData.totalSplits : Math.Min(PantheonsHitCounter.instance.globalData.totalSplits, 10), pantheon.bosses.Count);
            for (var b = 0; b < max; b++)
            {
                var bossHeight = 2 * TitleHeight + b * BossHeight;
                _panel.AddPanel("PHC-Boss-" + b, null, new Vector2(Margin, bossHeight), Vector2.zero, new Rect(0, TitleHeight, BackgroundWidth, BossHeight));
                var bossPanel = _panel.GetPanel("PHC-Boss-" + b);
                bossPanel.AddImage("BossBorderImage", bossImg, Vector2.zero, new Vector2(bossImg.width, bossImg.height), new Rect(0, 0, bossImg.width, bossImg.height));
                bossPanel.AddImage("BossImage", emptyImg, new Vector2(Margin, 0.8f * Margin), new Vector2(BossImageSize, BossImageSize), new Rect(0, 0, emptyImg.width, emptyImg.height));
                bossPanel.AddText("BossName", "", new Vector2(BossImageSize + 1.3f * Margin, Margin), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeSmall);
                bossPanel.AddText("BossHits", "", new Vector2(BackgroundWidth - BossHitsWidth - Margin, BossHeight - Margin), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeNormal);
                bossPanel.AddText("BossHitsPB", "", new Vector2(BackgroundWidth - BossHitsWidth + Margin, BossHeight - Margin), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeNormal);
                bossPanel.AddImage("BossSelected", selectedBossImg, new Vector2(-Margin, 0), new Vector2(selectedBossImg.width, selectedBossImg.height), new Rect(0, 0, selectedBossImg.width, selectedBossImg.height));
                bossPanel.AddImage("BossBackground-" + b, backgroundBoss, new Vector2(-Margin, 0), new Vector2(backgroundBoss.width, backgroundBoss.height), new Rect(0, 0, backgroundBoss.width, backgroundBoss.height));
            }
            // Total panel
            var maxHeight = 2 * TitleHeight + max * BossHeight;
            _panel.AddPanel("PHC-Total", null, new Vector2(Margin, maxHeight), Vector2.zero, new Rect(0, TitleHeight, BackgroundWidth, BossHeight));
            var totalPanel = _panel.GetPanel("PHC-Total");
            totalPanel.AddImage("TotalBossImage", bossImg, Vector2.zero, new Vector2(bossImg.width, bossImg.height), new Rect(0, 0, bossImg.width, bossImg.height));
            totalPanel.AddText("TotalText", "Total", new Vector2(1.5f * Margin, Center), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeNormal);
            totalPanel.AddText("TotalHits", "-", new Vector2(BackgroundWidth - BossHitsWidth - Margin, Center), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeNormal);
            totalPanel.AddText("TotalHitsPB", "-", new Vector2(BackgroundWidth - BossHitsWidth + Margin, Center), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeNormal);
            totalPanel.AddImage("BackgroundBottom", backgroundBottom, new Vector2(-Margin, 0), new Vector2(backgroundBottom.width, backgroundBottom.height), new Rect(0, 0, backgroundBottom.width, backgroundBottom.height));

            _compactMode = false;
        }

        private static void BuildCompactUI(GameObject canvas, Pantheon pantheon)
        {
            // Background
            var backgroundTop = ResourcesLoader.Instance.images[BackgroundTopCompactImage];
            var backgroundBoss = ResourcesLoader.Instance.images[BackgroundBossCompactImage];
            var backgroundBottom = ResourcesLoader.Instance.images[BackgroundBottomCompactImage];
            _panel = new CanvasPanel(canvas, backgroundTop, new Vector2(1920 - backgroundTop.width, 0), Vector2.zero, new Rect(0, 0, backgroundTop.width, backgroundTop.height));
            // Title
            _panel.AddPanel("PHC-Title", null, new Vector2(BossHeightCompact, TitleHeightCompact - BossHeightCompact), Vector2.zero, new Rect(0, 0, BackgroundWidth - 2 * BossHeightCompact, TitleHeightCompact));
            var titlePanel = _panel.GetPanel("PHC-Title");
            titlePanel.AddText("PantheonName", "-", new Vector2(CenterCompact, 0), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeSmall);
            titlePanel.AddText("PantheonBoss", "-/-", new Vector2(BackgroundWidth - BossHitsWidth, 0), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeSmall);
            var breakImg = ResourcesLoader.Instance.images[SplitCompactImage];
            titlePanel.AddImage("TitleBreak", breakImg, new Vector2(BossHeightCompact, BossHeightCompact - CenterCompact), new Vector2(breakImg.width, breakImg.height), new Rect(0, 0, breakImg.width, breakImg.height));
            var emptyImg = ResourcesLoader.Instance.images[EmptyImage];
            var selectedBossImg = ResourcesLoader.Instance.images[SelectedBossCompactImage];
            var max = Math.Min(PantheonsHitCounter.instance.globalData.totalSplits, pantheon.bosses.Count);
            for (var b = 0; b < max; b++)
            {
                _panel.AddPanel("PHC-Boss-" + b, null, new Vector2(BossHeightCompact, b * BossHeightCompact + TitleHeightCompact), Vector2.zero, new Rect(0, 0, BackgroundWidth - 2 * BossHeightCompact, BossHeightCompact));
                var bossPanel = _panel.GetPanel("PHC-Boss-" + b);
                bossPanel.AddText("BossName", "", new Vector2(BossHeightCompact + CenterCompact, CenterCompact), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeMini);
                bossPanel.AddText("BossHits", "", new Vector2(BackgroundWidth - BossHitsWidth, CenterCompact), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeSmall);
                bossPanel.AddText("BossHitsPB", "", new Vector2(BackgroundWidth - BossHitsWidth + BossHeightCompact, CenterCompact), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeSmall);
                bossPanel.AddImage("BossImage", emptyImg, Vector2.zero, new Vector2(BossHeightCompact, BossHeightCompact), new Rect(0, 0, emptyImg.width, emptyImg.height));
                bossPanel.AddImage("BossSelected", selectedBossImg, new Vector2(- 1.5f * BossHeightCompact, 0), new Vector2(selectedBossImg.width, selectedBossImg.height), new Rect(0, 0, selectedBossImg.width, selectedBossImg.height));
                bossPanel.AddImage("BossBackground-" + b, backgroundBoss, new Vector2(-BossHeightCompact, 0), new Vector2(backgroundBoss.width, backgroundBoss.height), new Rect(0, 0, backgroundBoss.width, backgroundBoss.height));
            }
            // Total panel
            _panel.AddPanel("PHC-Total", null, new Vector2(BossHeightCompact, max * BossHeightCompact + TitleHeightCompact), Vector2.zero, new Rect(0, 0, BackgroundWidth - 2 * BossHeightCompact, BossHeightCompact));
            var totalPanel = _panel.GetPanel("PHC-Total");
            totalPanel.AddText("TotalText", "Total", new Vector2(BossHeightCompact - CenterCompact, 2 * CenterCompact), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeSmall);
            totalPanel.AddText("TotalHits", "-", new Vector2(BackgroundWidth - BossHitsWidth, 2 * CenterCompact), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeSmall);
            totalPanel.AddText("TotalHitsPB", "-", new Vector2(BackgroundWidth - BossHitsWidth + BossHeightCompact, 2 * CenterCompact), Vector2.zero, Modding.CanvasUtil.TrajanBold, FontSizeSmall);
            totalPanel.AddImage("TotalBreak", breakImg, new Vector2(BossHeightCompact, 0.17f * CenterCompact), new Vector2(breakImg.width, breakImg.height), new Rect(0, 0, breakImg.width, breakImg.height));
            totalPanel.AddImage("BackgroundBottom", backgroundBottom, new Vector2(-BossHeightCompact, 0), new Vector2(backgroundBottom.width, backgroundBottom.height), new Rect(0, 0, backgroundBottom.width, backgroundBottom.height));

            _compactMode = true;
        }

        public static void UpdateUI(Pantheon pantheon)
        {
            if (_panel == null) return;

            if (_compactMode) UpdateCompactModeUI(pantheon);
            else UpdateDefaultUI(pantheon);
        }

        private static void UpdateDefaultUI(Pantheon pantheon)
        {
            // Title
            var bosses = pantheon.bosses;
            var titlePanel = _panel.GetPanel("PHC-Title");
            titlePanel.GetText("PantheonName").UpdateText(pantheon.name);
            titlePanel.GetText("PantheonBoss").UpdateText($"{pantheon.bossNumber + 1}/{bosses.Count}");
            // Bosss
            var max = Math.Min(PantheonsHitCounter.instance.globalData.compactMode ? PantheonsHitCounter.instance.globalData.totalSplits : Math.Min(PantheonsHitCounter.instance.globalData.totalSplits, 10), bosses.Count);
            for (var b = 0; b < max; b++)
            {
                var bossNumberNumber = GetBossNumber(pantheon.bossNumber, bosses.Count, max);
                var bossPanel = _panel.GetPanel("PHC-Boss-" + b);
                Boss boss = null;
                if (b + bossNumberNumber < pantheon.bosses.Count) boss = bosses[b + bossNumberNumber];
                Texture2D bossImg = null;
                if (boss != null) bossImg = ResourcesLoader.Instance.images[boss.name.Replace(" ", "_")];
                if (bossImg != null) bossPanel.GetImage("BossImage").UpdateImage(bossImg, new Rect(0, 0, bossImg.width, bossImg.height));
                bossPanel.GetImage("BossImage").SetActive(bossImg != null && _panel.Active);
                bossPanel.GetText("BossName").UpdateText(boss != null ? boss.name : "");
                bossPanel.GetText("BossHits").UpdateText(boss != null ? boss.hits + "" : "");
                bossPanel.GetText("BossHitsPB").UpdateText(boss != null ? boss.hitsPb < 0 ? "-" : boss.hitsPb + "" : "");
                bossPanel.GetImage("BossSelected").SetActive(b == GetSelectedBoss(pantheon.bossNumber, bosses.Count, max) && _panel.Active);
            }
            // Total
            _panel.GetPanel("PHC-Total").GetText("TotalHits").UpdateText(pantheon.TotalHits + "");
            _panel.GetPanel("PHC-Total").GetText("TotalHitsPB").UpdateText(pantheon.TotalHitsPb < 0 ? "-" : pantheon.TotalHitsPb + "");
        }

        private static void UpdateCompactModeUI(Pantheon pantheon)
        {
            // Title
            var bosses = pantheon.bosses;
            var titlePanel = _panel.GetPanel("PHC-Title");
            titlePanel.GetText("PantheonName").UpdateText(pantheon.name);
            titlePanel.GetText("PantheonBoss").UpdateText($"{pantheon.bossNumber + 1}/{bosses.Count}");
            // Bosss
            var max = Math.Min(PantheonsHitCounter.instance.globalData.totalSplits, bosses.Count);
            for (var b = 0; b < max; b++)
            {
                var bossNumberNumber = GetBossNumber(pantheon.bossNumber, bosses.Count, max);
                var bossPanel = _panel.GetPanel("PHC-Boss-" + b);
                Boss boss = null;
                if (b + bossNumberNumber < pantheon.bosses.Count) boss = bosses[b + bossNumberNumber];
                Texture2D bossImg = null;
                if (boss != null) bossImg = ResourcesLoader.Instance.images[boss.name.Replace(" ", "_")];
                if (bossImg != null) bossPanel.GetImage("BossImage").UpdateImage(bossImg, new Rect(0, 0, bossImg.width, bossImg.height));
                bossPanel.GetImage("BossImage").SetActive(bossImg != null && _panel.Active);
                bossPanel.GetText("BossName").UpdateText(boss != null ? boss.name : "");
                bossPanel.GetText("BossHits").UpdateText(boss != null ? boss.hits + "" : "");
                bossPanel.GetText("BossHitsPB").UpdateText(boss != null ? boss.hitsPb < 0 ? "-" : boss.hitsPb + "" : "");
                bossPanel.GetImage("BossSelected").SetActive(b == GetSelectedBoss(pantheon.bossNumber, bosses.Count, max) && _panel.Active);
            }
            // Total
            var totalPanel = _panel.GetPanel("PHC-Total");
            totalPanel.GetText("TotalHits").UpdateText(pantheon.TotalHits + "");
            totalPanel.GetText("TotalHitsPB").UpdateText(pantheon.TotalHitsPb < 0 ? "-" : pantheon.TotalHitsPb + "");
        }

        private static int GetBossNumber(int bossNumber, int splitsCount, int max)
        {
            var totalLength = Math.Min(max, splitsCount);
            var endLength = totalLength / 2;
            var startLength = totalLength - endLength - 1;

            if (bossNumber <= startLength) return 0;
            if (bossNumber >= splitsCount - endLength - 1) return splitsCount - totalLength;
            return bossNumber - startLength;
        }
        
        private static int GetSelectedBoss(int bossNumber, int splitsCount, int max)
        {
            var totalLength = Math.Min(max, splitsCount);
            var endLength = totalLength / 2;
            var startLength = totalLength - endLength - 1;
            var middleLength = splitsCount - startLength - endLength;

            if (bossNumber < startLength) return bossNumber;
            if (bossNumber >= splitsCount - endLength) return bossNumber - Math.Max(0, middleLength - 1);
            return startLength;
        }

        public static void Toggle(Pantheon currentCounter)
        {
            if (_panel == null) return;
            _panel.TogglePanel();
            if (_panel.Active) UpdateUI(currentCounter);
        }

        public static void Destroy()
        {
            _panel?.Destroy();
        }
    }
}