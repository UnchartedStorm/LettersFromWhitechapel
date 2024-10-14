/*
Name:             Bruno Sprenger
UvANetID:         13084151
Personal Project: Letters from Whitechapel
Examinator:       F. Speelman

This file contains the majority of algorithms used in the game.
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class Algorithms : MonoBehaviour {
    /*
    This class contains the shortest path algorithm and the blocking algorithm.
    These are used for Jack to find the shortest path home.
    This class is attached to the letters from whitechapel (cover) gameobject.
    In Unity this object is hidden so it doesn't cover the board. (Little eye next to the object in the hierarchy)
    */
    private static GameObject GameControl;

    string[] numbers = File.ReadAllLines("Assets/numbers.txt");
    string[] full = File.ReadAllLines("Assets/graph.txt");

    // Start is called before the first frame update
    void Start() {
        GameControl = GameObject.Find("board");
    }


    public int[] findShortestPath(int start, int end) {
        /*
        This function finds the shortest path between two points using a breadth first search with Dijkstra's algorithm.
        */

        Queue<int> queue = new Queue<int>();

        // Enqueue the starting location.
        queue.Enqueue(start);

        bool[] visited = new bool[196];

        // Mmark the starting location as visited.
        visited[start] = true;

        int[] previous = new int[196];

        // While the queue is not empty.
        while (queue.Count != 0)
        {
            int vertex = queue.Dequeue();
            string line = numbers[vertex - 1];

            // Get the adjacent vertices of the dequeued vertex from the numbers graph.
            int[] adjacentVertices = line.Split(' ').Select(int.Parse).ToArray();

            foreach (int adjacentVertex in adjacentVertices)
            {
                // If the path is blocked by a detective then skip the adjacent vertex.
                if (Blocked(vertex, adjacentVertex)) {
                    continue;
                }

                // If the adjacent vertex has not been visited.
                if (!visited[adjacentVertex])
                {
                    // Mark the adjacent vertex as visited.
                    visited[adjacentVertex] = true;

                    queue.Enqueue(adjacentVertex);

                    // Set the previous vertex of the adjacent vertex to the vertex.
                    previous[adjacentVertex] = vertex;
                }
            }
        }

        List<int> path = new List<int>();

        // Add the end to the path.
        path.Add(end);

        int currentVertex = end;

        // While the current vertex is not the start.
        while (currentVertex != start)
        {
            // Set the current vertex to the previous vertex of the current vertex.
            currentVertex = previous[currentVertex];

            if (currentVertex == 0) {
                Debug.Log("No path found");
                return null;
            }

            path.Add(currentVertex);
        }

        path.Reverse();

        return path.ToArray();
    }

    // Checks if a path is blocked by a detective.
    public bool Blocked(int p1, int p2) {
        /*
        This function checks if a path is blocked by a detective. It does this by checking if there is a path between
        the two points, if there is then the path is not blocked. If there is no path then the path is blocked.
        This is done by using a depth first search. Squares occupied by detectives are not considered and are skipped.
        Numbers that are not the target number are also skipped.
        */

        // Grab all detective locations from GameControl and put them in an array.
        string detectiveBlueLocation = "s" + GameControl.GetComponent<GameControl>().detectiveBlueLocation.ToString();
        string detectiveGreenLocation = "s" + GameControl.GetComponent<GameControl>().detectiveGreenLocation.ToString();
        string detectiveRedLocation = "s" + GameControl.GetComponent<GameControl>().detectiveRedLocation.ToString();
        string detectiveBrownLocation = "s" + GameControl.GetComponent<GameControl>().detectiveBrownLocation.ToString();
        string detectiveYellowLocation = "s" + GameControl.GetComponent<GameControl>().detectiveYellowLocation.ToString();

        string[] detectiveLocations = {detectiveBlueLocation, detectiveGreenLocation, detectiveRedLocation, detectiveBrownLocation, detectiveYellowLocation};

        Stack<int> stack = new Stack<int>();

        // Push the starting point onto the stack.
        stack.Push(p1);

        bool[] visited = new bool[430];

        // Mark the starting point as visited.
        visited[p1] = true;

        int[] previous = new int[430];

        // While the stack is not empty
        while (stack.Count != 0)
        {
            int vertex = stack.Pop();

            // Increment the vertex by 234 to get the right index in the full graph.
            if (p1 == vertex) {
                vertex += 234;
            }

            string[] adjacentVertices = full[vertex - 1].Split(' ');

            foreach (string adjacentVertex in adjacentVertices)
            {
                // If the adjacent vertex is the target vertex then return true.
                if (adjacentVertex == ("n" + p2.ToString())) {
                    return false;
                }

                // If the adjacent vertex is a number then skip.
                if (adjacentVertex.Contains("n")) {
                    continue;
                }

                // If a detective is on the adjacent vertex then skip.
                if (detectiveLocations.Contains(adjacentVertex)) {
                    continue;
                }

                // Convert the adjacent vertex to an int strip the identifier off.
                int adjacentVertexInt = int.Parse(adjacentVertex.Substring(1));

                // If the adjacent vertex has not been visited.
                if (!visited[adjacentVertexInt])
                {
                    // Mark the adjacent vertex as visited.
                    visited[adjacentVertexInt] = true;

                    stack.Push(adjacentVertexInt);

                    // Set the previous vertex of the adjacent vertex to the vertex.
                    previous[adjacentVertexInt] = vertex;
                }
            }
        }

        return true;
    }
}

