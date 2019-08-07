﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class Game : MonoBehaviour
{
    public float DAS { get { return startTime; } set { startTime = value; } }

    [SerializeField] private GameState state = GameState.Gamming;
    [SerializeField] private Block[] blocks;
    [SerializeField] LineRenderer linePrefab;

    public UnityEvent OnPauseCallback;
    public UnityEvent OnGammingCallback;

    private Tetris m_tetris;
    private VFX vfx;

    private const float offset = .5f;

    private bool left_pressed;
    private bool right_pressed;
    private bool up_pressed;

    private float startTime = .2f;
    private float lastStartTime;
    private bool firstStart;

    private float inputDelta = .02f;
    private float lastInputTime;


    private enum GameState
    {
        Pause,
        Gamming
    }

    void Start()
    {
        vfx = GameObject.Find("VFX").GetComponent<VFX>();
        // Tetirs Constructor
        if (vfx) m_tetris = new Tetris(new BlockSpawner(blocks), vfx);
        else m_tetris = new Tetris(new BlockSpawner(blocks));

        // draw row line
        for (int i = 0; i <= Tetris.Height + Tetris.ExtraHeight - 1; i++)
        {
            var row = Instantiate(linePrefab, this.transform);
            row.positionCount = 2;
            row.SetPosition(0, new Vector3(-offset, i - offset));
            row.SetPosition(1, new Vector3(Tetris.Width - offset, i - offset));
        }

        // draw col line
        for (int i = 0; i <= Tetris.Width; i++)
        {
            var col = Instantiate(linePrefab, this.transform);
            col.positionCount = 2;
            col.SetPosition(0, new Vector3(i - offset, -offset));
            col.SetPosition(1, new Vector3(i - offset, Tetris.Height + Tetris.ExtraHeight - 1 - offset));
        }

        state = GameState.Pause;

        StartCoroutine(Gamming());
    }


    void Update()
    {
        if (state == GameState.Gamming)
        {
            ProcessBlockInput();
            // auto drop
            m_tetris.Fall(Time.deltaTime);
        }
    }

    private void ProcessBlockInput()
    {
        // move left
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            m_tetris.MoveLeft();

            left_pressed = true;
        }
        if (!right_pressed && left_pressed && Input.GetKey(KeyCode.LeftArrow))
        {
            if (lastStartTime >= startTime)
            {
                if (!firstStart)
                {
                    m_tetris.MoveLeft();

                    firstStart = true;
                }

                if (lastInputTime >= inputDelta)
                {
                    m_tetris.MoveLeft();

                    lastInputTime = 0;
                }
                else
                {
                    lastInputTime += Time.deltaTime;
                }
            }
            else
            {
                lastStartTime += Time.deltaTime;
            }
        }
        if (left_pressed && Input.GetKeyUp(KeyCode.LeftArrow))
        {
            left_pressed = false;
            firstStart = false;
            lastStartTime = 0;
            lastInputTime = 0;
        }

        // move right
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            m_tetris.MoveRight();

            right_pressed = true;
        }
        if (!left_pressed && right_pressed && Input.GetKey(KeyCode.RightArrow))
        {
            if (lastStartTime >= startTime)
            {
                if (!firstStart)
                {
                    m_tetris.MoveRight();

                    firstStart = true;
                }

                if (lastInputTime >= inputDelta)
                {
                    m_tetris.MoveRight();

                    lastInputTime = 0;
                }
                else
                {
                    lastInputTime += Time.deltaTime;
                }
            }
            else
            {
                lastStartTime += Time.deltaTime;
            }
        }
        if (right_pressed && Input.GetKeyUp(KeyCode.RightArrow))
        {
            right_pressed = false;
            firstStart = false;
            lastStartTime = 0;
            lastInputTime = 0;
        }

        // rotate
        if (Input.GetKeyDown(KeyCode.Z))
        {
            m_tetris.AntiClockwiseRotation();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            m_tetris.ClockwiseRotation();
        }

        // hard & soft drop
        if (!up_pressed && Input.GetKeyDown(KeyCode.DownArrow))
        {
            up_pressed = true;
            m_tetris.SoftDrop();
        }
        else if (up_pressed && Input.GetKeyUp(KeyCode.DownArrow))
        {
            up_pressed = false;
            m_tetris.NormalDrop();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            m_tetris.HardDrop();
        }

        // hold
        if (Input.GetKeyDown(KeyCode.C))
        {
            m_tetris.HoldBlock();
        }
    }


    private void Pause()
    {
        state = GameState.Pause;
        OnPauseCallback?.Invoke();
    }

    
    private IEnumerator Gamming()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f);

        OnGammingCallback?.Invoke();

        vfx.TextVFX_Start();

        yield return waitForSeconds;
        yield return waitForSeconds;
        yield return waitForSeconds;
        yield return waitForSeconds;
        yield return waitForSeconds;

        state = GameState.Gamming;
        m_tetris.NextBlock();
        vfx.PlayBG(SV.MainThemeClip);
    }



#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_tetris != null) m_tetris.DrawGizmos();
    }
#endif
}
