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
		// As soon as the round starts reset the tanks and make sure they can't move.
		ResetAllBalls ();
		DisableBallsControl ();

		// Increment the round number and display text showing the players what round it is.
		m_RoundNumber++;
		m_MessageText.text = "ROUND " + m_RoundNumber;

		// Wait for the specified length of time until yielding control back to the game loop.
		yield return m_StartWait;
	}


	private IEnumerator RoundPlaying ()
	{
		// As soon as the round begins playing let the players control the tanks.
		EnableBallsControl ();

		// Clear the text from the screen.
		m_MessageText.text = string.Empty;

		// While there is not one tank left...
		while (!OneTankLeft())
		{
			// ... return on the next frame.
			yield return null;
		}
	}


	private IEnumerator RoundEnding ()
	{
		// Stop tanks from moving.
		DisableBallsControl ();

		// Clear the winner from the previous round.
		m_RoundWinner = null;

		// See if there is a winner now the round is over.
		m_RoundWinner = GetRoundWinner ();

		// If there is a winner, increment their score.
		if (m_RoundWinner != null)
			m_RoundWinner.m_Wins++;

		// Now the winner's score has been incremented, see if someone has one the game.
		m_GameWinner = GetGameWinner ();

		// Get a message based on the scores and whether or not there is a game winner and display it.
		string message = EndMessage ();
		m_MessageText.text = message;

		// Wait for the specified length of time until yielding control back to the game loop.
		yield return m_EndWait;
	}


	// This is used to check if there is one or fewer tanks remaining and thus the round should end.
	private bool OneTankLeft()
	{
		// Start the count of tanks left at zero.
		int numTanksLeft = 0;

		// Go through all the tanks...
		for (int i = 0; i < Balls.Length; i++)
		{
			// ... and if they are active, increment the counter.
			if (Balls[i].m_Instance.activeSelf)
				numTanksLeft++;
		}

		// If there are one or fewer tanks remaining return true, otherwise return false.
		return numTanksLeft <= 1;
	}


	// This function is to find out if there is a winner of the round.
	// This function is called with the assumption that 1 or fewer tanks are currently active.
	private BallManager GetRoundWinner()
	{
		// Go through all the tanks...
		for (int i = 0; i < Balls.Length; i++)
		{
			// ... and if one of them is active, it is the winner so return it.
			if (Balls[i].m_Instance.activeSelf)
				return Balls[i];
		}

		// If none of the tanks are active it is a draw so return null.
		return null;
	}


	// This function is to find out if there is a winner of the game.
	private BallManager GetGameWinner()
	{
		// Go through all the tanks...
		for (int i = 0; i < Balls.Length; i++)
		{
			// ... and if one of them has enough rounds to win the game, return it.
			if (Balls[i].m_Wins == m_NumRoundsToWin)
				return Balls[i];
		}

		// If no tanks have enough rounds to win, return null.
		return null;
	}


	// Returns a string message to display at the end of each round.
	private string EndMessage()
	{
		// By default when a round ends there are no winners so the default end message is a draw.
		string message = "DRAW!";

		// If there is a winner then change the message to reflect that.
		if (m_RoundWinner != null)
			message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

		// Add some line breaks after the initial message.
		message += "\n\n\n\n";

		// Go through all the tanks and add each of their scores to the message.
		for (int i = 0; i < Balls.Length; i++)
		{
			message += Balls[i].m_ColoredPlayerText + ": " + Balls[i].m_Wins + " WINS\n";
		}

		// If there is a game winner, change the entire message to reflect that.
		if (m_GameWinner != null)
			message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

		return message;
	}


	// This function is used to turn all the tanks back on and reset their positions and properties.
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