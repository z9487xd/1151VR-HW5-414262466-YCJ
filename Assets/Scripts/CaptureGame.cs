using UnityEngine;
using System.Collections;
using System.IO;

// CH06: generator -> Instantiate -> Rigidbody.AddForce -> OnCollisionEnter -> particles.
public class CaptureGame : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform pokemon, trainerArm, heldBall;
    public Material particleMaterial;
    public Camera sceneCamera;
    public enum Phase { Ready, Flying, Absorbing, Shaking, Captured, Missed }
    public Phase phase;
    public int attempts, catches;
    public Vector3 launchPoint = new Vector3(-.73f, 1.52f, -5.15f);
    Vector3 pokemonOrigin;
    GameObject activeBall;
    LineRenderer guide;
    AudioSource sound;
    float phaseTime;
    string captureDir;
    bool demo, recording;
    int frame;
    Coroutine captureRoutine;
    Color ink = new Color(.055f,.14f,.18f), mint = new Color(.65f,.96f,.69f);

    void Start()
    {
        Application.targetFrameRate = 60;
        pokemonOrigin = pokemon.position;
        sound = gameObject.AddComponent<AudioSource>();
        guide = new GameObject("Aim trajectory").AddComponent<LineRenderer>();
        guide.material = particleMaterial; guide.startColor = new Color(1,1,.75f,.55f); guide.endColor = new Color(1,1,.75f,.15f);
        guide.startWidth = .012f; guide.endWidth = .005f; guide.positionCount = 35;
        string[] args = System.Environment.GetCommandLineArgs();
        for(int i=0;i<args.Length;i++) {
            if(args[i]=="-demo") demo=true;
            if(args[i]=="-record") recording=true;
            if(args[i]=="-captureDir" && i+1<args.Length) captureDir=args[++i];
        }
        if(captureDir!=null) Directory.CreateDirectory(captureDir);
        if(recording) { Time.captureFramerate=30; StartCoroutine(Record()); }
        if(demo) StartCoroutine(Demo());
        if(System.Array.IndexOf(args,"-selfTest")>=0) { demo=true;StartCoroutine(SelfTest()); }
        Debug.Log("HW5_READY: scene loaded; mouse / Space / R / D supported.");
    }
    Vector3 Aim() { return AimAt(Input.mousePosition); }
    Vector3 AimAt(Vector3 screenPosition)
    {
        Ray ray=sceneCamera.ScreenPointToRay(screenPosition);
        if(Physics.Raycast(ray,out RaycastHit hit,100) && hit.collider.GetComponentInParent<CaptureTarget>()) return pokemonOrigin+Vector3.up*1.25f;
        Plane plane=new Plane(Vector3.forward,pokemonOrigin);
        if(plane.Raycast(ray,out float enter)) { Vector3 p=ray.GetPoint(enter); return new Vector3(Mathf.Clamp(p.x,-1,9),Mathf.Clamp(p.y,.3f,5),pokemonOrigin.z); }
        return pokemonOrigin+Vector3.up*1.25f;
    }
    Vector3 Velocity(Vector3 target) { float duration=1.12f; return (target-launchPoint-.5f*Physics.gravity*duration*duration)/duration; }
    void Update()
    {
        phaseTime+=Time.deltaTime;
        if(Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if(Input.GetKeyDown(KeyCode.R)) { StopAllCoroutines(); demo=false; ResetRound(); }
        if(Input.GetKeyDown(KeyCode.D) && !demo) { ResetRound(); demo=true; StartCoroutine(Demo()); }
        if(!demo && phase==Phase.Ready) {
            if(Input.GetKeyDown(KeyCode.Space)) Throw(pokemonOrigin+Vector3.up*1.25f);
            else if(Input.GetMouseButtonDown(0) && Input.mousePosition.y>105*Screen.height/720f && Input.mousePosition.y<Screen.height-110*Screen.height/720f) Throw(Aim());
        }
        guide.enabled=phase==Phase.Ready;
        if(heldBall) heldBall.gameObject.SetActive(phase==Phase.Ready);
        if(guide.enabled) {
            Vector3 velocity=Velocity(demo?pokemonOrigin+Vector3.up*1.25f:Aim());
            for(int i=0;i<35;i++){float t=i/34f*1.12f;guide.SetPosition(i,launchPoint+velocity*t+.5f*Physics.gravity*t*t);}
            pokemon.position=pokemonOrigin+Vector3.up*(Mathf.Sin(Time.time*2.7f)*.045f);
        }
        if(phase==Phase.Flying && phaseTime>3.3f) Miss();
        if(phase==Phase.Missed && phaseTime>1.6f) ResetRound();
        if(trainerArm) trainerArm.localRotation=Quaternion.Euler(0,0,phase==Phase.Flying ? -60+Mathf.Min(phaseTime*90,70): -20);
    }
    public void Throw(Vector3 target)
    {
        if(phase!=Phase.Ready)return;
        attempts++; SetPhase(Phase.Flying);
        activeBall=Instantiate(ballPrefab,launchPoint,Quaternion.identity);
        BallController ball=activeBall.GetComponent<BallController>(); ball.game=this;
        Rigidbody body=activeBall.GetComponent<Rigidbody>();
        body.AddForce(Velocity(target),ForceMode.VelocityChange);
        body.angularVelocity=new Vector3(0,0,-8);
        Tone(660,.12f);
        Debug.Log("HW5_THROW: "+attempts+" target="+target);
    }
    public void Hit(BallController ball)
    {
        if(phase!=Phase.Flying)return;
        SetPhase(Phase.Absorbing);
        Debug.Log("HW5_COLLISION: capture target hit through OnCollisionEnter.");
        captureRoutine=StartCoroutine(Capture(ball));
    }
    public void Miss() { if(phase!=Phase.Flying)return;SetPhase(Phase.Missed);Debug.Log("HW5_MISS: retry enabled."); }
    IEnumerator Capture(BallController ball)
    {
        Rigidbody body=ball.GetComponent<Rigidbody>(); body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;body.isKinematic=true;
        ball.GetComponent<Collider>().enabled=false;
        Vector3 start=pokemon.position;
        Burst(start+Vector3.up*1.2f,new Color(.3f,.95f,1),85,2.8f);
        Tone(880,.25f);
        for(float t=0;t<1;t+=Time.deltaTime/.85f){pokemon.localScale=Vector3.one*(1-t);pokemon.position=Vector3.Lerp(start,ball.transform.position,t);yield return null;}
        pokemon.gameObject.SetActive(false);
        Vector3 ballStart=ball.transform.position, floor=new Vector3(pokemonOrigin.x,.36f,pokemonOrigin.z);
        for(float t=0;t<1;t+=Time.deltaTime/.5f){ball.transform.position=Vector3.Lerp(ballStart,floor,t);yield return null;}
        ball.transform.position=floor; SetPhase(Phase.Shaking);
        for(int i=0;i<3;i++) {
            Tone(440+i*110,.12f);
            for(float t=0;t<.7f;t+=Time.deltaTime){ball.transform.rotation=Quaternion.Euler(0,0,Mathf.Sin(t/.7f*Mathf.PI*2)*22);yield return null;}
            yield return new WaitForSeconds(.25f);
        }
        ball.transform.rotation=Quaternion.identity;catches++;SetPhase(Phase.Captured);
        Burst(floor+Vector3.up*.4f,new Color(1,.85f,.18f),120,3.5f);
        Tone(1046,.35f);
        Debug.Log("HW5_CAPTURED: success; attempts="+attempts+" catches="+catches);
    }
    void Burst(Vector3 pos,Color color,int count,float speed)
    {
        GameObject obj=new GameObject("Capture particles");obj.transform.position=pos;
        ParticleSystem ps=obj.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.loop=false;main.duration=.5f;main.startLifetime=1.4f;main.startSpeed=speed;main.startSize=.13f;main.startColor=color;main.gravityModifier=.25f;
        var emission=ps.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)count)});
        var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.35f;
        var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));
        ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=particleMaterial;ps.Play();Destroy(obj,2.5f);
    }
    void Tone(float frequency,float duration)
    {
        int n=(int)(44100*duration);float[] samples=new float[n];
        for(int i=0;i<n;i++)samples[i]=Mathf.Sin(2*Mathf.PI*frequency*i/44100)*.13f*(1-i/(float)n);
        AudioClip clip=AudioClip.Create("Capture tone",n,1,44100,false);clip.SetData(samples,0);sound.PlayOneShot(clip);Destroy(clip,duration+1);
    }
    void SetPhase(Phase value){phase=value;phaseTime=0;}
    public void ResetRound()
    {
        if(captureRoutine!=null) { StopCoroutine(captureRoutine);captureRoutine=null; }
        if(activeBall)Destroy(activeBall);
        pokemon.gameObject.SetActive(true);pokemon.position=pokemonOrigin;pokemon.localScale=Vector3.one;SetPhase(Phase.Ready);
    }
    IEnumerator Demo()
    {
        yield return new WaitForSeconds(1.5f);Shot("01-scene");
        yield return new WaitForSeconds(1);Throw(new Vector3(8,.4f,8));
        yield return new WaitForSeconds(.6f);Shot("02-throw");
        yield return new WaitForSeconds(5);if(phase!=Phase.Ready)ResetRound();
        Throw(pokemonOrigin+Vector3.up*1.25f);
        yield return new WaitForSeconds(1.25f);Shot("03-capture-effect");
        yield return new WaitForSeconds(4);Shot("04-success");
        yield return new WaitForSeconds(2.5f);
        if(recording) { Debug.Log("HW5_RECORD_COMPLETE frames="+frame);Application.Quit(); }
        demo=false;
    }
    void Shot(string name){if(captureDir!=null)ScreenCapture.CaptureScreenshot(Path.Combine(captureDir,name+".png"));}
    IEnumerator Record(){while(true){yield return new WaitForEndOfFrame();if(captureDir!=null)ScreenCapture.CaptureScreenshot(Path.Combine(captureDir,"frame-"+(frame++).ToString("D5")+".png"));}}
    IEnumerator SelfTest()
    {
        yield return null;
        Vector3 aim=AimAt(sceneCamera.WorldToScreenPoint(pokemonOrigin+Vector3.up*1.25f));
        if(Vector3.Distance(aim,pokemonOrigin+Vector3.up*1.25f)>.01f){Fail("ScreenPointToRay aim");yield break;}
        Throw(new Vector3(8,.4f,8));Throw(aim);
        if(attempts!=1){Fail("duplicate throw guard");yield break;}
        yield return new WaitForSeconds(5.5f);
        if(phase!=Phase.Ready||catches!=0){Fail("miss and automatic retry");yield break;}
        Throw(aim);yield return new WaitForSeconds(1.2f);
        if(phase!=Phase.Absorbing){Fail("physical target collision");yield break;}
        ResetRound();yield return new WaitForSeconds(4);
        if(phase!=Phase.Ready||!pokemon.gameObject.activeSelf||pokemon.localScale!=Vector3.one||catches!=0){Fail("reset during capture cancels coroutine");yield break;}
        Throw(aim);yield return new WaitForSeconds(6);
        if(phase!=Phase.Captured||catches!=1||pokemon.gameObject.activeSelf){Fail("complete capture");yield break;}
        ResetRound();Throw(aim);yield return new WaitForSeconds(6);
        if(phase!=Phase.Captured||catches!=2||attempts!=4){Fail("repeat round");yield break;}
        Debug.Log("HW5_TEST_PASS: screen ray aim, duplicate guard, miss/retry, collision, reset during absorption, success, repeat round.");
        Application.Quit(0);
    }
    void Fail(string reason){Debug.LogError("HW5_TEST_FAIL: "+reason);Application.Quit(1);}
    void Box(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
    void Text(Rect r,string t,int size,Color color,FontStyle style=FontStyle.Normal)
    { var s=new GUIStyle(GUI.skin.label){fontSize=size,fontStyle=style};s.normal.textColor=color;GUI.Label(r,t,s); }
    void OnGUI()
    {
        GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
        Box(new Rect(0,0,1280,94),ink);
        Text(new Rect(32,16,620,40),"POKÉMON  /  FIRST PERSON",29,Color.white,FontStyle.Bold);
        Text(new Rect(34,57,650,24),"VERDANT VALLEY     •     CH06 / FIRST-PERSON CAPTURE LAB",12,mint);
        Text(new Rect(920,20,330,26),"YCJ   /   414262466",18,Color.white,FontStyle.Bold);
        Text(new Rect(920,52,350,25),"THROWS  "+attempts.ToString("00")+"     CAUGHT  "+catches.ToString("00"),14,mint);
        Box(new Rect(32,119,232,71),new Color(.04f,.12f,.14f,.85f));
        Text(new Rect(48,129,210,22),"FIELD NOTE   /   025",12,mint);
        Text(new Rect(48,151,210,30),"Pikachu   ·   Electric",20,Color.white,FontStyle.Bold);
        string[] status={"Aim at Pikachu","Ball in flight","Energy transfer","Checking capture...","Gotcha! Pikachu caught.","Missed — try again"};
        string[] detail={"Click Pikachu to throw from your left hand.","Rigidbody + gravity drive the arc.","Collision triggers the capture particles.","Three shakes to confirm your catch.","Press R to return to the field.","Your next Poké Ball is on its way."};
        if(phase==Phase.Captured){Box(new Rect(425,120,430,80),ink);Text(new Rect(449,133,400,35),"CAPTURE COMPLETE",27,mint,FontStyle.Bold);Text(new Rect(449,169,400,22),"025   /   PIKACHU ADDED TO YOUR TEAM",12,Color.white);}
        Box(new Rect(0,606,1280,114),ink);
        Box(new Rect(32,627,4,64),mint);
        Text(new Rect(51,619,660,36),status[(int)phase],26,Color.white,FontStyle.Bold);
        Text(new Rect(52,661,660,27),detail[(int)phase],15,new Color(.75f,.84f,.83f));
        Text(new Rect(825,622,440,30),"CLICK  Aim & throw    SPACE  Quick throw",16,mint);
        Text(new Rect(825,655,430,28),"R  Reset     D  Demo     ESC  Exit",15,Color.white);
        if(demo)Text(new Rect(1080,111,200,26),"● DEMO MODE",14,ink,FontStyle.Bold);
    }
}

