using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;

public class Node
{
	private int x;
	private int y;
	private int z;

	private double g;
	private double c;

	private Node predecessor;
	
	public int X { get { return x; } }
	public int Y { get { return y; } }
	public int Z { get { return z; } }
	public double G { 
		get { return g; } 
		set { g = value; }
	}
	public double C { get { return c; } }
	public Node Predecessor
	{
		get { return predecessor; }
		set { predecessor = value; }
	}

	public Node(Vector3 a)
	{
		x = (int)a.x;
		y = (int)a.y;
		z = (int)a.z;
		c = 0;
		g = -1;
		predecessor = null;
	}

	public Node(Vector3 a, double r)
	{
		x = (int)a.x;
		y = (int)a.y;
		z = (int)a.z;
		c = r;
		g = -1;
		predecessor = null;
	}

	Node FindNodeInList(List<Node> allNodes, Vector3 newNode)
	{
		return allNodes.FirstOrDefault(x => newNode == x);
	}

	public double DistanceFromPredecessor(int i, int j, int k)
	{
		return Math.Sqrt(Math.Pow(i, 2) + Math.Pow(j, 2) + Math.Pow(k, 2));
	}

	public Node GetNeighbor(int n, List<Node> allNodes)
	{
		for(int i=-1; i<2; i++)
		{
			for(int j=-1; j<2; j++)
			{
				for(int k=-1; k<2; k++)
				{
					if(9*i + 3*j + k == n)
					{
						Vector3 newNode = new Vector3(x+i, y+j, z+k);

						Node node = FindNodeInList(allNodes, newNode);
						if(!ReferenceEquals(node, null))
						{
							return node;
						}
						else
						{
							node = new Node(newNode, DistanceFromPredecessor(i, j, k));
							allNodes.Add(node);
							return node;
						}
					}
				}
			}
		}
		return null;
	}

	public Vector3 GetCoordinatesAsVector3()
	{
		return new Vector3(x, y, z);
	}

	public static bool operator == (Node a, Node b)
	{
		return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
	}

	public static bool operator != (Node a, Node b)
	{
		return !(a == b);
	}
	
	public static bool operator == (Vector3 a, Node b)
	{
		return a.x == b.X && a.y == b.Y && a.z == b.Z;
	}

	public static bool operator != (Vector3 a, Node b)
	{
		return !(a == b);
	}

	public static Vector3 operator - (Node a, Node b)
	{
		return new Vector3((float)(a.X - b.X), (float)(a.Y - b.Y), (float)(a.Z - b.Z));
	}

	public static Vector3 operator - (Vector3 a, Node b)
	{
		return new Vector3((a.x - (float)b.X), (a.y - (float)b.Y), (a.z - (float)b.Z));
	}
}

public class Queue
{
	private List<Node> queue;
	private List<double> key;
	
	public Queue()
	{
		queue = new List<Node>();
		key = new List<double>();
	}
	
	public int Size
	{
		get { return queue.Count; }
	}
	
	public void Enqueue(Node n, double k)
	{
		queue.Add(n);
		key.Add(k);
	}
	
	public int Min()
	{
		return key.IndexOf(key.Min());
	}
	
	public Node Dequeue()
	{
		int min = Min();
		var last = queue[min];
		key.RemoveAt(min);
		queue.RemoveAt(min);
		return last;
	}
	
	public bool Contains(Node node)
	{
		foreach (var elem in queue)
		{
			if (elem == node) 
			{
				return true;
			}
		}
		return false; 
	}
	
	public void Remove(Node node)
	{
		for (int i = 0; i < queue.Count; i++)
		{
			if (queue[i] == node)
			{
				queue.RemoveAt(i);
				key.RemoveAt(i);
				return;
			}
		}
	}
}

public class e3 : MonoBehaviour {

	public float speed;
	public GameObject drone;
	public float boxSize;
	public List<GameObject> destination = new List<GameObject>();

	private LineRenderer laserLine;
	private List<Vector3> path = new List<Vector3>();
	private List<Node> allNodes = new List<Node>();
	private bool WaitASecond = false;
	private float WaitASecondTime;
	private SerialPort stream = new SerialPort("COM6", 9600);
	private float prevTime;

	void Start () {
		stream.Open();
		prevTime = 0.0f;
	}

	void Update () {
		if(Time.time >= prevTime + 0.1)
		{
			prevTime = Time.time;
			if(stream.IsOpen)
			{
				stream.Write(DronePositionToString());
			}
			print(DronePositionToString());
		}

		if(destination.Count > 0 && Time.time > 2)
		{
			if(drone.transform.position != destination[0].transform.position)
			{
				if(path.Count != 0)
				{
					drone.transform.rotation = Quaternion.Lerp(drone.transform.rotation, Quaternion.LookRotation(new Vector3(path[path.Count-1].x - drone.transform.position.x, 0, path[path.Count-1].z - drone.transform.position.z), Vector3.up), Time.deltaTime * 5);

					float step = speed*Time.deltaTime;
					drone.transform.position = Vector3.MoveTowards(drone.transform.position, path[path.Count-1], step);

					if(drone.transform.position == path[path.Count-1])
					{
						path.RemoveAt(path.Count-1);
					}

					if(false)//LineChecker())
					{
						path.Clear();
						WaitASecond = true;
						WaitASecondTime = Time.time + 1;
					}
				}
				else if(WaitASecond)
				{
					if(Time.time >= WaitASecondTime)
					{
						allNodes.Clear();
						LineBuilder();
						LineSmootherer();
						LineDrawer();
						WaitASecond = false;
					}
				}
				else
				{
					allNodes.Clear();
					LineBuilder();
					LineSmootherer();
					LineDrawer();
				}
			}
			else
			{
				Destroy(destination[0]);
				destination.RemoveAt(0);
			}
		}
	}

