/*
Name:             Bruno Sprenger
UvANetID:         13084151
Personal Project: Letters from Whitechapel
Examinator:       F. Speelman

This script is used to control the game. It contains all the variables used in the game and the functions to change them.

Tree.dat contains the MCTS tree. Currently it is trained on Jack's home location being 89.
If you want to train it on another location, delete Tree.dat and run the game.
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class GameState {
    /*
    This class contains the current game state. It is used to keep track of the game state in the MCTS tree.
    */
    public int lastSeenLocation;
    public List<int> seenLocations;
    public int detectiveBlueLocation;
    public int detectiveGreenLocation;
    public int detectiveRedLocation;
    public int detectiveBrownLocation;
    public int detectiveYellowLocation;
    public int round;
    public int step;

    public GameState(int lastSeenLocation, List<int> seenLocations, int detectiveBlueLocation, int detectiveGreenLocation, int detectiveRedLocation, int detectiveBrownLocation, int detectiveYellowLocation, int round, int step) {
        this.lastSeenLocation = lastSeenLocation;
        this.seenLocations = seenLocations;
        this.detectiveBlueLocation = detectiveBlueLocation;
        this.detectiveGreenLocation = detectiveGreenLocation;
        this.detectiveRedLocation = detectiveRedLocation;
        this.detectiveBrownLocation = detectiveBrownLocation;
        this.detectiveYellowLocation = detectiveYellowLocation;
        this.round = round;
        this.step = step;
    }
}

public class GameControl : MonoBehaviour {
    /*
    This class is attached to the board gameobject. It is used to control the game.
    */

    // Declare gameobjects to interact with in Unity
    private static GameObject jack, detectiveBlue, detectiveGreen, detectiveRed, detectiveBrown, detectiveYellow, Algorithms;
    public GameObject button, cover, options, moveInput, enter;

    // Setting all variables to be used in the game
    public int jackLocation = 0;
    public int detectiveBlueLocation = 0;
    public int detectiveGreenLocation = 0;
    public int detectiveRedLocation = 0;
    public int detectiveBrownLocation = 0;
    public int detectiveYellowLocation = 0;

    public int jackHomeLocation = 0;
    public int lastSeenLocation = 0;
    public List<int> seenLocations;

    public bool jackHome = false;
    public int jackMoves = 0;
    public List<int> jackPath;

    // Turn ints: jack = 0, detectiveBlue = 1, detectiveGreen = 2, detectiveRed = 3, detectiveBrown = 4, detectiveYellow = 5.
    public int turn = 0;
    public bool next = false;
    public int round = 1;

    public int totalGames = 0;

    public MCTS mcts;

    // Start is called before the first frame update.
    void Start() {
        // Set camera aspect ratio.
        Camera.main.aspect = 16f / 9f;

        detectiveYellowLocation = 55;
        detectiveBlueLocation = 127;
        detectiveGreenLocation = 196;
        detectiveRedLocation = 182;
        detectiveBrownLocation = 77;

        // Connect gameobjects to their respective variables.
        jack = GameObject.Find("jack");
        detectiveBlue = GameObject.Find("detective_bl");
        detectiveGreen = GameObject.Find("detective_g");
        detectiveRed = GameObject.Find("detective_r");
        detectiveBrown = GameObject.Find("detective_br");
        detectiveYellow = GameObject.Find("detective_y");

        // Disable all ui elements.
        options.SetActive(false);
        moveInput.SetActive(false);
        enter.SetActive(false);

        // Create MCTS tree.
        mcts = new MCTS();

        // Set up the save file of the MCTS tree.
        mcts.Start();
        // mcts.Save();
        // mcts.Load();

        Debug.Log("root children: " + mcts.root.children.Count);
    }

    // Update is called once per frame and is used to check if the next turn can be started.
    void Update() {
        if (next == true) {
            NextTurn();
        }
    }

    // This function is called in JackScript.cs when Jack has reached home.
    public void JackHome() {
        // Jack has reached home so the round is over and the game is reset.
        ResetVariables(0);
    }

