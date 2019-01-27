﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform noseTransform;
    public Transform leftWingTransform;
    public Transform rightWingTransform;
    public Transform behindTransform;

    public float speed = 25;
	public Transform anchorTransform;

	private Vector3 _rotationAxis;
	private Transform _playerTransform;
    private bool _isAnchored = true;
    private float _rotationSpeed;

    //Shooting & Weaponry
    public List<WeaponScript> weapons;
    public GameObject powerUp;
    private BulletMover shotScript;

    public GameObject explosion;

    // Use this for initialization
    private void Start()
    {
        _playerTransform = gameObject.transform;
        UpdateRotation();
    }
	
	// Update is called once per frame
	private void Update () {
		var axis = Input.GetAxis("Vertical");
		if (Mathf.Abs(axis - 0f) > 0.01f)
		{
			speed += 5 * axis * Time.deltaTime; 
			var radius = _playerTransform.position - anchorTransform.position;
			UpdateRotationSpeed(radius);
		}
        if (_isAnchored)
        {
	        if (CheckButton("Jump"))
	        {
		        _isAnchored = false;
	        }
	        _playerTransform.RotateAround(anchorTransform.position, _rotationAxis, _rotationSpeed * Time.deltaTime);
        }
        else
        {
	        TryToAnchor();
            _playerTransform.Translate(speed * Vector3.up * Time.deltaTime);
        }

        //Shooting
        if (CheckButton("Fire1"))
        {
            foreach (var weapon in weapons)
            {
                weapon.Shoot();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Planets")
        {
            Destroy(gameObject);
            Instantiate(explosion, transform.position, transform.rotation);
            //GameManager.Instance.PlayerCrash();
            GameManager.Instance.isGameOver = true;
        }
        
    }

    private void TryToAnchor()
	{
		foreach (var planet in GameManager.Instance.planets)
		{
			if (planet.transform == anchorTransform) continue;
			var radius = _playerTransform.position - planet.gameObject.transform.position;
			if (radius.magnitude < planet.GravityRadius && Vector3.Angle(_playerTransform.up, radius) - 90 < 1.5)
			{
				anchorTransform = planet.transform;
				UpdateRotation();
				_isAnchored = true;
				return;
			}
		}
	}

	private void UpdateRotation()
	{
		//update rotation axis
		var direction = _playerTransform.up;
		var radius = _playerTransform.position - anchorTransform.position;
		_rotationAxis = Vector3.Cross(radius, direction).y < 0 ? Vector3.down : Vector3.up;

		UpdateRotationSpeed(radius);
		
		//update ship angle
		var modifier = _rotationAxis.y < 0 ? -90 : 90; 
		var angle = Vector3.SignedAngle(direction, radius, Vector3.up) + modifier;
		_playerTransform.Rotate(Vector3.up, angle, Space.World);
	}

	private void UpdateRotationSpeed(Vector3 radius)
	{
//update rotation speed (deg/sec)
		var perimeter = 2 * Mathf.PI * radius.magnitude;
		_rotationSpeed = speed * 360 / perimeter;
	}

	private bool CheckButton(string button)
    {
	    return Input.GetButton(button) && !GameManager.Instance.isGameOver;
    }

    //For PickUps
    public void SetPower(GameObject newPowerUp)
    {
        powerUp = newPowerUp;
    }

    public void SetWeapon(WeaponScript weapon)
    {
        ClearCurrentWeapon();
        switch (weapon.emissionPoint)
        {
            case HardPoint.Nose:
                weapons.Add(Instantiate(weapon.gameObject, noseTransform).GetComponent<WeaponScript>());
                break;
            case HardPoint.Wings:
                weapons.Add(Instantiate(weapon.gameObject, leftWingTransform).GetComponent<WeaponScript>());
                weapons.Add(Instantiate(weapon.gameObject, rightWingTransform).GetComponent<WeaponScript>());
                break;
            case HardPoint.Behind:
                weapons.Add(Instantiate(weapon.gameObject, behindTransform).GetComponent<WeaponScript>());
                break;
        }
    }

    private void ClearCurrentWeapon()
    {
        foreach (var weapon in weapons)
        {
            Destroy(weapon.gameObject);
        }
        weapons.Clear();
    }
}
