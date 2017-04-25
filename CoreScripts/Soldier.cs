using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum SOLDIER
{
    NONE,
    SHIELDMAN,
    LONGSWORD,
    MAGE,
    RANGER
}

public class Soldier
{
    public string myName;
    public bool isDead = false;
    public GameObject frame;
    public GameCore core;
    public float health;
    public float maxHealth;

    public float damageDoneBoost = 1f;
    public float damageTakenBoost = 1f;
    public float healingTakenBoost = 1f;

    public Mob myTarget;

    // atak

    public int castProgress = 0;
    public int castTime = 60;
    private int damage = 1;

    public int myID;

    private Absorb[] absorb = new Absorb[20]; // tablica absorbow
    private int absAmount = 0;
    public float totalAbsorb = 0;
    public SOLDIER type = SOLDIER.LONGSWORD;

    private int oneSecGap = 0;

    private Vector3 moveVector;
    private bool isMoving = false;
    private float moveVal;
    public SoldierEffectSystem effectSystem;

    public Soldier(int _myid, string _name, GameObject _frame, float _maxHealth, SOLDIER _type)
    {
        myID = _myid;
        core = GameCore.Core;
        frame = _frame;
        myName = _name;
        maxHealth = _maxHealth;
        type = _type;
        health = maxHealth;
        ApplySoldierType();
        effectSystem = new SoldierEffectSystem(this);
    }

    private void ApplySoldierType()
    {
        switch (type)
        {
            case SOLDIER.SHIELDMAN:
                {
                    maxHealth *= 1.2f;
                    castTime = 60;
                    damage = 1;
                }
                break;
            case SOLDIER.LONGSWORD:
                {
                    castTime = 120;
                    damage = 4;
                }
                break;
            case SOLDIER.MAGE:
                {
                    castTime = 180;
                    damage = 8;
                }
                break;
            case SOLDIER.RANGER:
                {
                    castTime = 90;
                    damage = 2;
                }
                break;
        }
    }

    public void moveTo(Vector3 destination, float _speed)
    {
        moveVector = destination;
        isMoving = true;
        moveVal = _speed;
    }

    private void moveHandler()
    {
        if (isMoving)
        {
            frame.transform.position = Vector3.MoveTowards(frame.transform.position, moveVector, moveVal);
            moveVal = Mathf.Max(0.05f, moveVal * 0.75f);
            if (frame.transform.position == moveVector)
                isMoving = false;
        }
    }

    public void UpdateDamageDoneBoost()
    {
        damageDoneBoost = 1f;
        /*
        switch (type)
        {
            case SOLDIER.LONGSWORD: damageDoneBoost *= 1.2f; break;
            case SOLDIER.MAGE: damageDoneBoost *= 1.4f; break;
            case SOLDIER.RANGER: damageDoneBoost *= 1.1f; break;
            case SOLDIER.SHIELDMAN: damageDoneBoost *= 0.8f; break;
        }
        */
    }

    public void UpdateDamageTakenBoost()
    {
        damageTakenBoost = 1f;
        switch (type)
        {
            case SOLDIER.LONGSWORD: damageTakenBoost *= 1f; break;
            case SOLDIER.MAGE: damageTakenBoost *= 1.2f; break;
            case SOLDIER.RANGER: damageTakenBoost *= 1.1f; break;
            case SOLDIER.SHIELDMAN: damageTakenBoost *= 0.6f; break;
        }
        if ((effectSystem.FindBuff((int)Buff.DB.WORD_OF_KINGS_FAITH) != null) && (core.myCaster.myAura[(int)AURA.IRON_FAITH].isActive))
        {
            damageTakenBoost *= 1 - (core.myCaster.myAura[(int)AURA.IRON_FAITH].stacks * 0.1f);
        }
    }

    public void UpdateHealingTakenBoost()
    {
        healingTakenBoost = 1f;
        if (effectSystem.FindDebuff(DEBUFF.CURSED_SWIPE) != null)
        {
            healingTakenBoost *= 0.5f;
        }
    }

    public float GetPercentHealth()
    {
        return health / maxHealth;
    }

    public float GetPercentShield()
    {
        return totalAbsorb / maxHealth;
    }

    public Color GetHealthColor()
    {
        return new Color(2f - GetPercentHealth() * 2f, GetPercentHealth() * 2f, 0f);
    }

