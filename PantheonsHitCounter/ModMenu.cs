using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using PantheonsHitCounter.UI;
using UnityEngine;
using UnityEngine.UI;
using Logger = Modding.Logger;
using Object = UnityEngine.Object;

namespace PantheonsHitCounter {
    public class ModMenu
    {
        public static MenuScreen mainMenu;
        private static MenuScreen _modsMenu;
        private static MenuOptionHorizontal _pantheonSelector, _stateSelector, _anonymizeSelector;
        private static ModToggleDelegates _stateToggle;
        private static int _selectedPantheon;
        private static readonly string[] ToggleState = { "Disabled", "Enabled" };

        public static bool isKeyboardBindsShown = true;
        public static bool isControllerBindsShown = true;
        public static bool needReorder = true;

        private static readonly List<GameObject> ControllerKeyBindOptions = new List<GameObject>();
        private static readonly List<GameObject> KeyBoardKeyBindOptions = new List<GameObject>();
        private static readonly List<GameObject> AllMenuOptions = new List<GameObject>();

        private static void AddMenuOptions(ContentArea area)
        {
            AddStateOption(area);
            AddAnonymizeOptions(area);
            AddResetOptions(area);
            AddBindingsOptions(area);
        }

        private static void AddStateOption(ContentArea area)
        {
            var modName = PantheonsHitCounter.instance.GetName();
            area.AddHorizontalOption(
                "State",
                new HorizontalOptionConfig
                {
                    Label = modName,
                    Description = new DescriptionInfo
                    {
                        Text = "Allows disabling the mod",
                        Style = DescriptionStyle.HorizOptionSingleLineVanillaStyle
                    },
                    Options = ToggleState,
                    ApplySetting = (_, i) => _stateToggle.SetModEnabled(i == 1),
                    RefreshSetting = (s, _) => s.optionList.SetOptionTo(_stateToggle.GetModEnabled() ? 1 : 0),
                    CancelAction = GoToModListMenu,
                    Style = HorizontalOptionStyle.VanillaStyle
                },
                out _stateSelector
            );
        }
        
        private static void AddAnonymizeOptions(ContentArea area)
        {
            area.AddHorizontalOption(
                "Anonymize",
                new HorizontalOptionConfig
                {
                    Label = "Anonymize bosses",
                    Description = new DescriptionInfo
                    {
                        Text = "Anonymize next bosses infos",
                        Style = DescriptionStyle.HorizOptionSingleLineVanillaStyle
                    },
                    Options = ToggleState,
                    ApplySetting = (_, i) => PantheonsHitCounter.instance.globalData.anonymize = i == 1,
                    RefreshSetting = (s, _) => s.optionList.SetOptionTo(PantheonsHitCounter.instance.globalData.anonymize ? 1 : 0),
                    CancelAction = GoToModListMenu,
                    Style = HorizontalOptionStyle.VanillaStyle
                },
                out _anonymizeSelector
            );
        }

        private static void AddResetOptions(ContentArea area)
        {
            if (!(area.Layout is RegularGridLayout layout)) return;
            var l = layout.ItemAdvance; 
            l.x = new RelLength(750f);
            layout.ChangeColumns(2, 0.5f, l);
            
            area.AddHorizontalOption(
                "Pantheons",
                new HorizontalOptionConfig
                {
                    Label = "",
                    Description = new DescriptionInfo
                    {
                        Text = "Select a pantheon to reset its PBs",
                    },
                    Options = PantheonsHitCounter.instance.pantheons.Select(s => s.name).ToArray(),
                    ApplySetting = (_, i) => _selectedPantheon = i,
                    RefreshSetting = (s, _) => s.optionList.SetOptionTo(_selectedPantheon),
                    CancelAction = GoToModListMenu,
                },
                out _pantheonSelector
            );

            _pantheonSelector.gameObject.transform.Find("CursorLeft").GetComponent<RectTransform>().anchoredPosition = new Vector2(280f, 0f);
            _pantheonSelector.gameObject.transform.Find("Description").GetComponent<RectTransform>().anchoredPosition = new Vector2(480f, 0f);

            // make text obj smaller so text can left align properly
            var textObj = _pantheonSelector.gameObject.transform.Find("Text").gameObject;
            textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(-700, 0);
            textObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            
            area.AddMenuButton(
                "Reset",
                new MenuButtonConfig
                {
                    Label = "Reset",
                    CancelAction = GoToModListMenu,
                    SubmitAction = Reset,
                    Proceed = true,
                    Style = MenuButtonStyle.VanillaStyle
                }, out _
            );
            
            l.x = new RelLength(1920f);
            layout.ChangeColumns(1, 0.25f, l);
            
            area.AddTextPanel("Note",
                new RelVector2(new Vector2(850f, 40f)),
                new TextPanelConfig{
                    Text = "Note: Reset PBs of a pantheon works only in-game",
                    Size = 25,
                    Font = TextPanelConfig.TextFont.TrajanRegular,
                    Anchor = TextAnchor.MiddleCenter
                }
            );
        }

