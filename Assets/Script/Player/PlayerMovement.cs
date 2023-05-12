using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    //public Transform AlanAnimatorHeirarchy;
    public float minSpeed;
    public float walkSpeed;
    public float runSpeed;
    public float speedCap;
    //public bool ultraMode;
    public float turnSmoothTime = 0.2f;
    float turnSmoothVelocity;
    public float stickPushedFromCenter;
    public int playerActionMode = 0;
    //0 = walking
    //1 = jumping
    //2 = knockback
    //3 = punching
    //4 = dragging something
    //5 = falling
    //6 = death
    //7 = secret dance
    //8 = jump windup
    [SerializeField] Vector2 inputmag;
    public float gndAccelleration;
    public float airAccelleration;
    public float gndFriction;
    public float airFriction;
    Transform cameraT;
    Vector2 input2;
    public Vector2 speed2;
    RaycastHit touchRay;
    RaycastHit xChk;
    RaycastHit yChk;

    public float vspeed;
    public float hspeed;
    //public float vspeed2;
    //public float hspeed2;
    public int animSubID;
    public int jumpHesitationFrames;
    float hesitationCounter;
    //public float yangle;
    public bool onJumpRamp = false;
    public float angleRun;
    public Transform debugCube;
    public Transform debugCubeFront;
    public Transform debugCubeUp;
    public Transform dropShadow;
    GameObject launchSoundObj;
    public Transform characterAnimator;
    AudioSource launchSoundControl;
    MeshRenderer dropShadowGraphics;
    //float currentSpeed;
    //float currentrunAngle;
    //float avgPushForce;
    public float avgPushAngle;
    float targetTravelAngle;
    float normalisedSpeed;
    public Animator animatorMesh;
    public float pushAngle;
    float targetTravelAngleDeg;
    float pushAngleDeg;
    public float jumpForce;
    public string characterName;
    public float deathTimer;
    public float deathTimerMax;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Game Start!!");
        cameraT = Camera.main.transform;
        //Get component collections
        debugCube = transform.Find("Data");
        launchSoundObj = debugCube.Find("Sound").Find("LaunchSound").gameObject;
        launchSoundControl = launchSoundObj.GetComponent<AudioSource>();
        dropShadow = transform.Find("dropShadow");
        dropShadowGraphics = dropShadow.gameObject.GetComponent<MeshRenderer>();
        characterAnimator = transform.Find("body");
        foreach (Transform child in characterAnimator)
        {
            if (child.name != characterName)
            { child.gameObject.SetActive(false); }
        }

        animatorMesh = GetComponentInChildren<Animator>();


        //angleRun = 0;
    }

    private void CollideWallTic()
    {

        float checkDist = speed2.magnitude * Time.deltaTime > 0.3f ? speed2.magnitude * Time.deltaTime : 0.3f;
        //checkDist = (speed2.magnitude * Time.deltaTime) * 4;
        checkDist += 0.1f;
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(-1, 0, 0) * checkDist), Color.red);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, -1, 0) * checkDist), Color.green);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, 0, -1) * checkDist), Color.blue);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(1, 0, 0) * checkDist), Color.red);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, 1, 0) * checkDist), Color.green);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, 0, 1) * checkDist), Color.blue);
        //Debug.Log(checkDist);
        for (float rot = 0; rot < 2; rot += 0.125f) //increments of 0.125 degrees collision rays
        {
            Vector3 colAngle = new Vector3(Mathf.Sin(rot * Mathf.PI), 0, Mathf.Cos(rot * Mathf.PI));
            if (Physics.Raycast(transform.position, transform.TransformDirection(colAngle), out touchRay, (checkDist * transform.localScale.x), 1))
            {
                //Debug.Log("Ray was cast forward, and we got a hit!");
                transform.position = touchRay.point;
                //Vector3 colnorm = touchRay.normal;
                //colnorm.y = 0;
                transform.Translate(colAngle * (transform.localScale.z * -0.4f), Space.World);
                //speed2.x *= 0.8f;
                //speed2.y *= 0.8f;
            }
        }



    }
    private int CollideFloorPitchModTic()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(0, -1, 0)), out touchRay, transform.localScale.y * 0.6f, 1))
        {
            //Debug.Log("Ray was cast downward, and we got a hit!");
            transform.position = touchRay.point;
            transform.Translate(transform.up * (transform.localScale.y * 0.48f), Space.World);
            if (vspeed < 0)
            {
                vspeed = 0;
            }
            if (touchRay.collider.gameObject.tag == "Respawn")
            {
                //gameObject.GetComponent<GameStateVariables>().health = 0; //restart if touching death surface
                playerActionMode = 6;
                return 1;
            }



        }
        else
        {
            //Debug.Log("Ray was cast downward, and we got a MISS! Switching player to freefall state");
            playerActionMode = 5;
            if (onJumpRamp)
            {
                launchSoundControl.Play();
                //play ramp launch sound
            }

        }
        return 0;

    }
    private void CollideFloorFreeFallTic()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(0, -1, 0)), out touchRay, transform.localScale.y * 0.5f, 1))
        {
            //Debug.Log("Ray was cast downward, and we got a hit! Switching back to running mode");
            playerActionMode = 0;
            transform.position = touchRay.point;
            transform.Translate(transform.up * (transform.localScale.y * 0.5f), Space.World);
            if (vspeed < 0)
            {
                vspeed = 0;
            }
            //TODO: Check for death surfaces.

        }
        else
        {
            vspeed -= 100f * Time.deltaTime; //gravity
        }
        transform.Translate((new Vector3(0, vspeed, 0) * transform.localScale.y) * Time.deltaTime);
        //hspeed = 0f;

    }
    private void MoveCharacterDuringFreeFall2Tic()
    {
        input2 = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); //Stick position
        Vector2 preinputmag = input2 * runSpeed; //Multiply to reach intended speed values for later math


        float pushAngle = (Mathf.Atan2(input2.x, input2.y)) + (cameraT.eulerAngles.y * Mathf.Deg2Rad) % 360; //Get stick angle and add camera angle to it, so that forwards is always forwards relative to camera
        targetTravelAngleDeg = pushAngle * Mathf.Rad2Deg;
        pushAngleDeg = pushAngle * Mathf.Rad2Deg;
        Vector2 inputmag = new Vector2(Mathf.Sin(pushAngle), Mathf.Cos(pushAngle)); //prevent weird rotational stumblings re. MoveCharacter4Tic
        inputmag = inputmag.normalized * (preinputmag.magnitude);
        inputmag.x = (float)(Mathf.Round(inputmag.x * 10000f) / 10000);
        inputmag.y = (float)(Mathf.Round(inputmag.y * 10000f) / 10000); //4 decimal places

        Vector2 forceAdd = inputmag.normalized * (hspeed * 4);
        if (true) //smooth accelleration
        {
            speed2.x += ((inputmag.x - speed2.x) * airAccelleration) * Time.deltaTime;
        }
        if (true)
        {
            speed2.y += ((inputmag.y - speed2.y) * airAccelleration) * Time.deltaTime;
        }
        speed2.x += forceAdd.x;
        speed2.y += forceAdd.y;
        hspeed = 0;
        if (Math.Abs(speed2.x) < minSpeed)
        {
            speed2.x = 0;
        }
        if (Math.Abs(speed2.y) < minSpeed) //prevent floating point shenanigans
        {
            speed2.y = 0;
        }

        stickPushedFromCenter = speed2.magnitude / (runSpeed / 4);
        if (input2.magnitude > 0.1f)
        {
            if (speed2.magnitude < 12)
            {
                angleRun = Mathf.Atan2(speed2.x, speed2.y) * Mathf.Rad2Deg;
            }
            else
            {
                angleRun = Mathf.Atan2(speed2.x, speed2.y) * Mathf.Rad2Deg;
            }
        }
        normalisedSpeed = speed2.magnitude;
        speed2 = speed2.normalized * (normalisedSpeed > speedCap ? speedCap : normalisedSpeed);
        transform.Translate((new Vector3(speed2.x, 0, speed2.y) * transform.localScale.x) * Time.deltaTime, Space.World);
        //characterAnimator.eulerAngles = new Vector3(characterAnimator.eulerAngles.x, angleRun, characterAnimator.eulerAngles.z);
    }


    private void MoveCharacter4Tic()
    {
        input2 = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 preinputmag = input2 * runSpeed; //Multiply to reach intended speed values for later math


        float targetTravelAngle = (Mathf.Atan2(input2.x, input2.y)) + (cameraT.eulerAngles.y * Mathf.Deg2Rad); //Get stick angle and add camera angle to it, so that forwards is always forwards relative to camera
        targetTravelAngleDeg = targetTravelAngle * Mathf.Rad2Deg;
        if (input2.magnitude > 0.02)
        {
            if (speed2.magnitude > minSpeed * 6)
            {
                pushAngleDeg = Mathf.SmoothDampAngle(pushAngleDeg, targetTravelAngleDeg, ref turnSmoothVelocity, 0.1f); //Smoothly rotate and limit cornering speed
            }
            else
            {
                pushAngleDeg = targetTravelAngleDeg;
            }
        }
        pushAngle = pushAngleDeg * Mathf.Deg2Rad;  //c# radian nonsense, why cant I set it to degrees for everything?
        Vector2 inputmag = new Vector2(Mathf.Sin(targetTravelAngle), Mathf.Cos(targetTravelAngle));
        inputmag = inputmag.normalized * (preinputmag.magnitude > speedCap ? speedCap : preinputmag.magnitude); //recalculate stick values based on calculated angle
        inputmag.x = (float)(Mathf.Round(inputmag.x * 10000f) / 10000);
        inputmag.y = (float)(Mathf.Round(inputmag.y * 10000f) / 10000); //4 decimal places
        if (Math.Abs(avgPushAngle - pushAngle) > Mathf.PI * 1.2f)
        {
            speed2 = speed2 * 0.5f; //speed braking thing that I'm not sure is doing anything but it doesnt seem to be causing any weirdness so leave it in or delete it if you want
        }
        avgPushAngle = (avgPushAngle + pushAngle) / 2; //mean current angle with the one from last tic

        Vector2 forceAdd = inputmag.normalized * (hspeed * 4);
        if (speed2.magnitude < inputmag.magnitude)
        {
            speed2.x += ((inputmag.x - speed2.x) * gndAccelleration) * Time.deltaTime;
            speed2.y += ((inputmag.y - speed2.y) * gndAccelleration) * Time.deltaTime;
        }
        else if (speed2.magnitude > inputmag.magnitude)
        {
            speed2.y *= gndFriction;
            speed2.x *= gndFriction;
        }
        speed2 = new Vector2(Mathf.Sin(pushAngle), Mathf.Cos(pushAngle)) * speed2.magnitude;

        speed2.x += forceAdd.x;
        speed2.y += forceAdd.y; //TODO: for ramps and speed pads, this needs work
        normalisedSpeed = speed2.magnitude;
        speed2 = speed2.normalized * (normalisedSpeed > speedCap ? speedCap : normalisedSpeed); //speed limit
        hspeed = 0;
        if (Math.Abs(speed2.x) < minSpeed)
        {
            speed2.x = 0;
        }
        if (Math.Abs(speed2.y) < minSpeed)
        {
            speed2.y = 0;
        } //eliminate floating point shenanigans so that zero means zero

        stickPushedFromCenter = speed2.magnitude / (runSpeed / 4);
        if (input2.magnitude > 0.06)
        {
            if (speed2.magnitude < 12)
            {
                angleRun = Mathf.Atan2(speed2.x, speed2.y) * Mathf.Rad2Deg;
            }
            else
            {
                angleRun = Mathf.Atan2(speed2.x, speed2.y) * Mathf.Rad2Deg;
            }
        }

        if (!WallColMoveTic()) { transform.Translate((new Vector3(speed2.x, 0, speed2.y) * transform.localScale.x) * Time.deltaTime, Space.World); }
        characterAnimator.eulerAngles = new Vector3(characterAnimator.eulerAngles.x, angleRun, characterAnimator.eulerAngles.z);
    }

    private void JumpAbilityTic()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(0, -1, 0)), out touchRay, (transform.localScale.y * 0.5f), 1))
        {
            if (touchRay.collider.gameObject.tag == "JumpStorageBoost")
            {
                vspeed = 150;
                hspeed = 150;
                onJumpRamp = true;
            }
            else
            {
                if (vspeed < 0)
                {
                    vspeed = 0;


                }
                hspeed = 0;
                onJumpRamp = false;
            }
        }
        if (Input.GetAxis("JB1") == 1f)
        {
            vspeed = jumpForce; // 70f;
            hspeed = 0f;

            //transform.Translate(transform.up * 2 , Space.World);
            //this is no longer needed here as jumping has been moved
            playerActionMode = 8;
            hesitationCounter = 0;
        }
    }

    private void JumpHesitate()
    {
        if (hesitationCounter < (jumpHesitationFrames) * (1 / 60))
        {
            hesitationCounter += Time.deltaTime;
        }
        else
        {
            transform.Translate(transform.up * (transform.localScale.y * 0.25f), Space.World);
            playerActionMode = 9;
        }
    }

    private void JumpSwitchToFallAnimation()
    {
        if (vspeed < 0f)
        {
            playerActionMode = 5;
        }
    }

    private bool WallColMoveTic()
    {
        return false;
        float checkDist = speed2.magnitude * Time.deltaTime > 0.3f ? speed2.magnitude * Time.deltaTime : 0.3f;
        checkDist = (speed2.magnitude * Time.deltaTime) * 4;
        //checkDist += 0.1f;
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(speed2.normalized.x, 0, speed2.normalized.y) * checkDist), Color.red);
        //Debug.Log(checkDist);
        CollideWallTic();
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(speed2.normalized.x, 0, speed2.normalized.y)), out touchRay, (checkDist * 1.1f) * transform.localScale.x, 1))
        {
            //Debug.Log("Normal!");
            Vector3 rAngle = transform.position - touchRay.point;
            rAngle.Normalize();
            transform.position = touchRay.point;
            transform.Translate((rAngle * 0.2f) * transform.localScale.y);
            //Debug.Log(touchRay.normal);
            CollideWallTic();
            return true;

        }
        else { return false; }
    }


    private int CollideCeilingTic()
    {
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, 1, 0) * 0.6f), Color.green);
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(0, 1, 0)), out touchRay, transform.localScale.y * 0.6f, 1))
        {
            //Debug.Log("Ray was cast upward, and we got a hit!");
            transform.position = touchRay.point;
            transform.Translate(transform.up * (transform.localScale.y * -0.61f), Space.World);

            vspeed *= -1.0f;
            //transform.Translate(transform.up * (transform.localScale.y * (0-Mathf.Abs(vspeed)) ), Space.World);


            if (touchRay.collider.gameObject.tag == "Respawn")
            {
                //gameObject.GetComponent<GameStateVariables>().health = 0; //restart if touching death surface
                playerActionMode = 6;
                return 1;
            }



        }
        return 0;
    }


    private void Update() // FixedUpdate is called once per 1/60s.
    {
        animatorMesh.SetInteger("mode", playerActionMode);
        if (playerActionMode == 0)
        {
            //running
            animatorMesh.SetFloat("speed", speed2.magnitude / (speedCap / 6));
            //WallColMoveTic();
            //CollideWallTic();
            MoveCharacter4Tic();
            //WallColMoveTic();
            CollideWallTic();
            JumpAbilityTic();
            CollideFloorPitchModTic();
        }
        else if (playerActionMode == 1)
        {
            //stuck1

        }
        else if (playerActionMode == 2)
        {
            //knockback
        }
        else if (playerActionMode == 3)
        {
            //stuck
            CollideWallTic();
        }
        else if (playerActionMode == 4)
        {
            //dragging/holding
        }
        else if (playerActionMode == 5)
        {
            //falling
            MoveCharacterDuringFreeFall2Tic();
            CollideFloorFreeFallTic();
            CollideWallTic();
            CollideCeilingTic();

        }
        else if (playerActionMode == 6)
        {
            Debug.Log("Death");
            Debug.Log(playerActionMode);
            //death
            deathTimer += Time.deltaTime;
            if(deathTimer > deathTimerMax)
            { 
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                playerActionMode = 0x7fffffff; //way out of bounds so nothing will happen
            
            }
        }
        else if (playerActionMode == 8)
        {
            //jump windup
            MoveCharacter4Tic();
            CollideWallTic();
            CollideFloorPitchModTic();
            JumpHesitate();

        }
        else if (playerActionMode == 9)
        {
            //jump upward animation hack
            MoveCharacterDuringFreeFall2Tic();
            CollideFloorFreeFallTic();
            CollideWallTic();
            CollideCeilingTic();
            JumpSwitchToFallAnimation();
        }
        //drop shadow
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(0, -1, 0)), out touchRay, 9999, 1))
        {
            //enable plane renderer and move shadow transform to raycast hit point
            dropShadowGraphics.enabled = true;
            dropShadow.position = touchRay.point + ((Vector3.up * 0.1f) * transform.localScale.y);
        }
        else
        {
            //missed, maybe above a bottomless pit? whatever the ground is so far below we can hide the drop shadow
            dropShadowGraphics.enabled = false;
        }

    }


}
