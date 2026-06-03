using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager
{
	private int score = 0;
	private int throws = 0;
	private int remainingBalls = 50;
	public static ScoreManager instance;

	public static ScoreManager GetInstance()
	{
		if (instance == null)
			instance = new ScoreManager();

		return instance;
	}

	public void UpdateScoreUI()
	{
		Text _score = GameObject.FindWithTag("Score").GetComponent<Text>();
		_score.text = $"Score: {score} | Balls: {remainingBalls}";
	}

	public void AddScore(int score)
	{
		this.score += score;
		UpdateScoreUI();
	}

	public int GetScore()
	{
		return score;
	}

	public void AddThrows()
	{
		throws++;
	}

	public int GetThrows()
	{
		return throws;
	}

	public int GetRemainingBalls()
	{
		return remainingBalls;
	}

	public void DecreaseBalls()
	{
		remainingBalls--;
		UpdateScoreUI();
	}

	public void AddBalls(int amount)
	{
		remainingBalls += amount;
		UpdateScoreUI();
	}

	public void Reset()
	{
		score = 0;
		throws = 0;
		remainingBalls = 50;
		UpdateScoreUI();
	}
}
