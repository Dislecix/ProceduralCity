using UnityEngine;
using System.Collections;

public class GameMaster : MonoBehaviour
{
	public class Node: System.IEquatable<Node>{
		public float fScore;
		public int x;
		public int y;
		public int gScore;
		public Node parent;

		public bool Equals(Node myNode){
			if(x == myNode.x && y == myNode.y){
				return true;
			}
			else{
				return false;
			}
		}

		public override bool Equals(object o){
			Node myNode = (Node) o;
			if(x == myNode.x && y == myNode.y){
				return true;
			}
			else{
				return false;
			}
		}
	}

	public class fScoreSorter: IComparer{
		public int Compare(object x, object y){
			Node left = (Node) x;
			Node right = (Node) y;
			if(left.fScore > right.fScore)
				return 1;
			if(left.fScore == right.fScore)
				//return 0;
				if(left.gScore > right.gScore)
					return -1;
				if(left.gScore < right.gScore)
					return 1;
				if(left.gScore == right.gScore)
					return 0;
			if(left.fScore < right.fScore)
				return -1;

			return -1;
		}
	}

	public ArrayList Enemies = new ArrayList ();
	int numEnemies;
	GenerateMaze generateMaze;
	public GameObject item;
	public GameObject enemy;
	bool turn = true;
	public bool hasItem = false;
	// Use this for initialization
	void Start ()
	{
		generateMaze = (GenerateMaze)GameObject.Find ("TheBuilder").GetComponent<GenerateMaze> ();
		numEnemies = generateMaze.lots.Capacity / 2;
		Debug.Log ("The number of Enemies " + numEnemies);
		spawnEnemies ();
	}

	// Update is called once per frame
	void Update ()
	{
		if (turn == true) {
			//Debug.Log ("Player's turn");
			if (Input.GetKeyDown (KeyCode.W) && !(this.transform.position.y + 1 > generateMaze.height + 1) && (this.transform.position.x == 0 || this.transform.position.x == generateMaze.width + 1 || this.transform.position.y + 1 == generateMaze.height + 1 || !generateMaze.maze [(int)this.transform.position.x - 1, (int)this.transform.position.y].building)) {
				this.transform.position = new Vector3 (this.transform.position.x, this.transform.position.y + 1, this.transform.position.z);
				turn = false;
			} else if (Input.GetKeyDown (KeyCode.A) && !(this.transform.position.x - 1 < 0) && (this.transform.position.y == 0 || this.transform.position.y == generateMaze.height + 1 || this.transform.position.x - 1 == 0 || !generateMaze.maze [(int)this.transform.position.x - 2, (int)this.transform.position.y - 1].building)) {
				this.transform.position = new Vector3 (this.transform.position.x - 1, this.transform.position.y, this.transform.position.z);
				turn = false;
			} else if (Input.GetKeyDown (KeyCode.S) && !(this.transform.position.y - 1 < 0) && (this.transform.position.x == 0 || this.transform.position.x == generateMaze.width + 1 || this.transform.position.y - 1 == 0 || !generateMaze.maze [(int)this.transform.position.x - 1, (int)this.transform.position.y - 2].building)) {
				this.transform.position = new Vector3 (this.transform.position.x, this.transform.position.y - 1, this.transform.position.z);
				turn = false;
			} else if (Input.GetKeyDown (KeyCode.D) && !(this.transform.position.x + 1 > generateMaze.width + 1) && (this.transform.position.y == 0 || this.transform.position.y == generateMaze.height + 1 || this.transform.position.x + 1 == generateMaze.width + 1 || !generateMaze.maze [(int)this.transform.position.x, (int)this.transform.position.y - 1].building)) {
				this.transform.position = new Vector3 (this.transform.position.x + 1, this.transform.position.y, this.transform.position.z);
				turn = false;
			}
			
			if (item!=null && this.transform.position.x == item.transform.position.x && this.transform.position.y == item.transform.position.y) {
				hasItem = true;
				Destroy (item);
			}
		} else if (Enemies.Count != 0) {
			foreach (GameObject e in Enemies) {
				if (e.transform.position.x == this.transform.position.x && e.transform.position.y == this.transform.position.y) {
					Enemies.Remove (e);
					Destroy (e);
					continue;
				}
				Node enemyMove = ASTAR (e.transform);
				e.transform.position = new Vector3 (enemyMove.x, enemyMove.y, e.transform.position.z);
				if (e.transform.position.x == this.transform.position.x && e.transform.position.y == this.transform.position.y) {
					Application.LoadLevel (2);
				}
			}
			Debug.Log ("Enemy have moved");
			turn = true;
		} else if (hasItem) {
			Application.LoadLevel (3);
		} else {
			turn = true;
		}
	}

