using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NewSideGame
{
    public class CubeCrushStageEditorWindow : EditorWindow
    {
        private const string StageFolderRootPath = "Assets/Game/CubeCrush/Stages";
        private const string StageDatabaseAssetPath = StageFolderRootPath + "/StageDatabase.asset";

        private const int GridSize = 8;

        // 预填格颜色面板（点击放置）。Clear 为“清空该格”。
        private readonly Color[] _paletteColors =
        {
            new Color(1f, 0.2f, 0.2f, 1f),  // red
            new Color(0.2f, 0.6f, 1f, 1f),  // blue
            new Color(1f, 0.9f, 0.2f, 1f),  // yellow
            new Color(0.2f, 1f, 0.5f, 1f),  // green
            new Color(0.8f, 0.8f, 0.8f, 1f), // gray
        };

        private bool _placeClear;
        private Color _selectedColor = new Color(1f, 0.2f, 0.2f, 1f);

        private CubeCrushStageDatabase _database;
        private CubeCrushStage _stage;

        private int _selectedStageIndex = 1;

        private Vector2 _scrollPos;
        private ReorderableList _spawnSequenceList;
        private SerializedObject _stageSO;
        private SerializedProperty _spawnSequenceProp;

        [MenuItem("Tools/CubeCrush/Stage Editor", false, 200)]
        public static void ShowWindow()
        {
            var window = GetWindow<CubeCrushStageEditorWindow>("CubeCrush Stage Editor");
            window.minSize = new Vector2(520, 620);
        }

        private void OnEnable()
        {
            LoadOrCreateDatabase();
            LoadStage(_selectedStageIndex);
        }

        private void LoadOrCreateDatabase()
        {
            _database = AssetDatabase.LoadAssetAtPath<CubeCrushStageDatabase>(StageDatabaseAssetPath);
            if (_database != null) return;

            EnsureFolders();

            _database = CreateInstance<CubeCrushStageDatabase>();
            AssetDatabase.CreateAsset(_database, StageDatabaseAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            // Unity 的 CreateFolder 只能创建一层，所以需要确保父目录存在。
            const string gameRoot = "Assets/Game";
            const string cubeCrushRoot = "Assets/Game/CubeCrush";

            if (!AssetDatabase.IsValidFolder(cubeCrushRoot))
            {
                AssetDatabase.CreateFolder(gameRoot, "CubeCrush");
            }

            if (!AssetDatabase.IsValidFolder(StageFolderRootPath))
            {
                AssetDatabase.CreateFolder(cubeCrushRoot, "Stages");
            }

            AssetDatabase.Refresh();
        }

        private void LoadStage(int stageIndex)
        {
            if (_database == null) return;

            CubeCrushStage found = null;
            if (_database.stages != null)
            {
                foreach (var s in _database.stages)
                {
                    if (s == null) continue;
                    if (s.stageIndex == stageIndex)
                    {
                        found = s;
                        break;
                    }
                }
            }

            if (found == null)
            {
                found = CreateStageAsset(stageIndex);
            }

            _selectedStageIndex = stageIndex;
            _stage = found;
            RebuildSerialized();
            Repaint();
        }

        private CubeCrushStage CreateStageAsset(int stageIndex)
        {
            EnsureFolders();

            var stage = CreateInstance<CubeCrushStage>();
            stage.stageIndex = stageIndex;

            string assetPath = StageFolderRootPath + "/Stage_" + stageIndex + ".asset";
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            AssetDatabase.CreateAsset(stage, assetPath);

            if (_database.stages == null) _database.stages = new List<CubeCrushStage>();
            _database.stages.Add(stage);

            EditorUtility.SetDirty(_database);
            EditorUtility.SetDirty(stage);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return stage;
        }

        private void RebuildSerialized()
        {
            if (_stage == null) return;

            _stageSO = new SerializedObject(_stage);
            _spawnSequenceProp = _stageSO.FindProperty("spawnSequence");

            _spawnSequenceList = new ReorderableList(_stageSO, _spawnSequenceProp, true, true, true, true);
            _spawnSequenceList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "spawnSequence（底部固定生成序列）"); };
            _spawnSequenceList.elementHeight = EditorGUIUtility.singleLineHeight + 6;
            _spawnSequenceList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _spawnSequenceProp.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    element, GUIContent.none);
            };
        }

        private void OnGUI()
        {
            if (_database == null)
            {
                EditorGUILayout.HelpBox("StageDatabase 未初始化", MessageType.Error);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawStageSelector();
            DrawTargetScore();
            DrawPalette();
            DrawGrid();
            DrawSpawnSequence();
            DrawSaveButton();

            EditorGUILayout.EndScrollView();
        }

        private void DrawStageSelector()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("关卡选择", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                _selectedStageIndex = EditorGUILayout.IntField("StageIndex (1-based)", _selectedStageIndex);

                if (GUILayout.Button("Load / Create", GUILayout.Width(140)))
                {
                    _selectedStageIndex = Mathf.Max(1, _selectedStageIndex);
                    LoadStage(_selectedStageIndex);
                }
            }

            if (_stage == null) return;

            EditorGUILayout.Space(10);
        }

        private void DrawTargetScore()
        {
            EditorGUILayout.LabelField("本关通关分数（本关内累计分）", EditorStyles.boldLabel);
            if (_stage == null) return;

            _stage.targetScoreLocal = EditorGUILayout.IntField("targetScoreLocal", _stage.targetScoreLocal);
        }

        private void DrawPalette()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("预填格颜色（点击网格放置）", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                // Clear 按钮
                var clearStyle = new GUIStyle(GUI.skin.button);
                clearStyle.normal.textColor = Color.white;
                if (GUILayout.Button("Clear", GUILayout.Width(70), GUILayout.Height(26)))
                {
                    _placeClear = true;
                    _selectedColor = Color.white;
                }

                foreach (var c in _paletteColors)
                {
                    var prev = GUI.color;
                    GUI.color = c;
                    if (GUILayout.Button("", GUILayout.Width(26), GUILayout.Height(26)))
                    {
                        _placeClear = false;
                        _selectedColor = c;
                    }

                    GUI.color = prev;
                }
            }
        }

        private void DrawGrid()
        {
            if (_stage == null) return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("8x8 预填网格（y=0 在底部）", EditorStyles.boldLabel);

            // 将 prefilledCells 转成字典，便于查询某格是否已预置
            Dictionary<Vector2Int, CubeCrushPrefilledCell> dict = new Dictionary<Vector2Int, CubeCrushPrefilledCell>();
            if (_stage.prefilledCells != null)
            {
                foreach (var cell in _stage.prefilledCells)
                {
                    if (cell == null) continue;
                    dict[new Vector2Int(cell.x, cell.y)] = cell;
                }
            }

            float cellSize = 34f;

            for (int y = GridSize - 1; y >= 0; y--)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int x = 0; x < GridSize; x++)
                    {
                        dict.TryGetValue(new Vector2Int(x, y), out var cell);
                        Color color = cell != null ? cell.color : new Color(0.1f, 0.1f, 0.1f, 0.15f);

                        var prev = GUI.color;
                        GUI.color = color;
                        Rect rect = GUILayoutUtility.GetRect(cellSize, cellSize, GUILayout.Width(cellSize), GUILayout.Height(cellSize));
                        if (GUI.Button(rect, GUIContent.none))
                        {
                            PlaceOrClearCell(x, y);
                        }

                        GUI.color = prev;
                    }
                }
            }
        }

        private void PlaceOrClearCell(int x, int y)
        {
            if (_stage.prefilledCells == null) _stage.prefilledCells = new List<CubeCrushPrefilledCell>();

            // 查找已存在的 cell
            CubeCrushPrefilledCell existing = null;
            for (int i = 0; i < _stage.prefilledCells.Count; i++)
            {
                var cell = _stage.prefilledCells[i];
                if (cell == null) continue;
                if (cell.x == x && cell.y == y)
                {
                    existing = cell;
                    break;
                }
            }

            if (_placeClear)
            {
                if (existing != null)
                {
                    _stage.prefilledCells.Remove(existing);
                }
            }
            else
            {
                if (existing == null)
                {
                    existing = new CubeCrushPrefilledCell { x = x, y = y, color = _selectedColor };
                    _stage.prefilledCells.Add(existing);
                }
                else
                {
                    existing.color = _selectedColor;
                }
            }

            EditorUtility.SetDirty(_stage);
        }

        private void DrawSpawnSequence()
        {
            if (_stage == null) return;

            EditorGUILayout.Space(10);
            if (_spawnSequenceList != null)
            {
                _stageSO.Update();
                _spawnSequenceList.DoLayoutList();
                _stageSO.ApplyModifiedProperties();
            }
        }

        private void DrawSaveButton()
        {
            EditorGUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Stage Data", GUILayout.Height(34)))
                {
                    EditorUtility.SetDirty(_stage);
                    EditorUtility.SetDirty(_database);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}

