using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public Player player { get; private set; }

    public InputActionReference PauseAction;

    IslandAwayTrigger islandAway;

    int islandAwayTimes = 0;

    int islandAwayButtonPresses = 0;

    public static UI UI { get; set; }

    public static Game game { get; private set; }

    private bool pause = true;

    private float platformDirection = -1f;

    private Coroutine DoNothingEnding;

    private Button island2Button;
    private Button treeButton;
    private Button sunButton;
    

    void Start()
    {
        game = this;

        PauseAction.action.started += OnPause;
        PauseAction.action.Enable();

        player = transform.Find("player").GetComponent<Player>();
        islandAway = transform.Find("island_2/island_away_trigger").GetComponent<IslandAwayTrigger>();

        if (SceneManager.sceneCount == 1)
        {
            SceneManager.LoadScene("ui", LoadSceneMode.Additive);
            PlayerPause(true);
        }
        else
        {
            UI.UpdateSettings();
            StartGame();
        }
            
        player.OnPlayerDeathEvent += OnPlayerDeath;
        islandAway.OnTriggerEvent += OnIslandAway;

        transform.Find("island_1/button").GetComponent<Button>().OnButtonPressEvent += delegate() {
            Transform island = transform.Find("island_3");
            Transform platforms = transform.Find("island_3_platforms");

            island.gameObject.SetActive(true);
            platforms.gameObject.SetActive(true);

            Move(island, 1.5f, island.position.y * Vector3.down);
            foreach (Transform platform in platforms)
                Move(platform, 1.5f, platforms.position.y * Vector3.down);
        };

        treeButton = transform.Find("island_1/tree/button").GetComponent<Button>();
        treeButton.OnButtonPressEvent += delegate() {
            Transform island = transform.Find("island_1/island");
            Transform forest = transform.Find("forest");
            forest.gameObject.SetActive(true);

            transform.Find("island_1/button").gameObject.SetActive(false);
            transform.Find("island_2").gameObject.SetActive(false);
            transform.Find("island_2_platforms").gameObject.SetActive(false);
            transform.Find("island_3").gameObject.SetActive(false);
            transform.Find("island_3_platforms").gameObject.SetActive(false);
            transform.Find("island_4").gameObject.SetActive(false);
            transform.Find("island_4_platforms").gameObject.SetActive(false);
            transform.Find("island_5").gameObject.SetActive(false);

            List<Vector3> origScales = new List<Vector3>();
            foreach (Transform tree in forest) {
                    origScales.Add(tree.localScale);
                }
            StartCoroutine(AnimationHelper.RunAnimation(2f, delegate(float timePerc, float secondsPassed) {
                int i = 0;
                foreach (Transform tree in forest) {
                    tree.localScale = origScales[i] * timePerc;
                    ++i;
                }
                island.localScale = new Vector3(1f, 1f, 1f) + new Vector3(9f, 0f, 9f) * timePerc;
            }, AnimationHelper.EaseOut, AnimationHelper.SineFunction));
     
            StartCoroutine(AnimationHelper.RunOnce(delegate() {
                Ending(2, "Like a forest, friendship doesn't grow in a day.\nBut not this forest.");
            }, 5f));

            CancelDoNothingEnding();
        };

        Button platformButton = transform.Find("island_2_platforms/platform_05/button").GetComponent<Button>();
        platformButton.OnButtonPressEvent += delegate() {
            ++islandAwayButtonPresses;
            if (islandAwayButtonPresses <= 5) {
                MoveIslandAway();
            }
            else {
                if (platformDirection < 0f)
                    player.Checkpoint = transform.Find("island_2/checkpoint");
                else
                    player.Checkpoint = transform.Find("island_1/checkpoint");

                Transform platform = transform.Find("island_2_platforms/platform_05");
                Move(platform, 1f, Vector3.forward * 303.2f * platformDirection);
                platform.GetComponent<AudioSource>().Play();
                platformDirection = -platformDirection;
            }
        };

        transform.Find("island_3/button").GetComponent<Button>().OnButtonPressEvent += delegate() {
            Transform island = transform.Find("island_4");
            Transform platforms = transform.Find("island_4_platforms");

            island.gameObject.SetActive(true);
            platforms.gameObject.SetActive(true);

            Move(island, 2f, Vector3.down * 50f);
            Move(platforms, 2f, platforms.position.y * Vector3.down);
        };

        transform.Find("island_4/button").GetComponent<Button>().OnButtonPressEvent += delegate() {    
            Transform island = transform.Find("island_4");
            Move(island, 2f, new Vector3(0f, island.position.y * -2f, 0f));
            foreach (Transform platform in transform.Find("island_4_platforms"))
                Move(platform, 2f, new Vector3(0f, platform.position.y * -2f, 0f));

            transform.Find("platform_01").gameObject.SetActive(true);
            transform.Find("island_5").gameObject.SetActive(true);
            island.GetComponent<AudioSource>().Play();
        };
        transform.Find("island_5/button").GetComponent<Button>().OnButtonPressEvent += delegate() {
            Transform platforms = transform.Find("island_2_platforms");
            platforms.gameObject.SetActive(true);
            foreach (Transform platform in platforms)
                Move(platform, 2f, platform.position.y * Vector3.down);
        };

        island2Button = transform.Find("island_2/button").GetComponent<Button>();
        island2Button.OnButtonPressEvent += delegate() {
            transform.Find("island_2/boat_horn").GetComponent<AudioSource>().Play();
            StartCoroutine(AnimationHelper.RunOnce(delegate() {
                Ending(1, "Farewell? Or a new beginning?\nThat, or just a boat.");
            }, 4f));
        };

        sunButton = transform.Find("island_4/sun_button").GetComponent<Button>();
        sunButton.OnButtonPressEvent += delegate() {
            RenderSettings.skybox = Resources.Load<Material>("material/skybox_lit");
            StartCoroutine(AnimationHelper.RunOnce(delegate() {
                Ending(3, "There was no light. And then it was.");
            }, 3f));
        };
    }

    void Ending(int num, string text)
    {
        pause = true;
        PlayerPause(true);
        UI.EndingMenu(num, 5, text);
    }

    private void OnPause(InputAction.CallbackContext obj)
    {
        pause = !pause;
        PlayerPause(pause);
        UI.PauseMenu(pause);
        UI.ReplaceMainMenuWithPauseMenu();
        if (pause)
            CancelDoNothingEnding();
    }


    void PlayerPause(bool state)
    {
        player.Controller.SetActive(!state);
        player.SetCrosshairActive(!state);
        //player.GetComponent<Rigidbody>().isKinematic = state;
    }

    public void StartGame()
    {
        ResumeGame();
        Debug.Log("DoNothingEnding coroutine has started.");
        DoNothingEnding = StartCoroutine(AnimationHelper.RunOnce(delegate() {
            Ending(4, "Hello? Are you there?\nYou've been doing nothing for 10 mintes.");
        }, 600f));
        RenderSettings.skybox = Resources.Load<Material>("material/skybox");
    }

    public void OnButtonPress(Button button)
    {
        if (button != island2Button && button != treeButton && button != sunButton) {
            if (Random.Range(0, 1001) == 0) {
                Ending(5, "You got lucky.\nThere's a 1/1000 chance of getting this ending when pressing a button.");
            }
        }
    }

    void CancelDoNothingEnding()
    {
        if (DoNothingEnding != null) {
            StopCoroutine(DoNothingEnding);
            Debug.Log("DoNothingEnding coroutine has been cancelled.");
            DoNothingEnding = null;
        }
    }

    public void ResumeGame()
    {
        pause = false;
        PlayerPause(false);
    }

    void OnPlayerDeath()
    {
    }

    void OnIslandAway()
    {
        ++islandAwayTimes;
        
        MoveIslandAway();

        if (islandAwayTimes == 1) {
            transform.Find("island_1/button").gameObject.SetActive(true);
            transform.Find("island_2/checkpoint_zone").gameObject.SetActive(true);
            CancelDoNothingEnding();
        }
        if (islandAwayTimes == 2) {
            transform.Find("island_2_platforms/platform_05/button").gameObject.SetActive(true);
            transform.Find("island_2/island_away_trigger").gameObject.SetActive(false);
        }
    }

    void MoveIslandAway()
    {
        Transform island = transform.Find("island_2");
        island.GetComponent<AudioSource>().Play();
        Move(island, 1f, Vector3.back * 50f);
    }

    void Move(Transform obj, float time, Vector3 distance)
    {
        Vector3 initPos = obj.position;
        StartCoroutine(AnimationHelper.RunAnimation(time, delegate(float timePerc, float secondsPassed) {
            obj.position = initPos + distance * timePerc;
        }, AnimationHelper.EaseOut, AnimationHelper.SineFunction));
    }

    void MoveAt(Transform obj, float time, Vector3 position)
    {
        Vector3 distance = position - obj.position;
        Move(obj, time, distance);
    }
}
