using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
	public bool canShoot;
	public float speed = 25f;

	public Transform nextBubblePosition;
	public GameObject currentBubble;
	public GameObject nextBubble;
	public GameObject bottomShootPoint;

	private Vector2 lookDirection;
	private float lookAngle;
	private GameObject line;
	private GameObject limit;
	private LineRenderer lineRenderer;
	private Vector2 gizmosPoint;
	private List<GameObject> dots = new();

	public void Awake()
	{
		line = GameObject.FindGameObjectWithTag("Line");
		limit = GameObject.FindGameObjectWithTag("Limit");
		lineRenderer = line.GetComponent<LineRenderer>();
		lineRenderer.enabled = false;
		SpriteRenderer sr = line.GetComponent<SpriteRenderer>();
		if(sr is not null) sr.enabled = false;
	}

	public void Update()
	{
		if (GameManager.instance.gameState == "play")
		{
			gizmosPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
			lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

			if (Input.GetMouseButton(0)
				&& (Camera.main.ScreenToWorldPoint(Input.mousePosition).y > bottomShootPoint.transform.position.y)
				&& (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < limit.transform.position.y))
			{
				line.transform.position = transform.position;
				line.transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90);

				if (LevelManager.instance is not null
				&& LevelManager.instance.GetBubbleAreaChildCount() > 0)
				{
					line.SetActive(true);

					//cast a ray between transform position and mouse position
					CastRay(transform.position, lookDirection.normalized);
				}
			}
			else
			{
				line.SetActive(false);
				foreach(var dot in dots) if(dot!=null) dot.SetActive(false);
			}

			if (canShoot
				&& Input.GetMouseButtonUp(0)
				&& (Camera.main.ScreenToWorldPoint(Input.mousePosition).y > bottomShootPoint.transform.position.y)
				&& (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < limit.transform.position.y))
			{
				canShoot = false;
				Shoot();
				foreach(var dot in dots) if(dot!=null) dot.SetActive(false);
			}
		}
	}

	private void CastRay(Vector2 pos, Vector2 dir)
	{
		int maxBounces = 4;
		
		foreach(var dot in dots) {
			if (dot is not null) dot.SetActive(false);
		}
		
		int dotIndex = 0;

		for (int i = 0; i < maxBounces; i++)
		{
			RaycastHit2D hit = Physics2D.Raycast(pos, dir, 300f);
			
			Vector2 targetPoint = hit.collider is not null ? hit.point : pos + dir * 300f;
			float dist = Vector2.Distance(pos, targetPoint);
			Vector2 stepDir = (targetPoint - pos).normalized;
			
			// Spawn dots every 0.8 units
			for (float d = 0.5f; d < dist; d += 0.8f)
			{
				Vector2 dotPos = pos + stepDir * d;
				
				if (dotIndex >= dots.Count)
				{
					GameObject newDot = Instantiate(LevelManager.instance.bubblesPrefabs[0]);
					Destroy(newDot.GetComponent<Bubble>());
					Destroy(newDot.GetComponent<CircleCollider2D>());
					newDot.transform.localScale = new Vector3(8f, 8f, 1f);
					SpriteRenderer sr = newDot.GetComponent<SpriteRenderer>();
					sr.color = new Color(1f, 1f, 1f, 0.6f);
					sr.sortingOrder = 10;
					dots.Add(newDot);
				}
				
				dots[dotIndex].transform.position = dotPos;
				dots[dotIndex].SetActive(true);
				dotIndex++;
			}

			if (hit.collider is not null)
			{
				if (hit.collider.CompareTag("Wall"))
				{
					pos = hit.point + hit.normal * 0.01f;
					dir = Vector2.Reflect(dir, hit.normal);
				}
				else if (hit.collider.CompareTag("Bubble") || hit.collider.CompareTag("Limit"))
				{
					break;
				}
			}
			else
			{
				break;
			}
		}
	}

	public void Shoot()
	{
		if (currentBubble is null) CreateNextBubble();
		ScoreManager.GetInstance().AddThrows();
		ScoreManager.GetInstance().DecreaseBalls();
		AudioManager.instance.PlaySound("shoot");
		transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
		currentBubble.transform.rotation = transform.rotation;
		currentBubble.GetComponent<CircleCollider2D>().enabled = true;
		Rigidbody2D rb = currentBubble.GetComponent<Rigidbody2D>();
		rb.AddForce(currentBubble.transform.up * speed, ForceMode2D.Impulse);
		rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rb.gravityScale = 0;
		currentBubble = null;
	}

	public void SwapBubbles()
	{
		if (currentBubble is null || nextBubble is null) return;
		
		List<GameObject> bubblesInScene = LevelManager.instance.bubblesInScene;
		if (bubblesInScene.Count < 1) return;

		currentBubble.transform.position = nextBubblePosition.position;
		nextBubble.transform.position = transform.position;
		GameObject temp = currentBubble;
		currentBubble = nextBubble;
		nextBubble = temp;
	}

	public void CreateNewBubbles()
	{
		if (nextBubble is not null)
			Destroy(nextBubble);

		if (currentBubble is not null)
			Destroy(currentBubble);

		nextBubble = null;
		currentBubble = null;
		CreateNextBubble();
		canShoot = true;
	}

	public void CreateNextBubble()
	{
		List<GameObject> bubblesInScene = LevelManager.instance.bubblesInScene;
		List<string> colors = LevelManager.instance.colorsInScene;

		if (bubblesInScene.Count < 1) return;

		if (nextBubble is null)
		{
			nextBubble = InstantiateNewBubble(bubblesInScene);
		}
		else
		{
			// if (!colors.Contains(nextBubble.GetComponent<Bubble>().bubbleColor.ToString()))
			// {
			// 	Destroy(nextBubble);
			// 	nextBubble = InstantiateNewBubble(bubblesInScene);
			// }
		}

		if (currentBubble is null)
		{
			currentBubble = nextBubble;
			currentBubble.transform.position = transform.position;
			nextBubble = InstantiateNewBubble(bubblesInScene);
		}
	}

	private GameObject InstantiateNewBubble(List<GameObject> bubblesInScene)
	{
		if (bubblesInScene.Count > 0)
		{
			GameObject newBubble = Instantiate(bubblesInScene[Random.Range(0, bubblesInScene.Count)]);
			newBubble.transform.position = nextBubblePosition.position;
			newBubble.GetComponent<Bubble>().isFixed = false;
			newBubble.GetComponent<CircleCollider2D>().enabled = false;
			Rigidbody2D rb2d = newBubble.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
			rb2d.gravityScale = 0f;
			return newBubble;
		}
		else
		{
			return null;
		}

	}
}