    public Sprite GetIcon()
    {
        switch (type)
        {
            case SOLDIER.SHIELDMAN: return Resources.Load<Sprite>("SoldierIcons/Shieldman");
            case SOLDIER.LONGSWORD: return Resources.Load<Sprite>("SoldierIcons/Longsword");
            case SOLDIER.MAGE: return Resources.Load<Sprite>("SoldierIcons/Mage");
            case SOLDIER.RANGER: return Resources.Load<Sprite>("SoldierIcons/Ranger");
            default: return null;
        }
    }

    private void CastHandler()
    {
        if (myTarget != null)
        {
            if ((castProgress++ >= castTime) && (myTarget.isAlive))
            {
                AttackOrder();
                castProgress = 0;
            }
        }
    }

    private void EffectsHandler()
    {
        if (!core.GameFinished)
        {
            moveHandler();
            if (!isDead)
            {
                effectSystem.SortDebuffs();
                effectSystem.ActivateDebuffs();

                if (health <= 0) isDead = true;

                effectSystem.SortBuffs();
                effectSystem.ActivateBuffs();
            }
        }
    }

    public void Update()
    {
        //frame.transform.GetChild(8).GetComponent<Image>().fillAmount = (float)castProgress / (float)castTime;
        CastHandler();
        EffectsHandler();        
        if (isDead)
            health = 0;
    }

    //******************************************** START ATAK ******************************************** \\

    public void SetTarget(Mob _target)
    {
        myTarget = _target;
    }

    public void AttackOrder()
    {
        if (myTarget != null)
        myTarget.TakeDamage(damage);
    }

    //******************************************** START BUFFY ******************************************** \\

    private int BuffsAmount = 0;

    public void BuffMe(int bufftype, int buffdur, Caster _caster, SpellInfo spellInfo, int _gap, int minv = 0, int maxv = 0)
    {
        effectSystem.BuffMe(bufftype, buffdur, _caster, spellInfo, _gap, minv, maxv);

        UpdateDamageDoneBoost();
        UpdateDamageTakenBoost();
        UpdateHealingTakenBoost();
    }

    public void Kill()
    {
        frame.GetComponent<scrUnitPanel>().Kill();
        isDead = true;
        ClearSoldier();
    }

    public void ClearSoldier()
    {
        effectSystem.Clear();
    }


    //******************************************** START HEALING ******************************************** \\
    public int Shield(int _value, HEALSOURCE _source)
    {
        absorb[absAmount++] = new Absorb(_value, _source);
        totalAbsorb += _value;
        frame.GetComponent<scrUnitPanel>().RefreshHealthInfo();
        return _value;
    }

    private Healing ApplyHealingModifiersBySource(Healing baseHealing, Caster caster, HEALSOURCE source)
    {
        Healing newHealing = baseHealing;
        if (source == HEALSOURCE.SCROLL_OF_RENEW)
        {
            newHealing.value = (int)(maxHealth * 0.5f);
            return newHealing;
        }

        if (source == HEALSOURCE.SHADOWSONG)
        {
            if (caster.myTalentTree.GetTalentByName("Shadowmend").Points > 0)
            {
                Buff myb = effectSystem.FindBuff((int)Buff.DB.SOOTHING_VOID);
                if (myb != null)
                {
                    newHealing.value = (int)(newHealing.value + newHealing.value * caster.myTalentTree.GetTalentByName("Shadowmend").Points * 0.2f);
                    return newHealing;
                }
            }
        }

        return newHealing;
    }

    private void ExecuteDebuffsTriggeredByHealing(Healing myHeal)
    {
        Debuff mindbomb = effectSystem.FindDebuff(DEBUFF.MIND_BOMB);
        if (mindbomb != null)
        {
            if (health / maxHealth > 0.95f)
                mindbomb.Remove();
        }

        Debuff voidflame = effectSystem.FindDebuff(DEBUFF.VOIDFLAME);
        if (voidflame != null)
        {
            voidflame.multiplier = Mathf.Max(0.2f, voidflame.multiplier - 0.6f);
            ((Voidflame)voidflame).RefreshIcon();
        }

        //if (FindDebuff(DEBUFF.SOUL_TOMB) != null)
        //    GameCore.Core.myEnemy.Heal((int)(myHeal.value / 10f));
    }

    private void CriticalHealTrigger(Healing myHeal)
    {
        if ((myHeal.healtype != HEALTYPE.PERIODIC_SINGLE) && (myHeal.healtype != HEALTYPE.PERIODIC_MULTI))
        {
            if (myHeal.isCrit)
            {
                core.CriticalHealOccured();
            }
        }
    }

