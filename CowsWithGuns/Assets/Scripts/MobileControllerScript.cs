using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileControllerScript : MonoBehaviour
{
    // Mobile controller graphics
    public Sprite joystickCircle;
    public Sprite joystickButton;
    public Sprite buttonImg;

    static Canvas mainCanvas;
    Vector2 resolution;
    public bool onMobile = false;

    // Data used by joysticks
    [System.Serializable]
    private class Joystick
    {
        // Key for joystickValues
        public string name;

        public Image backgroundCircle;
        public Image mainButton;
        public Rect defaultArea;
        public Vector2 centerPoint;
        public Vector2 touchOffset;
        public Vector2 currentTouchPos;
        public int touchID = -1;
        public bool isActive = false;
        public float movementRadius;
        public float deadzone;
    }
    // Data used by buttons
    [System.Serializable]
    private class TouchButton
    {
        // Key for touchButtonDown
        public string name;

        public Image buttonImage;
        public Rect buttonArea;
        public int touchID = -1;
        public bool isActive = false;
    }


    // Used to update joysticks
    List<Joystick> joysticks = new List<Joystick>();
    /// <summary>
    /// Holds a Vector2 for each joystick, in range of -1 to 1
    /// </summary>
    [HideInInspector] public Dictionary<string, Vector2> joystickValues = new Dictionary<string, Vector2>();

    // Used to update touch buttons
    List<TouchButton> buttons = new List<TouchButton>();
    /// <summary>
    /// Holds a bool for each touch button, set to true if it is down
    /// </summary>
    [HideInInspector] public Dictionary<string, bool> touchButtonDown = new Dictionary<string, bool>();



    // Simple alias
    /// <summary>
    /// Create a new joystick, accessable from 'joystickValues'
    /// </summary>
    /// <param name="name"> The name used as the joystick's key in 'joystickValues' </param>
    /// <param name="position"> The position of the outer circle relative to the bottom left corner </param>
    /// <param name="size"> The size of the outer circle </param>
    /// <param name="movementRadius"> How far from the center can the joystick move? </param>
    /// <param name="deadzone"> How far from the center does the button have to move to be regestered? </param>
    public void CreateNewJoystick(string name, Vector2 position, float size = 120f, float movementRadius = 75f, float deadzone = 19f)
    {
        CreateNewJoystick(name, position, Vector2.zero, Vector2.zero, Vector2.zero, size, movementRadius, deadzone);
    }

    /// <summary>
    /// Create a new joystick, accessable from 'joystickValues'
    /// </summary>
    /// <param name="name"> The name used as the joystick's key in 'joystickValues' </param>
    /// <param name="position"> The anchored position of the outer circle. This is relative to the pivot and anchor </param>
    /// <param name="pivot"> The outer circles pivot </param>
    /// <param name="anchorMin"> The outer circles min anchor </param>
    /// <param name="anchorMax"> The outer circles max anchor </param>
    /// <param name="size"> The size of the outer circle </param>
    /// <param name="movementRadius"> How far from the center can the joystick move? </param>
    /// <param name="deadzone"> How far from the center does the button have to move to be regestered? </param>
    public void CreateNewJoystick(string name, Vector2 position, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, float size = 120f, float movementRadius = 75f, float deadzone = 19f)
    {
        // Create a new joystick
        Joystick newJoystick = new Joystick
        {
            name = name,
            movementRadius = movementRadius,
            deadzone = deadzone
        };

        // Setup navigation background
        GameObject cntrlTmpObj = new GameObject("Movement Circle");
        cntrlTmpObj.transform.parent = mainCanvas.transform;
        cntrlTmpObj.transform.position = Vector3.zero;

        newJoystick.backgroundCircle = cntrlTmpObj.AddComponent<Image>();
        newJoystick.backgroundCircle.sprite = joystickCircle;
        newJoystick.backgroundCircle.type = Image.Type.Simple;
        newJoystick.backgroundCircle.useSpriteMesh = true;

        newJoystick.backgroundCircle.rectTransform.pivot = pivot;
        newJoystick.backgroundCircle.rectTransform.anchorMin = anchorMin;
        newJoystick.backgroundCircle.rectTransform.anchorMax = anchorMax;
        newJoystick.backgroundCircle.rectTransform.sizeDelta = new Vector2(size, size);
        newJoystick.backgroundCircle.rectTransform.anchoredPosition = position;

        // Navigation button
        cntrlTmpObj = new GameObject("Movement Button");
        cntrlTmpObj.transform.parent = mainCanvas.transform;
        cntrlTmpObj.transform.position = Vector3.zero;

        newJoystick.mainButton = cntrlTmpObj.AddComponent<Image>();
        newJoystick.mainButton.sprite = joystickButton;
        newJoystick.mainButton.type = Image.Type.Simple;
        newJoystick.mainButton.useSpriteMesh = true;

        newJoystick.mainButton.rectTransform.SetParent(newJoystick.backgroundCircle.transform);

        newJoystick.mainButton.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        newJoystick.mainButton.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        newJoystick.mainButton.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        newJoystick.mainButton.rectTransform.sizeDelta = new Vector2(size * 0.8f, size * 0.8f); // The button is 1/5 the size of the outer circle
        newJoystick.mainButton.rectTransform.anchoredPosition = Vector3.zero;


        // Save the default location of the joystick button in world space, using the botton left corner
        newJoystick.defaultArea = new Rect(
            newJoystick.mainButton.rectTransform.position.x - newJoystick.backgroundCircle.rectTransform.sizeDelta.x / 2.0f,
            newJoystick.mainButton.rectTransform.position.y - newJoystick.backgroundCircle.rectTransform.sizeDelta.y / 2.0f,
            newJoystick.mainButton.rectTransform.sizeDelta.x,
            newJoystick.mainButton.rectTransform.sizeDelta.y);
        // Save the default position in world space, using the center
        newJoystick.centerPoint = new Vector2(newJoystick.mainButton.rectTransform.position.x, newJoystick.mainButton.rectTransform.position.y);

        // Add the new joystick to the list
        joysticks.Add(newJoystick);
        joystickValues.Add(name, Vector2.zero);
    }


    // Simple alias
    /// <summary>
    /// Create a new touch button, accessable from 'touchButtonDown'
    /// </summary>
    /// <param name="name"> The name used as the button's key in 'touchButtonDown' </param>
    /// <param name="position"> The position of the button relative to the bottom left corner </param>
    public void CreateNewButton(string name, Vector2 position, float size = 100f)
    {
        CreateNewButton(name, position, Vector2.zero, Vector2.zero, Vector2.zero, size);
    }

    /// <summary>
    /// Create a new touch button, accessable from 'touchButtonDown'
    /// </summary>
    /// <param name="name"> The name used as the button's key in 'touchButtonDown' </param>
    /// <param name="position"> The anchored position of the button. This is relative to the pivot and anchor </param>
    public void CreateNewButton(string name, Vector2 position, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, float size = 100f)
    {
        TouchButton newButton = new TouchButton
        {
            name = name
        };

        // Setup button image
        GameObject cntrlTmpObj = new GameObject("Touch Button");
        cntrlTmpObj.transform.parent = mainCanvas.transform;
        cntrlTmpObj.transform.position = Vector3.zero;

        newButton.buttonImage = cntrlTmpObj.AddComponent<Image>();
        newButton.buttonImage.sprite = buttonImg;
        newButton.buttonImage.type = Image.Type.Simple;
        newButton.buttonImage.useSpriteMesh = true;

        newButton.buttonImage.rectTransform.pivot = pivot;
        newButton.buttonImage.rectTransform.anchorMin = anchorMin;
        newButton.buttonImage.rectTransform.anchorMax = anchorMax;
        newButton.buttonImage.rectTransform.sizeDelta = new Vector2(size, size);
        newButton.buttonImage.rectTransform.anchoredPosition = position;

        // Save the button area in world space
        newButton.buttonArea = new Rect(
            newButton.buttonImage.rectTransform.position.x - newButton.buttonImage.rectTransform.sizeDelta.x * pivot.x,
            newButton.buttonImage.rectTransform.position.y - newButton.buttonImage.rectTransform.sizeDelta.y * pivot.y,
            newButton.buttonImage.rectTransform.sizeDelta.x,
            newButton.buttonImage.rectTransform.sizeDelta.y);


        // Add the new button to the list
        buttons.Add(newButton);
        touchButtonDown.Add(name, false);
    }



    void Awake()
    {
        // If there is no canvas, create one
        if (mainCanvas == null)
        {
            // This function will initialize canvas element along with the joystick button
            GameObject tmpObj = new GameObject("Canvas");
            tmpObj.transform.position = Vector3.zero;
            mainCanvas = tmpObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.pixelPerfect = true;

            // Add Canvas Scaler component
            CanvasScaler canvasScaled = tmpObj.AddComponent<CanvasScaler>();
            canvasScaled.scaleFactor = 1;
            canvasScaled.referencePixelsPerUnit = 100;
            // Add Graphic Raycaster element
            tmpObj.AddComponent<GraphicRaycaster>();
        }

        // Get the current resolution so joysticks can be updated on changes
        resolution = new Vector2(Screen.width, Screen.height);


#if (UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1)
        onMobile = true;
#endif
    }

    void Update()
    {
        // Check for resolution changes
        if (resolution.x != Screen.width || resolution.y != Screen.height)
        {
            // Update each joystick's defaultArea and centerPoint
            foreach (Joystick joystick in joysticks)
            {
                joystick.defaultArea = new Rect(
                    joystick.mainButton.rectTransform.position.x - joystick.backgroundCircle.rectTransform.sizeDelta.x / 2.0f,
                    joystick.mainButton.rectTransform.position.y - joystick.backgroundCircle.rectTransform.sizeDelta.y / 2.0f,
                    joystick.mainButton.rectTransform.sizeDelta.x,
                    joystick.mainButton.rectTransform.sizeDelta.y );
                joystick.centerPoint = new Vector2(joystick.mainButton.rectTransform.position.x, joystick.mainButton.rectTransform.position.y);
            }

            // Update buttons. This might not be nessesary
            foreach (TouchButton button in buttons)
            {
                button.buttonArea = new Rect(
                    button.buttonImage.rectTransform.position.x - button.buttonImage.rectTransform.sizeDelta.x * button.buttonImage.rectTransform.pivot.x,
                    button.buttonImage.rectTransform.position.y - button.buttonImage.rectTransform.sizeDelta.y * button.buttonImage.rectTransform.pivot.y,
                    button.buttonImage.rectTransform.sizeDelta.x,
                    button.buttonImage.rectTransform.sizeDelta.y);
            }


            // Update resolution
            resolution.x = Screen.width;
            resolution.y = Screen.height;
        }


        // Update joysticks
        for (int i = 0; i < joysticks.Count; i++)
        {
            if (Application.isMobilePlatform)
            {
                // Mobile touch input
                for (var j = 0; j < Input.touchCount; j++)
                {
                    Touch touch = Input.GetTouch(j);

                    if (touch.phase == TouchPhase.Began)
                    {
                        MobileButtonsCheck(i, new Vector2(touch.position.x, touch.position.y), touch.fingerId);
                    }

                    if (touch.phase == TouchPhase.Moved)
                    {
                        if (joysticks[i].isActive && joysticks[i].touchID == touch.fingerId)
                        {
                            joysticks[i].currentTouchPos = touch.position;
                        }
                    }

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        MobileButtonStop(i, touch.fingerId);
                    }
                }
            }
            else
            {
                // Desktop mouse input for editor testing
                if (Input.GetMouseButtonDown(0))
                {
                    MobileButtonsCheck(i, new Vector2(Input.mousePosition.x, Input.mousePosition.y), -1);
                }
                if (Input.GetMouseButtonUp(0))
                {
                    MobileButtonStop(i, -1);
                }

                joysticks[i].currentTouchPos = Input.mousePosition;
            }


            // Moving
            if (joysticks[i].isActive)
            {
                // Clamp joystick position to movement radius
                Vector2 relitivePos = joysticks[i].currentTouchPos - joysticks[i].touchOffset - joysticks[i].centerPoint;
                if (relitivePos.magnitude > joysticks[i].movementRadius)
                    relitivePos = Vector3.ClampMagnitude(relitivePos, joysticks[i].movementRadius);
                joysticks[i].mainButton.rectTransform.position = joysticks[i].centerPoint + relitivePos;


                // Clamp joystick value to -1/1
                Vector2 newValue = relitivePos;
                if (Mathf.Abs(relitivePos.x) < joysticks[i].deadzone)
                    newValue.x = 0f;
                else
                    newValue.x = Mathf.Clamp(relitivePos.x / joysticks[i].movementRadius, -1.000f, 1.000f);
                if (Mathf.Abs(relitivePos.y) < joysticks[i].deadzone)
                    newValue.y = 0f;
                else
                    newValue.y = Mathf.Clamp(relitivePos.y / joysticks[i].movementRadius, -1.000f, 1.000f);

                joystickValues[joysticks[i].name] = newValue;
            }
            else
            {
                // Return to the center
                joysticks[i].mainButton.rectTransform.anchoredPosition = Vector3.zero;
                joystickValues[joysticks[i].name] = Vector2.zero;
            }
        }

        // Update buttons
        for (int i = 0; i < buttons.Count; i++)
        {
            if (Application.isMobilePlatform)
            {
                // Mobile touch input
                for (var j = 0; j < Input.touchCount; j++)
                {
                    Touch touch = Input.GetTouch(j);

                    if (touch.phase == TouchPhase.Began)
                    {
                        ActivateButton(i, touch.position, touch.fingerId);
                    }
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        DeactivateButton(i, touch.fingerId);
                    }
                }
            }
            else
            {
                // Desktop mouse input for editor testing
                if (Input.GetMouseButtonDown(0))
                {
                    ActivateButton(i, Input.mousePosition, -1);
                }
                if (Input.GetMouseButtonUp(0))
                {
                    DeactivateButton(i, -1);
                }
            }
        }
    }



    // Here we check if the clicked/tapped position is inside the joystick button
    void MobileButtonsCheck(int i, Vector2 touchPos, int touchID)
    {
        // Move controller
        if (joysticks[i].defaultArea.Contains(touchPos) && !joysticks[i].isActive)
        {
            joysticks[i].isActive = true;
            joysticks[i].touchOffset = touchPos - joysticks[i].centerPoint;
            joysticks[i].currentTouchPos = touchPos;
            joysticks[i].touchID = touchID;
        }
    }

    // Here we release the previously active joystick if we release the mouse button/finger from the screen
    void MobileButtonStop(int i, int touchID)
    {
        if (joysticks[i].isActive && joysticks[i].touchID == touchID)
        {
            joysticks[i].isActive = false;
            joysticks[i].touchOffset = Vector2.zero;
            joysticks[i].touchID = -1;
        }
    }


    void ActivateButton(int i, Vector2 touchPos, int touchID)
    {
        if (buttons[i].buttonArea.Contains(touchPos) && !buttons[i].isActive)
        {
            Color temp = buttons[i].buttonImage.color;
            temp.r -= 0.2f;
            temp.g -= 0.2f;
            temp.b -= 0.2f;
            buttons[i].buttonImage.color = temp;


            buttons[i].isActive = true;
            buttons[i].touchID = touchID;

            touchButtonDown[buttons[i].name] = true;
        }
    }

    void DeactivateButton(int i, int touchID)
    {
        if (buttons[i].isActive && buttons[i].touchID == touchID)
        {
            Color temp = buttons[i].buttonImage.color;
            temp.r += 0.2f;
            temp.g += 0.2f;
            temp.b += 0.2f;
            buttons[i].buttonImage.color = temp;


            buttons[i].isActive = false;
            buttons[i].touchID = -1;

            touchButtonDown[buttons[i].name] = false;
        }
    }
}