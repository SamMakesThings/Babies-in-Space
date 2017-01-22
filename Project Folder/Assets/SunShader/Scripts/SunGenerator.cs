//
// SunShader 1.0
//
// Panteleymonov Aleksandr 2016
//
// SunShader 1.0
//
// foxes@bk.ru
// manil@panteleymonov.ru
//

using UnityEngine;
using System.Collections;

namespace Panteleymonov {
[ExecuteInEditMode]
[AddComponentMenu ("Space/Star/SunGenerator") ]

public class SunGenerator : MonoBehaviour {

	public enum EModeSM { LIGHT_CPU, LIGHT_SM3, LIGHT_SM4, CPU_SM3,SM3,CPU_SM4,SM4 }
	public enum EMeshType { Billboard,Prisma } //Box,Sphere

	[Header("Base")]
	[Tooltip("Shader version and type")]
	public EModeSM ShaderMode = EModeSM.SM3;
	[Tooltip("Model of mesh for view body, Billboard, Prisma")]
	public EMeshType MeshType = EMeshType.Prisma;

	[Header("Body")]
	[Tooltip("Body radius")]
	public float Radius = 0.5f;
	[Tooltip("Rays radius")]
	public float RayString = 1.0f;
	[Tooltip("Full scale, object radius is ( Radius + RayString ) * Zoom")]
	public float Zoom = 1.0f;
	[Tooltip("Details of elements")]
	public int Detail = 2;
	[Tooltip("Seed")]
	public float Seed = 0.0f;

	[Header("Elements")]
	[Tooltip("Glowing around body, it is inverse parameter")]
	public float Glow = 4.0f;
	[Tooltip("The intensity of rays, with more details, it is inverse parameter")]
	public float Rays = 2.0f;
	[Tooltip("The intensity of the bunches of rays, forming a wave")]
	public float RayRing = 1.0f;
	[Tooltip("Brigtness of rays, it is inverse parameter")]
	public float RayGlow = 2.0f;

	[Header("Colors")]
	[Tooltip("Color of body glare")]
	public Color Light = new Vector4(1.0f,1.0f,1.0f,1.0f);
	[Tooltip("Color of body")]
	public Color Color = new Vector4(1.0f,1.0f,0.0f,1.0f);
	[Tooltip("Color of body ground")]
	public Color BaseColor = new Vector4(1.0f,0.0f,0.0f,1.0f);
	[Tooltip("Color of shadow ground")]
	public Color DarkColor = new Vector4(1.0f,0.0f,1.0f,1.0f);
	[Tooltip("Color of rays")]
	public Color RayLight = new Vector4(1.0f,0.95f,1.0f,1.0f);
	[Tooltip("Color of edge rays")]
	public Color RayColor = new Vector4(1.0f,0.6f,0.1f,1.0f);

	[Header("Animation")]
	[Tooltip("Motion of big elements")]
	public float SpeedLow = 2.0f;
	[Tooltip("Motion of little elements")]
	public float SpeedHi = 2.0f;
	[Tooltip("Motion of rays")]
	public float SpeedRay = 5.0f;
	[Tooltip("Motion of wave rays rings")]
	public float SpeedRing = 2.0f;

	[Header("Noise Generator")]
	[Tooltip("Brigtness of body layers")]
	public Vector4 BodyNoiseLight = new Vector4(0.625f,0.125f,0.0625f,0.03125f);
	[Tooltip("Scale of body layers")]
	public Vector4 BodyNoiseScale = new Vector4(3.6864f,61.44f,307.2f,600.0f);
	[Tooltip("Scale of ray layers")]
	public Vector4 RayNoiseScale = new Vector4(1.0f,10.0f,5.0f,3.0f);

	//[Tooltip("size of detailes")]

	private static Texture3D RNDt=null;
	private static Mesh Board=null;
	private static Mesh BoardDouble=null;
	private static Mesh Prizm=null;
	private static Mesh PrizmDouble=null;

