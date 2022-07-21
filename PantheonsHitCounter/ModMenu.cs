using System;
using System.Linq;
using Modding;
using PantheonsHitCounter.UI;
using Satchel.BetterMenus;

namespace PantheonsHitCounter
{
    public static class ModMenu
    {
        private static Menu _menuRef;
        private static int _selectedPantheon;
        private static bool _toggleKeybinds;
        private static bool _toggleButtonbinds;

        private static Menu PrepareMenu(ModToggleDelegates toggle)
        {
            return new Menu(PantheonsHitCounter.instance.GetName(),new Element[]
            {
                toggle.CreateToggle("Mod toggle", "Allows disabling the mod"),
                new HorizontalOption(
                    "Pantheon", 
                    "Select a pantheon to reset its PBs",
                    PantheonsHitCounter.instance.pantheons.Select(s => s.name).ToArray(),
                    i => _selectedPantheon = i,
                    () => _selectedPantheon,
                    "HorizontalOption-SelectedPantheon"
                ),
                new MenuButton(
                    "Reset PBs",
                    "",
                    _ => Reset()
                ),
                new CustomSlider(
                    "Maximum splits",
                    f =>
                    {
                        PantheonsHitCounter.instance.globalData.totalSplits = (int) f;
                        PantheonsHitCounter.instance.ToggleCurrentCounter();
                    },
                    () => PantheonsHitCounter.instance.globalData.totalSplits,
                    PantheonsHitCounter.SplitsMin,
                    PantheonsHitCounter.instance.globalData.compactMode ? PantheonsHitCounter.CompactSplitsCountMax : PantheonsHitCounter.DefaultSplitsCountMax,
                    true,
                    "CustomSlider-MaximumSplits"
                ),
                new HorizontalOption(
                    "Counter mode",
                    "Interface mode of the counter",
                    new []{ "Default", "Compact" },
                    mode => {
                        PantheonsHitCounter.instance.globalData.compactMode = mode == 1;
                        _menuRef?.Find("CustomSlider-MaximumSplits")?.updateAfter(el => {
                            var slider = (CustomSlider) el;
                            slider.maxValue = PantheonsHitCounter.instance.globalData.compactMode ? PantheonsHitCounter.CompactSplitsCountMax : PantheonsHitCounter.DefaultSplitsCountMax;
                            slider.value = Math.Min(PantheonsHitCounter.instance.globalData.totalSplits, slider.maxValue);
                        });
                        PantheonsHitCounter.instance.ToggleCurrentCounter();
                    },
                    () => PantheonsHitCounter.instance.globalData.compactMode ? 1 : 0
                ),
                new HorizontalOption(
                    "Anonymize",
                    "Hides bosses icon, hits and PB",
                    new []{ "Off", "On" },
                    anonymize => {
                        PantheonsHitCounter.instance.globalData.anonymize = anonymize == 1;
                        PantheonsHitCounter.instance.ToggleCurrentCounter();
                    },
                    () => PantheonsHitCounter.instance.globalData.anonymize ? 1 : 0
                ),
                new HorizontalOption(
                    "Translation",
                    "Enables local translation",
                    new []{ "Off", "On" },
                    translated => {
                        PantheonsHitCounter.instance.globalData.translated = translated == 1;
                        PantheonsHitCounter.instance.ToggleCurrentCounter();
                    },
                    () => PantheonsHitCounter.instance.globalData.translated ? 1 : 0
                ),
                new MenuButton(
                    "Keyboard bindings",
                    "Click to show keyboard bindings",
                    _ =>
                    {
                        _toggleKeybinds = !_toggleKeybinds;
                        _menuRef.Find("MenuButton-Keybinds")?.updateAfter(el => ((MenuButton)el).Description = $"Click to {(_toggleKeybinds ? "hide" : "show")} keyboard bindings");
                        if (_toggleKeybinds)
                        {
                            _menuRef?.Find("Keybind-ToggleCounter")?.Show();
                            _menuRef?.Find("Keybind-NextSplit")?.Show();
                            _menuRef?.Find("Keybind-PreviousSplit")?.Show();
                        }
                        else
                        {
                            _menuRef?.Find("Keybind-ToggleCounter")?.Hide();
                            _menuRef?.Find("Keybind-NextSplit")?.Hide();
                            _menuRef?.Find("Keybind-PreviousSplit")?.Hide();
                        }
                    },
                    Id: "MenuButton-Keybinds"
                ),
                new KeyBind("Toggle Counter", PantheonsHitCounter.instance.globalData.keybinds.toggleCounter, "Keybind-ToggleCounter"){ isVisible = false },
                new KeyBind("Next Split", PantheonsHitCounter.instance.globalData.keybinds.nextBossSplit, "Keybind-NextSplit"){ isVisible = false },
                new KeyBind("Previous Split", PantheonsHitCounter.instance.globalData.keybinds.previousBossSplit, "Keybind-PreviousSplit"){ isVisible = false },
                new MenuButton(
                    "Controller bindings",
                    "Click to show controller bindings",
                    _ =>
                    {
                        _toggleButtonbinds = !_toggleButtonbinds;
                        _menuRef.Find("MenuButton-Buttonbinds")?.updateAfter(el => ((MenuButton)el).Description = $"Click to {(_toggleButtonbinds ? "hide" : "show")} controller bindings");
                        if (_toggleButtonbinds)
                        {
                            _menuRef?.Find("ButtonBind-ToggleCounter")?.Show();
                            _menuRef?.Find("ButtonBind-NextSplit")?.Show();
                            _menuRef?.Find("ButtonBind-PreviousSplit")?.Show();
                        }
                        else
                        {
                            _menuRef?.Find("ButtonBind-ToggleCounter")?.Hide();
                            _menuRef?.Find("ButtonBind-NextSplit")?.Hide();
                            _menuRef?.Find("ButtonBind-PreviousSplit")?.Hide();
                        }
                    },
                    Id: "MenuButton-Buttonbinds"
                ),
                new ButtonBind("Toggle Counter", PantheonsHitCounter.instance.globalData.buttonbinds.toggleCounter, "ButtonBind-ToggleCounter"){ isVisible = false },
                new ButtonBind("Next Split", PantheonsHitCounter.instance.globalData.buttonbinds.nextBossSplit, "ButtonBind-NextSplit"){ isVisible = false },
                new ButtonBind("Previous Split", PantheonsHitCounter.instance.globalData.buttonbinds.previousBossSplit, "ButtonBind-PreviousSplit"){ isVisible = false }
            });
        }
        
        private static void Reset()
        {
            if (GameManager.instance.IsNonGameplayScene()) return;
            
            var pantheon = PantheonsHitCounter.instance.pantheons[_selectedPantheon];
            pantheon.ResetPbCounter();
            if (PantheonsHitCounter.instance.currentPantheon.number == pantheon.number)
                CounterUI.UpdateUI(pantheon);
            
            PantheonsHitCounter.instance.Log($"PBs for {pantheon.name} reset");
        }
        
        public static MenuScreen GetMenu(MenuScreen lastMenu, ModToggleDelegates? toggle)
        {
            if (toggle == null) return null;
            if (_menuRef == null)
                _menuRef = PrepareMenu((ModToggleDelegates) toggle);
            
            return _menuRef.GetMenuScreen(lastMenu);
        }
    }
}