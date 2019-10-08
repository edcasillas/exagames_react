﻿
using Enums;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayScene : MonoBehaviour {

	[SerializeField] private Button _playButton;
	[SerializeField] private Scenes _sceneName;

	private void Start()
	{
		_playButton.onClick.AddListener(() => LoadScene(_sceneName));
	}

	public void LoadScene(Scenes sceneName)
	{
		SceneManager.LoadScene(sceneName.ToString());
	}

}
