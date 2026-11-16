using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;

public static class BuildHomework
{
    static Material yellow,black,white,red,brown,green,blue,skin,leaf,particle;
    static string root="Assets/Art";
    static Material Mat(string name,string hex)
    {
        ColorUtility.TryParseHtmlString(hex,out Color color);
        Material m=new Material(Shader.Find("Standard")){name=name,color=color};m.SetFloat("_Glossiness",.18f);
        AssetDatabase.CreateAsset(m,root+"/"+name+".mat");return m;
    }
    static GameObject Part(Transform parent,string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material mat,Vector3 rotation=default)
    {
        GameObject o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=scale;o.transform.localEulerAngles=rotation;
        o.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(o.GetComponent<Collider>());return o;
    }
    static void Ell(Transform p,string n,Vector3 pos,Vector3 size,Material m,Vector3 rot=default){Part(p,n,PrimitiveType.Sphere,pos,size,m,rot);}
    static void Cube(Transform p,string n,Vector3 pos,Vector3 size,Material m,Vector3 rot=default){Part(p,n,PrimitiveType.Cube,pos,size,m,rot);}
    static Mesh Hemisphere(string name,bool top)
    {
        List<Vector3> v=new List<Vector3>();List<int> tri=new List<int>();
        for(int r=0;r<=12;r++)for(int s=0;s<=32;s++){
            float theta=(top?0:Mathf.PI*.5f)+r/12f*Mathf.PI*.5f,phi=s/32f*Mathf.PI*2;
            v.Add(new Vector3(Mathf.Sin(theta)*Mathf.Cos(phi),Mathf.Cos(theta),Mathf.Sin(theta)*Mathf.Sin(phi))*.32f);
        }
        for(int r=0;r<12;r++)for(int s=0;s<32;s++){int a=r*33+s,b=a+33;tri.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
        Mesh mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,root+"/"+name+".asset");return mesh;
    }
    static void MeshPart(Transform p,string n,Mesh mesh,Material mat){var go=new GameObject(n,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(p,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;}
    static GameObject Pokemon()
    {
        var o=new GameObject("Pikachu_Model");Transform p=o.transform;
        Ell(p,"Body",new Vector3(0,.82f,0),new Vector3(1.25f,1.5f,.9f),yellow);
        Ell(p,"Head",new Vector3(0,1.76f,-.06f),new Vector3(1.5f,1.22f,1.05f),yellow);
        Ell(p,"Left ear",new Vector3(-.55f,2.57f,0),new Vector3(.26f,1.03f,.23f),yellow,new Vector3(0,0,23));
        Ell(p,"Right ear",new Vector3(.55f,2.57f,0),new Vector3(.26f,1.03f,.23f),yellow,new Vector3(0,0,-23));
        Ell(p,"Black ear tip L",new Vector3(-.73f,2.96f,0),new Vector3(.23f,.39f,.23f),black,new Vector3(0,0,23));
        Ell(p,"Black ear tip R",new Vector3(.73f,2.96f,0),new Vector3(.23f,.39f,.23f),black,new Vector3(0,0,-23));
        foreach(float sign in new[]{-1f,1f}) {
            Ell(p,"Eye",new Vector3(sign*.31f,1.88f,-.54f),new Vector3(.18f,.23f,.08f),black);
            Ell(p,"Eye shine",new Vector3(sign*.30f,1.93f,-.581f),new Vector3(.065f,.075f,.025f),white);
            Ell(p,"Cheek",new Vector3(sign*.56f,1.58f,-.46f),new Vector3(.28f,.25f,.10f),red);
            Ell(p,"Foot",new Vector3(sign*.38f,.15f,-.21f),new Vector3(.43f,.25f,.66f),yellow);
            Ell(p,"Arm",new Vector3(sign*.65f,.98f,-.2f),new Vector3(.28f,.70f,.32f),yellow,new Vector3(0,0,sign*32));
        }
        Ell(p,"Nose",new Vector3(0,1.72f,-.59f),new Vector3(.10f,.07f,.06f),black);
        Cube(p,"Smile L",new Vector3(-.08f,1.52f,-.565f),new Vector3(.17f,.025f,.025f),black,new Vector3(0,0,-12));
        Cube(p,"Smile R",new Vector3(.08f,1.52f,-.565f),new Vector3(.17f,.025f,.025f),black,new Vector3(0,0,12));
        Cube(p,"Tail root",new Vector3(.7f,.6f,.42f),new Vector3(.5f,.22f,.16f),brown,new Vector3(0,0,30));
        Cube(p,"Tail zig 1",new Vector3(.97f,.95f,.45f),new Vector3(.22f,.65f,.18f),yellow,new Vector3(0,0,35));
        Cube(p,"Tail zig 2",new Vector3(1.19f,1.22f,.45f),new Vector3(.60f,.22f,.18f),yellow,new Vector3(0,0,15));
        Cube(p,"Lightning tail",new Vector3(1.40f,1.64f,.45f),new Vector3(.46f,.92f,.2f),yellow,new Vector3(0,0,-20));
        o.AddComponent<CaptureTarget>();var col=o.AddComponent<CapsuleCollider>();col.center=new Vector3(0,1.30f,0);col.height=2.6f;col.radius=.73f;
        return o;
    }
    static GameObject Trainer()
    {
        var o=new GameObject("Trainer_Model");var p=o.transform;
        foreach(float sign in new[]{-1f,1f}){
            Part(p,"Trousers",PrimitiveType.Capsule,new Vector3(sign*.22f,.67f,0),new Vector3(.32f,.62f,.34f),blue);
            Ell(p,"Sneaker",new Vector3(sign*.22f,.13f,-.13f),new Vector3(.38f,.25f,.65f),white);
            Cube(p,"Shoe sole",new Vector3(sign*.22f,.055f,-.13f),new Vector3(.38f,.08f,.64f),red);
        }
        Part(p,"Blue vest",PrimitiveType.Capsule,new Vector3(0,1.56f,0),new Vector3(.86f,.58f,.52f),blue);
        Cube(p,"Shirt center",new Vector3(0,1.55f,-.27f),new Vector3(.23f,.67f,.045f),white);
        Ell(p,"Neck",new Vector3(0,2.11f,0),new Vector3(.28f,.32f,.3f),skin);
        Ell(p,"Hair",new Vector3(0,2.45f,.03f),new Vector3(.79f,.85f,.70f),black);
        Ell(p,"Face",new Vector3(0,2.42f,-.14f),new Vector3(.68f,.72f,.58f),skin);
        foreach(float sign in new[]{-1f,1f})Ell(p,"Eye",new Vector3(sign*.14f,2.46f,-.422f),new Vector3(.075f,.12f,.025f),black);
        Ell(p,"Cap",new Vector3(0,2.78f,.01f),new Vector3(.86f,.35f,.77f),red);
        Ell(p,"Cap visor",new Vector3(0,2.69f,-.39f),new Vector3(.82f,.08f,.57f),red);
        Ell(p,"Cap badge",new Vector3(0,2.8f,-.36f),new Vector3(.23f,.18f,.04f),white);
        Cube(p,"Backpack",new Vector3(0,1.62f,.35f),new Vector3(.67f,.82f,.32f),green);
        var arm=new GameObject("ThrowArm");arm.transform.SetParent(p,false);arm.transform.localPosition=new Vector3(.48f,1.89f,0);
        Part(arm.transform,"Sleeve",PrimitiveType.Capsule,new Vector3(.15f,-.12f,0),new Vector3(.28f,.23f,.3f),white,new Vector3(0,0,50));
        Part(arm.transform,"Forearm",PrimitiveType.Capsule,new Vector3(.38f,-.22f,-.05f),new Vector3(.21f,.24f,.21f),skin,new Vector3(0,0,95));
        Ell(arm.transform,"Glove",new Vector3(.62f,-.22f,-.05f),Vector3.one*.26f,green);
        Part(p,"Left sleeve",PrimitiveType.Capsule,new Vector3(-.5f,1.73f,0),new Vector3(.29f,.25f,.30f),white,new Vector3(0,0,-20));
        Part(p,"Left arm",PrimitiveType.Capsule,new Vector3(-.62f,1.35f,0),new Vector3(.21f,.27f,.22f),skin);
        Ell(p,"Left glove",new Vector3(-.63f,1.10f,0),Vector3.one*.25f,green);
        return o;
    }
    static GameObject Ball()
    {
        GameObject o=new GameObject("PokeBall_Model");
        MeshPart(o.transform,"Red upper shell",Hemisphere("UpperHemisphere",true),red);
        MeshPart(o.transform,"White lower shell",Hemisphere("LowerHemisphere",false),white);
        Part(o.transform,"Equatorial band",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.65f,.025f,.65f),black);
        Ell(o.transform,"Button surround",new Vector3(0,0,-.316f),new Vector3(.22f,.22f,.07f),black);
        Ell(o.transform,"Button",new Vector3(0,0,-.359f),new Vector3(.145f,.145f,.04f),white);
        o.AddComponent<SphereCollider>().radius=.32f;
        var rb=o.AddComponent<Rigidbody>();rb.mass=1;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;rb.interpolation=RigidbodyInterpolation.Interpolate;
        o.AddComponent<BallController>();
        var trail=o.AddComponent<TrailRenderer>();trail.time=.4f;trail.startWidth=.16f;trail.endWidth=0;trail.sharedMaterial=particle;trail.startColor=new Color(1,.8f,.25f);trail.endColor=new Color(1,.8f,.25f,0);
        return o;
    }
    static GameObject SaveModel(GameObject model,string name)
    {
        ExportObj(model,root+"/Models/"+name+".obj");
        var prefab=PrefabUtility.SaveAsPrefabAsset(model,"Assets/Prefabs/"+name+".prefab");Object.DestroyImmediate(model);return prefab;
    }
    // Portable model source, in addition to the native Unity prefabs/materials.
    static void ExportObj(GameObject go,string path)
    {
        var sb=new System.Text.StringBuilder("# Original procedural educational model\n");int offset=1;
        foreach(var filter in go.GetComponentsInChildren<MeshFilter>()){
            Mesh mesh=filter.sharedMesh;sb.AppendLine("g "+filter.name.Replace(' ','_'));
            foreach(var vertex in mesh.vertices){Vector3 v=go.transform.InverseTransformPoint(filter.transform.TransformPoint(vertex));sb.AppendLine(System.FormattableString.Invariant($"v {v.x} {v.y} {v.z}"));}
            int[] t=mesh.triangles;for(int i=0;i<t.Length;i+=3)sb.AppendLine($"f {t[i]+offset} {t[i+1]+offset} {t[i+2]+offset}");offset+=mesh.vertexCount;
        }File.WriteAllText(path,sb.ToString());
    }
    [MenuItem("HW5/Build complete homework")]
    public static void Build()
    {
        Directory.CreateDirectory(root+"/Models");Directory.CreateDirectory("Assets/Prefabs");Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yellow=Mat("Pikachu gold","#FFCB32");black=Mat("Ink","#192F3A");white=Mat("Ivory","#FFF7E4");red=Mat("Vermilion","#E94D49");brown=Mat("Bark","#805839");green=Mat("Forest","#246D56");blue=Mat("Indigo","#336989");skin=Mat("Skin","#E8B78B");leaf=Mat("Leaves","#52A65D");
        particle=new Material(Shader.Find("Particles/Standard Unlit")){name="Capture Glow"};particle.SetColor("_Color",Color.white);AssetDatabase.CreateAsset(particle,root+"/CaptureGlow.mat");
        var pokePrefab=SaveModel(Pokemon(),"Pikachu");var trainerPrefab=SaveModel(Trainer(),"Trainer");var ballPrefab=SaveModel(Ball(),"PokeBall");
        TerrainData data=new TerrainData{name="Verdant valley sculpted terrain",heightmapResolution=129,size=new Vector3(90,17,65)};
        float[,] heights=new float[129,129];
        for(int z=0;z<129;z++)for(int x=0;x<129;x++){
            float wz=z/128f*65-12,wx=x/128f*90-45;
            float mountain=Mathf.SmoothStep(0,1,Mathf.InverseLerp(12,39,wz));
            heights[z,x]=mountain*(.25f+.5f*Mathf.PerlinNoise(wx*.065f+3,wz*.045f+2));
        }
        data.SetHeights(0,0,heights);AssetDatabase.CreateAsset(data,root+"/ValleyTerrain.asset");
        Texture2D texture=new Texture2D(64,64);Random.InitState(414262466);
        for(int y=0;y<64;y++)for(int x=0;x<64;x++)texture.SetPixel(x,y,Color.Lerp(new Color(.35f,.55f,.22f),new Color(.52f,.66f,.29f),Random.value));texture.Apply();
        File.WriteAllBytes(root+"/Grass.png",texture.EncodeToPNG());AssetDatabase.ImportAsset(root+"/Grass.png");
        TerrainLayer layer=new TerrainLayer{diffuseTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/Grass.png"),tileSize=new Vector2(5,5)};AssetDatabase.CreateAsset(layer,root+"/Grass.terrainlayer");data.terrainLayers=new[]{layer};
        var terrain=Terrain.CreateTerrainGameObject(data);terrain.name="Terrain - grassland and rolling mountains";terrain.transform.position=new Vector3(-45,-.04f,-12);
        var ground=Mat("Path sand","#B8AA84");
        Ell(null,"Sandy battle clearing",new Vector3(1,-.03f,3),new Vector3(16,.13f,10),ground);
        var env=new GameObject("Trees • grass • rocks");
        for(int i=0;i<32;i++){
            float x=Random.Range(-24f,24f),z=Random.Range(7f,24f);float y=terrain.GetComponent<Terrain>().SampleHeight(new Vector3(x,0,z))-.04f;
            var tree=new GameObject("Tree "+i);tree.transform.SetParent(env.transform);tree.transform.position=new Vector3(x,y,z);float h=Random.Range(2.6f,5f);
            Part(tree.transform,"Trunk",PrimitiveType.Cylinder,new Vector3(0,h*.35f,0),new Vector3(.32f,h*.35f,.32f),brown);
            Ell(tree.transform,"Canopy",new Vector3(0,h,0),new Vector3(h*.95f,h*.85f,h*.8f),i%2==0?leaf:green);
            Ell(tree.transform,"Canopy side",new Vector3(h*.3f,h*.88f,0),Vector3.one*h*.60f,leaf);
        }
        // Combine grass blades into one persistent mesh to keep draw calls small.
        List<Vector3> gv=new List<Vector3>();List<int> gt=new List<int>();
        for(int i=0;i<1700;i++){
            float x=Random.Range(-23f,23f),z=Random.Range(-7f,20f);if(Mathf.Abs(x-1)<8 && z>-2 && z<8)continue;
            float y=terrain.GetComponent<Terrain>().SampleHeight(new Vector3(x,0,z));float h=Random.Range(.16f,.45f);
            for(int a=0;a<2;a++){int n=gv.Count;Vector3 d=a==0?Vector3.right:Vector3.forward;gv.Add(new Vector3(x,y,z)-d*.07f);gv.Add(new Vector3(x,y+h,z)+d*.05f);gv.Add(new Vector3(x,y,z)+d*.07f);gt.AddRange(new[]{n,n+1,n+2,n+2,n+1,n});}
        }
        Mesh grass=new Mesh{name="Grass blades"};grass.SetVertices(gv);grass.SetTriangles(gt,0);var normals=new Vector3[gv.Count];for(int i=0;i<normals.Length;i++)normals[i]=Vector3.up;grass.normals=normals;AssetDatabase.CreateAsset(grass,root+"/GrassBlades.asset");MeshPart(env.transform,"Meadow grass",grass,leaf);
        for(int i=0;i<15;i++){float x=Random.Range(-19f,19f),z=Random.Range(5f,15f);float y=terrain.GetComponent<Terrain>().SampleHeight(new Vector3(x,0,z));Ell(env.transform,"River stone",new Vector3(x,y,z),new Vector3(1,.5f,.8f)*Random.Range(.5f,1.4f),black);}
        var trainer=(GameObject)PrefabUtility.InstantiatePrefab(trainerPrefab);trainer.name="First Person Trainer Rig";trainer.transform.position=new Vector3(0,0,-7);
        Transform firstArm=trainer.transform.Find("ThrowArm");firstArm.localPosition=new Vector3(-1.35f,1.65f,1.65f);
        foreach(var renderer in trainer.GetComponentsInChildren<Renderer>())renderer.enabled=renderer.transform.IsChildOf(firstArm);
        var pokemon=(GameObject)PrefabUtility.InstantiatePrefab(pokePrefab);pokemon.transform.position=new Vector3(3,0,5);
        var camObj=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camObj.tag="MainCamera";var cam=camObj.GetComponent<Camera>();cam.transform.SetParent(trainer.transform);cam.transform.localPosition=new Vector3(0,1.7f,0);cam.transform.LookAt(new Vector3(0,1.7f,5));cam.fieldOfView=48;cam.backgroundColor=new Color(.60f,.81f,.84f);cam.clearFlags=CameraClearFlags.SolidColor;cam.farClipPlane=140;
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.7f,.8f,.85f);RenderSettings.ambientEquatorColor=new Color(.55f,.61f,.49f);RenderSettings.ambientGroundColor=new Color(.28f,.35f,.25f);
        RenderSettings.fog=true;RenderSettings.fogColor=cam.backgroundColor;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=35;RenderSettings.fogEndDistance=100;
        var light=new GameObject("Afternoon sun",typeof(Light));light.transform.rotation=Quaternion.Euler(42,-32,0);var sun=light.GetComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.25f;sun.color=new Color(1,.94f,.79f);sun.shadows=LightShadows.Soft;
        for(int i=0;i<8;i++)Ell(null,"Cloud",new Vector3(-35+i*10,17+Random.Range(0,3),47),new Vector3(9,1.8f,3),white);
        var director=new GameObject("Game Director - CH06 generator").AddComponent<CaptureGame>();director.ballPrefab=ballPrefab;director.pokemon=pokemon.transform;director.trainerArm=trainer.transform.Find("ThrowArm");director.particleMaterial=particle;director.sceneCamera=cam;
        var held=Object.Instantiate(ballPrefab);held.name="Held Poké Ball";Object.DestroyImmediate(held.GetComponent<Rigidbody>());Object.DestroyImmediate(held.GetComponent<Collider>());Object.DestroyImmediate(held.GetComponent<BallController>());Object.DestroyImmediate(held.GetComponent<TrailRenderer>());held.transform.position=director.launchPoint;held.transform.localScale=Vector3.one*.8f;director.heldBall=held.transform;
        QualitySettings.SetQualityLevel(3);QualitySettings.shadowDistance=60;QualitySettings.antiAliasing=4;
        PlayerSettings.companyName="YCJ";PlayerSettings.productName="Verdant Valley - HW5";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/Scenes/VerdantValley.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/VerdantValley.unity",true)};
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Directory.CreateDirectory("Builds/Windows");
        BuildReport report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/VerdantValley.unity"},locationPathName="Builds/Windows/VerdantValley.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        Debug.Log("HW5_BUILD_RESULT "+report.summary.result);
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Windows build failed");
    }
}

