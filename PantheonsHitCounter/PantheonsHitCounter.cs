using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Modding;
using PantheonsHitCounter.UI;

namespace PantheonsHitCounter
{
    public class PantheonsHitCounter : Mod, IGlobalSettings<GlobalData>, ILocalSettings<LocalData>, ICustomMenuMod, ITogglableMod
    {
        public bool ToggleButtonInsideMenu => true;
        private LocalData _localData = new LocalData();
        public GlobalData globalData = new GlobalData();
        public List<Pantheon> pantheons;
        public Pantheon currentPantheon;
        private bool _loaded;
        private string _sceneName;
        private bool _inPantheon;
        private bool _inTransition;
        internal static PantheonsHitCounter instance;
        
        private static readonly string PantheonsBossesJsonPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/PantheonsBosses.json";
        private static readonly string[] TransitionScenes = { "GG_Spa", "GG_Engine", "GG_Engine_Prime", "GG_Engine_Root", "GG_Unn", "GG_Wyrm", "GG_End_Sequence" };
        private static readonly string[] GodhomeScenes = { "GG_Atrium", "GG_Atrium_Roof" };
        private static readonly string[] CompletedScene = { "GG_End_Sequence", "End_Game_Completion" };
        
        public PantheonsHitCounter() : base("Pantheons Hit Counter") {}
        public override string GetVersion() => "1.1.0";
        public void OnLoadGlobal(GlobalData data) => globalData = data;
        public GlobalData OnSaveGlobal() => globalData;
        public void OnLoadLocal(LocalData data) => _localData = data;
        public LocalData OnSaveLocal() => _localData;
        private static bool IsTransitionScene(string sceneName) => TransitionScenes.Contains(sceneName);
        private static bool IsFailingScene(string sceneName) => GodhomeScenes.Contains(sceneName);
        private static bool IsCompletedScene(string sceneName) => CompletedScene.Contains(sceneName);

        public override void Initialize()
        {
            instance = this;
            
            ModHooks.SavegameLoadHook += LoadPBs;
            On.QuitToMenu.Start += OnQuitToMenu;
            ModHooks.BeforeSceneLoadHook += OnSceneLoad;
            ModHooks.TakeHealthHook += OnHitTaken;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            On.BossSequenceDoor.Start += OnRandomizedPantheon;
            On.UIManager.ShowMenu += ReorderButtons;

            if (_loaded) return;
            _loaded = true;
            
            ResourcesLoader.Instance.LoadResources();
            LoadPantheonsBosses();
        }

        private void OnRandomizedPantheon(On.BossSequenceDoor.orig_Start orig, BossSequenceDoor self)
        {
            orig(self);
            
            var bossSequence = self.bossSequence;
            var number = int.Parse(self.descriptionKey.Replace("UI_CHALLENGE_DESC_", ""));
            var pantheon = pantheons[number - 1];
            var bosses = new List<Boss>();
            
            for (var b = 0; b < bossSequence.Count; b++)
            {
                var bossScene = bossSequence.GetBossScene(b);
                if (TransitionScenes.Contains(bossScene.sceneName)) continue;

                var boss = new Boss
                {
                    sceneName = bossScene.sceneName,
                    name = pantheon.GetBossBySceneName(bossScene.sceneName).name
                };
                bosses.Add(boss);
            }

            pantheon.bosses = bosses;
        }

        private void LoadPBs(int _)
        {
            for (var p = 0; p < pantheons.Count; p++)
            {
                var pantheon = pantheons[p];
                var zote = pantheon.GetBossBySceneName("GG_Grey_Prince_Zote");
                if (zote != null) zote.disabled = !PlayerData.instance.greyPrinceDefeated;
                
                var pantheonData = _localData.pantheons[p];
                if (pantheonData.bosses.Count == 0) continue;
                
                for (var b = 0; b < pantheon.bosses.Count; b++)
                    pantheon.bosses[b].hitsPb = pantheonData.bosses[b].hitsPb;

                Log($"Loaded PBs for {pantheon.name}");
            }
        }

        private string OnSceneLoad(string sceneName)
        {
            if (_inPantheon) OnNextBoss(sceneName);
            else OnEnterPantheon(Pantheon.FindPantheon(pantheons, _sceneName, sceneName));

            _sceneName = sceneName;
            return sceneName;
        }

        private int OnHitTaken(int damage)
        {
            if (!_inPantheon || _inTransition) return damage;
            
            currentPantheon.GetBossBySceneName(_sceneName).AddHit();
            CounterUI.UpdateUI(currentPantheon);

            return damage;
        }

        private void OnEnterPantheon(int pantheonNumber)
        {
            if (pantheonNumber <= 0) return;

            currentPantheon = pantheons[pantheonNumber - 1];
            _inPantheon = true;

            currentPantheon.ResetCounter();

            if (ResourcesLoader.Instance.canvas) ResourcesLoader.Instance.Destroy();
            ResourcesLoader.Instance.BuildMenus(currentPantheon);
            
            Log($"Started {currentPantheon.name}");
        }

