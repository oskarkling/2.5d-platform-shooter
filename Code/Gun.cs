using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour 
{
	public enum Firemode {Auto, Burst, Single} //Visar en dropdown lista i Inspectorn i Unity, så man kan välja vilken enum som är aktiv. (0,1,2)
	public Firemode fireMode;

	public Transform[] projectileSpawn;
	public Projectile projectile;
	public float msBetweenShots = 100;
	public float muzzleVelocity = 35;
	public int burstCount;
	public int projectilesPerMagazine;
	public float reloadTime =  0.3f;

	[Header("Recoil")] //Skapar en header i Inspectorn i Unity, lättare att organisera upp det i Inspectorn.
	public float recoilSettleTime = 0.1f;
	public float recoilSettleRotationTime = 0.1f;
	public Vector2 kickBackMinMax = new Vector2(0.05f, 0.2f);
	public Vector2 recoilAngleMinMax = new Vector2(3, 5);
	
	[Header("Effects")]
	public Transform shell;
	public Transform shellEject;

	MuzzleFlash _muzzleFlash;
	float nextShotTime;
	float recoilAngle;
	float recoilRotationSmoothDampVelocity;
	bool triggerReleasedSinceLastTrigger;
	bool isReloading;
	int shotsRemainingInBurst;
	int projectilesRemainingInMagazine;
	Vector3 RecoilSmoothDampVelocity;

	void Start()
	{
		_muzzleFlash = GetComponent<MuzzleFlash>();
		shotsRemainingInBurst = burstCount;
		projectilesRemainingInMagazine = projectilesPerMagazine;
	}
	
	void Update() 
	{
		transform.localEulerAngles = Vector3.left * recoilAngle;
	}

	void LateUpdate() //Lateupdate för ananrs körs Aim() före Update()
	{
		//Animerar rekylen på vapnet
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref RecoilSmoothDampVelocity, 
		recoilSettleTime);
		
		recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotationSmoothDampVelocity, recoilSettleRotationTime);
		
		transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

		//automatisk reload när man får slut på skott nedan
		if(!isReloading && projectilesRemainingInMagazine == 0)
		{
			Reload();
		}
	}
	

	void Shoot() // shoot behöver inte vara public, för vi kallar på den i OnTriggerHold()
    {
		if (!isReloading && Time.time > nextShotTime && projectilesRemainingInMagazine > 0) 
        {
			if(fireMode == Firemode.Burst)
			{
				if(shotsRemainingInBurst == 0) //om det är slut på burst skotten, avsluta metoden.
				{
					return;
				}
				shotsRemainingInBurst --;
			}
			else if(fireMode == Firemode.Single)
			{
				if(!triggerReleasedSinceLastTrigger)
				{
					return;
				}
			}

			//Nedan är loopen om vi skjuter flera skott på samma gång. Med tex ett shotgun.
			for(int i = 0; i < projectileSpawn.Length; i++)
			{
				if(projectilesRemainingInMagazine == 0)
				{
					break;
				}
				projectilesRemainingInMagazine --;
				nextShotTime = Time.time + msBetweenShots / 1000;
				Projectile newProjectile = Instantiate (projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
				newProjectile.SetSpeed(muzzleVelocity);
			}

			Instantiate(shell, shellEject.position, shellEject.rotation);
			_muzzleFlash.Activate();
			
			//Rekylen på vapnet per skott
			transform.localPosition -= Vector3.forward * Random.Range(kickBackMinMax.x, kickBackMinMax.y);
			recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
			recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
		}
	}

	public void Reload()
	{
		if(!isReloading && projectilesRemainingInMagazine != projectilesPerMagazine)
		{
			StartCoroutine(AnimateReload());
		}
	}

	IEnumerator AnimateReload()
	{
		isReloading = true;
		yield return new WaitForSeconds(0.2f);

		float reloadSpeed = 1f / reloadTime;
		float percent = 0f;
		Vector3 initialRotation = transform.localEulerAngles;
		float maxReloadAngle = 360; //30 grader

		while(percent < 1)
		{
			percent += Time.deltaTime * reloadSpeed;
			float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
			float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
			transform.localEulerAngles = initialRotation + Vector3.left * reloadAngle;
			yield return null;
		}

		projectilesRemainingInMagazine = projectilesPerMagazine;
		isReloading = false;
	}

	public void Aim(Vector3 aimPoint)
	{
		transform.LookAt(aimPoint);
	}

	public void OnTriggerHold()
	{
		Shoot();
		triggerReleasedSinceLastTrigger = false;
	}

	public void OnTriggerRelease()
	{
		triggerReleasedSinceLastTrigger = true;
		shotsRemainingInBurst = burstCount;
	}
}
 