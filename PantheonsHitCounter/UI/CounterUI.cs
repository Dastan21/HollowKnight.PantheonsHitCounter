using System.Linq;
using UnityEngine;

namespace PantheonsHitCounter.UI
{
    public class CounterUI
    {
        private static CanvasPanel _panel;
        // Images
        private const string BackgroundImage = "Background";
        private const string SplitBossImage = "SplitBoss";
        private const string SelectedBossImage = "SelectedBoss";
        private const string SelectedSplitImage = "SelectedSplit";
        private const string AnonymizedBossImage = "AnonymizedBoss";
        // Dimensions
        private const float BackgroundWidth = 360f;
        private const float TitleHeight = 60f;
        private const float SplitHeight = 80f;
        private const float BossImageSize = 55f;
        private const float BossHitsWidth = 115f;
        private const float Margin = 25f;
        
        public static void BuildMenu(GameObject canvas, Pantheon pantheon)
        {
            var bosses = pantheon.bosses.Where(boss => !boss.disabled).ToList();
            
            // Background
            var background = ResourcesLoader.Instance.images[BackgroundImage];
            _panel = new CanvasPanel(canvas, background, new Vector2(1920f - background.width, 0), Vector2.zero, new Rect(0, 0, background.width, background.height));
            // Title
            _panel.AddPanel("PHC-Title", null, new Vector2(0, TitleHeight), Vector2.zero, new Rect(0, 0, BackgroundWidth, TitleHeight));
            var titlePanel = _panel.GetPanel("PHC-Title");
            titlePanel.AddText("PantheonName", pantheon.name, new Vector2(1.2f * Margin, 0), Vector2.zero, ResourcesLoader.Instance.trajanBold, 20);
            titlePanel.AddText("PantheonBoss", $"-/-", new Vector2(1.2f * Margin, TitleHeight - 1.2f * Margin), Vector2.zero, ResourcesLoader.Instance.trajanBold, 20);
            // Bosses Split
            var splitImg = ResourcesLoader.Instance.images[SplitBossImage];
            var selectedBossImg = ResourcesLoader.Instance.images[SelectedBossImage];
            var selectedSplitImg = ResourcesLoader.Instance.images[SelectedSplitImage];
            var anonymizedBossImg = ResourcesLoader.Instance.images[AnonymizedBossImage];
            for (var b = 0; b < 4; b++)
            {
                _panel.AddPanel("PHC-BossSplit-" + b, null, new Vector2(Margin, 2 * TitleHeight + b * SplitHeight), Vector2.zero, new Rect(0, TitleHeight, BackgroundWidth, SplitHeight));
                var bossPanel = _panel.GetPanel("PHC-BossSplit-" + b);
                bossPanel.AddImage("BossSplitImage-" + b, splitImg, new Vector2(0, 0), new Vector2(splitImg.width, splitImg.height), new Rect(0, 0, splitImg.width, splitImg.height));
                var bossImg = ResourcesLoader.Instance.images[bosses[b + pantheon.bossNumber].name.Replace(" ", "_")];
                bossPanel.AddImage("BossImageSelected", selectedBossImg, new Vector2(Margin, 0.8f * Margin), new Vector2(BossImageSize, BossImageSize), new Rect(0, 0, selectedBossImg.width, selectedBossImg.height));
                bossPanel.AddImage("BossImageAnonymized", anonymizedBossImg, new Vector2(Margin, 0.8f * Margin), new Vector2(BossImageSize, BossImageSize), new Rect(0, 0, anonymizedBossImg.width, anonymizedBossImg.height));
                bossPanel.AddImage("BossImage", bossImg, new Vector2(Margin, 0.8f * Margin), new Vector2(BossImageSize, BossImageSize), new Rect(0, 0, bossImg.width, bossImg.height));
                bossPanel.AddText("BossName", "-", new Vector2(BossImageSize + 1.3f * Margin, Margin), Vector2.zero, ResourcesLoader.Instance.trajanBold, 16);
                bossPanel.AddText("BossHits", "-", new Vector2(BackgroundWidth - BossHitsWidth - Margin, SplitHeight - Margin), Vector2.zero, ResourcesLoader.Instance.trajanBold, 20);
                bossPanel.AddText("BossHitsPB", "-", new Vector2(BackgroundWidth - BossHitsWidth + Margin, SplitHeight - Margin), Vector2.zero, ResourcesLoader.Instance.trajanBold, 20);
                bossPanel.AddImage("BossSplitSelected", selectedSplitImg, new Vector2(-Margin, 0), new Vector2(selectedSplitImg.width, selectedSplitImg.height), new Rect(0, 0, selectedSplitImg.width, selectedSplitImg.height));
            }
            // Total panel
            _panel.AddPanel("PHC-Total", null, new Vector2(Margin, 2 * TitleHeight + 4 * SplitHeight), Vector2.zero, new Rect(0, TitleHeight, BackgroundWidth, SplitHeight));
            var totalPanel = _panel.GetPanel("PHC-Total");
            totalPanel.AddImage("TotalSplitImage", splitImg, new Vector2(0, 0), new Vector2(splitImg.width, splitImg.height), new Rect(0, 0, splitImg.width, splitImg.height));
            const float center = SplitHeight - 1.7f * Margin;
            totalPanel.AddText("TotalText", "Total", new Vector2(1.5f * Margin, center), Vector2.zero, ResourcesLoader.Instance.trajanBold, 20);
            totalPanel.AddText("TotalHits", pantheon.TotalHits + "", new Vector2(BackgroundWidth - BossHitsWidth - Margin, center), Vector2.zero, ResourcesLoader.Instance.trajanBold, 20);
            totalPanel.AddText("TotalHitsPB", pantheon.TotalHitsPb < 0 ? "-" : pantheon.TotalHitsPb + "", new Vector2(BackgroundWidth - BossHitsWidth + Margin, center), Vector2.zero, ResourcesLoader.Instance.trajanBold, 20);
            
            
            _panel.FixRenderOrder();
            UpdateUI(pantheon);
        }