        private void OnNextBoss(string sceneName)
        {
            if (IsCompletedScene(sceneName)) { OnLeavePantheon(true); return; }
            if (IsFailingScene(sceneName)) { OnLeavePantheon(); return; }

            if (!_inTransition)
            {
                var boss = currentPantheon.GetBossBySceneName(_sceneName);
                if (boss != null) Log($"{boss.name} - Hits: {boss.hits} | PB: {boss.hitsPb}");
            }

            
            _inTransition = IsTransitionScene(sceneName);
            if (_inTransition) { Log("Transition scene"); return; }

            var pantheon = currentPantheon;
            pantheon.NextBoss();
            CounterUI.UpdateUI(pantheon);
        }

        private void OnLeavePantheon(bool completed = false)
        {
            Log($"{(completed ? "Completed" : "Failed")} {currentPantheon.name}");

            if (completed && currentPantheon.IsPbRun()) UpdateData();

            _inPantheon = false;
            _inTransition = false;
        }

        private void UpdateData()
        {
            var pantheonNumber = currentPantheon.number - 1;
            var pantheon = pantheons[pantheonNumber];
            var pantheonData = _localData.pantheons[pantheonNumber];
            if (pantheonData.bosses.Count == 0) pantheonData.FillPantheonBosses();

            for (var b = 0; b < pantheon.bosses.Count; b++)
            {
                var boss = pantheon.bosses[b];
                boss.hitsPb = boss.hits;
                var bossData = pantheonData.bosses[b];
                bossData.name = boss.name;
                bossData.hitsPb = boss.hitsPb;
            }
            Log($"Saved PBs for {pantheon.name}");
        }
        
        private void LoadPantheonsBosses()
        {
            string json = null;
            try
            {
                json = JArray.Parse(File.ReadAllText(PantheonsBossesJsonPath)).ToString();
            }
            finally
            {
                if (json == null) throw new Exception($"Cannot read PantheonsBosses.json. Please make sure it is in {GetName()} mod folder.");
            }
            pantheons = JsonConvert.DeserializeObject<List<Pantheon>>(json);
            
            Log("Pantheons bosses loaded");
        }
        
        private void OnHeroUpdate()
        {
            if (globalData.keybinds.nextBossSplit.WasPressed || globalData.buttonbinds.nextBossSplit.WasPressed)
            {
                if (currentPantheon == null) return;
                currentPantheon.NextBoss();
                CounterUI.UpdateUI(currentPantheon);
            }
            if (globalData.keybinds.previousBossSplit.WasPressed || globalData.buttonbinds.previousBossSplit.WasPressed)
            {
                if (currentPantheon == null) return;
                currentPantheon.PreviousBoss();
                CounterUI.UpdateUI(currentPantheon);
            }
            if (globalData.keybinds.toggleCounter.WasPressed || globalData.buttonbinds.toggleCounter.WasPressed)
            {
                CounterUI.Toggle(currentPantheon);
            }
        }

        private static IEnumerator ReorderButtons(On.UIManager.orig_ShowMenu orig, UIManager self, MenuScreen menu)
        {
            // need to do this, probably something about components not being active in hierarchy and so the logic doesn't work
            if (menu == ModMenu.mainMenu)
                GameManager.instance.StartCoroutine(ReorderAfterFrame());
            yield return orig(self, menu);

        }

        private static IEnumerator ReorderAfterFrame()
        {
            yield return null;
            ModMenu.Reorder();
            ModMenu.needReorder = false;

        }

        private static IEnumerator OnQuitToMenu(On.QuitToMenu.orig_Start orig, QuitToMenu self)
        {
            ResourcesLoader.Instance.Destroy();
            ModMenu.needReorder = true;
            ModMenu.isControllerBindsShown = !ModMenu.isControllerBindsShown;
            ModMenu.isKeyboardBindsShown = !ModMenu.isKeyboardBindsShown;
            return orig(self);
        }
        
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggle)
        {
            var menu = ModMenu.CreateMenuScreen(modListMenu, toggle);
            ModMenu.RefreshOptions();
            return menu;
        }

        public void Unload()
        {
            ModHooks.SavegameLoadHook -= LoadPBs;
            On.QuitToMenu.Start -= OnQuitToMenu;
            ModHooks.BeforeSceneLoadHook -= OnSceneLoad;
            ModHooks.TakeHealthHook -= OnHitTaken;
            ModHooks.HeroUpdateHook -= OnHeroUpdate;
            On.BossSequenceDoor.Start -= OnRandomizedPantheon;
            On.UIManager.ShowMenu -= ReorderButtons;
        }
    }
}