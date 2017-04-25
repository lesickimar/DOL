﻿using UnityEngine;
using System.Collections;

public class DivineIntervention : SpellEffect
{
    public DivineIntervention() : base()
    {

    }

    public override void OnCast(Caster who, Soldier target)
    {
        GameCore.Core.buffSystem.BuffMe(CASTERBUFF.DIVINE_INTERVENTION, 900f, who);
        if (!target.frame.GetComponent<AudioSource>().isPlaying)
            target.frame.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Sounds/DivineInterventionSound"));
    }

    public override void OnCastFinished(Caster who, Soldier target, int minval = 0, int maxval = 0)
    {
        
    }

    public override void Execute(Caster who, Soldier target, int minval = 0, int maxval = 0)
    {
        
    }

}