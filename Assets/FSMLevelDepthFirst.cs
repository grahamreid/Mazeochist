using UnityEngine;
using System.Collections;

public class FSMLevelDepthFirst : MonoBehaviour {

#region Class-Level Attributes
	
	public GameObject columnObject;
	public GameObject roomObject;
	public GameObject wallObject;
	public GameObject objPlayer;	

	private ArrayList objaRooms = new ArrayList();

	private float fltRoomSize = 12f;
	public int intGridSize = 5;
	
	private enum neighborRelativePosition {Unassigned, Left, Right, Below, Above};
	private enum state {makeLevel, playingGame, gameWon, gameLost};	
	private state currentState;
	
	private GameObject _startingRoom;
	private GameObject _endingRoom;
	
	private GameObject StartingRoom
	{
		get {return _startingRoom;}
		set {_startingRoom = value;}
	}
	private GameObject EndingRoom
	{
		get {return _endingRoom;}
		set {_endingRoom = value;}
	}
	
#endregion
	
#region FSM Methods
	// Use this for initialization
	void Start () 
	{
		currentState = state.makeLevel;	
	}
		
	void Update () 
	{
		switch(currentState)
		{
			
			case(state.makeLevel):
			{
				createGrid();
				currentState = state.playingGame;
				break;
			}	
		}//switch
	}//Update
	
#endregion
	
#region Class Methods
	
	//generate level
	void createGrid()
	{		
		initializeGrid();
		getAllNeighboringRooms();
        //choose starting room
		StartingRoom = chooseStartingPoint();
		//create player
		Instantiate(objPlayer,
					new Vector3(StartingRoom.transform.position.x,
							StartingRoom.transform.position.y+1,
							StartingRoom.transform.position.z),
					Quaternion.identity);		
		//begin Recursive Depth-First Search for creating maze from grid of rooms
        visitNeighbors(neighborRelativePosition.Unassigned, StartingRoom);		
		
	}
	
	void initializeGrid()
	{
	    GameObject objCurrentRoom;

        //create grid of rooms
		int intCurrentZ;
		int intCurrentX;
		for (intCurrentX = 0; intCurrentX < intGridSize; intCurrentX++)
		{
			for(intCurrentZ = 0; intCurrentZ < intGridSize; intCurrentZ++)
			{
                //current location in grid
				float fltXCoord = intCurrentX*fltRoomSize;
				float fltZCoord = intCurrentZ*fltRoomSize;
				objCurrentRoom = (GameObject)Instantiate(roomObject,new Vector3(fltXCoord,0,fltZCoord),Quaternion.identity);
				objaRooms.Add(objCurrentRoom);
				objCurrentRoom.GetComponent<roomScript>().visited = false;
                //is room on border?
				if(intCurrentX == 0)
				{
					objCurrentRoom.GetComponent<roomScript>().leftRoom = true;
					objCurrentRoom.GetComponent<roomScript>().edgeRoom = true;	
				}
				if(intCurrentZ == 0)
				{
					objCurrentRoom.GetComponent<roomScript>().bottomRoom = true;
					objCurrentRoom.GetComponent<roomScript>().edgeRoom = true;	
				}
				if(intCurrentX == intGridSize-1)
				{
					objCurrentRoom.GetComponent<roomScript>().rightRoom = true;
					objCurrentRoom.GetComponent<roomScript>().edgeRoom = true;	
				}
				if(intCurrentZ == intGridSize-1)
				{
					objCurrentRoom.GetComponent<roomScript>().topRoom = true;
					objCurrentRoom.GetComponent<roomScript>().edgeRoom = true;	
				}
			}//for
		}//for
	}
	
	void getAllNeighboringRooms()
	{	
	    GameObject objCurrentRoom;
		
		for (int intRow = 0; intRow < intGridSize; intRow++)
		{
			for(int intColumn = 0; intColumn < intGridSize; intColumn++)
			{
				int intCurrentIndex = (intRow*intGridSize)+intColumn;
				objCurrentRoom = (GameObject)objaRooms[intCurrentIndex];
                //add neighboring rooms to this room's adjacent rooms arraylist
				if(objCurrentRoom.GetComponent<roomScript>().topRoom != true)
					objCurrentRoom.GetComponent<roomScript>().objaNeighboringRooms.Add(objaRooms[intCurrentIndex+1]);
				if(objCurrentRoom.GetComponent<roomScript>().bottomRoom != true)
					objCurrentRoom.GetComponent<roomScript>().objaNeighboringRooms.Add(objaRooms[intCurrentIndex-1]);							
				if(objCurrentRoom.GetComponent<roomScript>().leftRoom != true)
					objCurrentRoom.GetComponent<roomScript>().objaNeighboringRooms.Add(objaRooms[intCurrentIndex-intGridSize]);									
				if(objCurrentRoom.GetComponent<roomScript>().rightRoom != true)
					objCurrentRoom.GetComponent<roomScript>().objaNeighboringRooms.Add(objaRooms[intCurrentIndex+intGridSize]);											
			}//for
		}//for		
	}
	
