using UnityEngine;
using System;

#if UNITY_PS4
using UnityEngine.PS4;
#endif

public class GamePad : MonoBehaviour
{
	// Custom class for holding all the gamepad sprites
	[Serializable]
	public class PS4GamePad
	{
		public SpriteRenderer thumbstickLeft;
		public SpriteRenderer thumbstickRight;

		public SpriteRenderer cross;
		public SpriteRenderer circle;
		public SpriteRenderer triangle;
		public SpriteRenderer square;

		public SpriteRenderer dpadDown;
		public SpriteRenderer dpadRight;
		public SpriteRenderer dpadUp;
		public SpriteRenderer dpadLeft;

		public SpriteRenderer L1;
		public SpriteRenderer L2;
		public SpriteRenderer R1;
		public SpriteRenderer R2;

		public SpriteRenderer lightbar;
		public SpriteRenderer options;
		public SpriteRenderer speaker;
		public SpriteRenderer touchpad;
		public Transform gyro;
		public TextMesh text;
		public Light light;
	}
	public PS4GamePad gamePad;

	public int playerId = -1;
	public Transform[] touches;
	public Color inputOn = Color.white;
	public Color inputOff = Color.grey;

#if UNITY_PS4
    int m_StickId;
    Color m_LightbarColour;
    bool m_HasSetupGamepad;
    PS4Input.LoggedInUser m_LoggedInUser;
	PS4Input.ConnectionType m_ConnectionType;

	// Touchpad variables
    int m_TouchNum, m_Touch0X, m_Touch0Y, m_Touch0Id, m_Touch1X, m_Touch1Y, m_Touch1Id;
    int m_TouchResolutionX, m_TouchResolutionY, m_AnalogDeadZoneLeft, m_AnalogDeadZoneRight;
    float m_TouchPixelDensity;

	// Volume sampling variables
    const int k_QSamples = 1024; // array size
    float m_RmsValue; // sound level - RMS
    float[] m_Samples = new float[1024]; // audio samples

	void Start()
	{
		// Stick ID is the player ID + 1
		m_StickId = playerId + 1;

		ToggleGamePad(false);
	}

