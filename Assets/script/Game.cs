using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public Player player { get; private set; }
    IslandAwayTrigger islandAway;

    int islandAwayTimes = 0;

    int islandAwayButtonPresses = 0;

    public static UI UI { get; set; }

    public static Game game { get; private set; }

    private bool pause = true;

    private float platformDirection = -1f;
    

    void Start()
    {
        game = this;

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

        transform.Find("island_1/tree/button").GetComponent<Button>().OnButtonPressEvent += delegate() {
            if (islandAwayTimes == 0) {
                Debug.Log("NO! You weren't supposed to do that yet! Congratulations, you broke the game. I hope you are happy. Now get out.");
                return;
            }

            Transform platforms = transform.Find("island_2_platforms");
            platforms.gameObject.SetActive(true);
            foreach (Transform platform in platforms)
                Move(platform, 2f, platform.position.y * Vector3.down);
        };

        Button platformButton = transform.Find("island_2_platforms/platform_05/button").GetComponent<Button>();
        platformButton.OnButtonPressEvent += delegate() {
            ++islandAwayButtonPresses;
            if (islandAwayButtonPresses <= 5) {
                MoveIslandAway();
            }
            else {
                //Animator anim = transform.Find("island_2_platforms/platform_05").GetComponent<Animator>();
                //anim.enabled = true;
                //anim.GetComponent<Animator>().Play("move");

                if (platformDirection < 0f)
                    player.Checkpoint = transform.Find("island_2/checkpoint");
                else
                    player.Checkpoint = transform.Find("island_1/checkpoint");

                Transform platform = transform.Find("island_2_platforms/platform_05");
                Move(platform, 1f, Vector3.forward * 303.2f * platformDirection);
                platform.GetComponent<AudioSource>().enabled = true;
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
        };

        transform.Find("island_2/button").GetComponent<Button>().OnButtonPressEvent += delegate() {
            transform.Find("island_2/boat_horn").GetComponent<AudioSource>().Play();
            StartCoroutine(AnimationHelper.RunOnce(delegate() {
                Ending(1, "What's that sound?");
            }, 3f));
        };
    }

    void Ending(int num, string text)
    {
        pause = true;
        PlayerPause(true);
        UI.EndingMenu(num, 3, text);
    }

    void Update()
    {
        if(Input.GetButtonDown("Cancel")) {
            pause = !pause;
            PlayerPause(pause);
            UI.PauseMenu(pause);
            UI.ReplaceMainMenuWithPauseMenu();
        }
    }

    void PlayerPause(bool state)
    {
        player.Controller.ControllerPause(state);
        player.SetCrosshairActive(!state);
        //player.GetComponent<Rigidbody>().isKinematic = state;
    }

    public void StartGame()
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
            transform.Find("island_2/island/island").GetComponent<Island>().enabled = true;
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
