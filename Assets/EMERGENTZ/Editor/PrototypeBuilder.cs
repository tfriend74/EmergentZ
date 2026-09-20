#if UNITY_EDITOR
using Emergentz;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrototypeBuilder
{
    const string ScenePath = "Assets/EMERGENTZ/Scenes/Prototype.unity";

    [MenuItem("EMERGENTZ/Build First Playable")]
    public static void BuildFirstPlayable()
    {
        EnsureFolders();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientLight = new Color(0.16f, 0.18f, 0.2f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.09f, 0.11f, 0.12f);
        RenderSettings.fogDensity = 0.012f;

        CreateDirectionalLight();
        CreateGround();
        SurvivorController survivor = CreateSurvivor();
        InfectionClock infection = survivor.gameObject.AddComponent<InfectionClock>();
        CreateCamera(survivor.transform);
        CreateCampfire(new Vector3(0f, 0f, 18f));
        CreateEnvironment();

        GameObject systems = new GameObject("Run Systems");
        PrototypeGameManager manager = systems.AddComponent<PrototypeGameManager>();
        systems.AddComponent<PrototypeHud>();
        SetObjectReference(manager, "survivor", survivor);
        SetObjectReference(manager, "infectionClock", infection);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Selection.activeGameObject = survivor.gameObject;
        Debug.Log("EMERGENTZ first playable built successfully. Press Play.");
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/EMERGENTZ")) AssetDatabase.CreateFolder("Assets", "EMERGENTZ");
        if (!AssetDatabase.IsValidFolder("Assets/EMERGENTZ/Scenes")) AssetDatabase.CreateFolder("Assets/EMERGENTZ", "Scenes");
    }

    static void CreateDirectionalLight()
    {
        GameObject go = new GameObject("Moonlight");
        Light light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.35f;
        light.color = new Color(0.66f, 0.76f, 1f);
        go.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
    }

    static void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Abandoned Highway";
        ground.transform.localScale = new Vector3(8f, 1f, 8f);
        ground.GetComponent<Renderer>().material.color = new Color(0.12f, 0.13f, 0.14f);

        for (int i = -5; i <= 5; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "Road Stripe";
            stripe.transform.position = new Vector3(0f, 0.025f, i * 7f);
            stripe.transform.localScale = new Vector3(0.22f, 0.03f, 3.2f);
            stripe.GetComponent<Renderer>().material.color = new Color(0.75f, 0.6f, 0.12f);
            Object.DestroyImmediate(stripe.GetComponent<BoxCollider>());
        }
    }

    static SurvivorController CreateSurvivor()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Survivor";
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.GetComponent<Renderer>().material.color = new Color(0.15f, 0.5f, 0.72f);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.45f;
        controller.center = Vector3.zero;
        return player.AddComponent<SurvivorController>();
    }

    static void CreateCamera(Transform target)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 62f;
        camera.nearClipPlane = 0.1f;
        cameraObject.AddComponent<AudioListener>();
        ThirdPersonCamera follow = cameraObject.AddComponent<ThirdPersonCamera>();
        follow.SetTarget(target);
        cameraObject.transform.position = new Vector3(-4f, 4f, -6f);
    }

    static void CreateCampfire(Vector3 position)
    {
        GameObject camp = new GameObject("Campfire Checkpoint");
        camp.transform.position = position;
        SphereCollider trigger = camp.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 3.5f;
        camp.AddComponent<CampfireCheckpoint>();

        for (int i = 0; i < 3; i++)
        {
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = "Campfire Log";
            log.transform.SetParent(camp.transform);
            log.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            log.transform.localScale = new Vector3(0.18f, 1.2f, 0.18f);
            log.transform.localRotation = Quaternion.Euler(90f, i * 60f, 0f);
            log.GetComponent<Renderer>().material.color = new Color(0.26f, 0.11f, 0.04f);
            Object.DestroyImmediate(log.GetComponent<Collider>());
        }

        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Flame";
        flame.transform.SetParent(camp.transform);
        flame.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        flame.transform.localScale = new Vector3(0.7f, 1.15f, 0.7f);
        flame.GetComponent<Renderer>().material.color = new Color(1f, 0.22f, 0.02f);
        Object.DestroyImmediate(flame.GetComponent<Collider>());

        GameObject lightObject = new GameObject("Fire Light");
        lightObject.transform.SetParent(camp.transform);
        lightObject.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.32f, 0.05f);
        light.intensity = 8f;
        light.range = 12f;

        GameObject tent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tent.name = "Tent";
        tent.transform.position = position + new Vector3(4f, 1.1f, 1f);
        tent.transform.localScale = new Vector3(3.2f, 2.2f, 2.8f);
        tent.transform.rotation = Quaternion.Euler(0f, 18f, 45f);
        tent.GetComponent<Renderer>().material.color = new Color(0.22f, 0.27f, 0.12f);
    }

    static void CreateEnvironment()
    {
        Random.InitState(1337);
        for (int i = 0; i < 22; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            GameObject wreck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wreck.name = "Abandoned Wreck";
            wreck.transform.position = new Vector3(side * Random.Range(9f, 26f), Random.Range(0.5f, 1.3f), Random.Range(-36f, 38f));
            wreck.transform.localScale = new Vector3(Random.Range(2f, 5f), Random.Range(1f, 2.4f), Random.Range(2f, 5f));
            wreck.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            wreck.GetComponent<Renderer>().material.color = Color.Lerp(new Color(0.16f, 0.18f, 0.2f), new Color(0.32f, 0.12f, 0.06f), Random.value);
        }
    }

    static void SetObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
