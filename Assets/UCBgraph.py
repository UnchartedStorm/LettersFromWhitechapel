import matplotlib.pyplot as plt
import numpy as np

# Winrates data
constants = [0, 0.5, 1, np.sqrt(2), 1.5, 2, 2.5, 3]
winrates = np.array([
    [8, 20, 23],
    [43, 78, 31],
    [28, 39, 58],
    [15, 77, 40],
    [32, 32, 43],
    [21, 26, 85],
    [26, 34, 51],
    [9, 19, 21]

]) / 1000  # Convert to percentages


# Calculate lowest and highest values
lowest_values = np.min(winrates, axis=1)
highest_values = np.max(winrates, axis=1)

# Plotting with error bars
plt.figure(figsize=(10, 6))
plt.errorbar(constants, np.mean(winrates, axis=1), yerr=[np.mean(winrates, axis=1) - lowest_values, highest_values - np.mean(winrates, axis=1)],
             marker='o', linestyle='-', color='b', capsize=5, label='Average Winrate', ecolor='black', elinewidth=1)
plt.title('Winrates with UCB1 Formula after a 1000 games')
plt.xlabel('UCB Constant')
plt.ylabel('Winrate (%)')

# footnote stating the amount of runs, each data point has been run a 1000 times 3 times
# plt.figtext(0.99, 0.01, "Each constant variation has been run a 1000 times 3 times, resulting in 3 win rates for each constant.", horizontalalignment='right')
plt.legend()
plt.grid(True)
plt.show()
