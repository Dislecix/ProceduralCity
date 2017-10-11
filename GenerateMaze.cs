using UnityEngine;
using System.Collections;

// Used this awesome site to choose my algorithm (didn't want something in O(N^2) and to get a paragraph on how the algorithm works
// http://www.astrolog.org/labyrnth/algrithm.htm

public class GenerateMaze : MonoBehaviour {

	// A struct to hold each cell in the maze
	public struct Cell {
		public bool building;
		public bool path;
		public bool floor;
		public bool taken;
	}

	public struct Room{
		public int width;
		public int height;
		public int x;
		public int y;
	}

	public int width = 10; // Can be set by user to change size of maze
	public int height = 10; // Can be set by user to change size of maze
	public Cell[,] maze; // An array of cells to make the maze
	public GameObject floor; // What goes under the maze (should be a plane and will get resized)
	public GameObject path; // dirt path
	public GameObject building; // walls
	public GameObject buildingFloor; // inside walls
	public GameObject player;
	public int maxRoomWidth, maxRoomHeight, minRoomWidth, minRoomHeight;
	public ArrayList lots = new ArrayList();
	// Use this for initialization
	void Start () {
		// Initialize maze size
		maze = new Cell[width,height];
		// For loop to initialize bools to false
		for(int i = 0; i < width; i++){
			for(int j = 0; j < height; j++){
				maze[i,j].path = false;
			}
		}
		// Start dividing
		recursiveDivision(0,0,width,height);
		// Resize the floor
		floor.transform.localScale = new Vector3((width+1), (height+1), 1);
		// Instantiate everything
		build ();
		spawnPlayer();
	}

