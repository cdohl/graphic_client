using UnityEngine;
using System.Collections;

public class guiscript : MonoBehaviour {
	s_TCP myTCP;
	ArrayList graphicsArray = new ArrayList();

	const char cmd_plane = 'P';//simple plane no argument
	const char cmd_cube = 'C';//cube, "C,x,y,z,s" where x,y,z is position, and s is size
	const char cmd_pmesh = 'B';//single sided linear mesh "M,n1,n2,n3,n4,...." 
	const char cmd_cmesh = 'K';//cylinder mesh "K,n1,n2,n3,n4,...."
	const char cmd_mmesh = 'M';//planar Mesh "M,Nx,Nz,dNx,dNz,n1,n2,n3,...."
	const char cmd_mmesh_mod = 'm';//modify Mesh "m,n,n1,n2,n3,...." where n is the object number
	const char cmd_destroy = 'D';//destroy object "D,n" where n is the object number
	const char cmd_obj_transform = 't';//transform object : change position "t,p,n,x,y,z", change rotation "t,r,n,x,y,z", change scale "t,s,n,x,y,z"
	const char cmd_load_resource = 'L';//Load resource "L,coordinates" loads the resource named "coordinates", retuns object number
	public GameObject myplane;
	public GameObject mycube;
	public GameObject mymesh;
	public int ANZ=4;//Number of facets of Cylinder Mesh
	
	Mesh MakeDoubleSided( Mesh mesh){
		var vertices = mesh.vertices;
		var uv = mesh.uv;
		var normals = mesh.normals;
		var szV = vertices.Length;
		var newVerts = new Vector3[szV*2];
		var newUv = new Vector2[szV*2];
		var newNorms = new Vector3[szV*2];
		int k;
		for (int j=0; j< szV; j++){
			// duplicate vertices and uvs:
			newVerts[j] = newVerts[j+szV] = vertices[j];
			newUv[j] = newUv[j+szV] = uv[j];
			// copy the original normals...
			newNorms[j] = normals[j];
			// and revert the new ones
			newNorms[j+szV] = -normals[j];
		}
		var triangles = mesh.triangles;
		var szT = triangles.Length;
		var newTris = new int[szT*2]; // double the triangles

		for (int i=0; i< szT; i+=3){
			// copy the original triangle
			newTris[i] = triangles[i];
			newTris[i+1] = triangles[i+1];
			newTris[i+2] = triangles[i+2];
			// save the new reversed triangle
			k = i+szT; 
			newTris[k] = triangles[i]+szV;
			newTris[k+2] = triangles[i+1]+szV;
			newTris[k+1] = triangles[i+2]+szV;
		}
		mesh.vertices = newVerts;
		mesh.uv = newUv;
		mesh.normals = newNorms;
		mesh.triangles = newTris;// assign triangles last!
		return mesh;
	}
	
	Mesh CreateLinearMesh(float dN, float[] myfloats)
	{
		Mesh m = new Mesh();
		int Nm = myfloats.GetLength (0)/3;//Number of data points

		var myvertices = new Vector3[2 * Nm];
		var myuv = new Vector2[2 * Nm];
		var mytriangles = new int[3 * (Nm * 2)];
		m.name = "Linear Mesh";

		//set the vertices 2*datapoints to give it some depth
		for (int j = 0; j < 2; j++) 
			for (int i = 0; i < Nm; i++){
				myvertices[i+j*Nm]= new Vector3(myfloats[i*3], myfloats[i*3+1], myfloats[i*3+2]+dN*(float)j);//give some depth in z direction...
				myuv[i+j*Nm]= new Vector2(myfloats[i*3],myfloats[i*3+1]);// don't really understand...
			}
		m.vertices=myvertices;
		m.uv = myuv;
		//define the members of the triangular faces
		for (int i = 0; i< (Nm-1); i++){
			mytriangles[i*3+0] = i;
			mytriangles[i*3+1] = i+1;
			mytriangles[i*3+2] = i+Nm+1;
			mytriangles[(i+Nm-1)*3+0] = i;
			mytriangles[(i+Nm-1)*3+1] = i+Nm+1;
			mytriangles[(i+Nm-1)*3+2] = i+Nm;
		}

		m.triangles = mytriangles;
		m.RecalculateNormals();
		m=MakeDoubleSided (m);
		return m;
	}

