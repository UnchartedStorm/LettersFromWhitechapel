# This file reads squareneighbours.txt and creates a file numberneighbours.txt.
# Each line in squareneighbours respresents a square point, the line contains a number of points that are adjacent to that square.
# This file reverses that, creating a file where each line represents a point and the line contains a number of squares that are adjacent to that point.
# So if line 1 contains 24 in squareneighbours.txt, then line 24 in numberneighbours.txt will contain 1.

with open('squareneighbours.txt', 'r') as f:
    lines = f.readlines()

    # there are 234 squares and 195 points
    numberneighbours = [''] * 195

    # enumerate through each line
    for i, line in enumerate(lines):
        if line == '\n':
            continue
        # split the line into a list of numbers
        numbers = line.split(' ')
        # enumerate through each number
        for number in numbers:
            # add the number to the
            numberneighbours[int(number) - 1] += str(i + 1) + ' '

    # write the numberneighbours list to a file
    with open('numberneighbours.txt', 'w') as f:
        for line in numberneighbours:
            f.write(line + '\n')

