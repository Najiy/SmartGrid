import numpy as np
import matplotlib.pyplot as plt
import matplotlib.lines as lines
import sys
import urllib.request
from scipy.misc import imread
from decimal import Decimal

listA = []

listA.append([55.7752959982134,-5.48264974581061,55.7751428312336,-5.48253732857289])
listA.append([55.7754312679236,-5.4828384115201,55.7753045773685,-5.48306366621972,55.7753045773685,-5.48306366621972,55.7752474783264,-5.4831652856906,55.7752474783264,-5.4831652856906,55.7751906663107,-5.48326629242805,55.7751906663107,-5.48326629242805,55.7749978793386,-5.48328592531414,55.7749978793386,-5.48328592531414,55.7748844498228,-5.48316413396082,55.7748844498228,-5.48316413396082,55.7748640904988,-5.48267738341022])


xtile = sys.argv[1]
ytile = sys.argv[2]
max_lat = Decimal(sys.argv[3])
max_lon = Decimal(sys.argv[4])
min_lat = Decimal(sys.argv[5])
min_lon = Decimal(sys.argv[6])
address = 'http://a.tile.openstreetmap.fr/hot/16/{}/{}.png'.format(xtile, ytile)
filename = '{}.{}.png'.format(xtile, ytile)
urllib.request.urlretrieve(address, filename)

img = imread(filename)
fig, ax = plt.subplots()
ax.imshow(img)
# fuck knows why this doesn't work 
for vectors in listA:
    for i in range(0, len(vectors)-4, 2):
        x2 = vectors[i], vectors[i + 2]
        y2 = vectors[i+1], vectors[i + 3]
        ax.plot(x2, y2, marker='o', color='firebrick')

ax.imshow(img, extent=[min_lat, max_lat, min_lon, max_lon])
plt.show()
