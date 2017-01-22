using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public int m_NumRoundsToWin = 3;
	public float m_StartDelay = 3f;
	public float m_EndDelay = 3f;
	public MouseOrbitImproved  m_CameraControl;
	public Text m_MessageText;
	public GameObject BallPrefab;
	public BallManager[] Balls;        


	private int m_RoundNumber;         
	private WaitForSeconds m_StartWait;
	private WaitForSeconds m_EndWait;  
	private BallManager m_RoundWinner; 
	private BallManager m_GameWinner;  


	private void Start()
	{
		m_StartWait = new WaitForSeconds (m_StartDelay);
		m_EndWait = new WaitForSeconds (m_EndDelay);

		SpawnAllBalls();
	}
		
	private void SpawnAllBalls()
	{
		for (int i = 0; i < Balls.Length; i++)
		{
			Balls[i].m_Instance =
				Instantiate(BallPrefab, Balls[i].m_SpawnPoint.position, Balls[i].m_SpawnPoint.rotation) as GameObject;
			Balls[i].m_PlayerNumber = i + 1;
			Balls[i].Setup();
		}
	}

	private IEnumerator GameLoop ()
	{
		yield return StartCoroutine (RoundStarting ());
		yield return StartCoroutine (RoundPlaying());
		yield return StartCoroutine (RoundEnding());

		if (m_GameWinner != null)
		{
			Application.LoadLevel (Application.loadedLevel);
		}
		else
		{
			StartCoroutine (GameLoop ());
		}
	}


	private IEnumerator RoundStarting ()
	{
		ResetAllBalls ();
		DisableBallsControl ();

		m_RoundNumber++;
		m_MessageText.text = "ROUND " + m_RoundNumber;

		yield return m_StartWait;
	}


	private IEnumerator RoundPlaying ()
	{
		EnableBallsControl ();
		m_MessageText.text = string.Empty;

		while (!OneTankLeft())
		{
			yield return null;
		}
	}


	private IEnumerator RoundEnding ()
	{
		DisableBallsControl ();

		m_RoundWinner = null;
		m_RoundWinner = GetRoundWinner ();

		if (m_RoundWinner != null)
			m_RoundWinner.m_Wins++;

		m_GameWinner = GetGameWinner ();

		string message = EndMessage ();
		m_MessageText.text = message;

		yield return m_EndWait;
	}

	private bool OneTankLeft()
	{
		int numTanksLeft = 0;

		for (int i = 0; i < Balls.Length; i++)
		{
			if (Balls[i].m_Instance.activeSelf)
				numTanksLeft++;
		}

		return numTanksLeft <= 1;
	}

	private BallManager GetRoundWinner()
	{
		for (int i = 0; i < Balls.Length; i++)
		{
			if (Balls[i].m_Instance.activeSelf)
				return Balls[i];
		}

		return null;
	}

	private BallManager GetGameWinner()
	{
		for (int i = 0; i < Balls.Length; i++)
		{
			if (Balls[i].m_Wins == m_NumRoundsToWin)
				return Balls[i];
		}

		return null;
	}

	private string EndMessage()
	{
		string message = "DRAW!";

		if (m_RoundWinner != null)
			message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

		message += "\n\n\n\n";

		for (int i = 0; i < Balls.Length; i++)
		{
			message += Balls[i].m_ColoredPlayerText + ": " + Balls[i].m_Wins + " WINS\n";
		}

		if (m_GameWinner != null)
			message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

		return message;
	}


	private void ResetAllBalls()
	{
		for (int i = 0; i < Balls.Length; i++)
		{
			Balls[i].Reset();
		}
	}


	private void EnableBallsControl()
	{
		for (int i = 0; i < Balls.Length; i++)
		{
			Balls[i].EnableControl();
		}
	}


	private void DisableBallsControl()
	{
		for (int i = 0; i < Balls.Length; i++)
		{
			Balls[i].DisableControl();
		}
	}
}