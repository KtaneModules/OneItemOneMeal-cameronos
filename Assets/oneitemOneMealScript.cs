using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class oneitemOneMealScript : MonoBehaviour {

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

	// Use this for initialization
	void Awake()
	{
			moduleId = moduleIdCounter++;
	}

//cycleNumbers
//txt.GetComponent<UnityEngine.UI.Text>().text = score.ToString();
// private void cycleNumbers()
//     {
// 			foreach (int i in calories)
// 			{
// 				CounterText.GetComponent<TextMesh>().text = calories[0+1];
// 			}
//     }

	void Start ()
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
		Debug.LogFormat(correctTime.ToString());
	}

		void DetermineCorrectTime()
		{
			if(BombInfo.GetSerialNumberLetters().Any(x => x.EqualsAny('A', 'E', 'I', 'O', 'U')))
			{
				correctTime = 5;
			}
			else if(BombInfo.GetBatteryCount() == 4)
			{
				correctTime = 1;
			}
			else
			{
				correctTime = 3;
			}
		}

		void PickImage()
		{
			 imageIndex = UnityEngine.Random.Range(0,8);
			 image.material = imageOptions[imageIndex];
			 Debug.LogFormat("The image is {1}.", moduleId, imageOptions[imageIndex].name);
		}

		private KMSelectable.OnInteractHandler ButtonPressed(int btn)
    {
        return delegate
        {
						buttons[btn].AddInteractionPunch();
						Audio.PlaySoundAtTransform("ButtonPress", buttons[btn].transform);
						if(btn == 0)
						{
							currentIndex++;
							if(currentIndex == 8) currentIndex = 0;
							selectedCalorie = calories[currentIndex];
							CounterText.text = selectedCalorie.ToString();
						}
						if(btn == 1)
						{
							currentIndex--;
							if(currentIndex == -1) currentIndex = 7;
							selectedCalorie = calories[currentIndex];
							CounterText.text = selectedCalorie.ToString();
						}
						if(btn == 2)
						{
							selectedCalorie = calories[currentIndex];
							if(selectedCalorie == correctCalorie)
							{
								if((int)BombInfo.GetTime() % 10 == correctTime)
								{
									Audio.PlaySoundAtTransform("Solved", buttons[btn].transform);
									Module.HandlePass();
								}
								else
								{
									Audio.PlaySoundAtTransform("Strike", buttons[btn].transform);
									Module.HandleStrike();
								}
							}
							else
							{
								Audio.PlaySoundAtTransform("Solved", buttons[btn].transform);
								Module.HandleStrike();
							}
						}
            return false;
        };
    }

}

//Thanks to GoodHood for assisting in the code.
//Thanks Ambitious Outsiders (Terrace Stomp)
//Good people of the world
