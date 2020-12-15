using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Save level"))
        {

        }

        /* 1 Grabs a handle on the Level component and sets the Level GameObject’s position
        and rotation to 0, ensuring everything is positioned from the origin point in the
        scene. */
        Level level = (Level)target;
        level.transform.position = Vector3.zero;
        level.transform.rotation = Quaternion.identity;
        // 2 Establishes levelRoot as a reference to the Level GameObject, which is the parent of all the level items.
        var levelRoot = GameObject.Find("Level");
        // 3 Creates new instances of LevelDataRepresentation and LevelItemRepresentation.
        var ldr = new LevelDataRepresentation();
        var levelItems = new List<LevelItemRepresentation>();

        foreach (Transform t in levelRoot.transform)
        {
            /* 4 . Loops through every child Transform object of levelRoot. Then it takes a reference
            to any potential SpriteRenderer component and assigns it to the sr variable. It then
            creates a new LevelItemRepresentation to hold the position, rotation and scale of
            each item. */
            var sr = t.GetComponent<SpriteRenderer>();
            var li = new LevelItemRepresentation()
            {
                position = t.position,
                rotation = t.rotation.eulerAngles,
                scale = t.localScale
            };
            /* 5 This is a bit of a hack to store the name of each level item in the prefabName field.
            When loading a level later on, your code will need to know which prefabs to use to
            construct the level based on the name stored in the JSON file. */
            if (t.name.Contains(" "))
            {
                li.prefabName = t.name.Substring(0, t.name.IndexOf(" "));
            }
            else
            {
                li.prefabName = t.name;
            }
            /* 6 . Performs a check to ensure that a valid SpriteRenderer was found — because a level
            item might not have an attached sprite. If the sr variable is not null, then
            information about the sprite’s sorting, order and color stores in the
            LevelItemRepresentation object. */
            if (sr != null)
            {
                li.spriteLayer = sr.sortingLayerName;
                li.spriteColor = sr.color;
                li.spriteOrder = sr.sortingOrder;
            }
            /* 7 Lastly, the item that has all the collected information is added to the levelItems list */
            levelItems.Add(li);
        }

        /* 8 Converts the entire list of level items to an array of LevelItemRepresentation
        objects and stores them in the levelItems field on the LevelDataRepresentation
        object. */
        ldr.levelItems = levelItems.ToArray();
        ldr.playerStartPosition =
         GameObject.Find("SoyBoy").transform.position;
        /* 9 . Locates the CameraLerpToTransform script in the scene. Then it maps its settings
        against a new CameraSettingsRepresentation object and assigns that object to the
        LevelDataRepresentation object’s cameraSettings field. */
        var currentCamSettings = FindObjectOfType<CameraLerpToTransform>();
        if (currentCamSettings != null)
        {
            ldr.cameraSettings = new CameraSettingsRepresentation()
            {
                cameraTrackTarget = currentCamSettings.camTarget.name,
                cameraZDepth = currentCamSettings.cameraZDepth,
                minX = currentCamSettings.minX,
                minY = currentCamSettings.minY,
                maxX = currentCamSettings.maxX,
                maxY = currentCamSettings.maxY,
                trackingSpeed = currentCamSettings.trackingSpeed
            };
        }
        /* This uses JsonUtility to serialize the LevelDataRepresentation object, and all of its
        nested level item fields and values, to a JSON string named levelDataToJson. 
        Then it combines the level name specified in the editor with the game’s app data path
        (pointing to our Assets folder). This path is used to write the JSON string out to a file.*/
        var levelDataToJson = JsonUtility.ToJson(ldr);
        var savePath = System.IO.Path.Combine(Application.dataPath,
         level.levelName + ".json");
        System.IO.File.WriteAllText(savePath, levelDataToJson);
        Debug.Log("Level saved to " + savePath);
    }
}