	private void LineBuilder()
	{
		var openList = new Queue();
		var closedList = new List<Node>();
		
		var startNode = new Node(drone.transform.position);
		var endNode = new Node(destination[0].transform.position);
		Node currentNode = null;
		
		startNode.G = 0;
		openList.Enqueue(startNode, 0);

		while (openList.Size != 0)
		{
			currentNode = openList.Dequeue();
			if (currentNode == endNode)
			{
				break;
			}
			ExpandNode(currentNode, endNode, closedList, openList);
			closedList.Add(currentNode);
		}

		while (!ReferenceEquals(currentNode.Predecessor, null))
		{
			path.Add(currentNode.GetCoordinatesAsVector3());
			currentNode = currentNode.Predecessor;
		}

		return;
	}

	private void ExpandNode(    Node currentNode, 
							Node endNode, 
							List<Node> closedList, 
							Queue openList)
	{
		if(Physics.OverlapBox(currentNode.GetCoordinatesAsVector3(), new Vector3(boxSize, boxSize, boxSize), Quaternion.identity).Any())
		{
			return;
		}

		for (int i = -13; i < 14; i++)
		{
			if(i == 0)
			{
				continue;
			}

			var successorNode = currentNode.GetNeighbor(i, allNodes);
			if (closedList.Count(x => x == successorNode) > 0)
			{
				continue;
			}
			
			double tentativeG = currentNode.G + successorNode.C;
			
			if (openList.Contains(successorNode) && tentativeG >= successorNode.G)
			{
				continue;
			}
			
			successorNode.Predecessor = currentNode;
			successorNode.G = tentativeG;
			
			double f = tentativeG + CountH(successorNode, endNode);
			
			openList.Remove(successorNode);
			openList.Enqueue(successorNode, f);
		}
	}

	private void LineSmootherer()
	{
		for(int i=0; i < path.Count-2;)
		{
			if(!Physics.BoxCast(path[i], new Vector3(boxSize, boxSize, boxSize), DirectionToV3(path[i], path[i+2]), Quaternion.identity, DistanceBetweenV3(path[i], path[i+2])))
			{
				path.RemoveAt(i+1);
			}
			else
			{
				i++;
			}
		}
	}

	private void LineDrawer()
	{
		laserLine = GetComponent<LineRenderer>();
		laserLine.SetVertexCount(path.Count);
		
		for(int i=0; i<path.Count; i++)
		{
			laserLine.SetPosition(i, path[i]);
		}

		laserLine.enabled = true;
	}

	private bool LineChecker()
	{
		if(path.Count > 0)
		{
			RaycastHit hit;
			if(Physics.SphereCast(drone.transform.position, (float) Math.Sqrt(2), DirectionToV3(drone.transform.position, path[path.Count-1]), out hit, DistanceBetweenV3(drone.transform.position, path[path.Count-1])))
			{
				return true;
			}

			for(int i=0; i<path.Count-1; i++)
			{
				if(Physics.BoxCast(path[i], new Vector3(boxSize, boxSize, boxSize), DirectionToV3(path[i], path[i+1]), Quaternion.identity, DistanceBetweenV3(path[i], path[i+1])))
				{
					return true;
				}
			}
		}
		return false;
	}

	private string DronePositionToString()
	{
		string str = "";
		str += drone.transform.position.x + " ";
		str += drone.transform.position.y + " ";
		str += drone.transform.position.z + " ";
		str += drone.transform.eulerAngles.y;
		return str;
	}

	private float DistanceBetweenV3(Vector3 a, Vector3 b)
	{
		return (float) Math.Sqrt(Math.Pow((a.x - b.x), 2) + 
						 Math.Pow((a.y - b.y), 2) + 
						 Math.Pow((a.z - b.z), 2));
	}

	private Vector3 DirectionToV3(Vector3 a, Vector3 b)
	{
		return new Vector3(b.x-a.x, b.y-a.y, b.z-a.z);
	}

	private double DistanceBetweenNodes(Node a, Node b)
	{
		return (Math.Pow((a.X - b.X), 2) + 
						 Math.Pow((a.Y - b.Y), 2) + 
						 Math.Pow((a.Z - b.Z), 2));
	}
	
	private double CountH(Node a, Node b)
	{
		return DistanceBetweenNodes(a, b);
	}
}
