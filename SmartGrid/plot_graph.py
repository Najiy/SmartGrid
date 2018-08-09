import numpy as np
import matplotlib.pyplot as plt
import matplotlib.lines as lines
import sys

listA = []

fig = plt.figure()
axes = plt.gca()
fig.add_axes(axes)


for vectors in listA:
    for i in range(0, len(vectors)-4, 2):
        x2 = vectors[i], vectors[i + 2]
        y2 = vectors[i+1], vectors[i + 3]
        plt.plot(x2, y2, marker='o')

plt.show()
