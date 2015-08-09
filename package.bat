@echo off

rd dist /S /Q
rd $(BaseDir)\Macros /S /Q

md $(BaseDir)\Macros
copy sln\*.cs $(BaseDir)\Macros\

zip -9 -r temp.zip $(BaseDir)
md dist
copy temp.zip dist\FDMacros.fdz

del temp.zip
rd $(BaseDir)\Macros /S /Q
