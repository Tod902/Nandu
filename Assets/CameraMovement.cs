using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _dragSensitivity = 5f;
    [SerializeField] private float _moveSensitivity = 3f;
    [SerializeField] private float _scrollMoveSensitivity = 1.5f;
    [SerializeField] private float _scrollSteps = 10f;

    private Camera _cam;
    private float _minSize = 3;
    private float _maxSize = 3;
    private Vector2 _boardCenter = new Vector2(1.5f, -1.5f);
    //Box not circular distance
    private Vector2 _maxDistanceFromCenter = new Vector2(0,0);

    private float sizeIncreasePerScroll;

    private bool _isDragging = false;
    private Vector2 _previousCursorPos = Vector2.zero;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        Grid.OnSizeChange += AdjustCamera;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (MenuStatus.IsActive) return;
        float mouseScrollDelta = -Input.mouseScrollDelta.y;
        if (mouseScrollDelta != 0)
        {   
            //Changes the camera size based on Input
            float newTempSize = _cam.orthographicSize + sizeIncreasePerScroll * mouseScrollDelta;
            if (newTempSize >= _minSize && newTempSize <= _maxSize) 
            {
                _cam.orthographicSize = newTempSize;

                // Move the camera when the size has been changed, according to the distance of the camera to the mouse
                Vector2 camToMouse = (Vector2) (_cam.ScreenToWorldPoint(Input.mousePosition) - _cam.transform.position);
                if (camToMouse.magnitude > _cam.orthographicSize / 8f)
                {
                    AddToCameraPosition(camToMouse / (_scrollSteps) * -mouseScrollDelta);
                }
            }
        }

        //Moving the camera by dragging while pressing right click
        if (Input.GetMouseButtonDown(1))
        {
            //Start dragging
            _isDragging = true;
            _previousCursorPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1)) 
        {
            //Do the dragging
            Vector2 currenCursorPos = Input.mousePosition;
            Vector2 direction = -(currenCursorPos - _previousCursorPos).normalized;
            AddToCameraPosition(direction * _dragSensitivity * _cam.orthographicSize * Time.unscaledDeltaTime);
            _previousCursorPos = currenCursorPos;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            //Stop dragging
            _isDragging = false;
        }

        //Moving the Camera with Up,Down,Left,Right,W,A,S,D
        if (!_isDragging)
        {
            Vector2 direction = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) direction.y += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) direction.y -= 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direction.x -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direction.x += 1;
            AddToCameraPosition(direction.normalized * _moveSensitivity * _cam.orthographicSize * Time.unscaledDeltaTime);
        }
    }

    private void SetCameraPosition(Vector2 newPosition)
    {
        if (newPosition.x > _boardCenter.x + _maxDistanceFromCenter.x) newPosition.x = _boardCenter.x + _maxDistanceFromCenter.x;
        if (newPosition.x < _boardCenter.x - _maxDistanceFromCenter.x) newPosition.x = _boardCenter.x - _maxDistanceFromCenter.x;
        if (newPosition.y > _boardCenter.y + _maxDistanceFromCenter.y) newPosition.y = _boardCenter.y + _maxDistanceFromCenter.y;
        if (newPosition.y < _boardCenter.y - _maxDistanceFromCenter.y) newPosition.y = _boardCenter.y - _maxDistanceFromCenter.y;
        
        _cam.transform.position = new Vector3(newPosition.x, newPosition.y, -10);
    }

    private void AddToCameraPosition(Vector2 posChange)
    {
        SetCameraPosition((Vector2) _cam.transform.position + posChange);
    }

    private void AdjustCamera()
    {
        _boardCenter = new Vector2(Grid.Size.x / 2f + 0.5f, -Grid.Size.y / 2f - 0.5f);
        _maxDistanceFromCenter = new Vector2(Grid.Size.x /2f - _minSize / 2f, Grid.Size.y / 2f - _minSize / 2f);
        SetCameraPosition(new Vector2(Grid.Size.x / 2f + 0.5f, -Grid.Size.y / 2f - 0.5f));

        //_cam.transform.position = new Vector3(Grid.Size.x / 2f + 0.5f, -Grid.Size.y / 2f - 0.5f, -10);
        float newSize = Grid.Size.y / 2f + _minSize;
        if (newSize < Grid.Size.x / (2f * _cam.aspect) + _minSize) newSize = Grid.Size.x / (2f * _cam.aspect) + _minSize;
        _maxSize = newSize;
        _cam.orthographicSize = _maxSize;

        if ((_maxSize - _minSize) / _scrollSteps > 25) sizeIncreasePerScroll = 25;
        else sizeIncreasePerScroll = (_maxSize - _minSize) / _scrollSteps;
    }

}