[Serializable]
public class Node {
    // This class represents a node in the Monte Carlo Tree Search algorithm.
    public int visits;
    public int score;
    public int wins;
    public List<Node> children;
    public GameState state;

    public Node(GameState state) {
        // This constructor is used to create the root node.
        this.visits = 0;
        this.score = 0;
        this.wins = 0;
        this.children = new List<Node>();
        this.state = state;
    }
}

public class MCTS {
    /*
    This class contains the Monte Carlo Tree Search algorithm.
    It also contains all actions that can be performed by the algorithm.
    Further more it decides the move to be made by the UCB1 formula.
    */
    public GameObject GameControl;
    public Node root;
    public List <int> treePath = new List<int>();

    // Start is called before the first frame update.
    public void Start() {
        GameControl = GameObject.Find("board");

        // Check if a tree already has been saved to a file (tree.dat).
        // If it has deserialize it.
        if (File.Exists("tree.dat")) {
            root = TreeSerializer.DeserializeTree("tree.dat");
            Debug.Log("Tree loaded");
        }
        // If it hasn't then create a new tree.
        else {
            root = new Node(GameControl.GetComponent<GameControl>().GetGameState());
        }
    }

    // Saves the tree to a file.
    public void Save() {
        Debug.Log("Saving tree");

        Node root = GameControl.GetComponent<GameControl>().mcts.root;

        TreeSerializer.SerializeTree(root, "tree.dat");
    }

    // Loads the tree from a file.
    // public void Load() {
    //     Node root = TreeSerializer.DeserializeTree("tree.dat");
    // }

    // This function checks if the current node already has a child with the same state.
    public (bool, int) CheckChildren(Node child) {
        Node parent = CurrentNode();
        int[] childLocations = {child.state.lastSeenLocation, child.state.detectiveBlueLocation, child.state.detectiveGreenLocation, child.state.detectiveRedLocation, child.state.detectiveBrownLocation, child.state.detectiveYellowLocation};
        int childNr = 0;

        foreach (Node node in parent.children) {
            int[] nodeLocations = {node.state.lastSeenLocation, node.state.detectiveBlueLocation, node.state.detectiveGreenLocation, node.state.detectiveRedLocation, node.state.detectiveBrownLocation, node.state.detectiveYellowLocation};

            if (Enumerable.SequenceEqual(childLocations, nodeLocations)) {
                // If it does then update the parent's child's score and visits.
                // Debug.Log("Child already exists");
                return (true, childNr);
            }

            childNr++;
        }

        return (false, 0);
    }

