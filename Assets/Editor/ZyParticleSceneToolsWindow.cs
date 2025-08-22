using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;

[InitializeOnLoad]
public static class ZyParticleSceneToolsWindow
{
    public static bool IsShowPanel = true;
    [MenuItem("CommonTools/特效/场景特效面板")]
    static public void ShowPanel()
    {
        IsShowPanel = !IsShowPanel;
        if (IsShowPanel) 
        {
            Debug.Log("已<color=#00FF00>打开</color>场景特效面板，选中的objs有特效就会显示！");
        }
        else 
        {
            Debug.Log("已<color=#FF0000>关闭</color>场景特效面板！");
        }
    }
    static ZyParticleSceneToolsWindow()
    {
        Debug.Log($"Unity编辑器启动完成，开始执行初始化操作...");
        // 注册启动时要执行的方法
        SceneView.duringSceneGui += ZyParticleSceneToolsWindow.OnSceneGUI;
        Selection.selectionChanged += GetSelectedComps;

    }
    
    // 这是在Scene视图中绘制的核心方法
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
        // 开始在SceneView中绘制GUI区域
        Handles.BeginGUI();

        // 定义一个在Scene视图左上角的矩形区域
        GUILayout.BeginArea(new Rect(sv.position.width/2-125, 0, 260, 180));
        {
            // 绘制一个稍微透明的背景框
            GUI.color = new Color(1, 1, 1, 0.8f); // 80% 不透明度
            GUILayout.BeginVertical("Box");
            GUI.color = Color.white; // 重置颜色         

            GUILayout.BeginHorizontal();
            //if(listparticles.Count>0)
            //GUILayout.Label(listparticles[0].gameObject.name, EditorStyles.miniLabel);
            EditorGUIUtility.labelWidth = 50;
            pdelay = EditorGUILayout.FloatField("特效延迟", pdelay,GUILayout.Width(80));
            if (GUILayout.Button(!isPlaying?"播放":"暂停")) 
            {
                isPlaying = !isPlaying;
                tTemp = Time.realtimeSinceStartup;
            }
            timeScale = EditorGUILayout.FloatField("时间流速", timeScale, GUILayout.Width(80));
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
            GUILayout.Label($"秒", EditorStyles.miniLabel);
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
            listparticles[i].Simulate(t- pdelay, false, true); // 模拟到目标帧之前的状态       

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
        //Time.timeScale = 1f; // 重新启用时间流逝

    }
    static public float t = 0; // 目标帧数（单位：秒）
    static private float tTemp = -1f; // 目标帧数（单位：秒）

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
            // 另一种判断方式：检查物体是否是场景中的根物体或子物体
            bool isSceneObject = go.transform.root.gameObject.scene.isLoaded;
            if (!isSceneObject) continue;
            //EditorUtility.SetDirty(ps);

            var ps = go.GetComponentsInChildren<ParticleSystem>();
            listparticles.AddRange(ps);
            var animators = go.GetComponentsInChildren<Animator>();
            listAnimator.AddRange(animators);
        }
        //去掉随机
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