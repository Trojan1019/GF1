basePath=$(cd `dirname $0`; pwd)
xlsPath="$basePath"/../xls/
csvPath="$basePath"/../csv/

cd "$basePath"

for dir in "$xlsPath"*/; do
  java -jar ExcelToCsv.jar "$dir" "$csvPath" attributeType.config 2>ExcelToCsv.log
done

java -jar ExcelToCsv.jar "$xlsPath" "$csvPath" attributeType.config 2>ExcelToCsv.log

cp "$csvPath"GlobalConfig.csv "$basePath"/../../../Puzzle/Assets/Game/Configs/GlobalConfig.csv
