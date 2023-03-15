using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    [SerializeField]Vector2 speed2;
    RaycastHit touchRay;
    public float vspeed;
    public float hspeed;
    //public float vspeed2;
    //public float hspeed2;
    public int animSubID;
    public int jumpHesitationFrames;
    int hesitationCounter;
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
        animatorMesh = GetComponentInChildren<Animator>();


        //angleRun = 0;
    }

    private void CollideWallTic()
    {
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(-7,0, 0)), Color.red);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0,-7, 0)), Color.green);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, 0, -7)), Color.blue);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(7, 0, 0)), Color.red);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, 7, 0)), Color.green);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, 0, 7)), Color.blue);
        float checkDist = speed2.magnitude * Time.deltaTime > 0.3f ? speed2.magnitude * Time.deltaTime : 0.3f;
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(0, 0, 1)), out touchRay, checkDist, 1))
        {
            //Debug.Log("Ray was cast forward, and we got a hit!");
            transform.position = touchRay.point;
            transform.Translate(transform.forward * (transform.localScale.z * -0.3f), Space.World);
        }
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(0, 0, -1)), out touchRay, checkDist, 1))
        {
            //Debug.Log("Ray was cast backward, and we got a hit!");
            transform.position = touchRay.point;
            transform.Translate(transform.forward * (transform.localScale.z * 0.3f), Space.World);
        }
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(1, 0, 0)), out touchRay, checkDist, 1))
        {
            //Debug.Log("Ray was cast right, and we got a hit!");
            transform.position = touchRay.point;
            transform.Translate(transform.right * (transform.localScale.x * -0.3f), Space.World);
        }
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(-1, 0, 0)), out touchRay, checkDist, 1))
        {
            //Debug.Log("Ray was cast left, and we got a hit!");
            transform.position = touchRay.point;
            transform.Translate(transform.right * (transform.localScale.x * 0.3f), Space.World);
        }
    }
    private void CollideFloorPitchModTic()
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
                gameObject.GetComponent<GameStateVariables>().health = 0; //restart if touching death surface
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
        
    }
    private void CollideFloorFreeFallTic()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(0, -1, 0)), out touchRay, transform.localScale.y * 0.5f, 1))
        {
            Debug.Log("Ray was cast downward, and we got a hit! Switching back to running mode");
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
        transform.Translate(new Vector3(0, vspeed, 0) * Time.deltaTime);
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
        transform.Translate(new Vector3(speed2.x, 0, speed2.y) * Time.deltaTime, Space.World);
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
            if (speed2.magnitude > minSpeed*6)
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
        if (Math.Abs(avgPushAngle - pushAngle) > Mathf.PI*1.2f)
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
        transform.Translate(new Vector3(speed2.x, 0, speed2.y) * Time.deltaTime, Space.World);
        characterAnimator.eulerAngles = new Vector3(characterAnimator.eulerAngles.x, angleRun, characterAnimator.eulerAngles.z);
    }

    private void JumpAbilityTic()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(0, -1, 0)), out touchRay, transform.localScale.y * 0.5f, 1))
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
        if (hesitationCounter != jumpHesitationFrames)
        {
            hesitationCounter += 1;
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





    private void Update() // FixedUpdate is called once per 1/60s
    {
        animatorMesh.SetInteger("mode", playerActionMode);
        if(playerActionMode==0)
        {
            //running
            animatorMesh.SetFloat("speed", speed2.magnitude/ (speedCap/6));
            CollideWallTic();
            MoveCharacter4Tic();
            CollideWallTic();
            JumpAbilityTic();
            CollideFloorPitchModTic();
        }
        else if(playerActionMode==1)
        {
            //jumping
        }
        else if (playerActionMode == 2)
        {
            //knockback
        }
        else if (playerActionMode == 3)
        {
            //punching
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
            
        }
        else if (playerActionMode == 6)
        {
            //death
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
            JumpSwitchToFallAnimation();
        }
        //drop shadow
        if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(0, -1, 0)), out touchRay, 9999, 1))
        {
            //enable plane renderer and move shadow transform to raycast hit point
            dropShadowGraphics.enabled = true;
            dropShadow.position = touchRay.point + Vector3.up * 0.1f;
        }
        else
        {
            //missed, maybe above a bottomless pit? whatever the ground is so far below we can hide the drop shadow
            dropShadowGraphics.enabled = false;
        }

    }


}
