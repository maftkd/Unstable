using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class Ship : EditorWindow
{
	[MenuItem("Build/Ship")]
	public static void SendIt(){
		Debug.Log("shipping");
		//determine build path
		string buildPath=Application.dataPath;
		buildPath = Directory.GetParent(buildPath).FullName.Replace('\\', '/') + "/buildPath/HTML";
		if(!Directory.Exists(buildPath)){
			Directory.CreateDirectory(buildPath);
		}
		BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath,BuildTarget.WebGL, BuildOptions.None);
		string bat = "cd Butler\nbutler push "+buildPath+" rithmgaming/changes:HTML5";

        string batPath = Directory.GetParent(Application.dataPath).FullName.Replace('\\','/')+"/Butler/ship.bat";
        File.WriteAllText(batPath, bat);

		//before shipping lets change out the css stuff that unity gives by default
		//
		string indexPath = buildPath+"/index.html";
		string [] lines = File.ReadAllLines(indexPath);
		string html = "";
		foreach(string s in lines)
		{
			Debug.Log(s);
			if(s.Contains("margin"))
				html+="<div id=\"unityContainer\" style=\"width: 100%; height: 100%; position:fixed;left:50%;top:50%; transform:translate(-50%,-50%);\"></div>";
			else
				html+=s;
		}
		//<div id="unityContainer" style="width: 100%; height: 100%; position:fixed;left:50%;top:50%; transform:translate(-50%,-50%);"></div>
		File.WriteAllText(indexPath,html);

        System.Diagnostics.Process.Start("cmd.exe", "/k " + batPath);
	}

}
