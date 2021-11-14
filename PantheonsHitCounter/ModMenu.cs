using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using PantheonsHitCounter.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PantheonsHitCounter {
    public class ModMenu {
        private static MenuScreen _modsMenu;
        public static MenuOptionHorizontal pantheonSelector, stateSelector;
        private static ModToggleDelegates _stateToggle;
        private static int _selectedPantheon;
        private static readonly string[] ToggleState = { "Disabled", "Enabled" };

        public static bool isKeyboardBindsShown = true;
        public static bool isControllerBindsShown = true;

        private static List<GameObject> ControllerKeyBindOptions = new List<GameObject>();
        private static List<GameObject> KeyBoardKeyBindOptions = new List<GameObject>();
        private static List<GameObject> AllMenuOptions = new List<GameObject>();

        private static void AddMenuOptions(ContentArea area)
        {
            AddStateOption(area);
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
                out stateSelector
            );
        }

        private static void AddResetOptions(ContentArea area)
        {
            var layout = area.Layout as RegularGridLayout;
                var l = layout.ItemAdvance;
                l.x = new RelLength(750f);
                layout.ChangeColumns(2, 0.5f, l, 0.5f);
				
            area.AddHorizontalOption(
                "Pantheons",
                new HorizontalOptionConfig
                {
                    Label = "Pantheons",
                    Description = new DescriptionInfo
                    {
                        Text = "Select the pantheon to reset its PBs",
                    },
                    Options = PantheonsHitCounter.instance.pantheons.Select(s => s.name).ToArray(),
                    ApplySetting = (_, i) => _selectedPantheon = i,
                    RefreshSetting = (s, _) => s.optionList.SetOptionTo(_selectedPantheon),
                    CancelAction = GoToModListMenu,
                },
                out pantheonSelector
            );
            area.AddMenuButton(
                "Reset",
                new MenuButtonConfig
                {
                    Label = "Reset",
                    Description = new DescriptionInfo
                    {
                        Text = "Reset PBs of the pantheon selected above (works only in game)"
                    },
                    CancelAction = GoToModListMenu,
                    SubmitAction = Reset,
                    Proceed = true,
                    Style = MenuButtonStyle.VanillaStyle
                }, out var ResetButton
            );

            l.x = new RelLength(1920f);
            layout.ChangeColumns(1, 0.25f, l, 0.5f);

        }
        private static void AddBindingsOptions(ContentArea area)
        {
            KeyBoardKeyBindOptions.Clear();
            ControllerKeyBindOptions.Clear();
            
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
                        isKeyboardBindsShown = !isKeyboardBindsShown;
                        foreach (GameObject keyBind in KeyBoardKeyBindOptions)
                        {
                            keyBind.SetActive(isKeyboardBindsShown);
                        }

                        Reorder();
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
                }, out var KeyboardCounterButton
            );
            KeyBoardKeyBindOptions.Add(KeyboardCounterButton.gameObject);
            AllMenuOptions.Add(KeyboardCounterButton.gameObject);
            
            area.AddKeybind(
                "NextBossBind",
                PantheonsHitCounter.instance.globalData.keybinds.nextBossSplit,
                new KeybindConfig
                {
                    Label = "Next boss",
                    CancelAction = GoToModListMenu,
                }, out var KeyboardNextBossButton
            );
            KeyBoardKeyBindOptions.Add(KeyboardNextBossButton.gameObject);
            AllMenuOptions.Add(KeyboardNextBossButton.gameObject);
            
            area.AddKeybind(
                "PreviousBossBind",
                PantheonsHitCounter.instance.globalData.keybinds.previousBossSplit,
                new KeybindConfig
                {
                    Label = "Previous boss",
                    CancelAction = GoToModListMenu,
                }, out var KeyboardPreviousBossButton
            );
            KeyBoardKeyBindOptions.Add(KeyboardPreviousBossButton.gameObject);
            AllMenuOptions.Add(KeyboardPreviousBossButton.gameObject);
            
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
                            isControllerBindsShown = !isControllerBindsShown;
                            foreach (GameObject keyBind in ControllerKeyBindOptions)
                            {
                                keyBind.SetActive(isControllerBindsShown);
                            }

                            Reorder();
                        }
                    }, out var ControllerKeybindButton);
                AllMenuOptions.Add(ControllerKeybindButton.gameObject);
                
                area.AddButtonBind(
                    "ToggleCounterBindJoy",
                    PantheonsHitCounter.instance.globalData.buttonbinds.toggleCounter,
                    new ButtonBindConfig()
                    {
                        Label = "Toggle counter",
                        CancelAction = GoToModListMenu,
                    },
                    out var ControllerCounterButton
                );
                ControllerKeyBindOptions.Add(ControllerCounterButton.gameObject);
                AllMenuOptions.Add(ControllerCounterButton.gameObject);

                area.AddButtonBind(
                    "NextBossBindJoy",
                    PantheonsHitCounter.instance.globalData.buttonbinds.nextBossSplit,
                    new ButtonBindConfig
                    {
                        Label = "Next boss",
                        CancelAction = GoToModListMenu,
                    },
                    out var ControllerNextBossButton
                );
                ControllerKeyBindOptions.Add(ControllerNextBossButton.gameObject);
                AllMenuOptions.Add(ControllerNextBossButton.gameObject);
                
                area.AddButtonBind(
                    "PreviousBossBindJoy",
                    PantheonsHitCounter.instance.globalData.buttonbinds.previousBossSplit,
                    new ButtonBindConfig
                    {
                        Label = "Previous boss",
                        CancelAction = GoToModListMenu,
                    },
                    out var ControllerPreviousBossButton
                );
                ControllerKeyBindOptions.Add(ControllerPreviousBossButton.gameObject);
                AllMenuOptions.Add(ControllerPreviousBossButton.gameObject);
                
            } catch (Exception) { /**/ }
        }

        private static void GoToModListMenu(object _) {
            GoToModListMenu();
            RefreshOptions();
        }

        private static void GoToModListMenu() => (UIManager.instance).UIGoToDynamicMenu(_modsMenu);

        public static void RefreshOptions()
        {
            if(stateSelector != null) stateSelector.menuSetting.RefreshValueFromGameSettings();
            if(pantheonSelector != null) pantheonSelector.menuSetting.RefreshValueFromGameSettings();
        }

        public static MenuScreen CreateMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggle)
        {
            _modsMenu = modListMenu;
            if (toggle != null) _stateToggle = toggle.Value;

            var title = PantheonsHitCounter.instance.GetName();
            var builder = new MenuBuilder(UIManager.instance.UICanvas.gameObject, title)
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
                );
                builder.AddContent(new NullContentLayout(), c => c.AddScrollPaneContent(
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
            ));
            return builder.Build();
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

        private static void Reorder()
        {
            int Index = 2;
            RelVector2 ItemAdvance = new RelVector2(new Vector2(0.0f, -105f));
            AnchoredPosition Start = new AnchoredPosition
            {
                ChildAnchor = new Vector2(0.5f, 1f),
                ParentAnchor = new Vector2(0.5f, 1f),
                Offset = default
            };

            foreach (GameObject menuOption in AllMenuOptions.Where(x => x.activeInHierarchy))
            {
                (Start + ItemAdvance * new Vector2Int(Index, Index)).Reposition(menuOption.gameObject.GetComponent<RectTransform>());
                Index += 1;
            }
        }
    }
}