	GameObject chooseStartingPoint()
	{
		GameObject objCurrentRoom;
		int intRandom;
		do
		{
			intRandom = Random.Range (0,objaRooms.Count);
			objCurrentRoom = (GameObject)objaRooms[intRandom];
			objCurrentRoom = objCurrentRoom.gameObject;		
		} while (objCurrentRoom.GetComponent<roomScript>().edgeRoom == false);
		
		objCurrentRoom.GetComponent<roomScript>().visited = true;					
		return objCurrentRoom;
	}
	
	//Recursive function to remove walls until all neighbors are visited
    void visitNeighbors(neighborRelativePosition lastRelativePosition, GameObject objCurrentRoom)
    {
        int intNeighborCount = objCurrentRoom.GetComponent<roomScript>().objaNeighboringRooms.Count;
		int intUnvisitedCount = intNeighborCount;
        int intRandom;	
        GameObject objNeighbor;
		neighborRelativePosition thisRelativePosition = neighborRelativePosition.Unassigned;
		
		//are all neighbors visited yet?
		intUnvisitedCount = checkIfNeighborsAreVisited(objCurrentRoom,intNeighborCount);
        while (intUnvisitedCount != 0)
        {
            intRandom = Random.Range(0, objCurrentRoom.GetComponent<roomScript>().objaNeighboringRooms.Count);
            objNeighbor = (GameObject)objCurrentRoom.GetComponent<roomScript>().objaNeighboringRooms[intRandom];
            if (objNeighbor.GetComponent<roomScript>().visited == true)				
                continue;
            else
            {
                thisRelativePosition = removeWall(objCurrentRoom, objNeighbor);
				objNeighbor.GetComponent<roomScript>().visited = true;	
				//recurse
				visitNeighbors(thisRelativePosition,objNeighbor);
				intUnvisitedCount = checkIfNeighborsAreVisited(objCurrentRoom,intNeighborCount);
            }
        }
		return;
    }
	
	//remove a wall between two rooms
    neighborRelativePosition removeWall(GameObject objCurrentRoom, GameObject objNeighbor)
    {
		GameObject objCurrentWall;
		
		neighborRelativePosition neighborPosition = neighborRelativePosition.Unassigned;
        if(objCurrentRoom.transform.position.x < objNeighbor.transform.position.x)
			neighborPosition = neighborRelativePosition.Right;
		else if(objCurrentRoom.transform.position.x > objNeighbor.transform.position.x)
			neighborPosition = neighborRelativePosition.Left;	
		else if(objCurrentRoom.transform.position.z < objNeighbor.transform.position.z)
			neighborPosition = neighborRelativePosition.Above;		
		else if(objCurrentRoom.transform.position.z > objNeighbor.transform.position.z)
			neighborPosition = neighborRelativePosition.Below;		
		
		switch(neighborPosition)
		{
			case(neighborRelativePosition.Left):
			{
				objCurrentWall = objCurrentRoom.transform.FindChild("leftWall").gameObject;
				Destroy(objCurrentWall);
				objCurrentWall = objNeighbor.transform.FindChild("rightWall").gameObject;
				Destroy(objCurrentWall);
				break;
			}
			case(neighborRelativePosition.Right):
			{
				objCurrentWall = objCurrentRoom.transform.FindChild("rightWall").gameObject;
				Destroy(objCurrentWall);
				objCurrentWall = objNeighbor.transform.FindChild("leftWall").gameObject;
				Destroy(objCurrentWall);
				break;
			}
			case(neighborRelativePosition.Below):
			{
				objCurrentWall = objCurrentRoom.transform.FindChild("bottomWall").gameObject;
				Destroy(objCurrentWall);
				objCurrentWall = objNeighbor.transform.FindChild("topWall").gameObject;
				Destroy(objCurrentWall);
				break;
			}
			case(neighborRelativePosition.Above):
			{
				objCurrentWall = objCurrentRoom.transform.FindChild("topWall").gameObject;
				Destroy(objCurrentWall);
				objCurrentWall = objNeighbor.transform.FindChild("bottomWall").gameObject;
				Destroy(objCurrentWall);
				break;
			}
			default:
				break;			
		}//switch
		return neighborPosition;
	}
	
	//see if room is surrounded by visited rooms
	int checkIfNeighborsAreVisited(GameObject objCurrentRoom,int intNeighborCount)
	{
		int intUnvisitedCount = intNeighborCount;
		GameObject objNeighbor;
        for (int i = 0; i < intNeighborCount; i++)
		{
			objNeighbor = (GameObject)objCurrentRoom.GetComponent<roomScript>().objaNeighboringRooms[i];
            if (objNeighbor.GetComponent<roomScript>().visited == true)
                intUnvisitedCount--;
		}		
		return intUnvisitedCount;
	}
	
#endregion	
	
#region Controller Methods
	
#endregion

}