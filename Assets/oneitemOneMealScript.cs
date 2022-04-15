using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class oneitemOneMealScript : MonoBehaviour
{

    public KMAudio Audio;
    public AudioClip[] sounds;
    public KMBombInfo BombInfo;
    public KMSelectable[] buttons;
    public TextMesh CounterText;
    public GameObject FoodItem;
    public KMBombModule Module;

    private int TargetTime;
    private int TargetIndex;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    //initial
    public static string code = "";
    public static string inputCode = "";

    //calories
    int correctTime = 0;
    int currentIndex = 0;
    int selectedCalorie = 0;
    int correctCalorie = 0;
    int[] calories = { 526, 400, 394, 612, 160, 205, 725, 1254 };

    //image for item
    public Material[] imageOptions;
    public Renderer image;
    private int imageIndex = 0;

    private static readonly string[] _foodNames = new string[8] { "Cake", "Ramen", "Pizza", "Pie", "Chips", "Donut", "Tikka", "Steak" };

    // Use this for initialization
    void Awake()
    {
        moduleId = moduleIdCounter++;
    }

    void Start()
    {
        PickImage();
        for (int btn = 0; btn < buttons.Length; btn++)
        {
            buttons[btn].OnInteract = ButtonPressed(btn);
        }
        selectedCalorie = calories[currentIndex];
        CounterText.text = selectedCalorie.ToString();
        correctCalorie = calories[imageIndex];
        DetermineCorrectTime();
        Debug.LogFormat("[One Item One Meal #{0}] The number below the image must be set to {1}.", moduleId, correctCalorie);
        Debug.LogFormat("[One Item One Meal #{0}] The submit button must be pressed when the last digit of the timer is {1}.", moduleId, correctTime);
    }

    void DetermineCorrectTime()
    {
        if (BombInfo.GetSerialNumberLetters().Any(x => x.EqualsAny('A', 'E', 'I', 'O', 'U')))
            correctTime = 5;
        else if (BombInfo.GetBatteryCount() == 4)
            correctTime = 1;
        else
            correctTime = 3;
    }

    void PickImage()
    {
        imageIndex = UnityEngine.Random.Range(0, 8);
        image.material = imageOptions[imageIndex];
        Debug.LogFormat("[One Item One Meal #{0}] The shown image is {1}.", moduleId, _foodNames[imageIndex]);
    }

    private KMSelectable.OnInteractHandler ButtonPressed(int btn)
    {
        return delegate
        {
            buttons[btn].AddInteractionPunch();
            Audio.PlaySoundAtTransform("ButtonPress", buttons[btn].transform);
            if (btn == 0)
            {
                currentIndex++;
                if (currentIndex == 8) currentIndex = 0;
                selectedCalorie = calories[currentIndex];
                CounterText.text = selectedCalorie.ToString();
            }
            if (btn == 1)
            {
                currentIndex--;
                if (currentIndex == -1) currentIndex = 7;
                selectedCalorie = calories[currentIndex];
                CounterText.text = selectedCalorie.ToString();
            }
            if (btn == 2)
            {
                selectedCalorie = calories[currentIndex];
                if (selectedCalorie == correctCalorie)
                {
                    if ((int)BombInfo.GetTime() % 10 == correctTime)
                    {
                        Debug.LogFormat("[One Item One Meal #{0}] Correctly submitted {1} on a {2}. Module solved.", moduleId, selectedCalorie, correctTime);
                        Audio.PlaySoundAtTransform("Solved", buttons[btn].transform);
                        Module.HandlePass();
                    }
                    else
                    {
                        Debug.LogFormat("[One Item One Meal #{0}] Incorrectly submitted {1} on a {2}. Strike.", moduleId, selectedCalorie, correctTime);
                        Audio.PlaySoundAtTransform("Strike", buttons[btn].transform);
                        Module.HandleStrike();
                    }
                }
                else
                {
                    Debug.LogFormat("[One Item One Meal #{0}] Incorrectly submitted {1} on a {2}. Strike.", moduleId, selectedCalorie, correctTime);
                    Audio.PlaySoundAtTransform("Strike", buttons[btn].transform);
                    Module.HandleStrike();
                }
            }
            return false;
        };
    }

#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} submit 526 at 5. [Set the number to 526, then submit when the last digit of the timer is a 5.";
#pragma warning restore 0414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        var m = Regex.Match(command, @"^\s*submit\s+(\d+)\s+at\s+(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            yield break;
        int cal, time;
        if (!int.TryParse(m.Groups[1].Value, out cal) || !int.TryParse(m.Groups[2].Value, out time) || !calories.Contains(cal) || time < 0 || time > 9)
            yield break;
        yield return null;
        var target = Array.IndexOf(calories, cal);
        while (target != currentIndex)
        {
            var distance = (Math.Abs(currentIndex - target) + 4) % 8 - 4;
            if (currentIndex > target)
                distance *= -1;
            if (distance > 0)
                buttons[0].OnInteract();
            else if (distance < 0)
                buttons[1].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        while ((int)BombInfo.GetTime() % 10 != time)
            yield return null;
        buttons[2].OnInteract();
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        var target = Array.IndexOf(calories, correctCalorie);
        while (target != currentIndex)
        {
            var distance = (Math.Abs(currentIndex - target) + 4) % 8 - 4;
            if (currentIndex > target)
                distance *= -1;
            if (distance > 0)
                buttons[0].OnInteract();
            else if (distance < 0)
                buttons[1].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        while ((int)BombInfo.GetTime() % 10 != correctTime)
            yield return null;
        buttons[2].OnInteract();
    }
}

//Thanks to GoodHood for assisting in the code.
//Thanks Ambitious Outsiders (Terrace Stomp)
//Good people of the world
