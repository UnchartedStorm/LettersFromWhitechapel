# This file takes the black square grid from squares.txt and inserts the white points into the graph,
# this creates a full graph with both the black squares and the white points.
# The graph is then saved to a file called graph.txt. Black squares will get a s in front of their number, white points will get a p.
# Squareneighbours.txt contains the numbers that are adjacent to each square.
# Numberneighbours.txt contains the squares that are adjacent to each number.

# There are 234 squares and 195 points, so the graph will have 429 nodes starting with 1p and ending with 234s.
# Each line in the txt file will represent a point, so the black squares will be start from 1 and end at 234 and the white points will start from 235 and end at 429.

import re

# Read the black squares from squares.txt.
squares = open('squares.txt', 'r').readlines()

# Read the squareneighbours squares from squareneighbours.txt.
squareneighbours = open('squareneighbours.txt', 'r').readlines()

# Read the numberneighbours squares from numberneighbours.txt.
numberneighbours = open('numberneighbours.txt', 'r').readlines()

# Make a copy of squares called graph that we can modify.
graph = squares.copy()

# Make each number in graph start with a s to indicate that it is a black square. There can be multiple squares on a line.
for i in range(len(graph)):
    graph[i] = re.sub(r'(\d+)', r'n\1', graph[i])

# Save the graph to graph.txt.
with open('graph.txt', 'w') as f:
    for item in graph:
        f.write("%s" % item)

# Go through each line in squares.
for (i, line) in enumerate(squares):
    # Read what points are connected to the current square from squareneighbours.
    pointToInsert = squareneighbours[i].split()

    # Convert line to a list of strings called slist and add an s to each number.
    slist = ['s' + number for number in line.split()]
    toRemove = []

    for point in pointToInsert:
        # Grab the numberneighbours squares from numberneighbours.txt for the current point.
        adjacentSquares = numberneighbours[int(point)-1].split()

        # Intersect the adjacent squares with the squares in the line.
        intersect = list(set(adjacentSquares) & set(line.split()))

        # Replace the squares from intersect s# with the point p#.
        for square in intersect:
            print('s' + square + ' ' + 'n' + point)
            slist.append('n' + point)
            toRemove.append('s' + square)

    # Remove the numbers from toRemove from the line.
    for number in toRemove:
        if number in slist:
            slist.remove(number)


    # Remove duplicates from the slist.
    slist = list(dict.fromkeys(slist))
    # Save the slist to graph.txt.
    graph[i] = slist


# Add from 235 to 429 all numberneighbours squares straight from numberneighbours.txt.
for (i, line) in enumerate(numberneighbours):
    # Add the line to the graph but add s to each number.
    graph.append(['s' + number for number in line.split()])


# Hand fix the edge cases of two connecting numbers without a square in between
# numbers: 26-28, 45-47
# This means all the squares around these numbers should be fixed
# s4 -> s5 p1 p26
# s20 -> s21 p8 p26 p28
# s19 -> s18 s41 p7 p26
# s44 -> s45 p26 p27
# s46 -> s47 p27 p28
# p26 -> s4 s20 s19 s44 p28
# p28 -> s20 s46 p26

# s49 -> p45 p47 p61 s51
# s50 -> p45 p46 p47 s47
# s51 -> p47 p61 s49
# s47 -> s46 s53 p46 p48 s50
# p45 -> s49 s50 p47
# p47 -> s49 s50 s51 p45


graph[3] = ['s5', 'n1', 'n26']
graph[19] = ['s21', 'n8', 'n26', 'n28']
graph[18] = ['s18', 's41', 'n7', 'n26']
graph[43] = ['s45', 'n26', 'n27']
graph[45] = ['s47', 'n27', 'n28']
graph[259] = ['s4', 's20', 's19', 's44', 'n28']
graph[261] = ['s20', 's46', 'n26']

graph[48] = ['n45', 'n47', 'n61', 's51']
graph[49] = ['n45', 'n46', 'n47', 's47']
graph[50] = ['n47', 'n61', 's49']
graph[46] = ['s46', 's53', 'n46', 'n48', 's50']
graph[278] = ['s49', 's50', 'n47']
graph[280] = ['s49', 's50', 's51', 'n45']

# Save the graph to graph.txt, each line represents a point.
with open('graph.txt', 'w') as f:
    for item in graph:
        f.write("%s\n" % ' '.join(item))
