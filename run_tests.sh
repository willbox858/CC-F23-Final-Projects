
UNITY=/Applications/Unity/Hub/Editor/2022.3.4f1/Unity.app/Contents/MacOS/Unity
$UNITY -logFilfe log.txt -runTests -batchmode -projectPath . -testResults artifacts/playmode-results.xml -testPlatform PlayMode
python3 test_runner.py