	void Update()
    {
		if(PS4Input.PadIsConnected(playerId))
		{
			// Set the gamepad to the start values for the player
			if(!m_HasSetupGamepad)
				ToggleGamePad(true);

			// Handle each part individually
			Touchpad();
			Thumbsticks();
			InputButtons();
			DPadButtons();
			TriggerShoulderButtons();
			Lightbar();
			Speaker();

			// Options button is on its own, so we'll do it here
			if(Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button7", true)))
			{
				
			
				gamePad.options.color = inputOn;

				// Reset the gyro orientation and lightbar to default
				PS4Input.PadResetOrientation(playerId);
				PS4Input.PadResetLightBar(playerId);
				m_LightbarColour = GetPlayerColor(PS4Input.GetUsersDetails(playerId).color);
			}
			else
				gamePad.options.color = inputOff;

			// Make the gyro rotate to match the physical controller
			gamePad.gyro.localEulerAngles = new Vector3(-PS4Input.PadGetLastOrientation(playerId).x,
			                                            -PS4Input.PadGetLastOrientation(playerId).y,
			                                            PS4Input.PadGetLastOrientation(playerId).z) * 100;
														
			// rebuild the username everyframe, in case it's changed due to PSN access
			gamePad.text.text = PS4Input.RefreshUsersDetails(playerId).userName + "\n(" + m_ConnectionType + ")";
														
		}
		else if(m_HasSetupGamepad)
			ToggleGamePad(false);
	}

	// Toggle the gamepad between connected and disconnected states
	void ToggleGamePad(bool active)
	{
		if(active)
		{
			// Set the lightbar colour to the start/default value
			m_LightbarColour = GetPlayerColor(PS4Input.GetUsersDetails(playerId).color);

			// Set 3D Text to whoever's using the pad
			m_LoggedInUser = PS4Input.RefreshUsersDetails(playerId);
			gamePad.text.text = m_LoggedInUser.userName + "\n(" + m_ConnectionType + ")";

			// Reset and show the gyro
			gamePad.gyro.localRotation = Quaternion.identity;
			gamePad.gyro.gameObject.SetActive(true);

			m_HasSetupGamepad = true;
		}
		else
		{
			// Hide the touches
			touches[0].gameObject.SetActive(false);
			touches[1].gameObject.SetActive(false);
			
			// Set the lightbar to a default colour
			m_LightbarColour = Color.gray;
			gamePad.lightbar.color = m_LightbarColour;
			gamePad.light.color = Color.black;

			// Set the 3D Text to show the pad is disconnected
			gamePad.text.text = "Disconnected";

			// Hide the gyro
			gamePad.gyro.gameObject.SetActive(false);
			
			m_HasSetupGamepad = false;
		}
	}
	
	void Touchpad()
	{
		PS4Input.GetPadControllerInformation(playerId, out m_TouchPixelDensity, out m_TouchResolutionX, out m_TouchResolutionY, out m_AnalogDeadZoneLeft, out m_AnalogDeadZoneRight, out m_ConnectionType);
		PS4Input.GetLastTouchData(playerId, out m_TouchNum, out m_Touch0X, out m_Touch0Y, out m_Touch0Id, out m_Touch1X, out m_Touch1Y, out m_Touch1Id);

		// Show and move around up to 2 touch inputs
		if (m_TouchNum > 0)
		{
			float xPos;
			float yPos;

			// Touch 1
			if (m_Touch0X > 0 || m_Touch0Y > 0)
			{
				if (!touches[0].gameObject.activeSelf)
					touches[0].gameObject.SetActive(true);

				xPos = (3.57f / m_TouchResolutionX) * m_Touch0X;
				yPos = (1.35f / m_TouchResolutionY) * m_Touch0Y;

				touches[0].localPosition = new Vector3(xPos, -yPos, 1);
			}
			else if (touches[0].gameObject.activeSelf)
				touches[0].gameObject.SetActive(false);

			//Touch 2
			if (m_TouchNum > 1 && (m_Touch1X > 0 || m_Touch1Y > 0))
			{
				if (!touches[1].gameObject.activeSelf)
					touches[1].gameObject.SetActive(true);

				xPos = (3.57f / m_TouchResolutionX) * m_Touch1X;
				yPos = (1.35f / m_TouchResolutionY) * m_Touch1Y;

				touches[1].localPosition = new Vector3(xPos, -yPos, 1);
			}
			else if (touches[1].gameObject.activeSelf)
				touches[1].gameObject.SetActive(false);
		}
		else if (touches[0].gameObject.activeSelf || touches[1].gameObject.activeSelf)
		{
			touches[0].gameObject.SetActive(false);
			touches[1].gameObject.SetActive(false);
		}

		// Make the touchpad light up and play audio if it's pushed down
		if(Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button6", true)))
		{
			gamePad.touchpad.color = inputOn;
			TouchpadAudio(m_TouchResolutionX, m_TouchResolutionY, m_Touch0X, m_Touch0Y);
		}
		else
		{
			gamePad.touchpad.color = inputOff;
			GetComponent<AudioSource>().Stop();
		}
	}

	// Change the pitch and volume of an audio source, via the inputs of 
	// the touchpad, and play it through the controller speaker
	void TouchpadAudio(int maxX, int maxY, int posX, int posY)
	{
	    var touchInput = new Rect { width = maxX, height = maxY, x = posX, y = posY };

	    var xMod = touchInput.x / touchInput.width;
		var yMod = touchInput.y / touchInput.height;

		GetComponent<AudioSource>().pitch = xMod + 0.5f;
		GetComponent<AudioSource>().volume = 1f - yMod;

		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().PlayOnDualShock4(m_LoggedInUser.userId);
	}

	void Thumbsticks()
	{
		// Move the thumbsticks around
		gamePad.thumbstickLeft.transform.localPosition = new Vector3(0.4f*Input.GetAxis("leftstick" + m_StickId + "horizontal"),
		                                                              -0.4f*Input.GetAxis("leftstick" + m_StickId + "vertical"),
		                                                              0);

		gamePad.thumbstickRight.transform.localPosition = new Vector3(0.4f*Input.GetAxis("rightstick" + m_StickId + "horizontal"),
		                                                               -0.4f*Input.GetAxis("rightstick" + m_StickId + "vertical"),
		                                                               0);

		// Make the thumbsticks light up when pressed
		gamePad.thumbstickLeft.color = Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button8", true)) ? inputOn : inputOff;
		gamePad.thumbstickRight.color = Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button9", true)) ? inputOn : inputOff;
	}

	// Make the Cross, Circle, Triangle and Square buttons light up when pressed
	void InputButtons()
	{
		gamePad.cross.color = Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button0", true)) ? inputOn : inputOff;
		gamePad.circle.color = Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button1", true)) ? inputOn : inputOff;
		gamePad.square.color = Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button2", true)) ? inputOn : inputOff;
		gamePad.triangle.color = Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button3", true)) ? inputOn : inputOff;
	}

	// Make the DPad directions light up when pressed
	void DPadButtons()
	{
		gamePad.dpadRight.color = Input.GetAxis("dpad" + m_StickId + "_horizontal") > 0 ? inputOn : inputOff;
		gamePad.dpadLeft.color = Input.GetAxis("dpad" + m_StickId + "_horizontal") < 0 ? inputOn : inputOff;
		gamePad.dpadUp.color = Input.GetAxis("dpad" + m_StickId + "_vertical") > 0 ? inputOn : inputOff;
		gamePad.dpadDown.color = Input.GetAxis("dpad" + m_StickId + "_vertical") < 0 ? inputOn : inputOff;
	}
	
	void TriggerShoulderButtons()
	{
		// Make the triggers light up based on how "pulled" they are
		if(Math.Abs(Input.GetAxis("joystick" + m_StickId + "_left_trigger")) > 0.001f)
			gamePad.L2.color = inputOff+(inputOn*Input.GetAxis("joystick" + m_StickId + "_left_trigger"));
		else
			gamePad.L2.color = inputOff;
		
		if(Math.Abs(Input.GetAxis("joystick" + m_StickId + "_right_trigger")) > 0.001f)
			gamePad.R2.color = inputOff+(inputOn*-Input.GetAxis("joystick" + m_StickId + "_right_trigger"));
		else
			gamePad.R2.color = inputOff;

		// Make the shoulders light up when pressed
		gamePad.L1.color = Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button4", true)) ? inputOn : inputOff;
		gamePad.R1.color = Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button5", true)) ? inputOn : inputOff;
	}

	void Lightbar()
	{
		// Make the lightbar change colour when we hold down buttons
		if(Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button0", true)))
			m_LightbarColour = Color.Lerp (m_LightbarColour, Color.blue, Time.deltaTime * 4f);

		if(Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button1", true)))
			m_LightbarColour = Color.Lerp (m_LightbarColour, Color.red, Time.deltaTime * 4f);

		if(Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button2", true)))
			m_LightbarColour = Color.Lerp (m_LightbarColour, Color.magenta, Time.deltaTime * 4f);

		if(Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_StickId + "Button3", true)))
			m_LightbarColour = Color.Lerp (m_LightbarColour, Color.green, Time.deltaTime * 4f);

		// Set the lightbar sprite and the physical lightbar change to the current colour
		gamePad.lightbar.color = m_LightbarColour;
		gamePad.light.color = m_LightbarColour;
		PS4Input.PadSetLightBar(playerId,
		                        Mathf.RoundToInt(m_LightbarColour.r * 255),
		                        Mathf.RoundToInt(m_LightbarColour.g * 255),
		                        Mathf.RoundToInt(m_LightbarColour.b * 255));
	}

	// Get the volume being played in-game, and make the speaker light up based on the volume
	void Speaker()
	{
		GetVolume();
		gamePad.speaker.color = (Color.white * m_RmsValue) + (Color.white * 0.25f);
	}

	// Get a usable Color from an int
    static Color GetPlayerColor(int colorId)
	{
		switch (colorId)
		{
		case 0:
			return Color.blue;
		case 1:
			return Color.red;
		case 2:
			return Color.green;
		case 3:
			return Color.magenta;
		default:
			return Color.black;
		}
	}

	//Get the volume from an attached audio source component
	void GetVolume()
	{
		if(GetComponent<AudioSource>().time>0f)
		{
			GetComponent<AudioSource>().GetOutputData(m_Samples, 0); // fill array with samples
			int i;
			var sum = 0f;
			
			for(i=0; i < k_QSamples; i++)
				sum += m_Samples[i] * m_Samples[i]; // sum squared samples
			
			m_RmsValue = Mathf.Sqrt(sum/k_QSamples); // rms = square root of average

			m_RmsValue *= GetComponent<AudioSource>().volume;
		}
		else
			m_RmsValue = 0f;
	}
#endif
}