    private Healing ApplyHealingModifiersByBuff(HEALSOURCE source, Healing baseHeal)
    {
        Healing newHealing = baseHeal;
        Buff _myb = effectSystem.FindBuff((int)Buff.DB.FLASH_OF_FUTURE);
        if ((_myb != null) && (source == HEALSOURCE.WOK_LIGHT))
        {
            newHealing.value = (int)(newHealing.value * 1.5f);
            _myb.Remove();
            return newHealing;
        }

        return newHealing;
    }

    private int ApplyOverhealingModifiers(int overheal_value, Caster _caster, HEALSOURCE source)
    {
        if (core.buffSystem.FindBuff(CASTERBUFF.DIVINE_INTERVENTION) != null)
        {
            if (_caster.AuraActive(AURA.GUIDANCE_OF_RAELA))
            {
                if ((source != HEALSOURCE.GUIDANCE_OF_RAELA) && (source != HEALSOURCE.WOK_LOYALTY))
                {
                    core.CastAutoSpell((int)SPELL.GUIDANCE_OF_RAELA, null, overheal_value, overheal_value);
                    return 0;
                }
            }
        }
        return overheal_value;
    }

    private void WoKLoyaltyBeaconHealing(Healing myHeal, HEALSOURCE source, HEALTYPE healtype)
    {
        if ((GameCore.chosenChampion == CHAMPION.PALADIN) && (source != HEALSOURCE.WOK_LOYALTY) && (source != HEALSOURCE.GUIDANCE_OF_RAELA))
        {
            float perc = VALUES.WORD_OF_KINGS_LOYALTY_TRANSFER + core.myCaster.myAura[(int)AURA.SPIRIT_BOND].stacks * VALUES.SPIRIT_BOND_INCREASE;
            if ((core.myCaster.AuraActive(AURA.SPIRIT_BOND)) && ((healtype == HEALTYPE.DIRECT_MULTI) || (healtype == HEALTYPE.PERIODIC_MULTI)))
                perc *= 0.3f;
            core.BeaconHeal((int)(myHeal.value * perc), 4, this);
        }
    }

    private void UpdateHealthBar()
    {
        frame.GetComponent<scrUnitPanel>().RefreshHealthInfo();
    }

    public Healing Heal(int minh, int maxh, float critchance, Caster _caster, SpellInfo info, HEALSOURCE source, HEALTYPE healtype)
    {
        if (!isDead)
        {
            Healing myHeal = new Healing(0, false, healtype);
                   
            myHeal = CalculateHealing(minh, maxh, critchance, healtype);
            myHeal.value = (int)(myHeal.value * _caster.HealingMultiplier());
            myHeal.value = (int)(myHeal.value * healingTakenBoost);

            myHeal = ApplyHealingModifiersBySource(myHeal, _caster, source);

            LogHealing((int)myHeal.value, myHeal.isCrit);

            WoKLoyaltyBeaconHealing(myHeal, source, healtype);

            myHeal = ApplyHealingModifiersByBuff(source, myHeal);

            float heal_value = Mathf.Min(myHeal.value, maxHealth - health);
            int overheal_value = 0;
            overheal_value = Mathf.Max(0, (int)((myHeal.value + health) - maxHealth));

            overheal_value = ApplyOverhealingModifiers(overheal_value, _caster, source);
            myHeal.overhealing = overheal_value;

            health = health + heal_value;
            core.recount.AddEntry(source, (int)heal_value, overheal_value);

            ExecuteDebuffsTriggeredByHealing(myHeal);

            CriticalHealTrigger(myHeal);

            UpdateHealthBar();
            return myHeal;
        }
        return null;
    }

    private Healing CalculateHealing(int minh, int maxh, float critchance, HEALTYPE healtype)
    {
        int value = 0;
        bool isCrit = false;

        if (minh != maxh)
            value = Random.Range(minh, maxh);
        else
            value = minh;
        if (critchance > 0)
        {
            if (Random.Range(0, 100) < critchance)
            {
                value *= 2;
                isCrit = true;
            }
        }

        Healing myHeal = new Healing(value, isCrit, healtype);
        return myHeal;
    }

    private void LogHealing(float value, bool isCrit, int type = 0)
    {
        if ((value > 10) && (health + 10 < maxHealth))
        {
            float posx;
            Color mycolor;

            switch (type)
            {
                case 0:
                    {
                        posx = 0f;
                        mycolor = Color.green;  // healing
                    }
                    break;
                case 1:
                    {
                        posx = 0.3f;
                        mycolor = Color.white; // absorb
                    }
                    break;
                default:
                    {
                        posx = 0.3f;
                        mycolor = Color.green;  // healing
                    }
                    break;
            }

            if (value > 0)
            {
                if (frame != null)
                {
                    GameObject myLog = Object.Instantiate(Resources.Load("CombatText"), frame.transform.position + new Vector3(posx, 0, -2), frame.transform.rotation) as GameObject;
                    myLog.GetComponent<Text>().text = value.ToString();
                    myLog.GetComponent<Text>().color = mycolor;

                    myLog.GetComponent<CombatLogScript>().isCrit = isCrit;
                }
            }
            core.AddHealing(value);
        }
    }
    //******************************************** KONIEC HEALING ******************************************** \\

