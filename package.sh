#!/bin/sh

rm -rf dist
rm -rf '$(BaseDir)/Macros'

mkdir '$(BaseDir)/Macros'
cp sln/*.cs '$(BaseDir)/Macros/'

zip -9 -r temp.zip '$(BaseDir)'
mkdir dist
mv temp.zip dist/FDMacros.fdz

rm -rf '$(BaseDir)/Macros'