	public void spawnEnemies ()
	{
		int enemyX, enemyY;
		for (int i = 0; i <= numEnemies; i++) {
			enemyX = Random.Range (0, generateMaze.width);
			enemyY = Random.Range (0, generateMaze.height);
			while (generateMaze.maze[enemyX,enemyY].floor != true || generateMaze.maze[enemyX,enemyY].taken == true) {
				enemyX = Random.Range (0, generateMaze.width);
				enemyY = Random.Range (0, generateMaze.height);
			}		
			if(i == numEnemies){
				GameObject temp = (GameObject)Instantiate (item, new Vector3 (enemyX + 1, enemyY + 1, -4), Quaternion.identity);
				item = temp;
			}
			else{
				GameObject temp = (GameObject)Instantiate (enemy, new Vector3 (enemyX + 1, enemyY + 1, -4), Quaternion.identity);
				Enemies.Add (temp);
				generateMaze.maze [enemyX, enemyY].taken = true;
			}
		}

	}

	public float heuristic(float x, float y){
		float hx, hy, result, p;
		p = 1.0f/(generateMaze.width*generateMaze.height);
		hx =Mathf.Abs(x - this.gameObject.transform.position.x);
		hy =Mathf.Abs(y - this.gameObject.transform.position.y);
		result = hx + hy;
		result *= (1.0f + p);
		return result;
	}

	public Node[] getNeighbors(Node myNode){
		ArrayList temps = new ArrayList();
		Node tempNode = new Node();
		int i = 0;
		if(myNode.x - 1 >= 0){
			i++;
			tempNode.x = myNode.x - 1;
			tempNode.y = myNode.y;
			tempNode.fScore = 1 + heuristic (tempNode.x, tempNode.y); 
			temps.Add (tempNode);
		}
		if(myNode.x + 1 <= generateMaze.width + 1){
			i++;
			tempNode = new Node();
			tempNode.x = myNode.x + 1;
			tempNode.y = myNode.y;
			tempNode.fScore = 1 + heuristic (tempNode.x, tempNode.y); 
			temps.Add (tempNode);
		}
		if(myNode.y -1 >= 0){
			i++;
			tempNode = new Node();
			tempNode.x = myNode.x;
			tempNode.y = myNode.y - 1;
			tempNode.fScore = 1 + heuristic (tempNode.x, tempNode.y); 
			temps.Add (tempNode);
		}
		if(myNode.y + 1 <= generateMaze.height + 1){
			i++;
			tempNode = new Node();
			tempNode.x = myNode.x;
			tempNode.y = myNode.y + 1;
			tempNode.fScore = 1 + heuristic (tempNode.x, tempNode.y); 
			temps.Add (tempNode);
		}

		Node[] result = new Node[i];
		int count = 0;
		foreach (Node n in temps){
			result[count] = n;
			count++;
		}
		return result;
	}

	public Node ASTAR(Transform start){
		ArrayList open = new ArrayList();
		ArrayList closed = new ArrayList();
		int tempGScore;
		Node current = new Node();
		Node startNode = new Node();
		IComparer openComparer = new fScoreSorter();

		startNode.x = (int)start.position.x;
		startNode.y = (int)start.position.y;
		startNode.gScore = 0;
		startNode.fScore = startNode.gScore + heuristic (start.position.x, start.position.y);
		open.Add (startNode);
		while(open.Count > 0){
			open.Sort(openComparer);
			current = (Node)open[0];
			if(current.x == this.gameObject.transform.position.x && current.y == this.gameObject.transform.position.y){
				Node n = reconstruct(current);
				return n;
			}
			open.RemoveAt(0);
			closed.Add (current);
			Node[] neighbors = getNeighbors(current);
			for(int i = 0; i < neighbors.Length; i++){
				if(closed.Contains(neighbors[i])){
					//Debug.Log ("Closed already contains it");
				   continue;
				}
				tempGScore = current.gScore + 1;
				if(open.Contains (neighbors[i])){
					Node something = (Node)open[open.IndexOf (neighbors[i])];
					if(tempGScore < something.gScore){
						open.Remove (something);
					}
				}
				if(((neighbors[i].x == 0 || neighbors[i].y == 0 || neighbors[i].x == generateMaze.width + 1 || neighbors[i].y == generateMaze.height + 1) && !open.Contains (neighbors[i]))||(!open.Contains (neighbors[i]) && !generateMaze.maze[neighbors[i].x-1,neighbors[i].y-1].building)){
					neighbors[i].parent = current;
					neighbors[i].gScore = tempGScore;
					neighbors[i].fScore = neighbors[i].gScore + heuristic (neighbors[i].x,neighbors[i].y);
					open.Add (neighbors[i]);
				}
			}
		}
		return startNode;
	}

	public Node reconstruct(Node myNode){
		if(myNode.parent == null){
			return myNode;
		}
		if(myNode.parent.parent != null){
			return reconstruct (myNode.parent);
		}
		else{
			return myNode;
		}
	}
}
