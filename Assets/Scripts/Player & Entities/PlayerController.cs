using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    // Float | Speed & Velocity
    [SerializeField] float Acceleration;
    public float RotationalSpeed;
    [SerializeField] float MaxVelocity;
    [SerializeField] float MinHeatGainVel;
    [SerializeField] float LastVelocity;

    // Float | Heat
    public float Heat;
    [Range(0f, 10000f)] public float MaxHeat;
    [SerializeField] float HeatLoss;
    [SerializeField] float HeatGain;

    // Float | Primary
    [SerializeField] float PrimaryCooldown;
    [SerializeField] float PrimairyHeatCost;
    [SerializeField] float PrimaryFireDelay;
    [SerializeField] float PrimaryLaunchSpeed;
    [SerializeField][Range(0f, 40f)] float ScattershotSpread;
    [SerializeField] float TurretRotationSpeedController;

    // Float | Secondary
    [SerializeField] float SecondaryCooldown;
    [SerializeField] float SecondaryHeatCost;
    [SerializeField] float SecondaryFireDelay;
    [SerializeField] float SecondaryLaunchSpeed;

    // Float | Abilities
    [SerializeField] float RepairKitCooldown;
    [SerializeField] float RepairKitHeatCost;
    [SerializeField] float HeatVentCooldown;
    [SerializeField] float HeatVentDuration;
    [SerializeField] float HeatVentDrainAmount;

    // Float | Other
    float LastHeatAmount;

    // Int
    public int Health;
    public int MaxHealth;
    [SerializeField] int PrimaryDamage;
    [SerializeField] int SecondaryDamage;
    [SerializeField] int ChargedShotPenetration;
    [SerializeField] int ScattershotsAmount;

    // Bool | Ship
    public bool DisableHeat;
    public bool DisableMovement;
    bool AutoStop;
    bool UseController;

    // Bool | Weapons
    bool CanFirePrimary = true;
    bool CanFireSecondary = true;
    public bool DisableWeapons;
    public bool DisableAiming;

    // Bool | Other
    public bool IsDead;
    Dictionary<Ability, bool> IsAbilityAvailable;

    // GameObject
    [SerializeField] GameObject Laser;
    [SerializeField] GameObject Missile;
    GameObject Turret;
    [SerializeField] GameObject TorpedoTube;
    [SerializeField] GameObject CrossHair;

    // Vector2
    Vector2 AimDirection = Vector2.zero;
    Vector2 MovementVector = Vector2.zero;

    // ParticleSystem
    //[SerializeField] ParticleSystem[] EngineExhaust;
    [SerializeField] PolyParticleSystem EngineExhaust;
    [SerializeField] ParticleSystem Explosion;
    [SerializeField] ParticleSystem VentExhaust;

    // Audio Sources
    [SerializeField] AudioSource TurretFireSfx;
    [SerializeField] AudioSource EnginesSfx;
    [SerializeField] AudioSource HitSfx;

    // Other
    Rigidbody2D Rb;
    Warhead TorpedoWarhead;
    public PrimaryWeapon PrimaryType;
    public SecondaryWeapon SecondaryType;
    //public PlayerInputActions PlayerControls;
    Ability AbilityInUse;
    [SerializeField] LayerMask ProjectileHitMask;
    public UnityEvent<float> OnHeatChangePercentage;

    // InputActions
    /*InputAction Move;
    InputAction Look;
    InputAction PrimaryFire;
    InputAction SecondaryFire;
    InputAction Ability1;
    InputAction Ability2;*/

    // Start is called before the first frame update
    void Start()
    {
        Health = MaxHealth;
        Heat = MaxHeat / 2;

        Rb = GetComponent<Rigidbody2D>();

        TorpedoWarhead = new Warhead();
        TorpedoWarhead.Set(SecondaryDamage, "Enemy");

        IsAbilityAvailable = new Dictionary<Ability, bool>
        {
            { Ability.RepairKit, true },
            { Ability.HeatVent, true }
        };

        SoundManager soundManager = GameObject.Find(GameObjectNames.Managers).GetComponent<SoundManager>();

        soundManager.AddAudioSource(TurretFireSfx, SourceType.Sfx);
        soundManager.AddAudioSource(EnginesSfx, SourceType.Sfx);
        //soundManager.AddAudioSource(DeathSfx, SourceType.Sfx);

        //ChangeWeaponType(SecondaryWeapon.Missile);
    }

    public void Awake()
    {
        //PlayerControls = new();
        Turret = transform.Find("Turret").gameObject;
    }

    /*private void OnEnable()
    {
        Move = PlayerControls.Player.Move;
        Move.Enable();

        Look = PlayerControls.Player.Look;
        Look.Enable();

        PrimaryFire = PlayerControls.Player.FirePrimary;
        PrimaryFire.Enable();
        PrimaryFire.performed += (InputAction.CallbackContext context) => StartCoroutine(FirePrimary());

        SecondaryFire = PlayerControls.Player.FireSecondary;
        SecondaryFire.Enable();
        SecondaryFire.performed += (InputAction.CallbackContext context) => StartCoroutine(FireSecondary());

        Ability1 = PlayerControls.Player.Ability1;
        Ability1.Enable();
        Ability1.performed += (InputAction.CallbackContext context) => Ability1();

        Ability2 = PlayerControls.Player.Ability2;
        Ability2.Enable();
        Ability2.performed += (InputAction.CallbackContext context) => Ability2();
    }*/

    /*private void OnDisable()
    {
        Move.Disable();
        Look.Disable();
        PrimaryFire.Disable();
        SecondaryFire.Disable();
        Ability1.Disable();
        Ability2.Disable();
    }*/

    // Update is called once per frame
    void Update()
    {
        if (!IsDead)
        {
            if (float.IsNaN(Heat))
                Heat = LastHeatAmount;

            UseController = GameObject.Find(GameObjectNames.Managers).GetComponent<GameManager>().UseController;
            //SimpleSteering = GameObject.Find(GameObjectNames.Managers).GetComponent<GameManager>().SimpleSteering;

            CrossHair.SetActive(UseController);

            if (Health <= 0 || Heat < 0 || Heat > MaxHeat)
                StartCoroutine(PlayerDeath());

            if (AutoStop)
            {
                SlowDown();
                MovementVector = Vector2.zero;
                //EnableDisableExhaust(false);
            }

            if (!DisableAiming)
            {
                // Sets AimDirection to point at the mouse cursor if player doesn't use a controller
                if (!UseController)
                    AimDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;

                // Sets turret to face towards AimDirection
                Turret.transform.up = AimDirection;
            }

            if (!DisableHeat)
            {
                float heatloss = HeatLoss * Time.deltaTime;
                Heat -= heatloss;
                //Debug.Log($"Loss: {HeatLoss}");
                if (GetVelocity() >= MinHeatGainVel)
                {
                    float heatgain = (HeatGain - Mathf.Clamp(GetAcceleration(), 0f, HeatGain * 1000f)) * Time.deltaTime;
                    Heat += heatgain;
                    //Debug.Log($"Gain: {heatgain}");
                }

                //Debug.Log(GetAcceleration());
            }

            LastHeatAmount = Heat;
            OnHeatChangePercentage.Invoke(Heat/MaxHeat);

            if (Time.timeScale > 0)
            {
                CapVelocity();
                LastVelocity = GetVelocity();
            }

            if (!DisableMovement)
            {
                //if (SimpleSteering && UseController)
                //{
                //    // If the left controller stick is not being moved then slow down player ship
                //    if (MovementVector.y == 0 || MovementVector.y == 0)
                //        SlowDown();

                //    else
                //    {
                //        // How much the stick is being moved
                //        float moveStrength = MovementVector.magnitude;
                //        // Angle between where stick is being pointed and the front of the ship
                //        float angle = Vector2.Angle(transform.up, MovementVector);

                //        // Rotates the ship in the direction stick is pointed
                //        Vector3 cross = Vector3.Cross(transform.up, MovementVector);
                //        Rb.AddTorque(RotationalSpeed * cross.z * moveStrength * Time.deltaTime, ForceMode2D.Force);

                //        // Moves ship forward if the stick direction is less or equal to 45 degrees, else slow down ship
                //        if (angle <= 45f)
                //            Rb.AddForce(transform.up * Acceleration * moveStrength * Time.deltaTime, ForceMode2D.Force);
                //        else if (angle <= 90f)
                //            SlowDown();

                //        EnableEngines(MovementVector.magnitude > 0);
                //    }
                //}
                //else
                //{
                if (MovementVector.y < 0)
                    SlowDown();

                else if (MovementVector.y > 0)
                    Rb.AddForce(transform.up * Acceleration * MovementVector.y * Time.deltaTime, ForceMode2D.Force);

                EnableEngines(MovementVector.y > 0);

                Rb.AddTorque(-(RotationalSpeed * MovementVector.x * Time.deltaTime), ForceMode2D.Force);
                //}
            }
        }

        GameObjectHelper.AudioPauseCheck(TurretFireSfx);
        GameObjectHelper.AudioPauseCheck(EnginesSfx);
        //AudioPauseCheck(HitSfx);
    }

    #region Input Functions
    public void Move(InputAction.CallbackContext context)
    {
        if (!DisableMovement && !AutoStop)
            MovementVector = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (!DisableAiming && Gamepad.current != null)
        {
            // Sets AimDirection to where the left controller stick is pointing if a controller is connected
            if (context.ReadValue<Vector2>() != Vector2.zero)
                AimDirection = context.ReadValue<Vector2>();
        }
    }

    public void PrimaryFire(InputAction.CallbackContext context)
    {
        if (context.performed && !context.canceled && Time.timeScale > 0)
            StartCoroutine(FirePrimary());
    }

    public void SecondaryFire(InputAction.CallbackContext context)
    {
        if (context.performed && !context.canceled && Time.timeScale > 0)
            StartCoroutine(FireSecondary());
    }

    public void Ability1(InputAction.CallbackContext context)
    {
        if (GameObject.Find("Managers").GetComponent<UIManager>().CurrentInterface == MenuUI.None)
            if (context.performed && !context.canceled && Time.timeScale > 0)
                StartCoroutine(Repair());
    }

    public void Ability2(InputAction.CallbackContext context)
    {
        if (GameObject.Find("Managers").GetComponent<UIManager>().CurrentInterface == MenuUI.None)
            if (context.performed && !context.canceled && Time.timeScale > 0)
                StartCoroutine(VentHeat());
    }
    #endregion

    #region Weapon Functions

    IEnumerator FirePrimary()
    {
        if (CanFirePrimary && !DisableWeapons)
        {
            TurretFireSfx.Play();
            CanFirePrimary = false;
            //CanFireSecondary = false;
            yield return new WaitForSeconds(PrimaryFireDelay);

            if (PrimaryType == PrimaryWeapon.ScatterShot)
            {
                List<GameObject> lasers = new();
                List<Vector2> vectorSpread = VectorHelper.CreateVectorSpread(AimDirection, ScattershotSpread, ScattershotsAmount);

                for (int x = 0; x < ScattershotsAmount; x++)
                {
                    GameObject newLaser = Instantiate(Laser);
                    newLaser.transform.position = transform.position;

                    newLaser.GetComponent<Laser>().Set(PrimaryDamage, PrimaryLaunchSpeed, ProjectileHitMask);
                    newLaser.transform.up = vectorSpread[x];

                    newLaser.GetComponent<SpriteRenderer>().color = GameObject.Find("Managers").GetComponent<GameManager>().PlayerLaserColor;
                    newLaser.tag = "PlayerProjectile";
                    //newLaser.layer = SortingLayer.NameToID("Player Projectile");
                    lasers.Add(newLaser);
                }

                foreach (GameObject laser in lasers)
                    laser.GetComponent<Laser>().Fire();
            }
            else
            {
                GameObject newLaser = Instantiate(Laser);

                newLaser.GetComponent<Laser>().Set(PrimaryDamage, PrimaryLaunchSpeed, ProjectileHitMask);
                newLaser.GetComponent<SpriteRenderer>().color = GameObject.Find("Managers").GetComponent<GameManager>().PlayerLaserColor;

                if (PrimaryType == PrimaryWeapon.ChargedShot)
                    newLaser.GetComponent<Laser>().SetPenetration(ChargedShotPenetration);

                newLaser.transform.position = transform.position;
                newLaser.tag = "PlayerProjectile";
                //newLaser.layer = SortingLayer.NameToID("Player Projectile");
                newLaser.transform.up = AimDirection;
                newLaser.GetComponent<Laser>().Fire();
            }

            if (!DisableHeat || AbilityInUse == Ability.HeatVent)
                Heat += PrimairyHeatCost;

            //CanFireSecondary = true;
            yield return StartCoroutine(Cooldown(PrimaryCooldown, GameObject.Find("Managers").GetComponent<UIManager>().PrimaryTimer));
            CanFirePrimary = true;
        }
    }

    IEnumerator FireSecondary()
    {
        if (CanFireSecondary && !DisableWeapons)
        {
            CanFireSecondary = false;
            //CanFirePrimary = false;
            yield return new WaitForSeconds(SecondaryFireDelay);

            GameObject newTorpedo = Instantiate(Missile);

            Torpedo torpedo = newTorpedo.GetComponent<Torpedo>();

            if (TorpedoWarhead.GetType().ToString() == "Missile")
                torpedo.LoadWarhead<Missile>(TorpedoWarhead);
            else if (TorpedoWarhead.GetType().ToString() == "Flak")
                torpedo.LoadWarhead<Flak>(TorpedoWarhead);
            else
                torpedo.LoadWarhead<Warhead>(TorpedoWarhead);

            newTorpedo.tag = "PlayerProjectile";
            //newTorpedo.layer = SortingLayer.NameToID("Player Projectile");
            newTorpedo.transform.position = TorpedoTube.transform.position;
            newTorpedo.transform.rotation = transform.rotation;

            torpedo.Set(SecondaryDamage, SecondaryLaunchSpeed, ProjectileHitMask);
            torpedo.SetColor(GameObject.Find("Managers").GetComponent<GameManager>().PlayerTorpedoColor);
            torpedo.Fire();

            if (!DisableHeat || AbilityInUse == Ability.HeatVent)
                Heat += SecondaryHeatCost;

            //CanFirePrimary = true;

            yield return StartCoroutine(Cooldown(SecondaryCooldown, GameObject.Find("Managers").GetComponent<UIManager>().SecondaryTimer));
            CanFireSecondary = true;
        }
    }

    public void ChangeWeaponType(PrimaryWeapon newPrimary)
    {
        if (newPrimary != PrimaryType)
        {
            //switch (newPrimary)
            //{
            //    case PrimaryWeapon.ScatterShot:
            //        ScattershotsAmount = 5;
            //        break;
            //    case PrimaryWeapon.ChargedShot:
            //        ChargedShotPenetration = 1;
            //        break;
            //}

            PrimaryType = newPrimary;
        }
    }

    public void ChangeWeaponType(SecondaryWeapon newSecondary)
    {
        if (newSecondary != SecondaryType)
        {
            switch (newSecondary)
            {
                case SecondaryWeapon.Missile:
                    Missile missile = new Missile();
                    missile.Set(10, "Enemy", 1000, 30, 2f, 30);
                    missile.Set(1 << 7);
                    TorpedoWarhead = missile;
                    break;
                case SecondaryWeapon.Flak:
                    Flak flak = new Flak();
                    flak.Set(10, "Enemy", 5, 10);
                    TorpedoWarhead = flak;
                    break;
            }
        }
    }

    #endregion

    #region Ship Functions
    void SlowDown()
    {
        if (GetVelocity() < 0.5f)
            Rb.velocity = Vector2.zero;

        else
            Rb.AddForce(Rb.velocity.normalized * Acceleration * MovementVector.y * Time.deltaTime, ForceMode2D.Force);
    }

    void CapVelocity()
    {
        Rb.velocity = Vector2.ClampMagnitude(Rb.velocity, MaxVelocity);
    }

    IEnumerator PlayerDeath()
    {
        DisableHeat = true;
        DisableMovement = true;
        DisableWeapons = true;
        DisableAiming = true;
        IsDead = true;
        Cursor.visible = false;
        //EnableDisableExhaust(false);
        EnableEngines(false);

        GameObject.Find("Managers").GetComponent<UIManager>().DisplayHud(false);

        if (Health <= 0)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            transform.Find("Turret").GetComponent<SpriteRenderer>().enabled = false;
            Rb.velocity = Vector2.zero;
            Explosion.Play();
        }

        yield return new WaitForSeconds(2.5f);

        GameObject.Find("Managers").GetComponent<UIManager>().ShowGameOverScreen();
        Cursor.visible = true;

        if (Health <= 0)
            Destroy(gameObject);
    }

    public void Hurt(int damage)
    {
        Health -= damage;
        HitSfx.Play();
    }

    public void SetColor(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
        Turret.GetComponent<SpriteRenderer>().color = newColor;
    }

    public float GetVelocity()
    {
        return Rb.velocity.magnitude;
    }

    public float GetMaxVelocity()
    {
        return MaxVelocity;
    }

    float GetAcceleration()
    {
        float output = (GetVelocity() - LastVelocity) / Time.deltaTime;

        if (float.IsNaN(output))
            return 0;

        else
            return output;
    }
    #endregion

    #region Upgrade Functions
    public void Upgrade(Upgrade upgrade)
    {
        UpgradeType[] primaries = new UpgradeType[]
        {
            UpgradeType.Laser,
            UpgradeType.ScatterShot,
            UpgradeType.ChargedShot
        };

        UpgradeType[] secondaries = new UpgradeType[]
        {
            UpgradeType.Torpedo,
            UpgradeType.Missile,
            UpgradeType.Flak
        };

        if (upgrade.Type == UpgradeType.Ship)
            UpgradeShip(upgrade);

        else if (primaries.Contains(upgrade.Type))
            UpgradePrimary(upgrade);

        else if (secondaries.Contains(upgrade.Type))
            UpgradeSecondary(upgrade);
    }

    void UpgradeShip(Upgrade upgrade)
    {
        for (int x = 0; x < upgrade.Changes.Count; x++)
        {
            var temp = upgrade.Changes[x].GetType();
            StatChange change = upgrade.Changes[x];

            //ShipChange change = upgrade.Changes[x] as ShipChange;

            switch (change.Stat)
            {
                case Stat.Health:
                    MaxHealth += (int)change.Value;
                    Health += (int)change.Value;
                    break;
                case Stat.Acceleration:
                    Acceleration += change.Value;
                    break;
                case Stat.MaxVelocity:
                    MaxVelocity += change.Value;
                    break;
                case Stat.MaxHeat:
                    MaxHeat += change.Value;
                    break;
                    //case ShipStat.HeatGain:
                    //    HeatGain += change.Value;
                    //    break;
                    //case ShipStat.HeatLoss:
                    //    HeatLoss += change.Value;
                    //    break;

            }
        }
    }

    void UpgradePrimary(Upgrade upgrade)
    {
        foreach (StatChange change in upgrade.Changes)
            switch (change.Stat)
            {
                case Stat.Damage:
                    if (PrimaryDamage + (int)change.Value <= 0)
                        PrimaryDamage = 1;
                    else
                        PrimaryDamage += (int)change.Value;
                    break;
                case Stat.FireVelocity:
                    PrimaryLaunchSpeed += change.Value;
                    break;
                case Stat.HeatCost:
                    PrimairyHeatCost += change.Value;
                    break;
                case Stat.Spread:
                    if (ScattershotSpread + change.Value > 0)
                        ScattershotSpread += change.Value;
                    break;
                case Stat.ShotsAmount:
                    ScattershotsAmount += (int)change.Value;
                    break;
                case Stat.Penetration:
                    ChargedShotPenetration += (int)change.Value;
                    break;
                case Stat.WeaponUnlock:
                    switch (upgrade.Type)
                    {
                        case UpgradeType.ScatterShot:
                            ChangeWeaponType(PrimaryWeapon.ScatterShot);
                            break;
                        case UpgradeType.ChargedShot:
                            ChangeWeaponType(PrimaryWeapon.ChargedShot);
                            break;
                    }
                    break;
            }
    }

    void UpgradeSecondary(Upgrade upgrade)
    {
        foreach (StatChange change in upgrade.Changes)
            switch (change.Stat)
            {
                case Stat.Damage:
                    if (SecondaryDamage + (int)change.Value <= 0)
                        SecondaryDamage = 1;
                    else
                        SecondaryDamage += (int)change.Value;
                    break;
                case Stat.FireVelocity:
                    SecondaryLaunchSpeed += change.Value;
                    break;
                case Stat.HeatCost:
                    SecondaryHeatCost += change.Value;
                    break;
                case Stat.Acceleration:
                    Missile missile = TorpedoWarhead as Missile;
                    missile.Acceleration += change.Value;
                    break;
                case Stat.DamageRange:
                    Flak warhead = TorpedoWarhead as Flak;
                    warhead.DamageRange += change.Value;
                    break;
                case Stat.WeaponUnlock:
                    switch (upgrade.Type)
                    {
                        case UpgradeType.Missile:
                            ChangeWeaponType(SecondaryWeapon.Missile);
                            break;
                        case UpgradeType.Flak:
                            ChangeWeaponType(SecondaryWeapon.Flak);
                            break;
                    }
                    break;
            }
    }
    #endregion

    #region EnableDisable Functions
    public void EnableDisableHeat(bool newValue)
    {
        DisableHeat = newValue;
    }

    public void EnableDisableAutoStop(bool newValue)
    {
        AutoStop = newValue;
        DisableHeat = newValue;
    }

    public void EnableDisableMovement(bool newValue)
    {
        DisableMovement = newValue;
    }

    public void EnableDisableWeapons(bool newValue)
    {
        DisableWeapons = newValue;
    }

    public void EnableDisableAiming(bool newValue)
    {
        DisableAiming = newValue;
    }

    /*void EnableDisableExhaust(bool enable)
    {
        if (EngineExhaust[0].emission.enabled != enable)
            foreach (ParticleSystem exhaust in EngineExhaust)
            {
                var emission = exhaust.emission;
                emission.enabled = enable;
            }
    }*/

    public void Disable(bool newValue)
    {
        DisableWeapons = newValue;
        DisableAiming = newValue;
        DisableHeat = newValue;
        DisableMovement = newValue;
        AutoStop = newValue;
    }

    void EnableEngines(bool enable)
    {
        if (enable && !EnginesSfx.isPlaying)
        {
            EnginesSfx.Play();
            EngineExhaust.Play();
        }
        else if (!enable)
        {
            EnginesSfx.Stop();
            EngineExhaust.Stop();
        }

        //EnableDisableExhaust(enable);
    }

    #endregion
    public IEnumerator Repair()
    {
        if (AbilityInUse == Ability.None && IsAbilityAvailable[Ability.RepairKit] == true)
        {
            IsAbilityAvailable[Ability.RepairKit] = false;
            AbilityInUse = Ability.RepairKit;

            Health = Mathf.Clamp(Health + 10, Health, MaxHealth);

            Heat += RepairKitHeatCost;

            AbilityInUse = Ability.None;
            yield return StartCoroutine(Cooldown(RepairKitCooldown, GameObject.Find("Managers").GetComponent<UIManager>().Ability1Timer));
            IsAbilityAvailable[Ability.RepairKit] = true;
        }
    }

    public IEnumerator VentHeat()
    {
        if (AbilityInUse == Ability.None && IsAbilityAvailable[Ability.HeatVent] == true)
        {
            VentExhaust.Play();
            Timer timer = GameObject.Find("Managers").GetComponent<UIManager>().Ability2Timer;

            IsAbilityAvailable[Ability.HeatVent] = false;
            AbilityInUse = Ability.HeatVent;

            bool originalValue1 = DisableHeat;
            bool originalValue2 = AutoStop;

            DisableHeat = true;
            AutoStop = true;

            float heatVented = 0;
            float t = 0;

            while (t < HeatVentDuration)
            {
                float heatToVent = Mathf.Lerp(0, HeatVentDrainAmount, t / HeatVentDuration) - heatVented;
                Heat -= heatToVent;
                heatVented += heatToVent;
                timer.Set(1f - heatVented / HeatVentDrainAmount);
                t += Time.deltaTime;

                yield return null;
            }

            timer.Set(0);
            AbilityInUse = Ability.None;
            
            if (!originalValue1)
                DisableHeat = false;

            if (!originalValue2)
                AutoStop = false;

            VentExhaust.Stop();

            yield return StartCoroutine(Cooldown(HeatVentCooldown, timer));
            IsAbilityAvailable[Ability.HeatVent] = true;
        }
    }

    /// <summary>
    /// Coroutine for waiting a certain amount of seconds along with displaying how long left with a timer
    /// </summary>
    /// <param name="duration">how long cooldown will last</param>
    /// <param name="timer">Timer to display cooldown with</param>
    IEnumerator Cooldown(float duration, Timer timer, bool overlay = true)
    {
        timer.SetOverLay(overlay);
        float t = 0;

        while (t < duration)
        {
            timer.Set(t / duration);
            t += Time.deltaTime;

            yield return null;
        }

        timer.Set(1);
        timer.SetOverLay(false);
    }

    private void OnDestroy()
    {
        SoundManager soundManager = GameObject.Find(GameObjectNames.Managers).GetComponent<SoundManager>();

        soundManager.RemoveAudioSource(TurretFireSfx, SourceType.Sfx);
        soundManager.RemoveAudioSource(EnginesSfx, SourceType.Sfx);
        //soundManager.RemoveAudioSource(DeathSfx, SourceType.Sfx);
    }
}

public enum PrimaryWeapon
{
    Laser,
    ScatterShot,
    ChargedShot
}

public enum SecondaryWeapon
{
    Torpedo,
    Missile,
    Flak
}

public enum Ability
{
    None,
    RepairKit,
    HeatVent
}
