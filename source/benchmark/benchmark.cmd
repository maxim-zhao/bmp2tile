@echo off
setlocal
setlocal enabledelayedexpansion
set bmp2tile=..\bmp2tile.exe
set exts=pscompr psgcompr soniccompr aPLib PuCrunch

call :compress
call :report
pause
goto :eof

:compress
for %%f in (*.png;*.gif;*.bmp) do (
  for %%e in (%exts% bin) do (
    if not exist "%%f.%%e" %bmp2tile% "%%f" -savetiles "%%f.%%e" -exit
  )
)
goto :eof

:report
set /a totalUncompressed = 0
for %%b in ("*.bin") do set /a totalUncompressed=totalUncompressed + %%~zb
for %%e in (%exts%) do (
  set /a totalCompressed = 0
  for %%f in (*.png;*.gif;*.bmp) do (
    for %%c in ("%%f.%%e") do (
      set /a totalCompressed=totalCompressed + %%~zc
    )
  )
  set /a ratio="100 * ( totalUncompressed - totalCompressed ) / totalUncompressed"
  echo %%e compressed by !ratio!%% overall
)
goto :eof