	Mesh CreatePlanarMesh(int Nx, int Nz, float dNx, float dNz, float[] myfloats)
	{
				Mesh m = new Mesh ();

				var myvertices = new Vector3[Nx * Nz];
				var myuv = new Vector2[Nx * Nz];
				var mytriangles = new int[3 * 2 * (Nx - 1) * (Nz - 1)];
				m.name = "Planar Mesh (x-z) plane";

				for (int j = 0; j < Nz; j++) 
						for (int i = 0; i < Nx; i++) {
								myvertices [i + j * Nx] = new Vector3 (i * dNx, myfloats [(i + j * Nx)], j * dNz);
								myuv [i + j * Nx] = new Vector2 ((float)i / (float)Nx, (float)j / (float)Nz);
						}
				m.vertices = myvertices;
				m.uv = myuv;
				//define the members of the triangular faces
				for (int j = 0; j<(Nz-1); j++)
						for (int i = 0; i< (Nx-1); i++) {
								mytriangles [j * (Nx - 1) * 6 + i * 6 + 0] = i+j*Nx;
								mytriangles [j * (Nx - 1) * 6 + i * 6 + 1] = i + 1 + (j + 1) * Nx;
								mytriangles [j * (Nx - 1) * 6 + i * 6 + 2] = i + 1+j*Nx;
								mytriangles [j * (Nx - 1) * 6 + i * 6 + 3] = i+j*Nx;
								mytriangles [j * (Nx - 1) * 6 + i * 6 + 4] = i + (j + 1) * Nx;
								mytriangles [j * (Nx - 1) * 6 + i * 6 + 5] = i + 1 + (j + 1) * Nx;
						}
		
				m.triangles = mytriangles;
				m.RecalculateNormals ();
				m=MakeDoubleSided(m);
				return m;
		}



	Mesh CreateCylinderMesh(float dR, float[] myfloats){
		Mesh m = new Mesh();
		int Nm = myfloats.GetLength (0) / 3;
		var pos = new Vector3 [Nm];
		Vector3 n, p1, p2;
		var myvertices = new Vector3[Nm * ANZ+2];//The last two are for the end vertices
		var myuv = new Vector2[Nm * ANZ+2];
		var mytriangles = new int[6 * (Nm-1) * ANZ+6*ANZ];
		float dphi = 2F * Mathf.PI / (float) ANZ;
		float phi;

		m.name = "Cylinder Mesh";
		// create position vectors
		for (int i = 0; i < Nm; i ++)
			pos [i] = new Vector3 (myfloats [i * 3], myfloats [i * 3+1], myfloats [i * 3 +2]);

		// create cylinder vertices
		for (int i = 0; i < (Nm-1); i++){
			n=pos[i]-pos[i+1];
			n=n.normalized;//normal vector
			p1.z=-(n.x+n.y);//Using the scalar product to calculate a first normal vector to the normal, assuming p.x=p.y=n.z
			p1.x=n.z;
			p1.y=n.z;
			p1=p1.normalized;
			p2=Vector3.Cross(n,p1).normalized;
			//calculate the cylinder vectors for pos[i]
			for (int j=0; j<ANZ; j++) {
				phi = (float)j * dphi;
				myvertices[i*ANZ+j] =pos[i]+dR * (Mathf.Cos (phi) * p1 + Mathf.Sin (phi) * p2);
				myuv[i*ANZ+j] = new Vector2((float)i/(float)Nm,(float)j/(float)ANZ);
			}
			if (i==(Nm-2)){
				i=Nm-1;//do the last face with the same p1 and p2
				for (int j=0; j<ANZ; j++) {
					phi = (float)j * dphi;
					myvertices[i*ANZ+j] =pos[i]+dR * (Mathf.Cos (phi) * p1 + Mathf.Sin (phi) * p2);
					myuv[i*ANZ+j] = new Vector2(1.0F,(float) j/ (float) ANZ);

				}
			}
		}
		myvertices [Nm * ANZ] = pos [0];// center of the front face
		myuv[Nm * ANZ]=new Vector2(0.0F,0.0F);
		myvertices [Nm * ANZ+1] = pos [Nm-1];//center of the back face
		myuv[Nm * ANZ]=new Vector2(1.0F,0.0F);

		m.vertices=myvertices;
		m.uv = myuv;
		//create vertices
		for (int i=0;i<(Nm-1);i++){//loop over all points of the curve
			for (int j=0;j<(ANZ-1);j++){
				//j counts the corners
				mytriangles[j*6+i*ANZ*6+0] = j+i*ANZ;
				mytriangles[j*6+i*ANZ*6+1] = j+(i+1)*ANZ+1;
				mytriangles[j*6+i*ANZ*6+2] = j+i*ANZ+1;
				mytriangles[j*6+i*ANZ*6+3] = j+i*ANZ;
				mytriangles[j*6+i*ANZ*6+4] = j+(i+1)*ANZ;
				mytriangles[j*6+i*ANZ*6+5] = j+(i+1)*ANZ+1;
			}
			mytriangles[(ANZ-1)*6+i*ANZ*6+0]= (ANZ-1)+i*ANZ;
			mytriangles[(ANZ-1)*6+i*ANZ*6+1]= (i+1)*ANZ;
			mytriangles[(ANZ-1)*6+i*ANZ*6+2]= i*ANZ;
			mytriangles[(ANZ-1)*6+i*ANZ*6+3]= (ANZ-1)+i*ANZ;
			mytriangles[(ANZ-1)*6+i*ANZ*6+4]= (i+1)*ANZ+ANZ-1;
			mytriangles[(ANZ-1)*6+i*ANZ*6+5]= (i+1)*ANZ;
		}
		int fp = 6 * ANZ * (Nm - 1);
		//front and back facecap
		for (int i=0;i<(ANZ-1);i++){
			//front face

			mytriangles[fp+i*6+0]=Nm*ANZ;
			mytriangles[fp+i*6+1]=i;
			mytriangles[fp+i*6+2]=i+1;
			//back face
			mytriangles[fp+i*6+3]=Nm*ANZ+1;
			mytriangles[fp+i*6+4]=(Nm-1)*ANZ+i+1;//note orientation for back face
			mytriangles[fp+i*6+5]=(Nm-1)*ANZ+i;
		}
		//last triangles of the front and back face
		mytriangles[fp+(ANZ-1)*6+0]=Nm*ANZ;
		mytriangles[fp+(ANZ-1)*6+1]=(ANZ-1);
		mytriangles[fp+(ANZ-1)*6+2]=0;
		//back face
		mytriangles[fp+(ANZ-1)*6+3]=Nm*ANZ+1;
		mytriangles[fp+(ANZ-1)*6+4]=(Nm-1)*ANZ;//note orientation for back face
		mytriangles[fp+(ANZ-1)*6+5]=Nm*ANZ-1;

		m.triangles = mytriangles;
		m.RecalculateNormals();

		return m;

	}