    // This function adds a child to the current node.
    public void AddChild() {
        Node child = new Node(GameControl.GetComponent<GameControl>().GetGameState());

        (bool, int) checkChildren = CheckChildren(child);

        // Check if the parent already has this child.
        if (!checkChildren.Item1) {
            // If it doesn't then add the child.
            CurrentNode().children.Add(child);

            // Check how many children the parent has and add the number to the treePath.
            treePath.Add(CurrentNode().children.Count - 1);
        }
        else {
            // If it does then add the number of the child to the treePath.
            treePath.Add(checkChildren.Item2);
        }

        // Set next turn in GameControl to true.
        GameControl.GetComponent<GameControl>().next = true;
    }

    // This method returns the current node by following the treePath.
    public Node CurrentNode() {
        if (treePath.Count == 0) {
            return root;
        }

        Node currentNode = root;

        foreach (int move in treePath) {
            currentNode = currentNode.children[move];
        }

        return currentNode;
    }

    public void BackPropagate(int jackFound) {
        /*
        This function backpropagates the score up the tree, updating the score and visits of each node.
        We do this by following the treePath and updating the score and visits of each node.
        */
        Node currentNode = root;

        // Update the root node.
        currentNode.visits += 1;

        int nrSeenLocations = currentNode.state.seenLocations.Count;
        int step = currentNode.state.step;
        int round = currentNode.state.round;

        int newScore = (10 * jackFound * (20 - step) * (5 - round)) + (1 * nrSeenLocations) + step;

        if (currentNode.visits == 1) {
            currentNode.score = newScore;
        } else {
            // Calculating the new average score.
            currentNode.score = (currentNode.score * (currentNode.visits - 1) + newScore) / currentNode.visits;
        }

        if (jackFound == 1) {
            currentNode.wins += 1;
        }

        foreach (int move in treePath) {
            currentNode = currentNode.children[move];
            currentNode.visits += 1;

            nrSeenLocations = currentNode.state.seenLocations.Count;
            step = currentNode.state.step;
            round = currentNode.state.round;

            newScore = (10 * jackFound * (20 - step) * (5 - round)) + (1 * nrSeenLocations) + step;

            if (currentNode.visits == 1) {
                currentNode.score = newScore;
            } else {
                // Calculating the new average score.
                currentNode.score = (currentNode.score * (currentNode.visits - 1) + newScore) / currentNode.visits;
            }

            if (jackFound == 1) {
                currentNode.wins += 1;
            }
        }

        Debug.Log("Backpropagated");
    }

