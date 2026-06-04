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

	private Text _scoreText;
	private static GameObject ballCounterObj;
	private static TextMesh ballCounterText;

	public void UpdateScoreUI()
	{
		if (_scoreText == null) 
		{
			GameObject uiObj = GameObject.FindWithTag("Score");
			if (uiObj != null) _scoreText = uiObj.GetComponent<Text>();
		}
		
		if (_scoreText != null)
		{
			_scoreText.horizontalOverflow = HorizontalWrapMode.Overflow;
			_scoreText.verticalOverflow = VerticalWrapMode.Overflow;
			_scoreText.text = $"Score: {score}";
		}
		
		UpdateBallCounterUI();
	}

	private void UpdateBallCounterUI()
	{
		if (ballCounterObj == null)
		{
			if (LevelManager.instance == null || LevelManager.instance.bubblesPrefabs == null || LevelManager.instance.bubblesPrefabs.Count == 0) return;
			
			ballCounterObj = new GameObject("BallCounterIcon");
			
			// Add SpriteRenderer to make it a ball
			SpriteRenderer sr = ballCounterObj.AddComponent<SpriteRenderer>();
			sr.sprite = LevelManager.instance.bubblesPrefabs[0].GetComponent<SpriteRenderer>().sprite;
			sr.sortingOrder = 20;
			
			// Position it near bottom-left.
			if (Camera.main != null) 
			{
				Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.1f, 0.1f, 10f));
				ballCounterObj.transform.position = new Vector3(bottomLeft.x, bottomLeft.y, 0);
			}
			ballCounterObj.transform.localScale = new Vector3(28f, 28f, 1f);
			
			// Add TextMesh child
			GameObject textObj = new GameObject("Text");
			textObj.transform.SetParent(ballCounterObj.transform);
			textObj.transform.localPosition = new Vector3(0, 0, -1);
			textObj.transform.localScale = new Vector3(0.015f, 0.015f, 1f);
			
			ballCounterText = textObj.AddComponent<TextMesh>();
			ballCounterText.characterSize = 1f;
			ballCounterText.fontSize = 100;
			ballCounterText.anchor = TextAnchor.MiddleCenter;
			ballCounterText.alignment = TextAlignment.Center;
			ballCounterText.color = Color.white;
			
			Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			if (font != null) {
				ballCounterText.font = font;
				textObj.GetComponent<MeshRenderer>().material = font.material;
			}
		}
		
		if (ballCounterText != null)
		{
			ballCounterText.text = remainingBalls.ToString();
		}
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