    //******************************************** START DAMAGE ******************************************** \\
    public float lastdamage = 1f;

    public void Damage(float minh, float maxh, float critchance, DAMAGESOURCE source)
    {
        float damageamount = 0;
        damageamount = CalculateDamage(minh, maxh, critchance);
        DamageTaken(damageamount);
    }

    private void DamageTaken(float _damage)
    {
        float _absorbed = 0;
        int temp = absAmount - 1;

        if (effectSystem.FindBuff((int)Buff.DB.ROYALTY) != null)
        {
            LogHealing((int)_damage, false, 1);
            core.recount.AddEntry(HEALSOURCE.ROYALTY, (int)_damage, 0); // do logow
        }
        else
        {
            if (absAmount > 0) // jesli sa jakiekolwiek absorby
            {
                while ((_damage > 0) && (temp >= 0))
                {
                    float _abs = absorb[temp].value;

                    _abs = Mathf.Min(_damage, _abs);

                    _damage -= _abs; // zmniejszamy dmg o wartosc absorbu
                    _absorbed += _abs; // dodajemy do calkowitego absorbu ta wartosc
                    core.recount.AddEntry(absorb[temp].source, (int)_abs, 0); // do logow
                    absorb[temp].value -= _abs; // zuzywamy absorb
                    totalAbsorb -= _abs;

                    if (absorb[temp].value <= 0) // jesli absorb jest pusty
                    {
                        absorb[temp] = null; // to go usuwamy
                        absAmount--;
                    }

                    temp--;
                }
            }

            LogHealing((int)_absorbed, false, 1);

            health = Mathf.Max(health - _damage, 0);
            if (health <= 0)
                core.troopsHandler.SortSoldiers();

            frame.GetComponent<scrUnitPanel>().RefreshHealthInfo();
        }
    }

    private float CalculateDamage(float minh, float maxh, float critchance)
    {
        float value;

        value = Random.Range(minh, maxh) * damageTakenBoost * EnemyScript.bonusDmg;
        if (Random.Range(0, 100) < critchance)
            value *= 2;
        return value;
    }

    private void LogDamage(float value)
    {
        GameObject myLog = Object.Instantiate(Resources.Load("CombatText"), frame.transform.position + new Vector3(1, 0, 0), frame.transform.rotation) as GameObject;
        myLog.GetComponent<TextMesh>().text = value.ToString();
    }
    //******************************************** KONIEC DAMAGE ******************************************** \\

    private Debuff GetDebuffByID(DEBUFF _id)
    {
        switch (_id)
        {
            case DEBUFF.TANTRUM_DOT: return new Tantrum(this);
            case DEBUFF.CRY: return new Cry(this);
            case DEBUFF.CURSED_SWIPE: return new CursedSwipe(this);
            case DEBUFF.BRUTAL_BITE: return new BrutalBite(this);
            case DEBUFF.SHADOWFANG: return new Shadowfang(this);
            case DEBUFF.SOUL_TOMB: return new SoulTomb(this);
            case DEBUFF.MIND_BOMB: return new MindBomb(this);
            case DEBUFF.VOIDFLAME: return new Voidflame(this);
            case DEBUFF.DARK_ROSE: return new DarkRose(this);
            default: return null;
        }
    }

    public void HostileCastFinished(EnemySpellInfo _espell, int val1 = 0, int val2 = 0)
    {
        int damageVal = _espell.baseValue;

        if (_espell.ID == HOSTILESPELL.BLOODBOIL)
            damageVal = (int)(health * 0.33f);

        Damage(damageVal, damageVal * 1.1f, 0, _espell.damageSource);

        if (_espell.debuff != DEBUFF.NONE)
        {
            Debuff myd = effectSystem.DebuffMe(GetDebuffByID(_espell.debuff));
            if (myd != null)
            {
                if (_espell.debuff == DEBUFF.SHADOWFANG)
                    myd.multiplier += 4f / val1;
            }
        }
    }

    public void CastFinished(SpellEffect mySpell, Caster _caster, int minval = 0, int maxval = 0)
    {
        mySpell.OnCastFinished(_caster, this, minval, maxval);
    }
}


