using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using VK.BikeLab.Segway;
using System;
using TMPro;

public class FanController : MonoBehaviour
{
    [SerializeField] private GameObject vehicle;
    [SerializeField] private TMP_Text debug_text;
    [SerializeField] private TMP_Text debug_text2;
    public bool debug = false;
    public bool debug2 = false;

    private float max_velocity = 3f;
    private float send_interval = 0.2f;
    private int fan_offset = 30;
    private float angular_velocity_th = 4.5f;

    [NonSerialized] public int turning = 0; // -1 is left, 1 is right
    [NonSerialized] public bool fan_enabled = false;

    private Segway segway;
    private float prev_rotation;
    private float timer;

    private UdpClient udpClient;
    private string ipAddress = "192.168.0.63"; // MasterVR
    private int port = 5052;

    void Start()
    {
        if (!debug)
        {
            // set debug text gameobject disabled. 
            if (debug_text != null)
                debug_text.gameObject.SetActive(false);

            if (vehicle == null)
            {
                Debug.LogError("[FanController] Vehicle reference is missing.");
                fan_enabled = false;
                return;
            }

            segway = vehicle.GetComponent<Segway>();
            if (segway == null)
            {
                Debug.LogError("[FanController] Segway component not found.");
                fan_enabled = false;
                return;
            }

            udpClient = new UdpClient();
            SendFanCommand(90, 0);

            prev_rotation = GetVehicleYaw();
        }
        else
        {
            // set debug text gameobject enabled.
            if (debug_text != null)
                debug_text.gameObject.SetActive(true);

            udpClient = new UdpClient();
            SendFanCommand(120, 1);
        }

    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < send_interval)
            return;

        float delta_time = timer;
        timer = 0f;

        if (fan_enabled)
        {
            float current_rotation = GetVehicleYaw();
            float rotation_difference = Mathf.DeltaAngle(prev_rotation, current_rotation);
            float angular_velocity = rotation_difference / delta_time;

            int current_angle = 90;

            if (angular_velocity > angular_velocity_th)
            {
                current_angle = 90 + fan_offset; // turn right
                turning = 1;
            }
            else if (angular_velocity < -angular_velocity_th)
            {
                current_angle = 90 - fan_offset; // turn left
                turning = -1;
            }
            else
            {
                turning = 0;
            }

            // calculate velocity
            float velocity = Mathf.Clamp(segway.getVelosity(), 0f, max_velocity);
            float normalised_speed = Mathf.InverseLerp(0f, max_velocity, velocity);
            normalised_speed = Mathf.Round(normalised_speed * 10f) / 10f;

            SendFanCommand(current_angle, normalised_speed);

            prev_rotation = current_rotation;
        }
    }

    public void DisableFan()
    {
        fan_enabled = false;
        SendFanCommand(90, 0);
    }

    private float GetVehicleYaw()
    {
        return vehicle.transform.eulerAngles.y;
    }

    private void SendFanCommand(int angle, float speed)
    {
        string message = $"{angle},{speed:F1}";
        byte[] data = Encoding.UTF8.GetBytes(message);

        try
        {
            udpClient.Send(data, data.Length, ipAddress, port);
            Debug.Log($"[FanController] Sent: {message}");

            if (debug2 && debug_text2 != null)
                debug_text2.text = $"Sent: {message}";

            // if debug on, set the text on debug message as well
            if (debug && debug_text != null)
                debug_text.text = $"[FanController] Sent: {message}";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FanController] UDP send failed: {e.Message}");

            if (debug2 && debug_text2 != null)
                debug_text2.text = $"failed: {e.Message}";

            // if debug on, set the text on debug message as well
            if (debug && debug_text != null)
                debug_text.text = $"[FanController] UDP send failed: {e.Message}";
        }
    }

    void OnApplicationQuit()
    {
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
    }
}