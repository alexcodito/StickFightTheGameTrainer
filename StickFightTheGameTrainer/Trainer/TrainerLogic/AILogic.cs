using System;
using UnityEngine;

/// <summary>
/// Custom AI logic inheritted from the core AI class.
/// Decompiler-generated code that has been refactored and optimized.
/// </summary>
public class AILogic : AI
{
    /*

    //
    // Inheritted from AI base class.
    //

    public bool canAttack;
    public bool goForGuns;
    public bool attacking;
    public bool dontAimWhenAttacking;
    public float reactionTime;
    public float reactionHitReset;
    public float range;
    public float heightRange;
    public float preferredRange;
    public float jumpOffset;
    public float targetingSmoothing;
    public float velocitySmoothnes;
    public float startAttackDelay;
    public Rigidbody target;
    public Transform behaviourTarget;

    private Transform aimer;
    private Fighting fighting;
    private float counter; // Reset target after a certain amount of time (target reset ticker)
    private Controller controller;
    private CharacterInformation targetInformation;
    private CharacterInformation info;
    private Transform head;
    private Movement movement;
    private float reactionCounter;
    private float velocity;
    private ControllerHandler controllerHandler;

    public AILogic()
    {
        canAttack = true;
        goForGuns = true;
        reactionTime = 0.4f;
        reactionHitReset = -0.5f;
        range = 1f;
        jumpOffset = 1f;
        velocitySmoothnes = 2f;
        heightRange = float.PositiveInfinity;
    }

    private void Start()
    {
        controllerHandler = ControllerHandler.Instance;
        controller = base.GetComponent<Controller>();
        info = base.GetComponent<CharacterInformation>();
        movement = base.GetComponent<Movement>();
        fighting = base.GetComponent<Fighting>();
        head = base.GetComponentInChildren<Head>().transform;
        SetStats();
        aimer = base.GetComponentInChildren<AimTarget>().transform.parent;
    }

    private void SetStats()
    {
        movement.forceMultiplier *= UnityEngine.Random.Range(0.5f, 1f);
    }

    */

    private void UpdateHandler()
    {
        startAttackDelay -= Time.deltaTime;
        var targetPosition = Vector3.zero;

        if (behaviourTarget)
        {
            targetPosition = behaviourTarget.position;
        }
        else if (target)
        {
            targetPosition = target.position;
        }

        // Reset target (presumably to prevent being stuck in certain sittuations)
        if (counter > 1f)
        {
            counter = UnityEngine.Random.Range(-0.5f, 0.5f);
            target = null;
        }

        if (targetPosition != Vector3.zero && (!targetInformation || !targetInformation.isDead))
        {
            info.paceState = 0;

            if (!dontAimWhenAttacking || !fighting.isSwinging)
            {
                if (targetingSmoothing == 0f)
                {
                    aimer.rotation = Quaternion.LookRotation(targetPosition - head.position);
                }
                else
                {
                    aimer.rotation = Quaternion.Lerp(aimer.rotation, Quaternion.LookRotation(targetPosition - head.position), Time.deltaTime * (5f / targetingSmoothing));
                }
            }

            counter += Time.deltaTime;

            // Move towards the target
            if (Vector3.Distance(head.position, targetPosition) > preferredRange)
            {
                if (targetPosition.z < head.position.z)
                {
                    if (velocitySmoothnes == 0f)
                    {
                        velocity = -1f;
                    }
                    else
                    {
                        velocity = Mathf.Lerp(velocity, -1f, Time.deltaTime * (5f / velocitySmoothnes));
                    }
                }

                if (targetPosition.z > head.position.z)
                {
                    if (velocitySmoothnes == 0f)
                    {
                        velocity = 1f;
                    }
                    else
                    {
                        velocity = Mathf.Lerp(velocity, 1f, Time.deltaTime * (5f / velocitySmoothnes));
                    }
                }

                controller.Move(velocity);
            }

            // Jump if the target's head is above
            if (targetPosition.y > head.position.y + jumpOffset)
            {
                controller.Jump(false, false);
            }

            attacking = false;
            if (!behaviourTarget && canAttack && startAttackDelay < 0f)
            {
                attacking = true;
                reactionTime = 0.4f;
                var currentAttackRange = range;

                if (fighting.weapon)
                {
                    if (fighting.weapon.isGun)
                    {
                        currentAttackRange = 25f;
                        reactionTime = 0.25f;
                    }
                    else
                    {
                        currentAttackRange = 2f;
                        reactionTime = 0.25f;
                    }
                }

                // Perform an attack
                if (Vector3.Distance(head.position, targetPosition) < currentAttackRange && targetPosition.y - head.position.y < heightRange)
                {
                    reactionCounter += Time.deltaTime;

                    if (reactionCounter > reactionTime)
                    {
                        reactionCounter = 0f;
                        controller.Attack();
                        //return;
                    }
                }
                else if (reactionCounter > 0f)
                {
                    reactionCounter -= Time.deltaTime;
                    //return;
                }
            }
        }
        else if (!behaviourTarget)
        {
            var closestTargetDistance = 100f;
            WeaponPickUp weaponPickUp = null;

            if (goForGuns)
            {
                weaponPickUp = UnityEngine.Object.FindObjectOfType<WeaponPickUp>();
            }

            if (weaponPickUp && weaponPickUp.transform.position.y < 10f && controller.fighting.weapon == null)
            {
                target = weaponPickUp.GetComponent<Rigidbody>();
                return;
            }

            foreach (var playerController in controllerHandler.players)
            {
                if (playerController != null)
                {
                    var playerCharacterInformation = playerController.GetComponent<CharacterInformation>();

                    // Set target if it is an NPC or a PC that isn't the itself, and if it is not dead.
                    if ((playerController.playerID < 0 || playerController.playerID != this.controller.playerID) && !playerCharacterInformation.isDead)
                    {
                        var torsoTransform = playerController.GetComponentInChildren<Torso>().transform;
                        var targetDistance = Vector3.Distance(head.position, torsoTransform.position);

                        if (targetDistance < closestTargetDistance)
                        {
                            closestTargetDistance = targetDistance;
                            target = torsoTransform.GetComponent<Rigidbody>();
                            targetInformation = playerCharacterInformation;
                        }
                    }
                }
            }
        }
    }
}