	// Update is called once per frame
	void Update () {

	}

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // The algorithm essentially
    // It takes in four ints and gets called recursively
    // The first two are the bottom left corner of the current "sub-Maze"
    // i.e. you split the maze in half, [0,0] is bottom-left of one "sub-maze" and [width/2,height/2] is the bottom-left of the other "sub-maze"
    // The second two are the new width and height of the current "sub-Maze"
    // i.e. split in half, width/2 and height/2 are the sizes of the new "sub-Mazes"
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void recursiveDivision(int bottomLeftCellx, int bottomLeftCelly,int smallWidth,int smallHeight){
		bool widthBigger = true; // A boolean to signal if the width or height is bigger
		int rand, randRow, randCol, newWidth, newHeight, newBottomLeftx, newBottomLefty; // various ints that will be used

		if (smallWidth <= maxRoomWidth || smallHeight <= maxRoomHeight || smallWidth <= (minRoomWidth*2)+1 || smallHeight <= (minRoomHeight*2)+1) {
			//Debug.Log ("sW " + smallWidth + ", sH " + smallHeight);
			Room thisRoom = new Room();
			thisRoom.width = smallWidth;
			thisRoom.height = smallHeight;
			thisRoom.x = bottomLeftCellx;
			thisRoom.y = bottomLeftCelly;
			lots.Add (thisRoom);
			return;
		}

		// Find if width or height is bigger to determine which direction split will be in
		if(smallWidth > smallHeight){
			widthBigger = true;
		}
		else if(smallWidth < smallHeight){
			widthBigger = false;
		}
        // If both are the same size choose a random one
		else{
			rand = Random.Range (0, 2);
			switch(rand){
			case 0: widthBigger = true;
				break;
			case 1: widthBigger = false;
				break;
			}
		}
		// End if block

		// First pick a random col for the split to occur at or space
		randCol = Random.Range(minRoomWidth, smallWidth - minRoomWidth);
		// Then pick a random row to leave an open space at or split
		randRow = Random.Range(minRoomHeight, smallHeight - minRoomHeight);

		// Handling Vertical Split first (width is bigger)
		if(widthBigger){
			//This will ensure the split doesn't make a room too small
			//while((randCol) <= minRoomWidth || (smallWidth - (randCol+1)) <= minRoomWidth)
			//	randCol = Random.Range(0, smallWidth - 1);


			// Mark where the walls are on the split
			for(int j = bottomLeftCelly; j < smallHeight + bottomLeftCelly; j++){
					maze[bottomLeftCellx + randCol,j].path = true;
			}

            // We will handle the left "sub-Maze" first
			newBottomLeftx = bottomLeftCellx; // X doesn't change
			newBottomLefty = bottomLeftCelly; // Y doesn't change
			newWidth = randCol; // The new width is equal to the column + 1 (if col is 0 then width is 1)
			newHeight = smallHeight; // The height doesn't change
            // Make a recursion call using the new parameters
			recursiveDivision (newBottomLeftx,newBottomLefty,newWidth,newHeight);
            // End first "sub-Maze"

            // Now to handle the right "sub-Maze"
			newBottomLeftx = newBottomLeftx + randCol + 1; // Bottom left x is the cell to the right of where the split occured
			newBottomLefty = bottomLeftCelly; // Y is still the same
			newWidth = smallWidth - (randCol+1); // Subtract the width above from the original width
			newHeight = smallHeight; // Height doesn't change
            // Make a recursion call using the new parameters
			recursiveDivision (newBottomLeftx,newBottomLefty,newWidth,newHeight);
            // End second "sub-Maze" return to previous caller
		}
        // Handling the Horizontal Split now (height is bigger)
		else{
			//This will ensure the split doesn't make a room too small
			//while((randRow) <= minRoomHeight || (smallHeight - (randRow+1)) <= minRoomHeight)
			//	randRow = Random.Range(0, smallHeight - 1);

			// Mark where the walls are on the split
			for(int i = bottomLeftCellx; i < smallWidth+bottomLeftCellx; i++){
					maze[i,bottomLeftCelly + randRow].path = true;
			}

            // We will handle the bottom "sub-Maze" first
			newBottomLeftx = bottomLeftCellx; // X doesn't change
			newBottomLefty = bottomLeftCelly; // Y doesn't change
			newWidth = smallWidth; // The width doesn't change
			newHeight = randRow; // The new height is equal to the row + 1 (if row is 0 then height is 1)
			// Make a recursion call using the new parameters
			recursiveDivision (newBottomLeftx,newBottomLefty,newWidth,newHeight);
            // End first "sub-Maze"

            // Now to handle the upper "sub-Maze"
			newBottomLeftx = bottomLeftCellx; // X is still the same
			newBottomLefty = newBottomLefty + randRow+1; // Bottom left y is the cell above where the split occured
			newWidth = smallWidth; // Width doesn't change
			newHeight = smallHeight - (randRow+1); // Subtract the Height above from the original height
			// Make a recursion call using the new parameters
			recursiveDivision (newBottomLeftx,newBottomLefty,newWidth,newHeight);
            // End second "sub-Maze" return to previous caller
		}
	}

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // A function that actually places the pieces of the maze
    // It loops through each element in the maze array and checks the booleans
    // If up is true it means there should be a wall above it
    // If right is true it means there should be a wall to the right of it
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void build(){
        // First it draws the border around the maze
		drawBorder();
		makeBuildings ();

        // Double nested for loop that steps through each element in the maze array
		for (int i = 0; i < width; i++) {
			for(int j = 0; j < height; j++){
				if(maze[i,j].path == true){
					GameObject temp = (GameObject)Instantiate(path, new Vector3(i+1,j+1,0), Quaternion.identity);
					temp.transform.parent = GameObject.Find ("PathHolder").transform;
				}
				else if(maze[i,j].building == true){
					GameObject temp = (GameObject)Instantiate(building, new Vector3(i+1,j+1,0), Quaternion.identity);
					temp.transform.parent = GameObject.Find ("BuildingHolder").transform;
				}
				else if(maze[i,j].floor == true){
					GameObject temp = (GameObject)Instantiate(buildingFloor, new Vector3(i+1,j+1,0), Quaternion.identity);
					temp.transform.parent = GameObject.Find ("FloorHolder").transform;
				}
			}
		}

	}

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // A function to place the floor, intersection pillars, and create a border surrounding the maze
    // 3 simple loops, pretty self explanatory
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void drawBorder(){
		GameObject temp;
		//Place Floor with the bottom left at 0,0
		temp = (GameObject)Instantiate (floor, new Vector3((width+1)/2,(height+1)/2, 10), Quaternion.identity);
		temp.transform.parent = GameObject.Find ("TileHolder").transform;

		//Draw left and right border
		for (int i = 0; i < 2; i++) {
			for (int j = 0; j <= height+1; j++) {
                // Draws the left first, then draws the right
				temp = (GameObject)Instantiate (path, new Vector3(i*(width+1), j, 0), Quaternion.identity);
				temp.transform.parent = GameObject.Find ("PathHolder").transform;
			}
		}

		//Draw top and bottom border
		for (int i = 0; i < 2; i++) {
			for (int j = 0; j <= width+1; j++) {
                // Draws the bottom first, then draws the top
				temp = (GameObject)Instantiate (path, new Vector3(j,i*(height+1), 0), Quaternion.identity);
				temp.transform.parent = GameObject.Find ("PathHolder").transform;
			}
		}

		temp = (GameObject)Instantiate (path, new Vector3(width+1, height+1, 0), Quaternion.identity);
		temp.transform.parent = GameObject.Find ("PathHolder").transform;
	}

