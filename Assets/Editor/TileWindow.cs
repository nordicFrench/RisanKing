using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;

enum DRAWOPTION { Select, Paint, PaintOver, PaintPrefab, Erase };

public class TileWindow : EditorWindow {
    private static bool isEnabled;
    private Vector2 _scrollPos;
    private static Vector2 gridSize = new Vector2(0.32f, 0.32f);
    private static bool isGrid;
    private static bool isDraw;
    private static bool addBoxCollider;
    //private static bool isObjmode;					// USED TO DRAW OBJECTS ON DIFFERENT LAYERS
    private static DRAWOPTION selected;
    private static GameObject parentObj;
    private static int layerOrd;
    private int index;
    private string[] options;
    private Sprite[] allSprites;
    private string[] files;
    private static Sprite activeSprite;
    private static GameObject activeGo;
    public GUIStyle textureStyle;
    public GUIStyle textureStyleAct;

    // ADDED TAGS & LAYERS DROP DOWN "VEEBEEMEE GAMES" 13 FEB 2015
    private static bool addTag;
    private static string selectedTag = "";
    private static bool addLayer;
    private static int selectedLayer = 0;
    // ADDED Order in Layers "VEEBEEMEE GAMES" 18 FEB 2015
    private static bool addOrderInLayer;
    private static int orderInLayer;
    // ADDED PAINTPREFAB OPTION "VEEBEEMEE GAMES" 20 FEB 2015
    private static bool addPrefab;
    private static GameObject paintPrefab;

    [MenuItem("Tools/TilemapEditor")]
    private static void TilemapEditor() {
        EditorWindow.GetWindow(typeof(TileWindow));
    }

    void OnEnable() {
        //Debug.Log("Enabled On");
        //TILE WINDOW 2D IS ENABLED UNDER TOOLS
        isEnabled = true;
        Editor.CreateInstance(typeof(SceneViewEventHandler));
    }

    void OnDestroy() {
        //TILE WINDOW 2D IS DISABLED UNDER TOOLS
        //Debug.Log("Enabled Off");
        isEnabled = false;
    }