        private static void AddBindingsOptions(ContentArea area)
        {
            area.AddMenuButton("SectionBindingsKeyboard",
                new MenuButtonConfig
                {
                    CancelAction = GoToModListMenu,
                    Description = new DescriptionInfo
                    {
                        Text = "Click here to bind keyboard buttons",
                    },
                    Label = "Keyboard bindings",
                    Proceed = false,
                    SubmitAction = _ =>
                    {
                        HideShowKeyboardBinds();
                        Reorder(true);
                    }
                }, out var keyboardKeybindButton);
            
            AllMenuOptions.Add(keyboardKeybindButton.gameObject);
            
            area.AddKeybind(
                "ToggleCounterBind",
                PantheonsHitCounter.instance.globalData.keybinds.toggleCounter,
                new KeybindConfig
                {
                    Label = "Toggle counter",
                    CancelAction = GoToModListMenu,
                }, out var keyboardCounterButton
            );
            KeyBoardKeyBindOptions.Add(keyboardCounterButton.gameObject);
            AllMenuOptions.Add(keyboardCounterButton.gameObject);
            
            area.AddKeybind(
                "NextBossBind",
                PantheonsHitCounter.instance.globalData.keybinds.nextBossSplit,
                new KeybindConfig
                {
                    Label = "Next boss",
                    CancelAction = GoToModListMenu,
                }, out var keyboardNextBossButton
            );
            KeyBoardKeyBindOptions.Add(keyboardNextBossButton.gameObject);
            AllMenuOptions.Add(keyboardNextBossButton.gameObject);
            
            area.AddKeybind(
                "PreviousBossBind",
                PantheonsHitCounter.instance.globalData.keybinds.previousBossSplit,
                new KeybindConfig
                {
                    Label = "Previous boss",
                    CancelAction = GoToModListMenu,
                }, out var keyboardPreviousBossButton
            );
            KeyBoardKeyBindOptions.Add(keyboardPreviousBossButton.gameObject);
            AllMenuOptions.Add(keyboardPreviousBossButton.gameObject);
            
            try
            {
                area.AddMenuButton("SectionBindingsController",
                    new MenuButtonConfig
                    {
                        CancelAction = GoToModListMenu,
                        Description = new DescriptionInfo
                        {
                            Text = "Click here to bind controller buttons",
                        },
                        Label = "Controller bindings",
                        Proceed = false,
                        SubmitAction = _ =>
                        {
                            HideShowControllerBinds();
                            Reorder(true);
                        }
                    }, out var controllerKeybindButton);
                AllMenuOptions.Add(controllerKeybindButton.gameObject);
                
                area.AddButtonBind(
                    "ToggleCounterBindJoy",
                    PantheonsHitCounter.instance.globalData.buttonbinds.toggleCounter,
                    new ButtonBindConfig()
                    {
                        Label = "Toggle counter",
                        CancelAction = GoToModListMenu,
                    },
                    out var controllerCounterButton
                );
                ControllerKeyBindOptions.Add(controllerCounterButton.gameObject);
                AllMenuOptions.Add(controllerCounterButton.gameObject);

                area.AddButtonBind(
                    "NextBossBindJoy",
                    PantheonsHitCounter.instance.globalData.buttonbinds.nextBossSplit,
                    new ButtonBindConfig
                    {
                        Label = "Next boss",
                        CancelAction = GoToModListMenu,
                    },
                    out var controllerNextBossButton
                );
                ControllerKeyBindOptions.Add(controllerNextBossButton.gameObject);
                AllMenuOptions.Add(controllerNextBossButton.gameObject);
                
                area.AddButtonBind(
                    "PreviousBossBindJoy",
                    PantheonsHitCounter.instance.globalData.buttonbinds.previousBossSplit,
                    new ButtonBindConfig
                    {
                        Label = "Previous boss",
                        CancelAction = GoToModListMenu,
                    },
                    out var controllerPreviousBossButton
                );
                ControllerKeyBindOptions.Add(controllerPreviousBossButton.gameObject);
                AllMenuOptions.Add(controllerPreviousBossButton.gameObject);
                
            } catch (Exception) { /**/ }
            
            
            HideShowControllerBinds();
            HideShowKeyboardBinds();
        }

        private static void GoToModListMenu(object _) {
            GoToModListMenu();
            RefreshOptions();
        }

