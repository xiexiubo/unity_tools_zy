using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;

[InitializeOnLoad]
public static class ZyParticleSceneToolsWindow
{
    public static bool IsShowPanel = true;
    [MenuItem("CommonTools/��Ч/������Ч���")]
    static public void ShowPanel()
    {
        IsShowPanel = !IsShowPanel;
        if (IsShowPanel) 
        {
            Debug.Log("��<color=#00FF00>��</color>������Ч��壬ѡ�е�objs����Ч�ͻ���ʾ��");
        }
        else 
        {
            Debug.Log("��<color=#FF0000>�ر�</color>������Ч��壡");
        }
    }
    static ZyParticleSceneToolsWindow()
    {
        Debug.Log($"Unity�༭��������ɣ���ʼִ�г�ʼ������...");
        // ע������ʱҪִ�еķ���
        SceneView.duringSceneGui += ZyParticleSceneToolsWindow.OnSceneGUI;
        Selection.selectionChanged += GetSelectedComps;

    }
    
    // ������Scene��ͼ�л��Ƶĺ��ķ���
    public static void OnSceneGUI(SceneView sv)
    {
        if (!IsShowPanel) 
        {
            return;
        }
        if (listparticles.Count == 0 && listAnimator.Count == 0) 
        {
            return;
        }

        //selectedParticleSystem.useAutoRandomSeed = false;
        // ��ʼ��SceneView�л���GUI����
        Handles.BeginGUI();

        // ����һ����Scene��ͼ���Ͻǵľ�������
        GUILayout.BeginArea(new Rect(sv.position.width/2-125, 0, 260, 180));
        {
            // ����һ����΢͸���ı�����
            GUI.color = new Color(1, 1, 1, 0.8f); // 80% ��͸����
            GUILayout.BeginVertical("Box");
            GUI.color = Color.white; // ������ɫ         

            GUILayout.BeginHorizontal();
            //if(listparticles.Count>0)
            //GUILayout.Label(listparticles[0].gameObject.name, EditorStyles.miniLabel);
            EditorGUIUtility.labelWidth = 50;
            pdelay = EditorGUILayout.FloatField("��Ч�ӳ�", pdelay,GUILayout.Width(80));
            if (GUILayout.Button(!isPlaying?"����":"��ͣ")) 
            {
                isPlaying = !isPlaying;
                tTemp = Time.realtimeSinceStartup;
            }
            timeScale = EditorGUILayout.FloatField("ʱ������", timeScale, GUILayout.Width(80));
            //if (listAnimator.Count > 0)
            //    GUILayout.Label(listAnimator[0].gameObject.name, EditorStyles.miniLabel);
            if (listAnimator.Count > 0)
                clipName = EditorGUILayout.TextField(clipName,GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            t = GUILayout.HorizontalSlider(t, 0, duration, GUILayout.Width(200));
            t = EditorGUILayout.FloatField((float)Math.Round(t, 2), new GUIStyle { fontSize = 10, contentOffset = new Vector2(0, 1), normal = new GUIStyleState() { textColor = Color.white } }, GUILayout.Width(20));
            //t = (float)Math.Round(t, 2);
            if (isPlaying)
            {
                t = Time.realtimeSinceStartup - tTemp;
                t = timeScale * t;
                if (t > duration) 
                {
                    tTemp = Time.realtimeSinceStartup;
                    t = 0;
                }
            }
            GUILayout.Label($"/", EditorStyles.miniLabel,GUILayout.Width(10));
            duration = EditorGUILayout.FloatField(duration,new GUIStyle { fontSize=10,contentOffset =new Vector2(0,1),normal=new GUIStyleState() {textColor=Color.white } },GUILayout.Width(20));
            GUILayout.Label($"��", EditorStyles.miniLabel);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        GUILayout.EndArea();

        Handles.EndGUI();

        testPlay();
    }
    static public void testPlay()
    {
        for (int i = 0; i < listparticles.Count; i++)
            listparticles[i].Simulate(t- pdelay, false, true); // ģ�⵽Ŀ��֮֡ǰ��״̬       

         AnimationClip[] cs=null;
        for (int i = 0; i < listAnimator.Count; i++) 
        {
            var animator = listAnimator[i];
            cs = animator.runtimeAnimatorController.animationClips;
            var animClipNames = cs.Select((c) => c.name).ToArray();
            int index = -1;
            for (int y = 0; y < cs.Length; y++)
            {
                if (index == -1 && cs[y].name == clipName)
                {
                    index = y;
                    cs[index].SampleAnimation(animator.gameObject, t);
                    break;
                }
            }
            if (index == -1 && cs.Length > 0)
            {
                cs[0].SampleAnimation(animator.gameObject, t);
            }

        }
        //Time.timeScale = 1f; // ��������ʱ������

    }
    static public float t = 0; // Ŀ��֡������λ���룩
    static private float tTemp = -1f; // Ŀ��֡������λ���룩

    static private float duration = 2f;
    static private float pdelay = 0f;
    static private float timeScale = 1f;
    static private string clipName = "skill01";

    static private bool isPlaying = false;



    static List<ParticleSystem> listparticles = new List<ParticleSystem>();
    static List<Animator> listAnimator = new List<Animator>();
    private static void GetSelectedComps()
    {
        GameObject[] gos = Selection.gameObjects;
        if (gos.Length <= 0) return;
        listparticles.Clear();
        listAnimator.Clear();
        for (int i = 0; i < gos.Length; i++)
        {
            var go = gos[i];
            if (go == null) continue;
            //bool isSceneObject = go.scene.isLoaded && !string.IsNullOrEmpty(go.scene.name);


            //Undo.RecordObject(ps, "Change Emission Rate (Scene)");
            // ��һ���жϷ�ʽ����������Ƿ��ǳ����еĸ������������
            bool isSceneObject = go.transform.root.gameObject.scene.isLoaded;
            if (!isSceneObject) continue;
            //EditorUtility.SetDirty(ps);

            var ps = go.GetComponentsInChildren<ParticleSystem>();
            listparticles.AddRange(ps);
            var animators = go.GetComponentsInChildren<Animator>();
            listAnimator.AddRange(animators);
        }
        //ȥ�����
        for (int i = 0;i < listparticles.Count;i++) 
        {
            var p = listparticles[i];
            if (!p) continue;
            p.Stop();
            p.Clear();
        }
        for (int i = 0; i < listparticles.Count; i++)
        {
            var p = listparticles[i];
            if (!p || p.isPlaying) continue;
            Undo.RecordObject(p, "Change Emission Rate (Scene)");
            p.useAutoRandomSeed = false;
            EditorUtility.SetDirty(p);
        }

    }
}