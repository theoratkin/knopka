using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class UI : MonoBehaviour
{
    private const float DefaultSensitivity = 8f;
    private const float DefaultWebSensitivity = 4f;

    public bool Vsync { get; private set; }

    private Transform canvas;
    private Transform main;
    private Transform pause;
    private Transform settings;
    private Transform ending;
    private AudioSource music;
    private AudioSource[] sfx;

    private float fov = 60f;
    private float sensitivity = DefaultSensitivity;
    private float musicVolume = 50f;
    private float sfxVolume = 100f;
    private int antiAliasing = 3;

    void Start()
    {
        canvas = transform.Find("canvas");
        main = canvas.Find("main_menu");
        pause = canvas.Find("pause_menu");
        ending = canvas.Find("ending");

        Game.UI = this;
        
        main.Find("play").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            OnPlayClick();
            ReplaceMainMenuWithPauseMenu();
        };
        pause.Find("continue").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            OnContinueClick();
        };
        ending.Find("continue").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            OnContinueClick();
            pause.gameObject.SetActive(true);
            ending.gameObject.SetActive(false);  
        };
        pause.Find("restart").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            OnRestartClick();
        };
        ending.Find("restart").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            OnRestartClick();
            pause.gameObject.SetActive(true);
            ending.gameObject.SetActive(false);  
        };
        main.Find("settings").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            OnSettingsClick();
        };
        pause.Find("settings").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            OnSettingsClick();
        };
        main.Find("credits").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            OnCreditsClick();
        };
        pause.Find("credits").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            OnCreditsClick();
        };
        main.Find("exit").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            Application.Quit();
        };
        pause.Find("exit").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            Application.Quit();
        };
        ending.Find("exit").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            Application.Quit();
        };

        settings = canvas.Find("settings");
        SetFullscreen(Screen.fullScreen);
        settings.Find("fullscreen").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            SetFullscreen(!Screen.fullScreen);
        };
        settings.Find("antialiasing").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            ++antiAliasing;
            if (antiAliasing > 3)
                antiAliasing = 0;
            var postProcessing = Game.game.player.camera.GetComponent<PostProcessLayer>();
            
            string text = "";
            switch(antiAliasing) {
                case (0):
                    postProcessing.antialiasingMode = PostProcessLayer.Antialiasing.None;
                    text = "None";
                    break;
                case (1):
                    postProcessing.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                    postProcessing.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Low;
                    text = "Low";
                    break;
                case (2):
                    postProcessing.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Medium;
                    text = "Medium";
                    break;
                case (3):
                    postProcessing.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.High;
                    text = "High";
                    break;
            }
            
            settings.Find("antialiasing/text").GetComponent<Text>().text = text;
        };
        settings.Find("vsync").GetComponent<UIButton>().OnButtonPressEvent += delegate() {
            Vsync = !Vsync;
            settings.Find("vsync/text").GetComponent<Text>().text = Vsync ? "ON" : "OFF";
            QualitySettings.vSyncCount = Vsync ? 1 : 0;
        };

        Slider fovSlider = settings.Find("fov").GetComponent<Slider>();
        fovSlider.onValueChanged.AddListener(SetFOV);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            sensitivity = DefaultWebSensitivity;
        Slider sensSlider = settings.Find("sensitivity").GetComponent<Slider>();
        sensSlider.onValueChanged.AddListener(SetSensitivity);
       
        music = GameObject.Find("music").GetComponent<AudioSource>();

        Slider musicSlider = settings.Find("music").GetComponent<Slider>();
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        
        Slider sfxSlider = settings.Find("sfx").GetComponent<Slider>();
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);

        if (Game.game)
            UpdateSettings();
    }

    public void ReplaceMainMenuWithPauseMenu()
    {
        main.gameObject.SetActive(false);
        pause.gameObject.SetActive(true);
    }

    public void UpdateSettings()
    {
        sfx = Game.game.gameObject.GetComponentsInChildren<AudioSource>(true);
        
        SetSfxVolume(sfxVolume);
        SetFOV(fov);
        SetSensitivity(sensitivity);
    }

    void SetFullscreen(bool state)
    {
        Screen.fullScreen = state;
        settings.Find("fullscreen/text").GetComponent<Text>().text = state ? "ON" : "OFF";
    }

    void SetMusicVolume(float newVolume)
    {
        music.volume = newVolume / 100f;
        settings.Find("music_val").GetComponent<Text>().text = ((int)newVolume).ToString();
        musicVolume = newVolume;
    }

    void SetSfxVolume(float newVolume)
    {
        foreach (AudioSource sound in sfx)
            sound.volume = newVolume / 100f;
        settings.Find("sfx_val").GetComponent<Text>().text = ((int)newVolume).ToString();
        sfxVolume = newVolume;
    }

    void SetFOV(float newFOV)
    {
        Game.game.player.Controller.Camera.fieldOfView = newFOV;
        settings.Find("fov_val").GetComponent<Text>().text = ((int)newFOV).ToString();
        fov = newFOV;
    }

    void SetSensitivity(float newVal)
    {
        Game.game.player.Controller.MouseSensitivity = newVal;
        settings.Find("sensitivity").GetComponent<Slider>().SetValueWithoutNotify(newVal);
        settings.Find("sensitivity_val").GetComponent<Text>().text = newVal.ToString("0.0");
        sensitivity = newVal;
    }

    public void EndingMenu(int num, int total, string text)
    {
        gameObject.SetActive(true);
        pause.gameObject.SetActive(false);
        settings.gameObject.SetActive(false);
        canvas.Find("credits").gameObject.SetActive(false);
        ending.gameObject.SetActive(true);
        ending.Find("text").GetComponent<Text>().text = string.Format("Ending {0} of {1}", num, total);
        ending.Find("desc").GetComponent<Text>().text = text;
    }

    public void PauseMenu(bool state)
    {
        ending.gameObject.SetActive(false);
        gameObject.SetActive(state);
    }

    void OnSettingsClick()
    {
        canvas.Find("credits").gameObject.SetActive(false);
        GameObject s = canvas.Find("settings").gameObject;
        s.SetActive(!s.activeSelf);
    }

    void OnCreditsClick()
    {
        canvas.Find("settings").gameObject.SetActive(false);
        GameObject cr = canvas.Find("credits").gameObject;
        cr.SetActive(!cr.activeSelf);
    }

    public void OnPlayClick()
    {
        Game.game.StartGame();
        gameObject.SetActive(false);
    }

    public void OnContinueClick()
    {
        Game.game.ResumeGame();
        gameObject.SetActive(false);
    }

    public void OnRestartClick()
    {
        SceneManager.UnloadSceneAsync("main");
        SceneManager.LoadScene("main", LoadSceneMode.Additive);
        gameObject.SetActive(false);
    }
}