	void makeBuildings(){
		int numPossible;
		int buildingWidth;
		int buildingHeight;
		int bottomLeftx;
		int bottomLefty;
		bool side = false;
		int doorx;
		int doory;
		foreach(Room lot in lots){

			if(lot.width > lot.height){
				numPossible = (int)lot.width/(maxRoomWidth+1);
				//Debug.Log("Num Possible: " + numPossible);
			}
			else if(lot.height > lot.width){
				numPossible = (int)lot.height/(maxRoomHeight+1);
				//Debug.Log("Num Possible: " + numPossible);
			}
			else
				numPossible = 1;

			for(int k = 0; k < numPossible; k++){
				buildingWidth = Random.Range (minRoomWidth, maxRoomWidth+1);
				while(buildingWidth > lot.width){
					buildingWidth = Random.Range (minRoomWidth, maxRoomWidth+1);
				}
				buildingHeight = Random.Range (minRoomHeight, maxRoomHeight+1);
				while(buildingHeight > lot.height){
					buildingHeight = Random.Range (minRoomHeight, maxRoomHeight+1);
				}

				if(lot.width > lot.height){
					bottomLeftx = Random.Range (lot.x+((lot.width/numPossible)*k), (((lot.width/numPossible)*(k+1))+lot.x)- (maxRoomWidth+1));
					//Debug.Log("X range = " + (lot.x+((lot.width/numPossible)*k)) + "," + ((((lot.width/numPossible)*(k+1))+lot.x)- ((maxRoomWidth+1))));
					//Debug.Log ("Lot width = " + lot.width + ", numPossible = " + numPossible + ", k = " + k + ", lot.x = " + lot.x + ", maxRoomWidth" + maxRoomWidth);
					bottomLefty = Random.Range (lot.y, (lot.height+lot.y) - buildingHeight+1);
					side = false;
				}
				else{
					bottomLeftx = Random.Range (lot.x, (lot.width+lot.x) - buildingWidth+1);
					bottomLefty = Random.Range (lot.y+((lot.height/numPossible)*k),(((lot.height/numPossible)*(k+1))+lot.y) - (maxRoomHeight+1));
					side = true;
				}
				//Debug.Log ("Making a building with size " + buildingWidth + "x" + buildingHeight + " at location (" + bottomLeftx + "," + bottomLefty + ") in lot with size " + lot.width + "x" + lot.height + "at location (" + lot.x + "," + lot.y + ").");

				//switch(Random.Range (0, 2)){
				//case 0: side = true;
				//	break;
				//case 1: side = false;
				//	break;
				//}

				if(side == true){
					doorx = Random.Range(0, 2);
					if(doorx == 0){
						doorx = bottomLeftx;
					}
					else
						doorx = bottomLeftx+buildingWidth-1;

					doory = Random.Range(bottomLefty + 1, bottomLefty+buildingHeight-1);
				}
				else{
					doory = Random.Range(0, 2);
					if(doory == 0){
						doory = bottomLefty;
					}
					else
						doory = bottomLefty+buildingHeight-1;

					doorx = Random.Range(bottomLeftx + 1, bottomLeftx+buildingWidth-1);
				}

				for(int i = bottomLeftx; i < bottomLeftx+buildingWidth; i++){
					for(int j = bottomLefty; j < bottomLefty+buildingHeight; j++){
						if(i == doorx && j == doory){
							maze[i,j].path = true;
						}
						else if(i == bottomLeftx || i == bottomLeftx+buildingWidth-1 || j == bottomLefty || j == bottomLefty+buildingHeight-1){
							maze[i,j].building = true;
						}
						else{
							maze[i,j].floor = true;
						}
						//Instantiate (building , new Vector3(i+1, j+1, -1), Quaternion.identity);
					}
				}

				if(side == true && bottomLeftx == doorx){
					int i = bottomLeftx-1;
					while(i >= 0 &&maze[i,doory].path == false){
						maze[i,doory].path = true;
						i--;
					}
				}
				else if(side == true && bottomLeftx != doorx){
					int i = bottomLeftx+buildingWidth;
					while(i < width && maze[i,doory].path == false){
						maze[i,doory].path = true;
						i++;
					}
				}
				else if(side == false && bottomLefty == doory){
					int i = bottomLefty-1;
					while(i >= 0 &&maze[doorx,i].path == false){
						maze[doorx,i].path = true;
						i--;
					}
				}
				else if(side == false && bottomLefty != doory){
					int i = bottomLefty+buildingHeight;
					while(i < height && maze[doorx,i].path == false){
						maze[doorx,i].path = true;
						i++;
					}
				}

			}
		}
	}

	public void spawnPlayer(){
		int playerX,playerY;

		playerX = Random.Range (0, width);
		playerY = Random.Range (0, height);

		while(maze[playerX,playerY].floor != true){
			playerX = Random.Range (0, width);
			playerY = Random.Range (0, height);
		}

		Instantiate(player, new Vector3(playerX+1,playerY+1,-4), Quaternion.identity);
		maze[playerX,playerY].taken = true;
	}
}