using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace PantheonsHitCounter.UI
{
    public class ResourcesLoader : MonoBehaviour
    {
        public Font trajanBold;
        public Font arial;
        public Dictionary<string, Texture2D> images = new Dictionary<string, Texture2D>();

        public GameObject canvas;
        private static ResourcesLoader _instance;

        public void BuildMenus(Pantheon pantheon)
        {
            canvas = new GameObject();
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvas.AddComponent<GraphicRaycaster>();

            CounterUI.BuildMenu(canvas, pantheon);

            DontDestroyOnLoad(canvas);
        }

        public void LoadResources()
        {
            foreach (var f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (f != null && f.name == "TrajanPro-Bold") trajanBold = f;
                if (f != null && f.name == "Perpetua") arial = f;

                foreach (var font in Font.GetOSInstalledFontNames())
                {
                    if (!font.ToLower().Contains("arial")) continue;
                    arial = Font.CreateDynamicFontFromOSFont(font, 13);
                }
            }

            if (trajanBold == null || arial == null) PantheonsHitCounter.instance.LogError("Could not find game fonts");

            var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            foreach (var res in resourceNames)
            {
                if (!res.StartsWith("PantheonsHitCounter.UI.Images.")) continue;
                
                try
                {
                    var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
                    var buffer = new byte[imageStream.Length];
                    imageStream.Read(buffer, 0, buffer.Length);

                    var texture = new Texture2D(1, 1);
                    texture.LoadImage(buffer.ToArray());

                    var split = res.Split('.');
                    var internalName = split[split.Length - 2];
                    images.Add(internalName, texture);

                    PantheonsHitCounter.instance.Log("Loaded image: " + internalName);
                }
                catch (Exception e)
                {
                    PantheonsHitCounter.instance.LogError("Failed to load image: " + res + "\n" + e);
                }
            }
            
            PantheonsHitCounter.instance.Log("Resources loaded.");
        }

        public static ResourcesLoader Instance
        {
            get
            {
                if (_instance != null) return _instance;
                
                _instance = FindObjectOfType<ResourcesLoader>();
                if (_instance != null) return _instance;

                var guiObj = new GameObject();
                _instance = guiObj.AddComponent<ResourcesLoader>();
                DontDestroyOnLoad(guiObj);
                return _instance;
            }
        }

        public void Destroy()
        {
            CounterUI.Destroy();
        }
    }
}