﻿using System;
using UnityEngine;

[Serializable]
public class BallManager
{
	public Color m_PlayerColor;
	public Transform m_SpawnPoint;
	[HideInInspector] public int m_PlayerNumber;
	[HideInInspector] public string m_ColoredPlayerText;
	[HideInInspector] public GameObject m_Instance;
	[HideInInspector] public int m_Wins;


	private BoatController m_Movement;  
	private BulletController m_Shooting;


	public void Setup ()
	{
		m_Movement = m_Instance.GetComponent<BoatController> ();
		m_Shooting = m_Instance.GetComponent<BulletController> ();
		m_Movement.m_PlayerNumber = m_PlayerNumber;
		m_Shooting.m_PlayerNumber = m_PlayerNumber;

		m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";
	}

	public void DisableControl ()
	{
		m_Movement.enabled = false;
		m_Shooting.enabled = false;

		//m_CanvasGameObject.SetActive (false);
	}


	public void EnableControl ()
	{
		m_Movement.enabled = true;
		m_Shooting.enabled = true;

		//m_CanvasGameObject.SetActive (true);
	}

	public void Reset ()
	{
		m_Instance.transform.position = m_SpawnPoint.position;
		m_Instance.transform.rotation = m_SpawnPoint.rotation;

		m_Instance.SetActive (false);
		m_Instance.SetActive (true);
	}
}