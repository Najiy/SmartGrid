import matplotlib.pyplot as plt
import urllib.request
import json
import sys 

#filepath as argument
with open(sys.argv[1]) as f:
    data = json.load(f)

road_nodes = data['SmartGrid']['RoadNodes']
road_links = data['SmartGrid']['RoadLinks']
osm_tile = data['OsmTile']
xtile = osm_tile['XCoord']
ytile = osm_tile['YCoord']
address = 'http://a.tile.openstreetmap.fr/hot/16/{}/{}.png'.format(xtile, ytile)
filename = '{}.{}.png'.format(xtile, ytile)
urllib.request.urlretrieve(address, filename)
list_osor = []
list_osm = []
q = []
p=[]
for i in road_links:
    node = road_links[i]['Vectors']
    for j in node:
        if 'OSOR' in j['NodeFrom']:
            q.append(road_nodes[j['NodeFrom']]['Coordinate']['Latitude'])
            q.append(road_nodes[j['NodeFrom']]['Coordinate']['Longitude'])
            q.append(road_nodes[j['NodeTo']]['Coordinate']['Latitude'])
            q.append(road_nodes[j['NodeTo']]['Coordinate']['Longitude'])
        if 'OSM' in j['NodeFrom']:
            p.append(road_nodes[j['NodeFrom']]['Coordinate']['Latitude'])
            p.append(road_nodes[j['NodeFrom']]['Coordinate']['Longitude'])
            p.append(road_nodes[j['NodeTo']]['Coordinate']['Latitude'])
            p.append(road_nodes[j['NodeTo']]['Coordinate']['Longitude'])
    list_osor.append(q)
    list_osm.append(p)
    q = []
    p =[]


img = plt.imread(filename)

fig = plt.subplots()

for vectors in list_osor:
    for i in range(0, len(vectors)-3, 2):
        x2 = vectors[i], vectors[i + 2]
        y2 = vectors[i+1], vectors[i + 3]
        plt.plot(x2, y2, marker='o', color='firebrick')

for vectors in list_osm:
    for i in range(0, len(vectors)-3, 2):
        x2 = vectors[i], vectors[i + 2]
        y2 = vectors[i+1], vectors[i + 3]
        plt.plot(x2, y2, marker='o', color='blue')


max_x = float(sys.argv[2])
min_x = float(sys.argv[4])
max_y = float(sys.argv[3])
min_y = float(sys.argv[5])
scale = (max_x - min_x) / (max_y - min_y)
plt.imshow(img, extent=[min_x, max_x, min_y, max_y], aspect=scale)
plt.show()