	//Register Object for later reference and return value over socket
	void RegisterObject(GameObject myobj){
		graphicsArray.Add(myobj);
		myTCP.writeSocket((graphicsArray.Count-1).ToString());
	}

	// Use this for initialization
	void Start () {
		myTCP = gameObject.AddComponent<s_TCP> ();
			//new s_TCP();
		myTCP.setupSocket();
		Debug.Log (myTCP.socketReady);
		myTCP.writeSocket("get me started");//request data
	}
		
	// Update is called once per frame
	void Update () {
		string message;
		char cmd;
		float x, y, z, s;
		float dNx, dNz;
		string [] parts;
		GameObject r_clone;
		GameObject m_clone;
		GameObject game_obj;
		int Nf;
		int Nx, Nz;
		Mesh thisMesh;
		MeshFilter myMeshFilter;


		while (true) {
			message = myTCP.readSocket ();

			if (message.Length > 0) {
					Debug.Log ("Message " + message + "received.");
					cmd = (char)message [0];
					switch (cmd) {
					case cmd_plane:
						Debug.Log ("Plane received");
						Instantiate (myplane);
					break;
					case cmd_cube:
						Debug.Log ("Cube received");
						parts = message.Split (',');
						x = float.Parse (parts [1]);
						y = float.Parse (parts [2]);
						z = float.Parse (parts [3]);
						s = float.Parse (parts [4]);
						r_clone = Instantiate (mycube, new Vector3 (x, y, z), Quaternion.identity) as GameObject;
						r_clone.transform.localScale =new Vector3 (s,s,s);
						RegisterObject(r_clone);
					break;
					//				a, b, c = (int(i) for i in mystr.split()[1].split('.'))
					case cmd_pmesh:
						Debug.Log ("Band mesh received");
						parts=message.Split (',');
						Nf=parts.GetLength(0)-1;//Number of floats, divide by 3 is 
						var myfloats= new float[Nf];
						
						for (int i=1;i<parts.GetLength (0);i++)
							myfloats[i-1]=float.Parse (parts[i]);
						//check if Nf is Nm*3

						if (Mathf.Floor(Nf/3.0F)!=Nf/3.0F) //we need Triplets, is there a modulus operator in C#?
							break;
						m_clone = Instantiate (mymesh) as GameObject;
						MeshFilter meshFilter = (MeshFilter)m_clone.GetComponent<MeshFilter>();
						meshFilter.mesh = CreateLinearMesh(.1F,myfloats);//To do the thickness should be transferred to the
						RegisterObject(m_clone);
					break;
					case cmd_cmesh:
						Debug.Log ("Cylinder mesh received");
						parts=message.Split (',');
						Nf=parts.GetLength(0)-1;//Number of floats, divide by 3 is 
						//check if Nf is Nm*3
						if (Mathf.Floor(Nf/3.0F)!=Nf/3.0F) //we need Triplets, is there a modulus operator in C#?
							break;

						var myfloats1= new float[Nf];
						for (int i=1;i<parts.GetLength (0);i++)
							myfloats1[i-1]=float.Parse (parts[i]);

						game_obj = Instantiate (mymesh) as GameObject;
						myMeshFilter = (MeshFilter)game_obj.GetComponent<MeshFilter>();
						myMeshFilter.mesh = CreateCylinderMesh(1F,myfloats1);//calculate and attach mesh
						RegisterObject(game_obj);
					break;
					case cmd_mmesh:

						Debug.Log ("Planar Mesh generate");
						parts=message.Split (',');
						Nf=parts.GetLength(0)-1;//Number of parameters
						if (Nf<8){
							Debug.Log ("Not enough data provided for planar mesh.");
							break;
						}
						Nx=int.Parse (parts[1]);
						Nz=int.Parse (parts[2]);
						dNx=float.Parse (parts[3]);
						dNz=float.Parse (parts[4]);
						if (Nf!=(Nx*Nz+4)){
							Debug.Log ("Number of data points does not match definition.");
							Debug.Log ("Nf: "+Nf.ToString());
							break;
						}
						var myfloats2= new float[Nx*Nz];
						Debug.Log ("Nf: "+Nf.ToString());
						for (int i=0;i<(Nf-4);i++)
							myfloats2[i]=float.Parse (parts[i+5]);
						
						game_obj = Instantiate (mymesh) as GameObject;
						myMeshFilter = (MeshFilter)game_obj.GetComponent<MeshFilter>();
						myMeshFilter.mesh = CreatePlanarMesh(Nx,Nz,dNx,dNz,myfloats2);//calculate and attach mesh
						RegisterObject(game_obj);
					break;
					case cmd_mmesh_mod:
						Debug.Log ("Planar Mesh modify");
						parts=message.Split (',');
						Nf=parts.GetLength(0)-2;//Number of datapoints
						
						game_obj = (GameObject) graphicsArray[int.Parse (parts[1])];
						thisMesh = (Mesh)game_obj.GetComponent<MeshFilter>().mesh;
						Vector3[] myvertices = thisMesh.vertices; 
						for (int i =0;i<(int)myvertices.Length/2;i++){
							myvertices[i].y=float.Parse (parts[i+2]);
							myvertices[i+(int)(myvertices.Length/2)].y=float.Parse (parts[i+2]); //to do, currently unity crashes...
						}
						thisMesh.vertices=myvertices;
						thisMesh.RecalculateNormals();
					break;
					case cmd_obj_transform:
						Debug.Log ("Planar Mesh transform");
						parts=message.Split (',');
						Nf=parts.GetLength(0)-3;
						char subcmd=(char)(parts[1]) [0];
						switch (subcmd){
						case 'p'://Position
							game_obj = (GameObject) graphicsArray[int.Parse (parts[2])];
							game_obj.transform.position=new Vector3(float.Parse(parts[3]),float.Parse(parts[4]),float.Parse(parts[5]));
						break;
						case 'r'://Rotation
							game_obj = (GameObject) graphicsArray[int.Parse (parts[2])];
							Quaternion target = Quaternion.Euler (float.Parse(parts[3]),float.Parse(parts[4]),float.Parse(parts[5]));
							game_obj.transform.rotation=target;
						break;
						case 's'://Scale
							game_obj = (GameObject) graphicsArray[int.Parse (parts[2])];
							game_obj.transform.localScale=new Vector3(float.Parse(parts[3]),float.Parse(parts[4]),float.Parse(parts[5]));
						break;
					}					
					break;
				case cmd_load_resource:
					Debug.Log ("Load Resource");
					parts=message.Split (',');
					RegisterObject(Instantiate(Resources.Load<GameObject>(parts[1])) as GameObject);
					break;
				case cmd_destroy:
						parts=message.Split (',');
						if (graphicsArray[int.Parse (parts[1])] !=null){
							Destroy((GameObject)graphicsArray[int.Parse (parts[1])]);
							graphicsArray[int.Parse (parts[1])]=null;
							Debug.Log("Object "+parts[1]+" deleted.");
						}
					break;
					default:
						Debug.Log ("Unknown command");
					break;
					}
			} else
				return;
		}
	}

}