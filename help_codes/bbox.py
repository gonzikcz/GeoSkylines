# UPDATE VALUES HERE
x,y = 485463.721,4287814.414
inProjTxt = 'epsg:32629'
# UPDATE VALUES HERE



from pyproj import Proj, transform
import os


half = 17280/2

bbox_utm_wkt = "\"POLYGON (("

# point a = x – 8640 and y - 8640
xa = x-half
ya = y-half
bbox_utm_wkt += str(xa)
bbox_utm_wkt += " "
bbox_utm_wkt += str(ya)
bbox_utm_wkt += ", "

# point b = x + 8640 and y – 8640
xb = x+half
yb = y-half
bbox_utm_wkt += str(xb)
bbox_utm_wkt += " "
bbox_utm_wkt += str(yb)
bbox_utm_wkt += ", "

# point c = x + 8640 and y + 8640
xc = x+half
yc = y+half
bbox_utm_wkt += str(xc)
bbox_utm_wkt += " "
bbox_utm_wkt += str(yc)
bbox_utm_wkt += ", "

# point d = x – 8640 and y + 8640
xd = x-half
yd = y+half
bbox_utm_wkt += str(xd)
bbox_utm_wkt += " "
bbox_utm_wkt += str(yd)
bbox_utm_wkt += ", "

# close polygon by going back to point a
bbox_utm_wkt += str(xa)
bbox_utm_wkt += " "
bbox_utm_wkt += str(ya)
bbox_utm_wkt += "))\""

#print(bbox_utm_wkt)

path = os.environ['TEMP']
file_path = os.path.join(path, "bbox_utm.csv")
with open(file_path, 'w') as bbox_utm:
    bbox_utm.write('id,wkt\n')
    bbox_utm.write('1,'+bbox_utm_wkt+'\n')
    

inProj = Proj(init=inProjTxt)
outProj = Proj(init='epsg:4326')
lon,lat = transform(inProj,outProj,x,y)
lona,lata = transform(inProj,outProj,xa,ya)
lonb,latb = transform(inProj,outProj,xb,yb)
lonc,latc = transform(inProj,outProj,xc,yc)
lond,latd = transform(inProj,outProj,xd,yd)

bbox_wgs_wkt = "\"POLYGON (("

# point a
bbox_wgs_wkt += str(lona)
bbox_wgs_wkt += " "
bbox_wgs_wkt += str(lata)
bbox_wgs_wkt += ","

# point b
bbox_wgs_wkt += str(lonb)
bbox_wgs_wkt += " "
bbox_wgs_wkt += str(latb)
bbox_wgs_wkt += ","

# point c
bbox_wgs_wkt += str(lonc)
bbox_wgs_wkt += " "
bbox_wgs_wkt += str(latc)
bbox_wgs_wkt += ","

# point d
bbox_wgs_wkt += str(lond)
bbox_wgs_wkt += " "
bbox_wgs_wkt += str(latd)
bbox_wgs_wkt += ","

# point a
bbox_wgs_wkt += str(lona)
bbox_wgs_wkt += " "
bbox_wgs_wkt += str(lata)
bbox_wgs_wkt += "))\""

file_path = os.path.join(path, "bbox_wgs.csv")
with open(file_path, 'w') as bbox_wgs:
    bbox_wgs.write('id,wkt\n')
    bbox_wgs.write('1,'+bbox_wgs_wkt+'\n')
    
# prepare URL for Terrain.party
lons = [lona, lonb, lonc, lond]
lats = [lata, latb, latc, latd]
maxLon = max(lons)
minLon = min(lons)
maxLat = max(lats)
minLat = min(lats)
terrainParty_URL ="http://terrain.party/api/export?name=MyArea&box="
terrainParty_URL += str(maxLon)
terrainParty_URL += ","
terrainParty_URL += str(maxLat)
terrainParty_URL += ","
terrainParty_URL += str(minLon)
terrainParty_URL += ","
terrainParty_URL += str(minLat)

#gdal_text = "-projwin "
#gdal_text += 

file_path = os.path.join(path, "cs_area_info.txt")
with open(file_path, 'w') as cs_area:    
    cs_area.write('CenterLongitude: ' + str(lon) + '\n')
    cs_area.write('CenterLatitude: ' + str(lat) + '\n')
    cs_area.write('Terrain.party: ' + terrainParty_URL + '\n')