    public class SceneViewEventHandler : Editor {
        static SceneViewEventHandler() {
            // ALLOWS OBJECTS TO BE ADDED & DRAWN TO MAIN SCENE
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView aView) {
            if (isEnabled)	// IF TILEWINDOW2D WINDOW IS ACTIVE THEN...
			{
                if (isDraw)	// IF TOGGLED TRUE TO PAINT / PAINTOVER / ERASE THEN...
				{
                    // GREATE E EVENT
                    Event e = Event.current;

                    if (selected != DRAWOPTION.Select) // IF SELECT IS NOT SELECTED THEN...
					{
                        // WHEN CLICKING IN SCENE DISABLED CURSER TO PASSIVE TO AVOID CLICK & DRAG EFFECT
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                        // ALLOWS US TO CLICK AND DRAG MOUSE OVER OPTION OR CLICK OPTION & SETS E BUTTON TO 0 AND IF THERE IS A SPRITE SELECTED THEN...
                        if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 0) {
                            // ALLOWS US TO GET AN X/Y POSITION FOR OUR CURRENT MOUSE POSITION
                            Vector2 mousePos = Event.current.mousePosition;
                            // SETS OUR MOUSE POSITION.Y TO BE EXACTLY WHERE WE CLICKING
                            mousePos.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePos.y;
                            // GETS OUR WORLD MOUSE POSITION WHEN CLICKING FOR OUR X / Y
                            Vector3 mouseWorldPos = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(mousePos).origin;
                            // OUR Z WILL BE W.E LAYER ORDER WE CHOOSE... ( I CHOOSE TO ALWAYS BE ON 0 FOR Z)
                            mouseWorldPos.z = 0;   // ORGINAL --> mouseWorldPos.z = layerOrd;

                            if (gridSize.x > 0.05f && gridSize.y > 0.05f) {
                                mouseWorldPos.x = Mathf.Floor(mouseWorldPos.x / gridSize.x) * gridSize.x + gridSize.x / 2.0f;
                                mouseWorldPos.y = Mathf.Ceil(mouseWorldPos.y / gridSize.y) * gridSize.y - gridSize.y / 2.0f;
                            }

                            // CREATES AN ARRAY OF OBJECTS OF TYPE GAMEOBJECT...
                            GameObject[] allgo = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
                            // CREATE A INT..
                            int brk = 0;

                            if (activeSprite != null) {
                                // IF PAINT OPTION SELECTED THEN...
                                if (selected == DRAWOPTION.Paint) {
                                    // CREATE A LIST OF ALL CURRENT GAME OBJECTS
                                    for (int i = 0; i < allgo.Length; i++) {
                                        // IF THERE IS GAME OBJECTS ALREADY IN IT'S X/Y/Z POSITION THEN PAINT OR BREAK
                                        if (Mathf.Approximately(allgo[i].transform.position.x, mouseWorldPos.x) && Mathf.Approximately(allgo[i].transform.position.y, mouseWorldPos.y) && Mathf.Approximately(allgo[i].transform.position.z, mouseWorldPos.z)) {
                                            brk++;
                                            break;
                                        }
                                    }
                                    // IF THERE IS NO OBJECT IN THE SPACE THEN..
                                    if (brk == 0) {
                                        // CREATE OBJECT SPRITE WITH ACTIVE SPRITE SELECTED 
                                        GameObject newgo = new GameObject(activeSprite.name, typeof(SpriteRenderer));
                                        // AT THE CURRENT WORLD POSITION
                                        newgo.transform.position = mouseWorldPos;
                                        // AND SET THEN SPRITE COMPONENT TO SHOW ACTIVE SPRITE
                                        newgo.GetComponent<SpriteRenderer>().sprite = activeSprite;
                                        // IF ADD BOX TOGGLED ADD A "BOX COLLIDER 2D"
                                        if (addBoxCollider)
                                            newgo.AddComponent<BoxCollider2D>();
                                        // IF PARENT OBJECT SELECTED ADD UNDER PARENT OBJECT
                                        if (parentObj != null)
                                            newgo.transform.parent = parentObj.transform;
                                        // UPDATED							// ADDED TAGS & LAYERS DROP DOWN "VEEBEEMEE GAMES" 13 FEB 2015
                                        if (addTag)
                                            newgo.tag = selectedTag;
                                        if (addLayer)
                                            newgo.layer = selectedLayer;
                                        // UPDATED							// ADDED Order in Layers "VEEBEEMEE GAMES" 18 FEB 201
                                        if (addOrderInLayer)
                                            newgo.GetComponent<SpriteRenderer>().GetComponent<Renderer>().sortingOrder = orderInLayer;
                                    }
                                }
                                // IF PAINT OVER OPTION SELECTED THEN...
                                else if (selected == DRAWOPTION.PaintOver) {
                                    // CREATE A LIST OF ALL CURRENT GAME OBJECTS
                                    for (int i = 0; i < allgo.Length; i++) {
                                        if (allgo[i].GetComponent<SpriteRenderer>() != null & Mathf.Approximately(allgo[i].transform.position.x, mouseWorldPos.x) && Mathf.Approximately(allgo[i].transform.position.y, mouseWorldPos.y) && Mathf.Approximately(allgo[i].transform.position.z, mouseWorldPos.z)) {
                                            // DESTROYS CURRENT OBJECT IN PLACE... THEN REPLACES :-)
                                            GameObject.DestroyImmediate(allgo[i]);

                                            // CREATE OBJECT SPRITE WITH ACTIVE SPRITE SELECTED 
                                            GameObject newgo = new GameObject(activeSprite.name, typeof(SpriteRenderer));
                                            // AT THE CURRENT WORLD POSITION
                                            newgo.transform.position = mouseWorldPos;
                                            // AND SET THEN SPRITE COMPONENT TO SHOW ACTIVE SPRITE
                                            newgo.GetComponent<SpriteRenderer>().sprite = activeSprite;

                                            // IF ADD BOX TOGGLED ADD A "BOX COLLIDER 2D"
                                            if (addBoxCollider)
                                                newgo.AddComponent<BoxCollider2D>();
                                            // IF PARENT OBJECT SELECTED ADD UNDER PARENT OBJECT
                                            if (parentObj != null)
                                                newgo.transform.parent = parentObj.transform;
                                            // UPDATED							// ADDED TAGS & LAYERS DROP DOWN "VEEBEEMEE GAMES" 13 FEB 2015
                                            if (addTag)
                                                newgo.tag = selectedTag;
                                            if (addLayer)
                                                newgo.layer = selectedLayer;
                                            // UPDATED							// ADDED Order in Layers "VEEBEEMEE GAMES" 18 FEB 2015
                                            if (addOrderInLayer)
                                                newgo.GetComponent<SpriteRenderer>().GetComponent<Renderer>().sortingOrder = orderInLayer;

                                            brk++;
                                        }
                                    }
                                    if (brk == 0) {
                                        GameObject newgo = new GameObject(activeSprite.name, typeof(SpriteRenderer));
                                        newgo.transform.position = mouseWorldPos;
                                        newgo.GetComponent<SpriteRenderer>().sprite = activeSprite;

                                        if (addBoxCollider)
                                            newgo.AddComponent<BoxCollider2D>();

                                        // UPDATED							// IF PARENT OBJECT SELECTED ADD UNDER PARENT OBJECT
                                        if (parentObj != null)
                                            newgo.transform.parent = parentObj.transform;
                                        // ADDED TAGS & LAYERS DROP DOWN "VEEBEEMEE GAMES" 13 FEB 2015
                                        if (addTag)
                                            newgo.tag = selectedTag;
                                        if (addLayer)
                                            newgo.layer = selectedLayer;
                                        // UPDATED							// ADDED Order in Layers "VEEBEEMEE GAMES" 18 FEB 201
                                        if (addOrderInLayer)
                                            newgo.GetComponent<SpriteRenderer>().GetComponent<Renderer>().sortingOrder = orderInLayer;
                                    }
                                }
                            } // END OF SELECTED != DRAWOPTION.Select

                            // IF PAINT OPTION SELECTED THEN...
                            if (selected == DRAWOPTION.PaintPrefab) {
                                // CREATE A LIST OF ALL CURRENT GAME OBJECTS
                                for (int i = 0; i < allgo.Length; i++) {
                                    // IF THERE IS GAME OBJECTS ALREADY IN IT'S X/Y/Z POSITION THEN PAINT OR BREAK
                                    if (Mathf.Approximately(allgo[i].transform.position.x, mouseWorldPos.x) && Mathf.Approximately(allgo[i].transform.position.y, mouseWorldPos.y) && Mathf.Approximately(allgo[i].transform.position.z, mouseWorldPos.z)) {
                                        brk++;
                                        break;
                                    }
                                }
                                // IF THERE IS NO OBJECT IN THE SPACE THEN..
                                if (brk == 0) {
                                    // CREATE OBJECT SPRITE WITH ACTIVE SPRITE SELECTED 
                                    //GameObject newgo = new GameObject(activeSprite.name, typeof(SpriteRenderer));
                                    GameObject newgo = Instantiate(paintPrefab, mouseWorldPos, Quaternion.identity) as GameObject;

                                    // IF PARENT OBJECT SELECTED ADD UNDER PARENT OBJECT
                                    if (parentObj != null)
                                        newgo.transform.parent = parentObj.transform;
                                }
                            }
                            else if (selected == DRAWOPTION.Erase) {
                                // CREATE A LIST OF ALL CURRENT GAME OBJECTS
                                for (int i = 0; i < allgo.Length; i++) {
                                    // IF THERE IS GAME OBJECTS ALREADY IN IT'S X/Y/Z POSITION THEN PAINT OR BREAK
                                    if (Mathf.Approximately(allgo[i].transform.position.x, mouseWorldPos.x) && Mathf.Approximately(allgo[i].transform.position.y, mouseWorldPos.y) && Mathf.Approximately(allgo[i].transform.position.z, mouseWorldPos.z))
                                        // DESTROY CURRENT OBJECT
                                        GameObject.DestroyImmediate(allgo[i]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // DISABLED TO AVOID ISSUE WITH MAIN INSPECTOR VIEW
    //[CustomEditor(typeof(GameObject))]
    public class SceneGUITest : Editor {
        [DrawGizmo(GizmoType.NotSelected)] // ALLOWS "SHOW GRID" UNDER GIZMOS
        static void RenderCustomGizmo(Transform objectTransform, GizmoType gizmoType) {
            // IF WINDOW IS OPEN AND IS GRID TRUE THEN...
            if (isEnabled && isGrid) {
                // CREATE GRIDS BASED ON SIZE FOR X / Y
                Gizmos.color = Color.white;
                Vector3 minGrid = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(new Vector2(0f, 0f)).origin;
                Vector3 maxGrid = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(new Vector2(SceneView.currentDrawingSceneView.camera.pixelWidth, SceneView.currentDrawingSceneView.camera.pixelHeight)).origin;
                for (float i = Mathf.Round(minGrid.x / gridSize.x) * gridSize.x; i < Mathf.Round(maxGrid.x / gridSize.x) * gridSize.x && gridSize.x > 0.05f; i += gridSize.x)
                    Gizmos.DrawLine(new Vector3(i, minGrid.y, 0.0f), new Vector3(i, maxGrid.y, 0.0f));
                for (float j = Mathf.Round(minGrid.y / gridSize.y) * gridSize.y; j < Mathf.Round(maxGrid.y / gridSize.y) * gridSize.y && gridSize.y > 0.05f; j += gridSize.y)
                    Gizmos.DrawLine(new Vector3(minGrid.x, j, 0.0f), new Vector3(maxGrid.x, j, 0.0f));
                SceneView.RepaintAll();
            }
        }
    }

    // TILEWINDOW2D GUI OPTIONS AND TOGGLES
    void OnGUI() {
        textureStyle = new GUIStyle(GUI.skin.button);
        textureStyle.margin = new RectOffset(2, 2, 2, 2);
        textureStyle.normal.background = null;
        textureStyleAct = new GUIStyle(textureStyle);
        textureStyleAct.margin = new RectOffset(0, 0, 0, 0);
        textureStyleAct.normal.background = textureStyle.active.background;

        if (!Directory.Exists(Application.dataPath + "/Tilemaps/")) {
            //Directory.CreateDirectory(Application.dataPath + "/Tilemaps/");
            AssetDatabase.CreateFolder("Assets", "Tilemaps");
            AssetDatabase.Refresh();
            Debug.Log("Created Tilemaps Directory");
        }
        files = Directory.GetFiles(Application.dataPath + "/Tilemaps/", "*.png");
        options = new string[files.Length];
        EditorGUILayout.LabelField("Tile Map", GUILayout.Width(256));
        for (int i = 0; i < files.Length; i++) {
            options[i] = files[i].Replace(Application.dataPath + "/Tilemaps/", "");
        }
        // GETS LIST OF SPRITES UNDER TILEMAPS FOLDER
        index = EditorGUILayout.Popup(index, options, GUILayout.Width(256));

        // GRID SIZE OPTIONS
        GUILayout.BeginHorizontal();
        // TOGGLE FOR ON/OFF GRID
        isGrid = EditorGUILayout.Toggle(isGrid, GUILayout.Width(16));
        // TEXT FOR GRID SIZE
        gridSize = EditorGUILayout.Vector2Field("Grid Size (0.05 minimum)", gridSize, GUILayout.Width(236));
        GUILayout.EndHorizontal();

        // OPTIONS FOR PARENT OBJECT
        EditorGUILayout.LabelField("Parent Object", GUILayout.Width(256));
        parentObj = (GameObject)EditorGUILayout.ObjectField(parentObj, typeof(GameObject), true, GUILayout.Width(256));

        // OPTIONS FOR BOX COLLIDER2D
        GUILayout.BeginHorizontal();
        addBoxCollider = EditorGUILayout.Toggle(addBoxCollider, GUILayout.Width(16));
        EditorGUILayout.LabelField("Add Box Collider", GUILayout.Width(256));
        GUILayout.EndHorizontal();

        // OPTIONS FOR ADDING A TAG DROP DOWN "VEEBEEMEE GAMES" 13 FEB 2015
        GUILayout.BeginHorizontal();
        addTag = EditorGUILayout.Toggle(addTag, GUILayout.Width(16));
        EditorGUILayout.LabelField("Add Tag", GUILayout.Width(50));
        selectedTag = EditorGUILayout.TagField("" + selectedTag, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        // OPTIONS FOR ADDING A LAYER DROP DOWN "VEEBEEMEE GAMES" 13 FEB 2015
        GUILayout.BeginHorizontal();
        addLayer = EditorGUILayout.Toggle(addLayer, GUILayout.Width(16));
        EditorGUILayout.LabelField("Add Layer", GUILayout.Width(75));
        selectedLayer = EditorGUILayout.LayerField(selectedLayer, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        // OPTIONS FOR ADDING A LAYER DROP DOWN "VEEBEEMEE GAMES" 18 FEB 2015
        GUILayout.BeginHorizontal();
        addOrderInLayer = EditorGUILayout.Toggle(addOrderInLayer, GUILayout.Width(16));
        EditorGUILayout.LabelField("Order in Layer", GUILayout.Width(100));
        orderInLayer = EditorGUILayout.IntField(orderInLayer, GUILayout.Width(126));
        GUILayout.EndHorizontal();

        // OPTIONS FOR ADDING A LAYER DROP DOWN "VEEBEEMEE GAMES" 20 FEB 2015
        GUILayout.BeginHorizontal();
        addPrefab = EditorGUILayout.Toggle(addPrefab, GUILayout.Width(16));
        EditorGUILayout.LabelField("Add Prefab", GUILayout.Width(100));
        paintPrefab = (GameObject)EditorGUILayout.ObjectField(paintPrefab, typeof(GameObject), true, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        //EditorGUILayout.LabelField("Layer Order", GUILayout.Width(256));

        //GUILayout.BeginHorizontal();
        //layerOrd = EditorGUILayout.IntField(layerOrd,  GUILayout.Width(126));
        //isObjmode = EditorGUILayout.Toggle(isObjmode, GUILayout.Width(16));
        //EditorGUILayout.LabelField("Layer based on Y", GUILayout.Width(110));
        //GUILayout.EndHorizontal();

        // TOGGLE TO PAINT / PAINTOVER / ERASE
        GUILayout.BeginHorizontal();
        isDraw = EditorGUILayout.Toggle(isDraw, GUILayout.Width(16));
        selected = (DRAWOPTION)EditorGUILayout.EnumPopup(selected, GUILayout.Width(236));
        GUILayout.EndHorizontal();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        GUILayout.BeginHorizontal();
        float ctr = 0.0f;

        // CREATES INDEX BELOW FOR SPRITES IN TILEMAPS FILE SELECTED
        if (options.Length > index) {
            allSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Tilemaps/" + options[index]).Select(x => x as Sprite).Where(x => x != null).ToArray();
            foreach (Sprite singsprite in allSprites) {
                if (ctr > singsprite.textureRect.x) {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                ctr = singsprite.textureRect.x;
                if (activeSprite == singsprite) {
                    GUILayout.Button("", textureStyleAct, GUILayout.Width(singsprite.textureRect.width + 6), GUILayout.Height(singsprite.textureRect.height + 4));
                    GUI.DrawTextureWithTexCoords(new Rect(GUILayoutUtility.GetLastRect().x + 3f,
                                                          GUILayoutUtility.GetLastRect().y + 2f,
                                                          GUILayoutUtility.GetLastRect().width - 6f,
                                                          GUILayoutUtility.GetLastRect().height - 4f),
                                                 singsprite.texture,
                                                 new Rect(singsprite.textureRect.x / (float)singsprite.texture.width,
                             singsprite.textureRect.y / (float)singsprite.texture.height,
                             singsprite.textureRect.width / (float)singsprite.texture.width,
                             singsprite.textureRect.height / (float)singsprite.texture.height));
                }
                else {
                    if (GUILayout.Button("", textureStyle, GUILayout.Width(singsprite.textureRect.width + 2), GUILayout.Height(singsprite.textureRect.height + 2)))
                        activeSprite = singsprite;
                    GUI.DrawTextureWithTexCoords(GUILayoutUtility.GetLastRect(), singsprite.texture,
                                                 new Rect(singsprite.textureRect.x / (float)singsprite.texture.width,
                             singsprite.textureRect.y / (float)singsprite.texture.height,
                             singsprite.textureRect.width / (float)singsprite.texture.width,
                             singsprite.textureRect.height / (float)singsprite.texture.height));
                }
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
        SceneView.RepaintAll();
    }
}