        private static void GoToModListMenu() => UIManager.instance.UIGoToDynamicMenu(_modsMenu);

        public static void RefreshOptions()
        {
            if(_stateSelector != null) _stateSelector.menuSetting.RefreshValueFromGameSettings();
            if(_anonymizeSelector != null) _anonymizeSelector.menuSetting.RefreshValueFromGameSettings();
            if(_pantheonSelector != null) _pantheonSelector.menuSetting.RefreshValueFromGameSettings();
        }

        public static MenuScreen CreateMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggle)
        {
            _modsMenu = modListMenu;
            if (toggle != null) _stateToggle = toggle.Value;
            
            AllMenuOptions.Clear();
            KeyBoardKeyBindOptions.Clear();
            ControllerKeyBindOptions.Clear();

            var title = PantheonsHitCounter.instance.GetName();
            mainMenu = new MenuBuilder(UIManager.instance.UICanvas.gameObject, title)
                .CreateTitle(title, MenuTitleStyle.vanillaStyle)
                .CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 803f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ))
                .CreateControlPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 259f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -502f)
                    )
                ))
                .SetDefaultNavGraph(new ChainedNavGraph())
                .AddControls(
                    new SingleContentLayout(new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, 0f)
                    )),
                    c => c.AddMenuButton(
                        "ApplyButton",
                        new MenuButtonConfig
                        {
                            Label = "Apply",
                            CancelAction = GoToModListMenu,
                            SubmitAction = Apply,
                            Proceed = true,
                            Style = MenuButtonStyle.VanillaStyle
                        },
                        out _
                    )
                )
                .AddControls(
                    new SingleContentLayout(new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -64f)
                    )),
                    c => c.AddMenuButton(
                        "BackButton",
                        new MenuButtonConfig
                        {
                            Label = "Back",
                            CancelAction = GoToModListMenu,
                            SubmitAction = GoToModListMenu,
                            Proceed = true,
                            Style = MenuButtonStyle.VanillaStyle
                        },
                        out _
                    )
                ).AddContent(new NullContentLayout(), c => c.AddScrollPaneContent(
                    new ScrollbarConfig
                    {
                        CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(_modsMenu),
                        Navigation = new Navigation
                        {
                            mode = Navigation.Mode.Explicit
                        },
                        Position = new AnchoredPosition
                        {
                            ChildAnchor = new Vector2(0f, 1f),
                            ParentAnchor = new Vector2(1f, 1f),
                            Offset = new Vector2(-310f, 0f)
                        }
                    },
                    new RelLength(1600f), 
                    RegularGridLayout.CreateVerticalLayout(105f),
                    AddMenuOptions
            )).Build();
            
            return mainMenu;
        }

        private static void Apply(Object _) => Apply();
        private static void Apply()
        {
            _stateToggle.ApplyChange();
            
            PantheonsHitCounter.instance.Log("Settings applied");
            
            GoToModListMenu();
        }

        private static void Reset(Object _) => Reset();
        private static void Reset()
        {
            if (GameManager.instance.IsNonGameplayScene()) return;
            
            var pantheon = PantheonsHitCounter.instance.pantheons[_selectedPantheon];
            pantheon.ResetPbCounter();
            if (PantheonsHitCounter.instance.currentPantheon.number == pantheon.number)
                CounterUI.UpdateUI(pantheon);
            
            PantheonsHitCounter.instance.Log($"PBs for {pantheon.name} reset");
        }

        private static void HideShowKeyboardBinds()
        {
            isKeyboardBindsShown = !isKeyboardBindsShown;
            foreach (var keyBind in KeyBoardKeyBindOptions)
                keyBind.SetActive(isKeyboardBindsShown);
        }
        private static void HideShowControllerBinds()
        {
            isControllerBindsShown = !isControllerBindsShown;
            foreach (var keyBind in ControllerKeyBindOptions)
                keyBind.SetActive(isControllerBindsShown);
        }
        
        public static void Reorder(bool notInOnHook = false)
        {
            if (!notInOnHook && !needReorder) return;
            
            var index = 2;
            var itemAdvance = new RelVector2(new Vector2(0.0f, -105f));
            var margin = new RelVector2(new Vector2(0.0f, -210f));
            var start = new AnchoredPosition
            {
                ChildAnchor = new Vector2(0.5f, 1f),
                ParentAnchor = new Vector2(0.5f, 1f),
                Offset = default
            };

            foreach (var menuOption in AllMenuOptions.Where(x => x.activeInHierarchy))
            {
                (start + margin + itemAdvance * new Vector2Int(index, index)).Reposition(menuOption.gameObject.GetComponent<RectTransform>());
                index += 1;
            }
        }
    }
}