        public static void UpdateUI(Pantheon pantheon)
        {
            if (_panel == null) return;

            var bosses = pantheon.bosses.Where(boss => !boss.disabled).ToList();

            // Title
            _panel.GetPanel("PHC-Title").GetText("PantheonBoss").UpdateText($"{pantheon.bossNumber + 1}/{bosses.Count}");
            // Bosses Split
            for (var b = 0; b < 4; b++)
            {
                var currentBossNumber = GetBossNumber(pantheon.bossNumber, bosses.Count);
                var bossPanel = _panel.GetPanel("PHC-BossSplit-" + b);
                var boss = bosses[b + currentBossNumber];
                var bossImg = ResourcesLoader.Instance.images[boss.name.Replace(" ", "_")];
                var activeSplit = b == GetSelectedSplit(pantheon.bossNumber, bosses.Count) && _panel.Active;
                var anonymizeSplit = b > GetSelectedSplit(pantheon.bossNumber, bosses.Count) && _panel.Active && PantheonsHitCounter.instance.globalData.anonymize;
                bossPanel.GetImage("BossImageSelected").SetActive(activeSplit);
                bossPanel.GetImage("BossImage").UpdateImage(bossImg, new Rect(0, 0, bossImg.width, bossImg.height));
                bossPanel.GetImage("BossImageAnonymized").SetActive(anonymizeSplit);
                bossPanel.GetImage("BossImage").SetActive(!anonymizeSplit && _panel.Active);
                bossPanel.GetText("BossName").UpdateText(anonymizeSplit ? "????" : boss.name);
                bossPanel.GetText("BossHits").UpdateText(anonymizeSplit ? "?" : boss.hits + "");
                bossPanel.GetText("BossHitsPB").UpdateText(anonymizeSplit ? "?" : boss.hitsPb < 0 ? "-" : boss.hitsPb + "");
                bossPanel.GetImage("BossSplitSelected").SetActive(activeSplit);
            }
            // Total panel
            _panel.GetPanel("PHC-Total").GetText("TotalHits").UpdateText(pantheon.TotalHits + "");
            _panel.GetPanel("PHC-Total").GetText("TotalHitsPB").UpdateText(pantheon.TotalHitsPb < 0 ? "-" : pantheon.TotalHitsPb + "");
        }

        private static int GetBossNumber(int bossNumber, int count)
        {
            return bossNumber >= count - 3 ? count - 4 : bossNumber < 2 ? 0 : bossNumber - 1;
        }
        
        private static int GetSelectedSplit(int bossNumber, int count)
        {
            return bossNumber == 0 ? 0 : bossNumber < count - 2 ? 1 : bossNumber == count - 2 ? 2 : 3;
        }

        public static void Toggle(Pantheon currentPantheon)
        {
            if (_panel == null) return;
            _panel.TogglePanel();
            if (_panel.Active) UpdateUI(currentPantheon);
        }

        public static void Destroy()
        {
            _panel?.Destroy();
        }
    }
}