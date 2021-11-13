using System;
using System.Linq;
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
            area.AddHorizontalOption(
                "Pantheons",
                new HorizontalOptionConfig
                {
                    Label = "Pantheons",
                    Description = new DescriptionInfo
                    {
                        Text = "Select the pantheon to reset its PBs",
                        Style = DescriptionStyle.HorizOptionSingleLineVanillaStyle
                    },
                    Options = PantheonsHitCounter.instance.pantheons.Select(s => s.name).ToArray(),
                    ApplySetting = (_, i) => _selectedPantheon = i,
                    RefreshSetting = (s, _) => s.optionList.SetOptionTo(_selectedPantheon),
                    CancelAction = GoToModListMenu,
                    Style = HorizontalOptionStyle.VanillaStyle
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
                }
            );
        }

        private static void AddBindingsOptions(ContentArea area)
        {
            area.AddTextPanel("SectionBindings",
                new RelVector2(new Vector2(850f, 105f)),
                new TextPanelConfig{
                    Text = "Keyboard bindings",
                    Size = 55,
                    Font = TextPanelConfig.TextFont.TrajanBold,
                    Anchor = TextAnchor.MiddleCenter
                });
            
            area.AddKeybind(
                "ToggleCounterBind",
                PantheonsHitCounter.instance.globalData.keybinds.toggleCounter,
                new KeybindConfig
                {
                    Label = "Toggle counter",
                    CancelAction = GoToModListMenu,
                }
            );
            
            area.AddKeybind(
                "NextBossBind",
                PantheonsHitCounter.instance.globalData.keybinds.nextBossSplit,
                new KeybindConfig
                {
                    Label = "Next boss",
                    CancelAction = GoToModListMenu,
                }
            );
            
            area.AddKeybind(
                "PreviousBossBind",
                PantheonsHitCounter.instance.globalData.keybinds.previousBossSplit,
                new KeybindConfig
                {
                    Label = "Previous boss",
                    CancelAction = GoToModListMenu,
                }
            );

            try
            {
                area.AddTextPanel("SectionBindingsController",
                    new RelVector2(new Vector2(850f, 105f)),
                    new TextPanelConfig{
                        Text = "Controller bindings",
                        Size = 55,
                        Font = TextPanelConfig.TextFont.TrajanBold,
                        Anchor = TextAnchor.MiddleCenter
                    });

                area.AddButtonBind(
                    "ToggleCounterBindJoy",
                    PantheonsHitCounter.instance.globalData.buttonbinds.toggleCounter,
                    new ButtonBindConfig()
                    {
                        Label = "Toggle counter",
                        CancelAction = GoToModListMenu,
                    },
                    out _
                );

                area.AddButtonBind(
                    "NextBossBindJoy",
                    PantheonsHitCounter.instance.globalData.buttonbinds.nextBossSplit,
                    new ButtonBindConfig
                    {
                        Label = "Next boss",
                        CancelAction = GoToModListMenu,
                    },
                    out _
                );

                area.AddButtonBind(
                    "PreviousBossBindJoy",
                    PantheonsHitCounter.instance.globalData.buttonbinds.previousBossSplit,
                    new ButtonBindConfig
                    {
                        Label = "Previous boss",
                        CancelAction = GoToModListMenu,
                    },
                    out _
                );
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
    }
}