    // This function gives the next turn to the next player.
    void NextTurn() {
        // Turn ints: jack = 0, detectiveBlue = 1, detectiveGreen = 2, detectiveRed = 3, detectiveBrown = 4, detectiveYellow = 5.
        turn = (turn + 1) % 6;

        switch (turn) {
            case 0:
                if (jackMoves > 15) {
                    // If jack has made more than 15 moves, he loses.
                    Debug.Log("Jack loses: out of moves");
                    // Jack has lost so backpropagate 1.
                    ResetVariables(1);
                }

                jack.GetComponent<JackScript>().jackTurn = true;
                jackMoves++;
                break;

            case 1:
                detectiveBlue.GetComponent<DetectiveScript>().detectiveTurn = true;
                break;

            case 2:
                detectiveGreen.GetComponent<DetectiveScript>().detectiveTurn = true;
                break;

            case 3:
                detectiveRed.GetComponent<DetectiveScript>().detectiveTurn = true;
                break;

            case 4:
                detectiveBrown.GetComponent<DetectiveScript>().detectiveTurn = true;
                break;

            case 5:
                detectiveYellow.GetComponent<DetectiveScript>().detectiveTurn = true;
                break;

            default:
                break;
        }

        // The tree is updated with the new move. After this is done this function will set the next turn to true.
        mcts.AddChild();
    }

    // This function gets called by a button in the UI and is used to start the game.
    public void getHomeLocation(InputField input) {
        // If input field is empty, choose a random location 1-195
        if (input.text == "") {
            // jackHomeLocation = UnityEngine.Random.Range(1, 196);
            jackHomeLocation = 89; // Randomly picked by my partner to show results faster.
        }
        else {
            // If input field is not empty, set jackHomeLocation to the input
            jackHomeLocation = int.Parse(input.text);
        }
        Debug.Log("Jack's home is " + jackHomeLocation);

        // Deactivate the input field, button and cover
        input.gameObject.SetActive(false);
        button.SetActive(false);
        cover.SetActive(false);

        // If autoJack is true, we can start the game by setting jack gameobject to active
        if (jack.GetComponent<JackScript>().autoJack == true) {
            jack.SetActive(true);
        }
        else {
            // Activate the options, enter and input field
            enter.SetActive(true);
            moveInput.SetActive(true);
        }

        options.SetActive(true);
    }

    // Whenever a round ends or the game ends, this function is called to reset all variables.
    public void ResetVariables(int jackCaught) {
        lastSeenLocation = 0;
        jackHome = false;
        jackPath.Clear();
        jackMoves = 0;

        // Reset all detective locations
        detectiveYellow.GetComponent<DetectiveScript>().squarepointIndex = 55;
        detectiveBlue.GetComponent<DetectiveScript>().squarepointIndex = 127;
        detectiveGreen.GetComponent<DetectiveScript>().squarepointIndex = 196;
        detectiveRed.GetComponent<DetectiveScript>().squarepointIndex = 182;
        detectiveBrown.GetComponent<DetectiveScript>().squarepointIndex = 77;

        // Reset all detective locations in GameControl
        detectiveYellowLocation = 55;
        detectiveBlueLocation = 127;
        detectiveGreenLocation = 196;
        detectiveRedLocation = 182;
        detectiveBrownLocation = 77;

        turn = 0;
        next = false;
        jack.GetComponent<JackScript>().pointIndex = 0;
        jack.GetComponent<JackScript>().jackTurn = true;

        round++;

        // If Jack has survived 4 rounds, he wins.
        if (round == 5) {
            Debug.Log("Game over");
            Debug.Log("Jack seen: " + seenLocations.Count);
            Debug.Log("============================================");

            // Jack not caught so backpropagate 0
            mcts.BackPropagate(jackCaught);

            mcts.treePath.Clear();
            seenLocations.Clear();
            round = 1;
            totalGames++;

            if (totalGames % 5 == 0) {
                // Print the tree to a file.
                mcts.PrintTree();
                mcts.Save();
                // Debug.Break();
            }
        }

        if (jackCaught == 1) {
            // Jack caught so backpropagate 1
            mcts.BackPropagate(jackCaught);

            mcts.treePath.Clear();
            seenLocations.Clear();
            round = 1;
            totalGames++;

            Debug.Log("Jack is caught");
            Debug.Log("Jack seen: " + seenLocations.Count);
            Debug.Log("============================================");

            if (totalGames % 5 == 0) {
                // Print the tree to a file.
                mcts.PrintTree();
                mcts.Save();
                // Debug.Break();
            }
        }

        if (totalGames == 1000) {
            // Print the tree to a file.
            mcts.PrintTree();
            mcts.Save();
            Debug.Break();
        }
    }

    // This function gets the current game state for another script.
    public GameState GetGameState() {
        return new GameState(lastSeenLocation, seenLocations, detectiveBlueLocation, detectiveGreenLocation, detectiveRedLocation,
                             detectiveBrownLocation, detectiveYellowLocation, round, jackMoves);
    }
}