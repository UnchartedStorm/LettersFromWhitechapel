/*
Name:             Bruno Sprenger
UvANetID:         13084151
Personal Project: Letters from Whitechapel
Examinator:       F. Speelman

This script controls Jack. It is attached to the Jack gameobject.
*/

using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class JackScript : MonoBehaviour {
    private static GameObject GameControl;
    private static GameObject Algorithms;

    // Last input variable.
    private string lastInput = "";

    [SerializeField]
    public Text outputText;

    // Points where Jack can move to.
    public Transform[] points;

    // Starting the game with Jack's turn.
    public bool jackTurn = true;
    public bool autoJack = false;

    [HideInInspector]
    public int pointIndex = 0;
    public int number = 1;

    // Starting options.
    // public int[] options = {3, 21, 27, 65, 84, 147, 149, 158};
    public int[] options = {149}; // To speed up learning and showing results. This also needs to be changed in Unity.


    string[] numbers = File.ReadAllLines("Assets/numbers.txt");

    // Start is called before the first frame update.
    void Start() {
        if (autoJack == true) {
            outputText.text = "Auto Jack";
        }
        else {
            // Display all options from the options array.
            outputText.text = "Options:\n" + string.Join("\n", options.Select(x => x.ToString()).ToArray());
        }

        // Hide the object until the user enters the home location.
        gameObject.SetActive(false);
        GameControl = GameObject.Find("board");
    }

    // Update is called once per frame.
    void Update() {
        if (jackTurn == true) {
            if (autoJack == true) {
                AutoMove();
            }
            else {
                if (lastInput != "") {
                    InputMove();
                }
            }
        }
    }

    // This function automatically moves Jack based on the shortest path.
    public void AutoMove() {
        // If no move has been made yet, choose a random starting point from the options array.
        if (pointIndex == 0) {
            StartingMove();
            return;
        }

        // Calculate shortest path using algorithms.cs.
        Algorithms = GameObject.Find("Main Camera");

        // Get the shortest path from the algorithms script from the current jack location to home.
        int home = GameControl.GetComponent<GameControl>().jackHomeLocation;
        int[] shortestPath = Algorithms.GetComponent<Algorithms>().findShortestPath(pointIndex, home);

        if (shortestPath == null) {
            // Random possible move from options array.
            pointIndex = options[Random.Range(0, options.Length)];
        }
        // If the path is not empty, move to the next point in the path.
        else if (shortestPath.Length != 0) {
            pointIndex = shortestPath[1];
        }
        // If the path is empty, Jack has been caught.
        else if (shortestPath.Length == 0) {
            Debug.Log("Jack has been caught");
        }

        // Move Jack to the selected point.
        transform.position = points[pointIndex - 1].transform.position;

        // Add pointIndex to the jackPath array.
        GameControl.GetComponent<GameControl>().jackPath.Add(pointIndex);

        // Set jackLocation to the current pointIndex.
        GameControl.GetComponent<GameControl>().jackLocation = pointIndex;

        // Set jackTurn to false and set next turn to true.
        jackTurn = false;
        GameControl.GetComponent<GameControl>().next = true;

        // Update last seen location to the starting point.
        if (GameControl.GetComponent<GameControl>().lastSeenLocation == 0) {
            // Debug.Log("Last Seen Location: " + pointIndex);
            GameControl.GetComponent<GameControl>().lastSeenLocation = pointIndex;
        }

        if (shortestPath != null){ // Making sure shortestPath.Length can be used.
            if (shortestPath.Length == 2) {
                GameControl.GetComponent<GameControl>().JackHome();
            }
        }

    }

    // This function moves Jack to a random starting point.
    public void StartingMove() {
        // Choose a random number from the options array.
        pointIndex = options[Random.Range(0, options.Length)];

        // Move Jack to the selected point.
        transform.position = points[pointIndex - 1].transform.position;

        // Add pointIndex to the jackPath array.
        GameControl.GetComponent<GameControl>().jackPath.Add(pointIndex);

        // Set jackLocation to the current pointIndex.
        GameControl.GetComponent<GameControl>().jackLocation = pointIndex;

        // Set jackTurn to false and set next turn to true.
        jackTurn = false;
        GameControl.GetComponent<GameControl>().next = true;

        // Update last seen location to the starting point.
        if (GameControl.GetComponent<GameControl>().lastSeenLocation == 0) {
            GameControl.GetComponent<GameControl>().lastSeenLocation = pointIndex;
        }
    }

    // This function moves Jack based on user input.
    public void InputMove() {
        number = int.Parse(lastInput) - 1;

        // Check if the number is in the options array and allowed.
        if (number <= points.Length && options.Contains(number + 1)) {
            // Move Jack to the selected point.
            transform.position = points[number].transform.position;

            // Set the current point index to the selected point.
            pointIndex = number + 1;

            // Grab the options from the number grid.
            string gridOptions = numbers[number];

            // Display all options from the options array.
            options = gridOptions.Split(' ').Select(int.Parse).ToArray();
            outputText.text = "Options:\n" + string.Join("\n", options.Select(x => x.ToString()).ToArray());

            // Add pointIndex to the jackPath array.
            GameControl.GetComponent<GameControl>().jackPath.Add(pointIndex);

            // Set jackLocation to the current pointIndex.
            GameControl.GetComponent<GameControl>().jackLocation = pointIndex;

            // Set jackTurn to false and set next turn to true.
            jackTurn = false;
            GameControl.GetComponent<GameControl>().next = true;

            // Update last seen location to the starting point.
            if (GameControl.GetComponent<GameControl>().lastSeenLocation == 0) {
                GameControl.GetComponent<GameControl>().lastSeenLocation = pointIndex;
            }
        }
    }

    // This function gets called by the input field and is used to get the last input.
    public void GetInputText(InputField input)
    {
        lastInput = input.text.ToLower();

        // Set game object active if the user has entered the first command.
        if (lastInput != "" && gameObject.activeSelf == false && options.Contains(int.Parse(lastInput)))
        {
            gameObject.SetActive(true);

            // Start the game object out on the first input.
            number = int.Parse(lastInput) - 1;
            transform.position = points[number].transform.position;
        }
    }

}