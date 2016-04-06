﻿/******************************************************************************\
* Copyright (C) Leap Motion, Inc. All rights reserved.               *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.SceneManagement;
using Leap;

namespace Leap.Unity.VRVisualizer{
  public class VisualizerManager : MonoBehaviour {
    public GameObject m_PCVisualizer = null;
    public GameObject m_VRVisualizer = null;
    public UnityEngine.UI.Text m_warningText;
    public UnityEngine.UI.Text m_trackingText;
    public UnityEngine.UI.Text m_frameRateText;
    public UnityEngine.UI.Text m_dataFrameRateText;

    public KeyCode keyToToggleHMD = KeyCode.V;
  
    private Controller m_controller = null;
    private bool m_leapConnected = false;
    private float m_deltaTime = 0.0f;
    private int m_framrateUpdateCount = 0;
    private int m_framerateUpdateInterval = 30;
    private void FindController()
    {
      LeapServiceProvider provider = FindObjectOfType<LeapServiceProvider>();
      if (provider != null)
        m_controller = provider.GetLeapController();
    }
  
    void Awake()
    {
      Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, false);
      if (VRDevice.isPresent)
      {
        m_PCVisualizer.gameObject.SetActive(false);
        m_VRVisualizer.gameObject.SetActive(true);
        m_warningText.text = "Please put on your head-mounted display";
      }
      else
      {
        m_VRVisualizer.gameObject.SetActive(false);
        m_PCVisualizer.gameObject.SetActive(true);
        m_warningText.text = "No head-mounted display detected. Orion performs best in a head-mounted display";
      }
    }
  
    void Start()
    {
      m_trackingText.text = "";
      FindController();
      if (m_controller != null)
        m_leapConnected = m_controller.IsConnected;
    }
  
    void Update()
    {
      if (m_controller == null)
      {
        FindController();
        return;
      }
  
      if (m_leapConnected == false && m_controller.IsConnected)
      {
        // HACK (wyu): LeapProvider should listen to events and update itself when Leap devices are connected/disconnected instead of having to reload the scene to reinitialize variables
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
      }
      m_leapConnected = m_controller.IsConnected;
      if (!m_leapConnected)
      {
        m_trackingText.text = "";
        return;
      }
  
      m_trackingText.text = "Tracking Mode: ";
      m_trackingText.text += (m_controller.IsPolicySet(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD)) ? "Head-Mounted" : "Desktop";


      // In Desktop Mode
      if (m_PCVisualizer.activeInHierarchy)
      {
        if (m_controller.IsPolicySet(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD))
        {
          m_trackingText.text += " (Press '" + keyToToggleHMD + "' to switch to desktop mode)";
          if (Input.GetKeyDown(keyToToggleHMD))
            m_controller.ClearPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
        }
        else
        {
          m_trackingText.text += " (Press '" + keyToToggleHMD + "' to switch to head-mounted mode)";
          if (Input.GetKeyDown(keyToToggleHMD))
            m_controller.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
        }
      }
      //update render frame display
      m_deltaTime += (Time.deltaTime - m_deltaTime) * 0.1f;
      if (m_framrateUpdateCount > m_framerateUpdateInterval) {
        updateRenderFrameRate();
        m_framrateUpdateCount = 0;
      }
      m_framrateUpdateCount++;
    }

    private void updateRenderFrameRate() {
      float msec = m_deltaTime * 1000.0f;
      float fps = 1.0f / m_deltaTime;
      string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
      m_frameRateText.text = "Render Time: " + text;
      m_dataFrameRateText.text = "Data Framerate: " + m_controller.Frame().CurrentFramesPerSecond;
    }
  }
}
