del *.bin
del *.inc
del *.pscompr
del *.psgcompr
del *.aPLib

..\bmp2tile.exe akmw.png      -removedupes -mirror -savetiles akmwtiles.inc               -savetilemap akmwtilemap.inc              -palcl123 -savepalette "akmw (palette).inc"     -exit
..\bmp2tile.exe sonic.pcx     -removedupes -mirror -savetiles sonictiles.bin              -savetilemap sonictilemap.bin             -palsms   -savepalette "sonicpalette.bin"       -exit
..\bmp2tile.exe ps.gif        -removedupes -mirror -savetiles "ps (tiles).pscompr"        -savetilemap "ps (tilemap).pscompr"       -palsms   -savepalette "ps (palette).bin"       -exit
..\bmp2tile.exe BBR.bmp       -removedupes -mirror -savetiles "BBR (tiles).psgcompr"      -savetilemap "BBR (tilemap).pscompr"      -palsms   -savepalette "BBR (palette).bin"      -exit
..\bmp2tile.exe populous.png  -removedupes -mirror -savetiles "populous (tiles).aPLib"    -savetilemap "populous (tilemap).aPLib"   -palsms   -savepalette "populous (palette).bin" -exit
