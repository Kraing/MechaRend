using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Linq;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MainApp : MonoBehaviour
{
	GameObject PointCloudMesh;
	GameObject Controller;
	Mesh mesh;
	// Define number of vertex
	int num_vertex;
	private Vector3[] vertex_pos;
	private float[] pressure;
	private float max_p = 0f;
	private float min_p = 0f;
	

    // Start is called before the first frame update
    void Start()
    {	
		
		// Reference object
		PointCloudMesh = GameObject.Find("PointCloudMesh");
		Controller = GameObject.Find("Controller");

		// Access to loaded variable
		num_vertex = SceneController.num_vertex_m;
		vertex_pos = Controller.GetComponent<SceneController>().vertex_pos_m;
		pressure = Controller.GetComponent<SceneController>().pressure;


		/*
		// Load vertex pos
		ReadVertexPos();
		// Load pressure data
		ReadPressure();
		*/

		// Create Mesh
		CreateMesh();
		Debug.Log("Model Mesh Created");
		// Rotate object
		PointCloudMesh.transform.Rotate(-90f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void ReadMesh()
	{
		// Load binary file
    	byte[] fileBytes = File.ReadAllBytes("Assets/StreamingAssets/mesh.bin");
		MemoryStream stream = new MemoryStream(fileBytes);
		BinaryReader reader = new BinaryReader(stream);

		// read number of triangles
		int num_triangle = reader.ReadInt32();
		int num_t1 = 6637495;
		int num_t2 = num_triangle - num_t1;
		Debug.Log("Triangles: " + num_triangle);

		// read triangle data
		int[,] tri_mat1 = new int[3, num_t1];
		int[,] tri_mat2 = new int[3, num_t2];

		for (int j=0; j<num_t1; j++)
		{
			for(int i=0; i<3; i++)
			{
				tri_mat1[i, j] = reader.ReadInt32();
			}
		}

		for (int j=0; j<num_t2; j++)
		{
			for(int i=0; i<3; i++)
			{
				tri_mat2[i, j] = reader.ReadInt32();
			}
		}

		// Read dummy string long 40
		char[] dummy_str_1 = reader.ReadChars(8);
		char[] dummy_str_2 = reader.ReadChars(32);
		
		// Read min/max pressure
		byte[] b1 = reader.ReadBytes(4);
		byte[] b2 = reader.ReadBytes(4);
		float min_v = System.BitConverter.ToSingle(b1, 0);
		float max_v = System.BitConverter.ToSingle(b2, 0);

		// Read number of vertices
		int num_vertex = reader.ReadInt32();
		Debug.Log("Vertex: " + num_vertex);
		// Read vetex value [x y z nx ny nz pressure]
		float[,] ver_mat = new float[7, num_vertex];
		for (int j=0; j<num_vertex ; j++)
		{
			for(int i=0; i<7; i++)
			{
				byte[] tmp = reader.ReadBytes(4);
				ver_mat[i, j] = System.BitConverter.ToSingle(tmp, 0);
			}
		}

		Debug.Log("Finished to parse the data.");
	}

	(int[,], int[,]) ReadTriangles()
	{
		// Load binary file
    	byte[] fileBytes = File.ReadAllBytes("Assets/StreamingAssets/triangles.bin");
		MemoryStream stream = new MemoryStream(fileBytes);
		BinaryReader reader = new BinaryReader(stream);

		// Set matrix length
		int num_t1 = 6637495;
		int num_t2 = 6637494;

		// read triangle data
		int[,] tri_mat1 = new int[3, num_t1];
		int[,] tri_mat2 = new int[3, num_t2];

		for (int j=0; j<num_t1; j++)
		{
			for(int i=0; i<3; i++)
			{
				tri_mat1[i, j] = reader.ReadInt32();
			}
		}

		for (int j=0; j<num_t2; j++)
		{
			for(int i=0; i<3; i++)
			{
				tri_mat2[i, j] = reader.ReadInt32();
			}
		}

		return (tri_mat1, tri_mat2);
	}


	// Read vertex coordinates
	void ReadVertexPos()
	{
		// Load binary file
		string file_name = "vertex_pos.bytes";
		string tmp_path = Path.Combine(Application.streamingAssetsPath, file_name);
		byte[] fileBytes;

		if (Application.platform == RuntimePlatform.Android)
		{
			UnityWebRequest www = UnityWebRequest.Get(tmp_path);
			www.SendWebRequest();
			while (!www.isDone) ;
			fileBytes = www.downloadHandler.data;

		}
		else
		{
			fileBytes = File.ReadAllBytes(tmp_path);
		}

		MemoryStream stream = new MemoryStream(fileBytes);
		BinaryReader reader = new BinaryReader(stream);

		float tmp_x;
		float tmp_y;
		float tmp_z;
		byte[] tmp;

		// Read vetex value [x y z]
		for (int i=0; i<num_vertex ; i++)
		{
			// save x
			tmp = reader.ReadBytes(4);
			tmp_x = System.BitConverter.ToSingle(tmp, 0);

			// save y
			tmp = reader.ReadBytes(4);
			tmp_y = System.BitConverter.ToSingle(tmp, 0);

			// save z
			tmp = reader.ReadBytes(4);
			tmp_z = System.BitConverter.ToSingle(tmp, 0);

			// store values
			vertex_pos[i] = new Vector3(tmp_x, tmp_y, tmp_z);
		}
	}


	// Read vertex pressure
	void ReadPressure()
	{
		string file_name = "vertex_pressure.bytes";
		string tmp_path = Path.Combine(Application.streamingAssetsPath, file_name);
		byte[] fileBytes;

		if (Application.platform == RuntimePlatform.Android)
		{
			UnityWebRequest www = UnityWebRequest.Get(tmp_path);
			www.SendWebRequest();
			while (!www.isDone) ;
			fileBytes = www.downloadHandler.data;

		}
		else
		{
			fileBytes = File.ReadAllBytes(tmp_path);
		}

		MemoryStream stream = new MemoryStream(fileBytes);
		BinaryReader reader = new BinaryReader(stream);
		byte[] tmp;

		for (int i=0; i<num_vertex ; i++)
		{
			tmp = reader.ReadBytes(4);
			pressure[i] = System.BitConverter.ToSingle(tmp, 0);
		}

		max_p = pressure.Max();
		min_p = pressure.Min();

		// Normalize the pressure from (0-1)
		float delta = (max_p - min_p) / 1f;
		for (int i=0; i<num_vertex ; i++)
		{
			pressure[i] = (pressure[i] - min_p) / delta;
		}
	}

	
	void CreateMesh()
	{
		int[] indecies = new int[num_vertex];
		Color[] colors = new Color[num_vertex];

		mesh = new Mesh();
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshRenderer> ().material = new Material (Shader.Find("Custom/VertexColor"));


		for(int i=0; i<num_vertex; i++)
		{
			indecies[i] = i;

			if(pressure[i] < 0.6f)
			{
				colors[i] = Color.Lerp(Color.magenta, Color.blue, pressure[i] / 0.6f);
			}
			else if(pressure[i] >= 0.6f && pressure[i] < 0.65f)
			{
				colors[i] = Color.Lerp(Color.blue, Color.green, (pressure[i] - 0.6f) / 0.05f);
			}
			else if(pressure[i] >= 0.65f && pressure[i] < 0.675f)
			{
				colors[i] = Color.Lerp(Color.green, Color.yellow, (pressure[i] - 0.65f) / 0.025f);
			}
			else if(pressure[i] >= 0.675f && pressure[i] < 0.7f)
			{
				colors[i] = Color.Lerp(Color.yellow, Color.red, (pressure[i] - 0.675f) / 0.025f);
			}
			else
			{
				colors[i] = Color.Lerp(Color.red, Color.black, (pressure[i] - 0.7f) / 0.3f);
			}
		}

		mesh.vertices = vertex_pos;
		mesh.colors = colors;
		mesh.SetIndices(indecies, MeshTopology.Points,0);
	}
}
