using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

	#region Singleton
	public static LevelManager instance;

	private void Awake()
	{
		if (instance is null)
		{
			instance = this;
		}
		DontDestroyOnLoad(gameObject);
	}
	#endregion

	public Grid grid;
	public Transform bubblesArea;
	public List<GameObject> bubblesPrefabs;
	public GameObject specialBubblePrefab;
	public List<GameObject> bubblesInScene;
	public List<GameObject> levels;
	public List<string> colorsInScene;
	public int currentLevel = 0;
	public GameObject levelText;

	private void Start()
	{
		grid = GetComponent<Grid>();
	}

	public void NextLevel()
	{
		GameManager.instance.WinMenu.SetActive(false);
		StartLevel(currentLevel + 1);
	}

	public void RestartLevel()
	{
		GameManager.instance.LoseMenu.SetActive(false);
		StartLevel(currentLevel);
	}

	public void StartNewGame()
	{
		GameManager.instance.startUI.SetActive(false);
		StartLevel(0);
	}

	System.Collections.IEnumerator LoadLevel(int level)
	{
		yield return new WaitForSeconds(0.1f);

		ScoreManager.GetInstance().Reset();
		GenerateProceduralLevel(level);

		InsertSpecialBubbles();
		UpdateListOfBubblesInScene();

		GameManager.instance.shootScript.CreateNewBubbles();
	}

	private void GenerateProceduralLevel(int level)
	{
		GameObject topObj = GameObject.Find("Top");
		int topY = topObj is not null ? grid.WorldToCell(topObj.transform.position).y : 5;
		
		int rows = Mathf.Min(5 + (level / 5), 15);
		int cols = 11;
		
		for (int y = 0; y < rows; y++)
		{
			int currentY = topY - 1 - y;
			int currentCols = (y % 2 == 0) ? cols : cols - 1;
			int startX = -currentCols / 2;
			
			for (int x = 0; x < currentCols; x++)
			{
				Vector3 spawnPos = grid.GetCellCenterWorld(new Vector3Int(startX + x, currentY, 0));
				GameObject bubblePrefab = bubblesPrefabs[Random.Range(0, bubblesPrefabs.Count)];
				GameObject bubble = Instantiate(bubblePrefab, bubblesArea);
				bubble.transform.position = spawnPos;
				
				Bubble bScript = bubble.GetComponent<Bubble>();
				if (bScript is not null) {
					bScript.isFixed = true;
					bScript.isConnected = true;
				}
				
				SnapToNearestGripPosition(bubble.transform);
			}
		}
	}

	public void StartLevel(int level)
	{
		GameManager.instance.levelsUI.SetActive(false);
		currentLevel = level;
		levelText.GetComponent<UnityEngine.UI.Text>().text = "Level " + (level + 1);
		StartCoroutine(LoadLevel(level));
	}

	public void InsertSpecialBubbles()
	{
		int specialCount = Random.Range(1, 6);
		List<Transform> specials = new();
		for (int i = 0; i < specialCount; i++)
		{
			int randomBubble = Random.Range(0, bubblesArea.childCount);
			Transform bubble = bubblesArea.GetChild(randomBubble);

			if (!specials.Contains(bubble))
			{
				specials.Add(bubble);
				Instantiate(specialBubblePrefab, bubble.position, Quaternion.identity, bubblesArea);
				Destroy(bubble.gameObject);
			}
		}
	}

	public void ClearLevel()
	{
		foreach (Transform t in bubblesArea)
			Destroy(t.gameObject);
	}

	public int GetBubbleAreaChildCount()
	{
		return bubblesArea.childCount;
	}

	#region Snap to Grid
	private void SnapChildrensToGrid(Transform parent)
	{
		foreach (Transform t in parent)
		{
			SnapToNearestGripPosition(t);
		}
	}

	public void SnapToNearestGripPosition(Transform t)
	{
		Vector3Int cellPosition = grid.WorldToCell(t.position);
		t.position = grid.GetCellCenterWorld(cellPosition);
		t.rotation = Quaternion.identity;

	}
	#endregion



	public void UpdateListOfBubblesInScene()
	{
		List<string> colors = new List<string>();
		List<GameObject> newListOfBubbles = new();

		foreach (Transform t in bubblesArea)
		{
			Bubble bubbleScript = t.GetComponent<Bubble>();
			if (colors.Count < bubblesPrefabs.Count && !colors.Contains(bubbleScript.bubbleColor.ToString()))
			{
				string color = bubbleScript.bubbleColor.ToString();

				foreach (GameObject prefab in bubblesPrefabs)
				{
					if (color.Equals(prefab.GetComponent<Bubble>().bubbleColor.ToString()))
					{
						colors.Add(color);
						newListOfBubbles.Add(prefab);
					}
				}
			}
		}

		colorsInScene = colors;
		bubblesInScene = newListOfBubbles;
	}

	public void SetAsBubbleAreaChild(Transform bubble)
	{
		SnapToNearestGripPosition(bubble);
		bubble.SetParent(bubblesArea);
	}
}
