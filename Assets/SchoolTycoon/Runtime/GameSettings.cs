using System;
using UnityEngine;
namespace KoSch.SchoolTycoon
{
    [Serializable] public sealed class GameSettings
    {
        public string Language="de";public float Music=.45f,Effects=.7f,CameraSensitivity=1;public int Graphics=1,FrameRate=30;public bool ReducedMotion,Labels=true,Autosave=true;public float UIScale=1;
        public static GameSettings Load()
        {
            GameSettings s=null;try{s=JsonUtility.FromJson<GameSettings>(PlayerPrefs.GetString("rektor-settings","") );}catch{ }
            if(s==null)s=new GameSettings{Language=Application.systemLanguage==SystemLanguage.German?"de":"en",Graphics=Application.isMobilePlatform?1:2,FrameRate=Application.isMobilePlatform?30:60};s.Normalize();return s;
        }
        public void Normalize(){if(Language!="de"&&Language!="en")Language="de";Music=Mathf.Clamp01(Music);Effects=Mathf.Clamp01(Effects);CameraSensitivity=Mathf.Clamp(CameraSensitivity,.5f,2);Graphics=Mathf.Clamp(Graphics,0,2);FrameRate=FrameRate==60?60:30;UIScale=Mathf.Clamp(UIScale,.9f,1.25f);}
        public void Save(){Normalize();PlayerPrefs.SetString("rektor-settings",JsonUtility.ToJson(this));PlayerPrefs.Save();}
        public void Apply(SchoolApp app)
        {
            Normalize();Application.targetFrameRate=FrameRate;QualitySettings.antiAliasing=Graphics==0?0:Graphics==1?2:4;QualitySettings.shadows=Graphics==0?ShadowQuality.Disable:ShadowQuality.All;QualitySettings.shadowDistance=Graphics==2?85:50;
            if(app.CameraRig!=null){CampusPost post=app.CameraRig.GetComponent<CampusPost>();if(post!=null)post.enabled=Graphics==2;}
            Shader.SetGlobalFloat("_RektorMotion",ReducedMotion?0:1);app.WorldLabels=Labels;if(app.Sound!=null)app.Sound.Apply();if(app.Campus!=null)app.Campus.SetViewFloor();
        }
    }
    public sealed class CampusAudio : MonoBehaviour
    {
        private SchoolApp app;private AudioSource music,effects;private AudioClip click;
        public void Initialize(SchoolApp owner)
        {
            app=owner;music=gameObject.AddComponent<AudioSource>();effects=gameObject.AddComponent<AudioSource>();music.loop=true;music.playOnAwake=false;effects.playOnAwake=false;const int rate=22050,seconds=16;float[] samples=new float[rate*seconds];float[][] notes={new[]{130.81f,164.81f,196f},new[]{110f,130.81f,164.81f},new[]{87.31f,130.81f,174.61f},new[]{98f,146.83f,196f}};
            for(int i=0;i<samples.Length;i++){float t=i/(float)rate,phase=t%4;int chord=(int)(t/4);float env=Mathf.Sin(Mathf.PI*phase/4);foreach(float n in notes[chord])samples[i]+=.034f*env*(Mathf.Sin(2*Mathf.PI*n*t)+.25f*Mathf.Sin(4*Mathf.PI*n*t));}
            music.clip=AudioClip.Create("Rektor quiet campus",samples.Length,1,rate,false);music.clip.SetData(samples,0);
            float[] taps=new float[rate/8];for(int i=0;i<taps.Length;i++){float t=i/(float)rate;taps[i]=Mathf.Sin(2*Mathf.PI*660*t)*Mathf.Exp(-t*35)*.16f;}
            click=AudioClip.Create("Soft menu note",taps.Length,1,rate,false);click.SetData(taps,0);Apply();music.Play();
        }
        public void Apply(){if(music!=null)music.volume=app.Settings.Music;if(effects!=null)effects.volume=app.Settings.Effects;}
        public void Click(){if(effects!=null&&click!=null)effects.PlayOneShot(click);}
        private void OnDestroy(){if(music!=null&&music.clip!=null)Destroy(music.clip);if(click!=null)Destroy(click);}
    }
}