	// Use this for initialization
	void Start () {
		OnValidate ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	// for animation scale
	// you can remove it for more performance
	void FixedUpdate()
	{
		float r = (Radius+RayString)*Zoom;
		float localScalex = transform.localScale.x;
			if (r == localScalex)
			return;
		MeshRenderer mr = GetComponent<MeshRenderer>();
		if (mr == null)
			return;

		Zoom=localScalex/(Radius+RayString);
		float invZoom = 1.0f / Zoom;
		if (ShaderMode == EModeSM.CPU_SM3 || ShaderMode == EModeSM.SM3) {
			mr.sharedMaterials[0].SetFloat ("_Zoom",invZoom);
			mr.sharedMaterials[1].SetFloat ("_Zoom",invZoom);
		} else {
			mr.sharedMaterials[0].SetFloat ("_Zoom",invZoom);
		}
	}

	void OnValidate()
	{
		if (Radius<0.0f) Radius=0.0f;
		if (RayString<0.0f) RayString=0.0f;
		if (ShaderMode==EModeSM.CPU_SM3) GenSSM3 ();
		if (ShaderMode==EModeSM.SM3) GenSM3 ();
		if (ShaderMode==EModeSM.CPU_SM4) GenSSM4 ();
		if (ShaderMode==EModeSM.SM4) GenSM4 ();
		if (ShaderMode == EModeSM.LIGHT_CPU) GenLS();
        if (ShaderMode == EModeSM.LIGHT_SM3) GenLSM3();
        if (ShaderMode == EModeSM.LIGHT_SM4) GenLSM4();
    }

        void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, Radius*Zoom);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, (Radius+RayString)*Zoom);
	}

	MeshFilter PrepeareMesh()
	{
		MeshFilter mf = GetComponent<MeshFilter>();
		if (mf == null)
		{
			mf = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		}
		mf.sharedMesh = new Mesh();
		mf.sharedMesh.Clear();
		return mf;
	}

	void MeshBillboard(float r)
	{
		MeshFilter mf = PrepeareMesh();
	
		if (Board == null) {
			Board = new Mesh();
			Vector3[] versi = new Vector3[4] {new Vector3 (-r,-r,0),new Vector3 (r,-r,0),new Vector3 (r,r,0),new Vector3 (-r,r,0)};
			Vector3[] normalsi = new Vector3[4];
			Vector2[] uvs = new Vector2[4];
			Board.subMeshCount = 1;
			Board.vertices = versi;
			Board.normals = normalsi;
			Board.uv = uvs;
			Board.triangles=new int[6] {1, 0, 2, 3, 2, 0};
			Board.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(r, r, r)*2.0f);
		}

		mf.sharedMesh = Board;
	}

	void MeshDoubleBillboard(float r)
	{
		MeshFilter mf = PrepeareMesh();

		if (BoardDouble == null) {
			BoardDouble = new Mesh();
			Vector3[] versi = new Vector3[8] {new Vector3 (-r,-r,0),new Vector3 (r,-r,0),new Vector3 (r,r,0),new Vector3 (-r,r,0),new Vector3 (-r,-r,-r*0.1f),
				new Vector3 (r,-r,-r*0.1f),new Vector3 (r,r,-r*0.1f),new Vector3 (-r,r,-r*0.1f)};
			Vector3[] normalsi = new Vector3[8];
			Vector2[] uvs = new Vector2[8];
			BoardDouble.subMeshCount = 2;
			BoardDouble.vertices = versi;
			BoardDouble.normals = normalsi;
			BoardDouble.uv = uvs;

			int[] tris = new int[6] {1, 0, 2, 3, 2, 0};
			BoardDouble.SetTriangles (tris,0);
			tris = new int[6] {5, 4, 6, 7, 6, 4};
			BoardDouble.SetTriangles (tris,1);
			BoardDouble.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(r, r, r)*2.0f);
		}
		
		mf.sharedMesh = BoardDouble;
	}

	void MeshPrisma(float r)
	{
		MeshFilter mf = PrepeareMesh();

		if (Prizm == null) {
			Prizm = new Mesh();
			Vector3[] versi = new Vector3[6] {new Vector3 (-r,-r,-r),new Vector3 (r,-r,-r),new Vector3 (r,r,-r),new Vector3 (-r,r,-r),new Vector3 (0,0,0),new Vector3 (0,0,-r*2.0f)};
			Vector3[] normalsi = new Vector3[6];
			Vector2[] uvs = new Vector2[6];
		
			Prizm.subMeshCount = 1;
			Prizm.vertices = versi;
			Prizm.normals = normalsi;
			Prizm.uv = uvs;
		
			Prizm.triangles=new int[24] {1, 0, 4, 2, 1, 4, 3, 2, 4, 0, 3, 4, 0, 1, 5, 1, 2, 5, 2, 3, 5, 3, 0, 5};
		
			Prizm.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(r, r, r)*2.0f);
		}

		mf.sharedMesh = Prizm;
	}
	
	void MeshDoublePrisma(float r)
	{
		MeshFilter mf = PrepeareMesh();

		if (PrizmDouble == null) {
			PrizmDouble = new Mesh();

			Vector3[] versi = new Vector3[10] {new Vector3 (-r,-r,-r),new Vector3 (r,-r,-r),new Vector3 (r,r,-r),new Vector3 (-r,r,-r),new Vector3 (-r*0.9f,-r*0.9f,-r),
				new Vector3 (r*0.9f,-r*0.9f,-r),new Vector3 (r*0.9f,r*0.9f,-r),new Vector3 (-r*0.9f,r*0.9f,-r),new Vector3 (0,0,0),new Vector3 (0,0,-r*2.0f)};
			Vector3[] normalsi = new Vector3[10];
			Vector2[] uvs = new Vector2[10];
		
			PrizmDouble.subMeshCount = 2;
			PrizmDouble.vertices = versi;
			PrizmDouble.normals = normalsi;
			PrizmDouble.uv = uvs;
		
			int[] tris = new int[24] {1, 0, 8, 2, 1, 8, 3, 2, 8, 0, 3, 8, 0, 1, 9, 1, 2, 9, 2, 3, 9, 3, 0, 9};
			PrizmDouble.SetTriangles (tris,0);
			tris = new int[24] {5, 4, 8, 6, 5, 8, 7, 6, 8, 4, 7, 8, 4, 5, 9, 5, 6, 9, 6, 7, 9, 7, 4, 9};
			PrizmDouble.SetTriangles (tris,1);
		
			PrizmDouble.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(r, r, r)*2.0f);
		}

		mf.sharedMesh = PrizmDouble;
	}

	void fillShaderData( Material material)
	{
		material.SetFloat ("_Radius",Radius);
		material.SetFloat ("_Detail",(float)Detail);
		material.SetFloat ("_RayString",RayString);
		material.SetFloat ("_Glow",Glow);
		material.SetFloat ("_Rays",Rays);
		material.SetFloat ("_RayRing",RayRing);
		material.SetFloat ("_RayGlow",RayGlow);
		material.SetFloat ("_Zoom",1.0f/Zoom);
		material.SetVector ("_Light",Light);
		material.SetVector ("_Color",Color);
		material.SetVector ("_Base",BaseColor);
		material.SetVector ("_Dark",DarkColor);
		material.SetVector ("_RayLight",RayLight);
		material.SetVector ("_Ray",RayColor);
		material.SetFloat ("_SpeedHi",SpeedHi);
		material.SetFloat ("_SpeedLow",SpeedLow);
		material.SetFloat ("_SpeedRay",SpeedRay);
		material.SetFloat ("_SpeedRing",SpeedRing);
		material.SetFloat ("_Seed",Seed);
		material.SetVector ("_BodyNoiseL",BodyNoiseLight);
		material.SetVector ("_BodyNoiseS",BodyNoiseScale);
		material.SetVector ("_RayNoiseS",RayNoiseScale);
	}

	void fillShaderDataS( Material[] materials)
	{
		materials [0].SetFloat ("_Radius",Radius);
		materials [1].SetFloat ("_Radius",Radius);
		materials [0].SetFloat ("_Detail",(float)Detail);
		materials [1].SetFloat ("_Detail",(float)Detail);
		materials [1].SetFloat ("_RayString",RayString);
		materials [1].SetFloat ("_Glow",Glow);
		materials [1].SetFloat ("_Rays",Rays);
		materials [1].SetFloat ("_RayRing",RayRing);
		materials [1].SetFloat ("_RayGlow",RayGlow);
		materials [0].SetFloat ("_Zoom",1.0f/Zoom);
		materials [1].SetFloat ("_Zoom",1.0f/Zoom);
		materials [0].SetVector ("_Light",Light);
		materials [0].SetVector ("_Color",Color);
		materials [0].SetVector ("_Base",BaseColor);
		materials [0].SetVector ("_Dark",DarkColor);
		materials [1].SetVector ("_RayLight",RayLight);
		materials [1].SetVector ("_Ray",RayColor);
		materials [0].SetFloat ("_SpeedHi",SpeedHi);
		materials [0].SetFloat ("_SpeedLow",SpeedLow);
		materials [1].SetFloat ("_SpeedRay",SpeedRay);
		materials [1].SetFloat ("_SpeedRing",SpeedRing);
		materials [0].SetFloat ("_Seed",Seed);
		materials [1].SetFloat ("_Seed",Seed);
		materials [0].SetVector ("_BodyNoiseL",BodyNoiseLight);
		materials [0].SetVector ("_BodyNoiseS",BodyNoiseScale);
		materials [1].SetVector ("_RayNoiseS",RayNoiseScale);
	}

	void CreateTexture()
	{
		if (RNDt==null) {
			int size = 128;
			RNDt = new Texture3D (size,size,size,TextureFormat.ARGB32,false);
			Color[] Colors= new Color[size * size * size];
			for(int i = 0; i < size; i++) {
				for(int j = 0; j < size; j++) {
					for(int k = 0; k < size; k++) {
						float id = (i * 1.0f + j * 113.0f + k * 157.0f);
						float val1=Mathf.Sin(id) *1399763.5453123f;
						val1=Mathf.Abs(val1-Mathf.Floor(val1));
						float val2 = Mathf.Sin(id+228.25f) * 1399763.5453123f;
						val2 = Mathf.Abs(val2 - Mathf.Floor(val2));
						float val3 = Mathf.Sin(id + 228.25f*2.0f) * 1399763.5453123f;
						val3 = Mathf.Abs(val3 - Mathf.Floor(val3));
						float val4 = Mathf.Sin(id + 228.25f*3.0f) * 1399763.5453123f;
						val4 = Mathf.Abs(val4 - Mathf.Floor(val4));
						Colors[i + (j*size) + (k*size*size)]=new Color(val1,val2,val3,val4);
					}
				}
			}
			RNDt.SetPixels (Colors);
			RNDt.Apply ();
		}
	}

	void GenLS()
	{
		CreateTexture ();
		
		float r = (Radius+RayString)*Zoom;
		if (MeshType==EMeshType.Billboard) MeshBillboard (1.2f);
		if (MeshType==EMeshType.Prisma) MeshPrisma (1.0f);

		MeshRenderer mr = GetComponent<MeshRenderer>();
		transform.localScale = new Vector3 (r,r,r);
		if (mr == null) mr = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		Material[] materials = new Material[1];
		materials [0] = new Material(Shader.Find("Space/Star/Sun_soft_rnd"));
		fillShaderData (materials [0]);
		materials [0].SetTexture ("_RND",RNDt);
		mr.sharedMaterials = materials;
		mr.receiveShadows = false;
		mr.castShadows = false;
	}

	void GenSSM3()
	{
		CreateTexture ();
		
		float r = (Radius+RayString)*Zoom;
		if (MeshType==EMeshType.Billboard) MeshDoubleBillboard (1.2f);
		if (MeshType==EMeshType.Prisma) MeshDoublePrisma (1.0f);
		
		MeshRenderer mr = GetComponent<MeshRenderer>();
		transform.localScale = new Vector3 (r,r,r);
		if (mr == null) mr = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		Material[] materials = new Material[2];
		materials [0] = new Material(Shader.Find("Space/Star/Body_rnd"));
		materials [1] = new Material(Shader.Find("Space/Star/Ray_rnd"));
		fillShaderDataS (materials);
		materials [0].SetTexture ("_RND",RNDt);
		materials [1].SetTexture ("_RND",RNDt);
		mr.sharedMaterials = materials;
		mr.receiveShadows = false;
		mr.castShadows = false;
	}

	void GenSSM4()
	{
		CreateTexture ();
		
		float r = (Radius+RayString)*Zoom;
		if (MeshType==EMeshType.Billboard) MeshBillboard (1.2f);
		if (MeshType==EMeshType.Prisma) MeshPrisma (1.0f);

		MeshRenderer mr = GetComponent<MeshRenderer>();
		transform.localScale = new Vector3 (r,r,r);
		if (mr == null) mr = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		Material[] materials = new Material[1];
		materials [0] = new Material(Shader.Find("Space/Star/Sun_rnd"));
		fillShaderData (materials [0]);
		materials [0].SetTexture ("_RND",RNDt);
		mr.sharedMaterials = materials;
		mr.receiveShadows = false;
		mr.castShadows = false;
	}

	void GenLSM3()
	{
		float r = (Radius+RayString)*Zoom;
		if (MeshType==EMeshType.Billboard) MeshBillboard (1.2f);
		if (MeshType==EMeshType.Prisma) MeshPrisma (1.0f);
		
		MeshRenderer mr = GetComponent<MeshRenderer>();
		transform.localScale = new Vector3 (r,r,r);
		if (mr == null) mr = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		Material[] materials = new Material[1];
		materials [0] = new Material(Shader.Find("Space/Star/Sun_soft_low"));
		fillShaderData (materials [0]);
		mr.sharedMaterials = materials;
		mr.receiveShadows = false;
		mr.castShadows = false;
	}

	void GenLSM4()
	{
		float r = (Radius+RayString)*Zoom;
		if (MeshType==EMeshType.Billboard) MeshBillboard (1.2f);
		if (MeshType==EMeshType.Prisma) MeshPrisma (1.0f);
		
		MeshRenderer mr = GetComponent<MeshRenderer>();
		transform.localScale = new Vector3 (r,r,r);
		if (mr == null) mr = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		Material[] materials = new Material[1];
		materials [0] = new Material(Shader.Find("Space/Star/Sun_soft"));
		fillShaderData (materials [0]);
		mr.sharedMaterials = materials;
		mr.receiveShadows = false;
		mr.castShadows = false;
	}

	void GenSM3()
	{
		float r = (Radius+RayString)*Zoom;
		if (MeshType==EMeshType.Billboard) MeshDoubleBillboard (1.2f);
		if (MeshType==EMeshType.Prisma) MeshDoublePrisma (1.0f);
		
		MeshRenderer mr = GetComponent<MeshRenderer>();
		transform.localScale = new Vector3 (r,r,r);
		if (mr == null) mr = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		Material[] materials = new Material[2];
		materials [0] = new Material(Shader.Find("Space/Star/Body"));
		materials [1] = new Material(Shader.Find("Space/Star/Ray"));
		fillShaderDataS (materials);
		mr.sharedMaterials = materials;
		mr.receiveShadows = false;
		mr.castShadows = false;
	}
	
	void GenSM4()
	{
		float r = (Radius+RayString)*Zoom;
		if (MeshType==EMeshType.Billboard) MeshBillboard (1.2f);
		if (MeshType==EMeshType.Prisma) MeshPrisma (1.0f);
		
		MeshRenderer mr = GetComponent<MeshRenderer>();
		transform.localScale = new Vector3 (r,r,r);
		if (mr == null) mr = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		Material[] materials = new Material[1];
		materials [0] = new Material(Shader.Find("Space/Star/Sun"));
		fillShaderData (materials [0]);
		mr.sharedMaterials = materials;
		mr.receiveShadows = false;
		mr.castShadows = false;
	}
}
}