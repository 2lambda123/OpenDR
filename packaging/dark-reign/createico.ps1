$imgMagick = "& magick.exe"
$env:Path = $env:Path + ';C:\Program Files\ImageMagick-7.0.9-Q16'
$image = "mod_scalable_yellow.svg"

Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -define icon:auto-resize out\icon.ico"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 16x16 out\icon_16x16.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 24x24 out\icon_24x24.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 32x32 out\icon_32x32.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 48x48 out\icon_48x48.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 64x64 out\icon_64x64.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 96x96 out\icon_96x96.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 100x100 out\icon_100x100.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 128x128 out\icon_128x128.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 256x256 out\icon_256x256.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 512x512 out\icon_512x512.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 768x768 out\icon_768x768.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 1024x1024 out\icon_1024x1024.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 1024 $image -resize 252x252 out\banner_252x252.png"
Invoke-Expression "$imgMagick convert -transparent white -background None -density 384 $image -resize 1280x640 out\github_socialmedia_1280x640.png"
Invoke-Expression "$imgMagick convert -transparent black -background None -density 1024 $image -resize 256x256 out\logo-ingame.png"
