basePath=$(cd `dirname $0`; pwd)
outputPath="$basePath"/../uiText_output
csvPath="$basePath"/../csv
uiTextPath="$basePath"/../uiText.xls;
uiTextTemPath="$basePath"\\uiText.xls;

cd $basePath

mv $uiTextPath $uiTextTemPath

java -jar Translation.jar $outputPath/ 2 2>Translation.log

mv $uiTextTemPath $uiTextPath

java -jar ExcelToCsv.jar $outputPath/ $csvPath/ 2>ExcelToCsv.log

cp "$csvPath"/uiText_*.csv "$basePath"/../../../GF_ToBeName/Assets/Game/Localization

