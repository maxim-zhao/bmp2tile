@echo off
for %%e in (bin pscompr psgcompr aPLib soniccompr) do (
  del *.%%e
  for %%f in (*.bmp *.png *.pcx *.gif) do (
    ..\bmp2tile "%%f" -savetiles "%%~nf (tiles).%%e" -exit
  )
  echo Extension: %%e
  dir *.%%e | find "File(s)"
)
pause