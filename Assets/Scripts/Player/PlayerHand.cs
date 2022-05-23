using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class PlayerHand : PlayerBehaviour {

    [SerializeField] float reach;
    [SerializeField] Image crosshair;
    [SerializeField] LayerMask layerMask;
    [SerializeField] GameObject armObj;
    [SerializeField] float smoothMove;
    float smoothMoveDefault;
    [SerializeField] float smoothRot;
    float smoothRotDefault;
    [SerializeField] Transform armTarget;

    Vector3 armVel = Vector3.zero;
    float armVelRot = 0f;

    bool place;
    bool breakKey;

    [SerializeField] GameObject highlightObject;

    [SerializeField] WorldGeneration world;

    bool breaking;
    float breakLength = 0.5f;
    float breakAmount = 0f;
    float breakIncrement = 0.04f;
    int steppedBreakPercentage;

    Coordinates cursorPlacement;

    public LineRenderer laser;
    public Light pointLight;

    ParticleSystem.TextureSheetAnimationModule textureAnim;

    PlayerInventory inventory;

    public Material cursorMaterial;
    GameObject cursorObject;
    Mesh cursorMesh;
    MeshFilter cursorMeshFilter;
    MeshRenderer cursorMeshRenderer;
    List<Vector3> meshVerticies = new List<Vector3>();
    List<int> meshTriangles = new List<int>();
    List<Vector2> meshUVs = new List<Vector2>();


    public Material worldMaterial;

    List<Vector2> voxelUVs = new List<Vector2>();

    GameObject voxelArm;
    Mesh armMesh;
    MeshFilter armMeshFilter;

    List<Vector2> armUVs = new List<Vector2>();

    public GameObject arm;
    public ParticleSystem breakingParticles;
    public GameObject burstParticle;

    PlayerCamera camera;

    Vector2 cursorSheetSize = new Vector2(9, 2);

    void Start() {
        inventory = GetComponent<PlayerInventory>();
        camera = player.GetPlayerBehaviour<PlayerCamera>();
        smoothMoveDefault = smoothMove;
        smoothRotDefault = smoothRot;
        CalculateMesh();
        CreateVoxelArm();
        CreateSpriteMeshObject();
        CreateCursor();
        textureAnim = breakingParticles.textureSheetAnimation;

        player.input.actions["Place"].performed += (InputAction.CallbackContext context) => { place = true; };
        player.input.actions["Place"].canceled += (InputAction.CallbackContext context) => { place = false; };
        player.input.actions["Break"].performed += (InputAction.CallbackContext context) => { breakKey = true; };
        player.input.actions["Break"].canceled += (InputAction.CallbackContext context) => { breakKey = false; };
    }

    void Update() {
        if (!uiOpen) {
            Interaction();
        }
    }

    void FixedUpdate() {
        if (breaking) Breaking();
    }

    void LateUpdate() {
        if (!player.sliding && !player.dashing) {
            armObj.transform.position = Vector3.SmoothDamp(armObj.transform.position, armTarget.position, ref armVel, smoothMove);
            Quaternion currentRotation = armObj.transform.rotation;
            Quaternion goalRotation = armTarget.rotation;
            float smoothX = Mathf.SmoothDamp(currentRotation.x, goalRotation.x, ref armVelRot, smoothRot);
            float smoothY = Mathf.SmoothDamp(currentRotation.y, goalRotation.y, ref armVelRot, smoothRot);
            float smoothZ = Mathf.SmoothDamp(currentRotation.z, goalRotation.z, ref armVelRot, smoothRot);
            float smoothW = Mathf.SmoothDamp(currentRotation.w, goalRotation.w, ref armVelRot, smoothRot);
            Quaternion smoothQuaternion = new Quaternion(smoothX, smoothY, smoothZ, smoothW);
            armObj.transform.rotation = smoothQuaternion;
        } else {
            armObj.transform.position = armTarget.position;
            armObj.transform.rotation = armTarget.rotation;
        }

    }

    void CreateVoxelArm() {
        voxelArm = new GameObject();
        armMesh = new Mesh();
        armMeshFilter = voxelArm.AddComponent<MeshFilter>();
        MeshRenderer voxelMeshRenderer = voxelArm.AddComponent<MeshRenderer>();
        voxelMeshRenderer.material = worldMaterial;
        armMesh.vertices = meshVerticies.ToArray();
        armMesh.triangles = meshTriangles.ToArray();
        voxelArm.transform.parent = laser.transform.parent;
        voxelArm.transform.localPosition = new Vector3(-0.2f, 0, 0.2f);
        voxelArm.transform.localRotation = Quaternion.Euler(80, 45, -100);
        voxelArm.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        voxelMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }


    GameObject spriteObject;
    MeshFilter spriteMeshFilter;
    MeshRenderer spriteMeshRenderer;

    void CreateSpriteMeshObject() {
        spriteObject = new GameObject();
        spriteMeshFilter = spriteObject.AddComponent<MeshFilter>();
        spriteMeshRenderer = spriteObject.AddComponent<MeshRenderer>();
        spriteObject.transform.parent = laser.transform.parent;
        spriteObject.transform.localPosition = new Vector3(1f, 0, 1.1f);
        spriteObject.transform.localRotation = Quaternion.Euler(215, 10, 40);
        spriteObject.transform.localScale = new Vector3(2f, 2f, 2f);
        spriteMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void UpdateSpriteMesh() {
        spriteMeshFilter.mesh = SpriteMeshify.Meshify(Resources.Load<Texture2D>("Sprites/" + inventory.inventoryItems[inventory.currentItem].ID.ToString()));
        spriteMeshRenderer.material = Resources.Load<Material>("Materials/" + inventory.inventoryItems[inventory.currentItem].ID.ToString());
    }

    public void UpdateArmUVs(byte ID) {
        armUVs.Clear();
        armUVs.AddRange(VoxelData.faceUVsFromIndex(new Vector2(ID - 1, 0), VoxelData.spriteSheetSize));
        armUVs.AddRange(VoxelData.faceUVsFromIndex(new Vector2(ID - 1, 2), VoxelData.spriteSheetSize));
        for (int i = 0; i < 6; i++) {
            armUVs.AddRange(VoxelData.sideUVsFromIndex(new Vector2(ID - 1, 1), VoxelData.spriteSheetSize));
        }
        armMesh.uv = armUVs.ToArray();
        armMesh.RecalculateNormals();
        armMeshFilter.mesh = armMesh;
    }

    void CreateVoxelEntity(Vector3 position, byte voxelType) {
        voxelUVs.Clear();
        GameObject voxel = new GameObject();
        Mesh voxelMesh = new Mesh();
        MeshFilter voxelMeshFilter = voxel.AddComponent<MeshFilter>();
        MeshRenderer voxelMeshRenderer = voxel.AddComponent<MeshRenderer>();
        voxelMeshRenderer.material = worldMaterial;
        voxelMesh.vertices = meshVerticies.ToArray();
        voxelMesh.triangles = meshTriangles.ToArray();
        MeshCollider voxelCollider = voxel.AddComponent<MeshCollider>();
        UnityEngine.Physics.IgnoreCollision(player.controller, voxelCollider, true);
        voxelCollider.sharedMesh = voxelMesh;
        voxelCollider.convex = true;
        Rigidbody rb = voxel.AddComponent<Rigidbody>();
        voxelUVs.AddRange(VoxelData.faceUVsFromIndex(new Vector2(voxelType - 1, 0), VoxelData.spriteSheetSize));
        voxelUVs.AddRange(VoxelData.faceUVsFromIndex(new Vector2(voxelType - 1, 2), VoxelData.spriteSheetSize));
        for (int i = 0; i < 6; i++) {
            voxelUVs.AddRange(VoxelData.sideUVsFromIndex(new Vector2(voxelType - 1, 1), VoxelData.spriteSheetSize));
        }
        voxelMesh.uv = voxelUVs.ToArray();
        voxelMesh.RecalculateNormals();
        voxelMeshFilter.mesh = voxelMesh;
        voxel.transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);
        voxel.transform.position = position;
        voxel.AddComponent<VoxelEntity>().Setup(transform, voxelType);
        Vector3 force = new Vector3(Random.Range(-1f, 1f), 1, Random.Range(-1f, 1f)) * 20;
        rb.AddForce(force);
        rb.AddRelativeTorque(force);
    }

    void CreateCursor() {
        cursorObject = new GameObject();
        cursorMesh = new Mesh();
        cursorObject.transform.parent = highlightObject.transform;
        cursorMeshFilter = cursorObject.AddComponent<MeshFilter>();
        cursorMeshRenderer = cursorObject.AddComponent<MeshRenderer>();
        cursorMeshRenderer.material = cursorMaterial;
        cursorMesh.vertices = meshVerticies.ToArray();
        cursorMesh.triangles = meshTriangles.ToArray();
        cursorObject.transform.localScale = new Vector3(1.001f, 1.002f, 1.001f);
        cursorObject.transform.localPosition -= new Vector3(0, 0, 0.001f);
        cursorObject.SetActive(false);
        UpdateCursorUVs(0);
    }

    void CalculateMesh() {
        // Bottom face
        Vector3[] bottomVerticies = VoxelData.relativeVoxelVerticies(Vector3.zero);
        meshVerticies.AddRange(bottomVerticies);
        meshTriangles.AddRange(VoxelData.relativeVoxelTriangles(false, meshVerticies.Count));
        // Top face
        Vector3[] topVerticies = VoxelData.relativeVoxelVerticies(Vector3.up);
        meshVerticies.AddRange(topVerticies);
        meshTriangles.AddRange(VoxelData.relativeVoxelTriangles(true, meshVerticies.Count));
        // Sides
        for (int i = 0; i < 6; i++) {
            int[] sideVerticies = new int[4];
            meshVerticies.Add(bottomVerticies[i]);
            sideVerticies[0] = meshVerticies.Count - 1;
            meshVerticies.Add(bottomVerticies[i + 1 < 6 ? i + 1 : 0]);
            sideVerticies[1] = meshVerticies.Count - 1;
            meshVerticies.Add(topVerticies[i]);
            sideVerticies[2] = meshVerticies.Count - 1;
            meshVerticies.Add(topVerticies[i + 1 < 6 ? i + 1 : 0]);
            sideVerticies[3] = meshVerticies.Count - 1;
            foreach (int triangle in VoxelData.sideTriangles) {
                meshTriangles.Add(sideVerticies[triangle]);
            }
        }
    }

    void UpdateCursorUVs(int state) {
        meshUVs.Clear();
        meshUVs.AddRange(VoxelData.faceUVsFromIndex(new Vector2(state, 1), cursorSheetSize));
        meshUVs.AddRange(VoxelData.faceUVsFromIndex(new Vector2(state, 1), cursorSheetSize));
        for (int i = 0; i < 6; i++) {
            meshUVs.AddRange(VoxelData.sideUVsFromIndex(new Vector2(state, 0), cursorSheetSize));
        }
        cursorMesh.uv = meshUVs.ToArray();
        cursorMeshFilter.mesh = cursorMesh;
    }

    ChunkCoordinates chunkCoordinates;
    Chunk chunk;
    Coordinates localCoordinates;
    Coordinates globalCoordinates;

    bool broken = false;

    void BreakBlock() {
        chunkCoordinates = new ChunkCoordinates(globalCoordinates);
        chunk = world.chunkMap[chunkCoordinates.x, chunkCoordinates.z];
        GameObject burst = Instantiate(burstParticle);
        burst.transform.position = globalCoordinates.worldPosition + (Vector3.up * 0.5f);
        ParticleSystem.TextureSheetAnimationModule anim;
        ParticleSystem particleSystem;
        particleSystem = burst.GetComponent<ParticleSystem>();
        anim = particleSystem.textureSheetAnimation;
        byte ID = world.blockIDAtCoordinates(globalCoordinates);
        anim.startFrame = (ID + (Voxels.voxelAmount - 1)) / (Voxels.voxelAmount * 3f);
        anim.numTilesX = Voxels.voxelAmount;
        particleSystem.Play();
        localCoordinates = Coordinates.GlobalToLocalOffset(globalCoordinates, chunkCoordinates);
        if (Voxels.types[chunk.voxelMap[localCoordinates.x, localCoordinates.y, localCoordinates.z]].blockDrop != 0) {
            if (Voxels.types[chunk.voxelMap[localCoordinates.x, localCoordinates.y, localCoordinates.z]].toolRequired) {
                if (inventory.inventoryItems[inventory.currentItem].ID > 200) {
                    if (Voxels.types[chunk.voxelMap[localCoordinates.x, localCoordinates.y, localCoordinates.z]].effectiveTool == Tools.tools[inventory.inventoryItems[inventory.currentItem].ID - 201].type) {
                        CreateVoxelEntity(globalCoordinates.worldPosition + (Vector3.up * 0.5f), Voxels.types[chunk.voxelMap[localCoordinates.x, localCoordinates.y, localCoordinates.z]].blockDrop);
                    }
                }
            } else {
                CreateVoxelEntity(globalCoordinates.worldPosition + (Vector3.up * 0.5f), Voxels.types[chunk.voxelMap[localCoordinates.x, localCoordinates.y, localCoordinates.z]].blockDrop);
            }
        }
        chunk.EditVoxel(localCoordinates, 0);
        SetBreakAmount(0);
    }

    void SetBreakAmount(float amount) {
        breakAmount = amount;
        float breakPercentage = breakAmount / breakLength;
        if (breakLength == 0) {
            breakPercentage = 100;
        }
        steppedBreakPercentage = Mathf.FloorToInt(breakPercentage * cursorSheetSize.x);
        UpdateCursorUVs(steppedBreakPercentage);
    }

    void Breaking() {
        SetBreakAmount(breakAmount + breakIncrement);
        if (steppedBreakPercentage >= cursorSheetSize.x && !broken) {
            broken = true;
            BreakBlock();
        }
    }

    public void UpdateArm() {
        if (inventory.inventoryItems[inventory.currentItem].ID != 0) {
            if (inventory.inventoryItems[inventory.currentItem].ID <= 100) {
                spriteObject.SetActive(false);
                //arm.SetActive(false);
                voxelArm.SetActive(true);
                UpdateArmUVs(inventory.inventoryItems[inventory.currentItem].ID);
            } else {
                arm.SetActive(false);
                voxelArm.SetActive(false);
                spriteObject.SetActive(true);
                UpdateSpriteMesh();
            }
        } else {
            //arm.SetActive(true);
            voxelArm.SetActive(false);
            spriteObject.SetActive(false);
        }
    }

    public float laserWidthMultiplier;
    public Transform goalLaser;

    Vector3 velocity = Vector3.zero;

    public float smoothTime;

    float animTime = 0f;

    public ParticleSystem ps;

    float time;

    void Interaction() {
        RaycastHit hit;
        UnityEngine.Physics.Raycast(camera.cam.transform.position, camera.cam.transform.forward, out hit, reach, layerMask);
        if (hit.transform != null) {
            globalCoordinates = Coordinates.WorldToCoordinates(hit.point + (camera.cam.transform.forward * 0.01f));
            if (!globalCoordinates.Equals(cursorPlacement)) {
                highlightObject.SetActive(true);
                highlightObject.transform.position = globalCoordinates.worldPosition;
                byte ID = world.blockIDAtCoordinates(globalCoordinates);
                textureAnim.startFrame = (ID + (Voxels.voxelAmount - 1)) / (Voxels.voxelAmount * 3f);
                textureAnim.numTilesX = Voxels.voxelAmount;
                SetBreakAmount(0);
                cursorPlacement = globalCoordinates;
                broken = false;
            }
            if (breakKey) {
                breakLength = world.blockSpeedAtCoordinates(globalCoordinates);
                if (inventory.inventoryItems[inventory.currentItem].ID > 200) {
                    if (Voxels.types[world.blockIDAtCoordinates(globalCoordinates)].effectiveTool == Tools.tools[inventory.inventoryItems[inventory.currentItem].ID - 201].type) {
                        breakLength = breakLength * Tools.tools[inventory.inventoryItems[inventory.currentItem].ID - 201].multiplier;
                    }
                }
                breaking = true;
                if (breakingParticles.isPlaying == false) {
                    byte ID = world.blockIDAtCoordinates(globalCoordinates);
                    textureAnim.startFrame = (ID + (Voxels.voxelAmount - 1)) / (Voxels.voxelAmount * 3f);
                    textureAnim.numTilesX = Voxels.voxelAmount;
                    breakingParticles.Play();
                }
                laser.gameObject.SetActive(true);
                float dist = Vector3.Distance(hit.point, camera.cam.transform.position);
                laser.endWidth = laser.startWidth / Mathf.Clamp(dist, 1f, Mathf.Infinity);
                laser.SetPosition(0, laser.transform.position);
                laser.SetPosition(1, Vector3.Lerp(laser.transform.position, Vector3.SmoothDamp(laser.GetPosition(1), goalLaser.position, ref velocity, smoothTime), animTime));
                animTime += Time.deltaTime * 10f;
                pointLight.transform.position = (hit.point + (hit.normal * 0.01f));
                ps.transform.position = pointLight.transform.position;
                pointLight.transform.LookAt(hit.normal);
                crosshair.enabled = false;
                pointLight.gameObject.SetActive(true);
                time += Time.deltaTime;
                if (time > 0.01f) {
                    ps.Emit(1);
                    time = 0;
                }
                cursorObject.SetActive(true);
            }

            if (!breakKey) {
                if (breaking) {
                    crosshair.enabled = true;
                    animTime = 0;
                    laser.gameObject.SetActive(false);
                    pointLight.gameObject.SetActive(false);
                    breaking = false;
                    breakingParticles.Stop();
                    cursorObject.SetActive(false);
                    SetBreakAmount(0);
                    cursorPlacement = Coordinates.zero;
                }
            }

            if (place) {
                globalCoordinates = Coordinates.WorldToCoordinates(hit.point - (camera.cam.transform.forward * 0.01f));
                chunkCoordinates = new ChunkCoordinates(globalCoordinates);
                chunk = world.chunkMap[chunkCoordinates.x, chunkCoordinates.z];
                localCoordinates = Coordinates.GlobalToLocalOffset(globalCoordinates, chunkCoordinates);
                Coordinates playerHead = Coordinates.GlobalToLocalOffset(Coordinates.WorldToCoordinates(transform.position + (Vector3.up * 0.5f)), chunkCoordinates);
                Coordinates playerLegs = Coordinates.GlobalToLocalOffset(Coordinates.WorldToCoordinates(transform.position + (Vector3.down * 0.5f)), chunkCoordinates);
                if (!localCoordinates.Equals(playerHead) && !localCoordinates.Equals(playerLegs)) {
                    if (inventory.inventoryItems[inventory.currentItem].quantity > 0 && inventory.inventoryItems[inventory.currentItem].ID <= 100) {
                        //armAnim.SetTrigger("Place");
                        chunk.EditVoxel(localCoordinates, inventory.inventoryItems[inventory.currentItem].ID);
                        inventory.RemoveItem(inventory.currentItem, 1);
                    }
                }
                UpdateArm();
            }
        } else {
            if (!cursorPlacement.Equals(Coordinates.zero)) {
                cursorPlacement = Coordinates.zero;
            }
            if (breaking) {
                crosshair.enabled = true;
                animTime = 0;
                laser.gameObject.SetActive(false);
                cursorPlacement = Coordinates.zero;
                pointLight.gameObject.SetActive(false);
                breaking = false;
                breakingParticles.Stop();
                cursorObject.SetActive(false);
                SetBreakAmount(0);
            }
            if (highlightObject.activeInHierarchy) {
                highlightObject.SetActive(false);
            }
        }
    }

    bool uiOpen { get { return inventory.inventoryOpen; } }
}