    public int MoveChoice(int[] options, string colorDetective) {
        /*
        This function chooses a move based on the UCB1 formula or score.
        */
        int[] scoreOptions = new int[options.Length];
        int[] winsOptions = new int[options.Length];
        int[] visitsOptions = new int[options.Length];

        Node currentNode = CurrentNode();

        // Check all the children of the current node to match the move options and collect the corresponding weighted options.
        foreach (Node child in currentNode.children) {
            int childLocation;
            switch (colorDetective) {
                case "detective_bl":
                    childLocation = child.state.detectiveBlueLocation + 1;
                    break;
                case "detective_g":
                    childLocation = child.state.detectiveGreenLocation + 1;
                    break;
                case "detective_r":
                    childLocation = child.state.detectiveRedLocation + 1;
                    break;
                case "detective_br":
                    childLocation = child.state.detectiveBrownLocation + 1;
                    break;
                case "detective_y":
                    childLocation = child.state.detectiveYellowLocation + 1;
                    break;
                default:
                    childLocation = 0;
                    Debug.Log("Error: No detective found");
                    break;
            }

            // Get index of the child's location in the options array.
            int index = Array.IndexOf(options, childLocation);

            // If the index is -1 then the child's location is in a new round.
            // This means we should return a random option to avoid errors.
            if (index == -1) {
                return options[UnityEngine.Random.Range(0, options.Length)];
            }

            // Add the child's score to the weighted options.
            scoreOptions[index] = child.score;
            winsOptions[index] = child.wins;
            visitsOptions[index] = child.visits;
        }

        // Apply the UCB1 formula to the score and wins options, wins are better than score.
        double[] ucb1Options = new double[options.Length];

        for (int i = 0; i < options.Length; i++) {
            if (visitsOptions[i] == 0) {
                ucb1Options[i] = 0;
            } else {
                double c = Mathf.Sqrt(2); // Exploration parameter naturally set to sqrt(2).
                ucb1Options[i] = (winsOptions[i] / visitsOptions[i]) + c * Math.Sqrt(Math.Log(currentNode.visits) / visitsOptions[i]);
            }
        }

        // If all the wins options are 0 and the scores are also 0 then pick a random option.
        // This is where the MCTS goes wrong, it doesn't know how to handle new rounds.
        if (winsOptions.All(x => x == 0) && scoreOptions.All(x => x == 0)) {
            return options[UnityEngine.Random.Range(0, options.Length)];
        }
        // If else all the options are 0 then base it off the highest score.
        else if (ucb1Options.All(x => x == 0)) {
            return options[scoreOptions.ToList().IndexOf(scoreOptions.Max())];
        }

        return options[ucb1Options.ToList().IndexOf(ucb1Options.Max())];
    }

    public void PrintTree() {
        /*
        This function prints the tree to the PrintedTree.txt file.
        Printing the tree from left to right in the text file.
        The first column is the root node. Every time a node gets a child it its in the next column.
        If a node already has a child it will be printed in the same column right under the other child.

        The tree is printed in the following format:
        root child1 child1.1
                    child1.2
             child2 child2.1
                    child2.2

        Each node is printed in the following format:
        wins/visits
        */
        Node currentNode = CurrentNode();

        // Create a new file. Print the tree depth first.
        using (StreamWriter sw = File.CreateText("PrintedTree.txt")) {
            string root = currentNode.wins + "/" + currentNode.visits;
            // Call the recursive function to print the tree.
            PrintTreeRecursive(root, currentNode, sw, 0);
        }
    }

    public void PrintTreeRecursive(string line, Node currentNode, StreamWriter sw, int depth) {
        // If the current node has no children then return.
        if (currentNode.children.Count == 0) {
            sw.WriteLine(line);
            return;
        }

        int lineL = line.Length;

        foreach (Node child in currentNode.children) {
            // only print every 6th depth.
            if (depth % 6 == 0) {
                // continue;

                // Add the child to the line.
                line += " " + child.wins + "/" + child.visits;
            }

            // Call the recursive function to print the tree.
            PrintTreeRecursive(line, child, sw, depth + 1);

            // Replace the line with empty spaces with the same length.
            line = new String(' ', lineL);
        }
    }
}

public class TreeSerializer {
    /*
    This class contains the functions to serialize and deserialize the tree.
    */
    public static void SerializeTree(Node root, string filePath)
    {
        using (Stream stream = File.Open("tree.dat", FileMode.Create)) {
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, root);
        }

        Debug.Log("Serialized");
    }

    public static Node DeserializeTree(string filePath)
    {
        Node tree;

        using (Stream stream = File.Open("tree.dat", FileMode.Open)) {
            BinaryFormatter bformatter = new BinaryFormatter();
            tree = (Node)bformatter.Deserialize(stream);
        }

        Debug.Log("Deserialized");

        Debug.Log("root children D: " + tree.children.Count);
        Debug.Log("Visits: " + tree.visits);

        return tree;
    }
}




