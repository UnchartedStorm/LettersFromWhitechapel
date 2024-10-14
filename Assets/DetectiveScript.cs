/*
Name:             Bruno Sprenger
UvANetID:         13084151
Personal Project: Letters from Whitechapel
Examinator:       F. Speelman

This script is attached to the detective gameobjects. It is used to control the detectives.
Each detective is a different gameobject and has a different name. However, they all use the same script.
To make sure that the correct detective is moved, the name of the detective is used in the Move() function.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class DetectiveScript : MonoBehaviour {
    public Transform[] squarepoints;

    [SerializeField]
    public int squarepointIndex; // set to y 55, bl 127, g 196, r 182, br 77 in Unity

    public bool detectiveTurn = false;

    private static GameObject GameControl;

    string[] squareneighbours = File.ReadAllLines("Assets/squareneighbours.txt");     // Points that detectives can ask about
    string[] squares = File.ReadAllLines("Assets/squares.txt");                       // Detectives grid

    // Start is called before the first frame update.
    void Start() {
        transform.position = squarepoints[squarepointIndex].transform.position;

        GameControl = GameObject.Find("board");
    }

    // Update is called once per frame.
    void Update() {
        if (detectiveTurn == true) {
            Move();
            Guess();

            // Set the detective turn to false.
            detectiveTurn = false;
            GameControl.GetComponent<GameControl>().next = true;
        }
    }

    // This function tries to guess where jack is.
    void Guess() {
        // Look up guessable points from the current square.
        if (squareneighbours[squarepointIndex] == "") {
            return;
        }

        int[] guessOptions = squareneighbours[squarepointIndex].Split(' ').Select(int.Parse).ToArray();

        for (int i = 0; i < guessOptions.Length; i++) {
            // Pick a random guess from the guessOptions array to see if jack has been there.
            int ranNum = Random.Range(0, guessOptions.Length);
            int guess = guessOptions[ranNum];

            // Check if jack has been to the guess square by checking jackPath from GameControl.
            if (GameControl.GetComponent<GameControl>().jackPath.Contains(guess)) {
                // Add the guess to the seenLocations list.
                GameControl.GetComponent<GameControl>().seenLocations.Add(guess);
                // Set the lastSeenLocation to the guess.
                GameControl.GetComponent<GameControl>().lastSeenLocation = guess;

                break;
            }
            else {
                // Remove the guess from the guessOptions array.
                guessOptions = guessOptions.Where(val => val != guess).ToArray();
            }
        }
    }

    // This function moves the detective.
    void Move() {
        // Get options to move for the detective.
        string squaregridOptions = squares[squarepointIndex];

        // Convert string to a List of ints and subtract 1 from each value.
        List<int> squaregridOptionsInt = squaregridOptions.Split(' ').Select(int.Parse).ToList();

        // Add the current square to the squaregridOptionsInt array.
        squaregridOptionsInt.Add(squarepointIndex + 1);

        // Convert the list to an array.
        int[] newOptions = squaregridOptionsInt.ToArray();

        // Get the MCTS from GameControl.
        MCTS mcts = GameControl.GetComponent<GameControl>().mcts;

        // Get the move choice from the MCTS.
        int moveChoice = mcts.MoveChoice(newOptions, gameObject.name) - 1;

        // Set the squarepointIndex to the moveChoice.
        squarepointIndex = moveChoice;

        if (squarepointIndex != newOptions.Length) {
            // Move the detective to the selected square.
            transform.position = squarepoints[squarepointIndex].transform.position;
        }

        // Update the detective location in GameControl.
        switch (gameObject.name) {
            case "detective_y":
                GameControl.GetComponent<GameControl>().detectiveYellowLocation = squarepointIndex;
                break;
            case "detective_bl":
                GameControl.GetComponent<GameControl>().detectiveBlueLocation = squarepointIndex;
                break;
            case "detective_g":
                GameControl.GetComponent<GameControl>().detectiveGreenLocation = squarepointIndex;
                break;
            case "detective_r":
                GameControl.GetComponent<GameControl>().detectiveRedLocation = squarepointIndex;
                break;
            case "detective_br":
                GameControl.GetComponent<GameControl>().detectiveBrownLocation = squarepointIndex;
                break;
            default:
                break;
        }
    }
}
