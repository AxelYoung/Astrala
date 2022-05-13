using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    [Header("Movement")]
    [SerializeField] float speed;
    public float sprintMultiplier;
    [SerializeField] float jumpHeight;

    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Vector3 moveDir;

    [Header("Crouching")]
    [SerializeField] float crouchingHeightMulitplier;
    public float crouchingSpeedMultiplier;
    [SerializeField] float crouchAnimLength;

    [HideInInspector] public bool crouching;
    bool crouchAnimPlaying;
    float standingHeight;
    Vector3 standingCenter = Vector3.zero;

    [Header("Physics")]
    [SerializeField] float gravity;
    [SerializeField] float maxGroundDistance;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float defaultVelocity;

    Transform feet;
    bool fallen = false;

    [Header("Footsteps")]
    [SerializeField] float defaultFootstepVol;
    [SerializeField] AudioClip[] stoneAudioClips;
    [SerializeField] AudioClip[] grassAudioClips;
    AudioSource footstepAudioSource;

    [Header("Controls")]
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] KeyCode crouchKey = KeyCode.C;
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode breakKey = KeyCode.Mouse0;
    [SerializeField] KeyCode placeKey = KeyCode.Mouse1;
    [SerializeField] KeyCode dashKey = KeyCode.Q;

    [HideInInspector] public bool sprinting => Input.GetKey(sprintKey) && !crouching;
    bool jumping => Input.GetKey(jumpKey) && controller.isGrounded;
    bool crouch => Input.GetKeyDown(crouchKey) && !crouchAnimPlaying && controller.isGrounded;

    [Header("Dash")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashLength;
    [SerializeField] ParticleSystem dashParticle;

    ParticleSystem.VelocityOverLifetimeModule dashParticleVel;

    bool dashing => Input.GetKeyDown(dashKey) && !dashAnimPlaying;
    bool dashAnimPlaying;

    [Header("Slide")]
    [SerializeField] float slideSpeed;
    [SerializeField] float slideLength;

    bool sliding;

    [Header("Camera Settings")]
    [SerializeField] float sensitivity;
    [SerializeField] float bobSpeed;
    [SerializeField] float bobAmount;
    [SerializeField] Camera overlayCam;
    [SerializeField] float fovSmooth;
    float refVel = 0f;

    Camera cam;
    float xRotation;
    float defaultCamPos;
    float cameraTimer;

    bool stepped = false;

    [Header("Interaction")]
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

    bool place => Input.GetKeyDown(placeKey);

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

    [HideInInspector] public bool craftingOpen = false;

    public GameObject craftingUI;

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


    # region Initialization

    void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        inventory = GetComponent<PlayerInventory>();
        InitializeMovement();
        InitializeCamera();
        InitializeHand();
    }

    void InitializeCamera() {
        cam = transform.GetChild(0).GetComponent<Camera>();
        defaultCamPos = cam.transform.localPosition.y;
    }

    void InitializeMovement() {
        dashParticleVel = dashParticle.velocityOverLifetime;
        controller = GetComponent<CharacterController>();
        footstepAudioSource = GetComponent<AudioSource>();
        feet = transform.GetChild(1);
        standingHeight = controller.height;
    }

    void InitializeHand() {
        smoothMoveDefault = smoothMove;
        smoothRotDefault = smoothRot;
        CalculateMesh();
        CreateVoxelArm();
        CreateSpriteMeshObject();
        CreateCursor();
        textureAnim = breakingParticles.textureSheetAnimation;
    }

    #endregion Initialization

    void Update() {
        if (!uiOpen) {
            CameraMovement();
            Movement();
            Interaction();
        }
        if (craftingOpen) {
            if (Input.GetKeyUp(KeyCode.E)) {
                craftingOpen = false;
                craftingUI.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        Physics();
    }

    void FixedUpdate() {
        if (breaking) Breaking();
    }

    void LateUpdate() {
        if (!sliding && !dashing) {
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


    #region Movement
    void Movement() {

        if (jumping) {
            moveDir.y = jumpHeight;
        }

        if (sprinting && crouch) {
            StartCoroutine(Slide());
        } else if (crouch || (crouching && Input.GetKeyDown(sprintKey))) StartCoroutine(Crouch());
        if (dashing) StartCoroutine(Dash());

        if (!dashAnimPlaying && !sliding) {
            // Get movement input for axises and translate into local Vector3
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (input != Vector2.zero) {
                if (sprinting) {
                    SmoothFOV(100, fovSmooth);
                } else {
                    SmoothFOV(90, fovSmooth);
                }
            } else {
                SmoothFOV(90, fovSmooth);
            }

            float yDir = moveDir.y;
            moveDir = (transform.right * input.x + transform.forward * input.y) * (crouching ? speed * crouchingSpeedMultiplier : sprinting ? speed * sprintMultiplier : speed);
            moveDir.y = yDir;
        }
    }

    void SmoothFOV(int goal, float smoothTime) {
        if (cam.fieldOfView != goal) {
            float fov = Mathf.SmoothDamp(cam.fieldOfView, goal, ref refVel, smoothTime);
            cam.fieldOfView = fov;
            overlayCam.fieldOfView = fov;
        }
    }

    IEnumerator Crouch() {
        // Check if object above player before standing
        if (crouching && UnityEngine.Physics.Raycast(transform.position, Vector3.up, 2f)) yield break;

        crouchAnimPlaying = true;

        // Reset time of animation to 0 and set variables according to mulipliers
        float elapsedTime = 0;
        float targetHeight = crouching ? standingHeight : standingHeight * crouchingHeightMulitplier;
        float currentHeight = controller.height;
        Vector3 targetCenter = crouching ? standingCenter : standingCenter + (Vector3.up * crouchingHeightMulitplier);
        Vector3 currentCenter = controller.center;

        while (elapsedTime < crouchAnimLength) {
            // Lerp both height and center of player controller from starting to goal height and center
            controller.height = Mathf.Lerp(currentHeight, targetHeight, elapsedTime / crouchAnimLength);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, elapsedTime / crouchAnimLength);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        controller.height = targetHeight;
        controller.center = targetCenter;

        crouching = !crouching;

        crouchAnimPlaying = false;
    }

    IEnumerator Dash() {
        dashAnimPlaying = true;

        // Reset time of animation to 0 and set variables according to mulipliers
        float elapsedTime = 0;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        smoothMove = 0f;

        if (input != Vector2.zero) {
            float yDir = moveDir.y;
            Vector3 dashDir = (transform.right * input.x + transform.forward * input.y) * dashSpeed;

            while (elapsedTime < dashLength) {
                moveDir = new Vector3(dashDir.x, moveDir.y, dashDir.z);
                SmoothFOV(120, fovSmooth / 2f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            dashParticleVel.x = -input.x * 3;
            dashParticleVel.z = -input.y * 3;
            dashParticle.Emit(100);
        }

        smoothMove = smoothMoveDefault;
        dashAnimPlaying = false;
    }

    IEnumerator Slide() {
        sliding = true;

        // Reset time of animation to 0 and set variables according to mulipliers
        float elapsedTime = 0;
        float currentHeight = controller.height;
        Vector3 currentCenter = controller.center;

        float newElapsedTime = 0;

        float crouchedHeight = standingHeight * crouchingHeightMulitplier;
        Vector3 crouchedCenter = standingCenter + (Vector3.up * crouchingHeightMulitplier);

        controller.height = crouchedHeight;
        controller.center = crouchedCenter;

        float standTransitionLength = 0.2f;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input != Vector2.zero) {
            float yDir = moveDir.y;
            Vector3 dashDir = (transform.right * input.x + transform.forward * input.y) * slideSpeed;

            while (elapsedTime < slideLength) {
                if (elapsedTime >= slideLength - standTransitionLength) {
                    if (!UnityEngine.Physics.Raycast(transform.position, Vector3.up, 1f)) {
                        controller.height = Mathf.Lerp(crouchedHeight, standingHeight, newElapsedTime / standTransitionLength);
                        controller.center = Vector3.Lerp(crouchedCenter, standingCenter, newElapsedTime / standTransitionLength);
                        newElapsedTime += Time.deltaTime;
                        elapsedTime += Time.deltaTime;
                    }
                } else {
                    elapsedTime += Time.deltaTime;
                }

                moveDir = new Vector3(dashDir.x, moveDir.y, dashDir.z);
                SmoothFOV(110, fovSmooth / 2f);
                yield return null;
            }

            controller.height = standingHeight;
            controller.center = standingCenter;
        }

        sliding = false;
    }

    void Physics() {
        // If grounded set to default velocity and play sound
        if (controller.isGrounded) {
            if (!fallen) {
                footstepAudioSource.volume = sprintMultiplier * defaultFootstepVol;
                //PlayStepSound();
                fallen = true;
            }
        } else {
            fallen = false;
            // Continously add gravity and apply velocity to player
            moveDir.y -= gravity * Time.deltaTime;
        }


        if (controller.velocity.y < -1 && controller.isGrounded) {
            moveDir.y = 0;
        }

        controller.Move(moveDir * Time.deltaTime);
    }

    #region Footsteps

    void Footsteps(float bob) {
        if (!stepped) {
            // Change step volume based on player state (ex. Quieter when crouching)
            footstepAudioSource.volume = crouching ? crouchingSpeedMultiplier * defaultFootstepVol : sprinting ? sprintMultiplier * defaultFootstepVol : defaultFootstepVol;

            // Checks if close to lowest point of head bob
            if (Mathf.Sin(cameraTimer) < 0) {
                PlayStepSound();
                stepped = true;
            }
        } else if (Mathf.Sin(cameraTimer) > 0) {
            stepped = false;
        }
    }

    void PlayStepSound() {
        // Raycast below to detect ground type
        Transform hitTransform;
        RaycastHit hit;
        UnityEngine.Physics.Raycast(cam.transform.position, Vector3.down, out hit, controller.height + 0.5f);
        hitTransform = hit.transform;

        // If raycast returns null, use backup sphere cast to detect ground type if player is walking along edge of object
        if (hit.transform == null) {
            hitTransform = UnityEngine.Physics.OverlapSphere(feet.transform.position, 1f)[0].transform;
        }

        // Ground type determined by tag, play random audio clip from array of sounds to corresponding ground type
        switch (hitTransform.tag) {
            case "Stone":
                footstepAudioSource.PlayOneShot(stoneAudioClips[Random.Range(0, stoneAudioClips.Length - 1)]);
                break;
            case "Grass":
                footstepAudioSource.PlayOneShot(grassAudioClips[Random.Range(0, grassAudioClips.Length - 1)]);
                break;
        }
    }

    #endregion Footsteps

    #endregion Movement

    #region Camera

    void CameraMovement() {
        // Recive input from two axises, adjust sensitivity
        Vector2 input = new Vector2(Input.GetAxis("Mouse X") * sensitivity, Input.GetAxis("Mouse Y") * sensitivity);

        // Clamp X rotation
        xRotation -= input.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotations to cam
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * input.x);

        Headbob();
    }

    void Headbob() {
        // Head bob only when grounded and moving
        if (controller.isGrounded && !sliding) {
            if (moveDir.x != 0 || moveDir.z != 0) {
                // Calculate bob speed and amount based on mulitiplers, bob is simple sin wave
                cameraTimer += Time.deltaTime * (crouching ? bobSpeed * crouchingSpeedMultiplier : sprinting ? bobSpeed * sprintMultiplier : bobSpeed);
                float bob = defaultCamPos + Mathf.Sin(cameraTimer) * (crouching ? bobAmount * crouchingSpeedMultiplier : sprinting ? bobAmount * sprintMultiplier : bobAmount);

                // Apply bob and pass through to footstep function
                cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, bob, cam.transform.localPosition.z);
                Footsteps(bob);
            } else {
                //(REMOVED FOR NOW, MIGHT REIMPLEMENT (slightly jarring)) Reset to default pos when stopped
                //cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, defaultCamPos, cam.transform.localPosition.z);
            }
        }
    }

    #endregion Camera

    #region Hand

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
        armUVs.AddRange(voxelFaceUVsFromIndex(new Vector2(ID - 1, 0)));
        armUVs.AddRange(voxelFaceUVsFromIndex(new Vector2(ID - 1, 2)));
        for (int i = 0; i < 6; i++) {
            armUVs.AddRange(voxelSideUVsFromIndex(new Vector2(ID - 1, 1)));
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
        UnityEngine.Physics.IgnoreCollision(controller, voxelCollider, true);
        voxelCollider.sharedMesh = voxelMesh;
        voxelCollider.convex = true;
        Rigidbody rb = voxel.AddComponent<Rigidbody>();
        voxelUVs.AddRange(voxelFaceUVsFromIndex(new Vector2(voxelType - 1, 0)));
        voxelUVs.AddRange(voxelFaceUVsFromIndex(new Vector2(voxelType - 1, 2)));
        for (int i = 0; i < 6; i++) {
            voxelUVs.AddRange(voxelSideUVsFromIndex(new Vector2(voxelType - 1, 1)));
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

    Vector2[] voxelFaceUVsFromIndex(Vector2 index) {
        Vector2[] uvs = new Vector2[6];
        for (int i = 0; i < 6; i++) {
            uvs[i] = new Vector2(((VoxelData.hexUVs[i].x + 0.5f + index.x) / Voxels.voxelAmount), ((VoxelData.hexUVs[i].y + 0.5f + index.y) / 3f));
        }
        return uvs;
    }

    Vector2[] voxelSideUVsFromIndex(Vector2 index) {
        Vector2[] uvs = new Vector2[4];
        for (int i = 0; i < 4; i++) {
            uvs[i] = new Vector2(((VoxelData.hexSideUVs[i].x + 0.5f + index.x) / Voxels.voxelAmount), ((VoxelData.hexSideUVs[i].y + index.y) / 3f));
        }
        return uvs;
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
        Vector3[] bottomVerticies = relativeVoxelVerticies(Vector3.zero);
        meshVerticies.AddRange(bottomVerticies);
        meshTriangles.AddRange(relativeVoxelTriangles(false));
        // Top face
        Vector3[] topVerticies = relativeVoxelVerticies(Vector3.up);
        meshVerticies.AddRange(topVerticies);
        meshTriangles.AddRange(relativeVoxelTriangles(true));
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
        meshUVs.AddRange(faceUVsFromIndex(state));
        meshUVs.AddRange(faceUVsFromIndex(state));
        for (int i = 0; i < 6; i++) {
            meshUVs.AddRange(sideUVsFromIndex(state));
        }
        cursorMesh.uv = meshUVs.ToArray();
        cursorMeshFilter.mesh = cursorMesh;
    }

    Vector3[] relativeVoxelVerticies(Vector3 center) {
        Vector3[] verticies = new Vector3[6];
        for (int i = 0; i < 6; i++) {
            verticies[i] = new Vector3(VoxelData.hexVerticies[i].x + center.x, center.y, VoxelData.hexVerticies[i].y + center.z);
        }
        return verticies;
    }

    int[] relativeVoxelTriangles(bool top) {
        int[] triangles = new int[VoxelData.topHexagonalFace.Length];
        for (int i = 0; i < triangles.Length; i++) {
            triangles[i] = (meshVerticies.Count - 6) + (top ? VoxelData.topHexagonalFace[i] : VoxelData.bottomHexagonalFace[i]);
        }
        return triangles;
    }

    Vector2[] faceUVsFromIndex(int index) {
        Vector2[] uvs = new Vector2[6];
        for (int i = 0; i < 6; i++) {
            uvs[i] = new Vector2(((VoxelData.hexUVs[i].x + 0.5f + index) / breakSpriteSheetSize), ((VoxelData.hexUVs[i].y + 1.5f) / 2f));
        }
        return uvs;
    }

    Vector2[] sideUVsFromIndex(int index) {
        Vector2[] uvs = new Vector2[4];
        for (int i = 0; i < 4; i++) {
            uvs[i] = new Vector2(((VoxelData.hexSideUVs[i].x + 0.5f + index) / breakSpriteSheetSize), (VoxelData.hexSideUVs[i].y) / 2f);
        }
        return uvs;
    }

    ChunkCoordinates chunkCoordinates;
    Chunk chunk;
    Coordinates localCoordinates;
    Coordinates globalCoordinates;

    bool broken = false;

    int breakSpriteSheetSize = 9;

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
        steppedBreakPercentage = Mathf.FloorToInt(breakPercentage * breakSpriteSheetSize);
        UpdateCursorUVs(steppedBreakPercentage);
    }

    void Breaking() {
        SetBreakAmount(breakAmount + breakIncrement);
        if (steppedBreakPercentage >= breakSpriteSheetSize && !broken) {
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
    public float xSpaceMultiplier;
    public float ySpaceMultiplier;

    public Transform goalLaser;

    Vector3 velocity = Vector3.zero;

    public float smoothTime;

    float animTime = 0f;

    public ParticleSystem ps;

    float time;

    void Interaction() {
        RaycastHit hit;
        UnityEngine.Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, reach, layerMask);
        if (hit.transform != null) {
            globalCoordinates = Coordinates.WorldToCoordinates(hit.point + (cam.transform.forward * 0.01f));
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
            if (Input.GetKey(breakKey)) {
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
                float dist = Vector3.Distance(hit.point, cam.transform.position);
                laser.endWidth = laser.startWidth / Mathf.Clamp(dist, 1f, Mathf.Infinity);
                laser.SetPosition(0, laser.transform.position);
                //laser.SetPosition(1, goalLaser.position);
                laser.SetPosition(1, Vector3.Lerp(laser.transform.position, Vector3.SmoothDamp(laser.GetPosition(1), goalLaser.position, ref velocity, smoothTime), animTime));
                animTime += Time.deltaTime * 10f;
                //Vector3 laserDir = (laser.transform.position - hit.point);
                //laser.transform.parent.transform.LookAt(laserDir, Vector3.up);
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

            if (!Input.GetKey(breakKey)) {
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
                Coordinates voxelInView = Coordinates.WorldToCoordinates(hit.point + (cam.transform.forward * 0.01f));
                if (world.blockIDAtCoordinates(voxelInView) == Voxels.CraftingTable.ID) {
                    craftingOpen = true;
                    craftingUI.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    moveDir = new Vector3(0, moveDir.y, 0);
                } else {
                    globalCoordinates = Coordinates.WorldToCoordinates(hit.point - (cam.transform.forward * 0.01f));
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

    #endregion Hand

    bool uiOpen { get { return !inventory.inventoryOpen && !craftingOpen ? false : true; } }
}
