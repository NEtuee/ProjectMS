@echo on
xcopy Assets\Data\*.* DevBuild\Assets\Data\ /s /i /f /y /exclude:DataCopyExcludeList.txt
xcopy Assets\Data\*.* ReleaseBuild\Assets\Data\ /s /i /f /y /exclude:DataCopyExcludeList.txt
pause