using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[RequireComponent(typeof(MOB))]
public class MinionAI : MonoBehaviour
{
    MOB me;
    GameObject[] waypoints;

    void Start()
    {
        me = GetComponent<MOB>();
    }
}

// https://gist.github.com/LiamKarlMitchell/1274ea5bcf3e634aa31449f1c98b9cfb
// Every 0.25 seconds, each minion will go down a checklist of options until it finds a valid one to do. It won’t follow invalid actions, and it won’t follow actions after that. This means that in this list of options, the catch-all default option is last (walking towards a waypoint).
//Follow any current specialized behavior rules, such as from CC (Taunts, Flees, Fears)
//Continue attacking(or moving towards) their current target if that target is still valid.
//If they have failed to attack their target for 4 seconds, they temporarily ignore them instead.
//Find a new valid target in the minion’s acquisition range to attack.
//If multiple valid targets, prioritize based on “how hard is it for me to path there?”
//Check if near a target waypoint, if so change the target waypoint to the next in the line.
//Walk towards the target waypoint. If a minion can’t follow any of these behaviors, it will do nothing. Minions have a lot of checks to note whether or not a target is valid or not. There’s obvious ones like “which team is the target on” but also non-obvious ones like “where on the map is my target”. Many of these will be covered further down.