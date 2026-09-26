using System;
using System.Globalization;
using UnityEngine;

namespace Shade.Generics
{
    [Serializable]
    public class Timer
    {
        private const float m_kMinValue = 0f;

        private float m_CurrentTime = 0f;

        // Shade: Tracks whether this timer is actually counting
        private bool m_IsRunning = false;

        // Shade: Event that fires immediately when the timer finishes.
        public event Action OnTimerExpired = null;

        [SerializeField, Tooltip("The total duration of the timer in seconds.")]
        private float m_TimeLimit = 5f;

        [SerializeField, Tooltip("If true, the timer counts down from limit to 0. If false, it counts up from 0 to limit.")]
        private bool m_CountDown = true;

        [SerializeField, Tooltip("Whether the timer automatically reset when it reaches its target.")]
        private bool m_AutoReset = false;

        /// <summary>
        /// Creates a default timer. You will need to set its limit before starting it.
        /// </summary>
        public Timer() {}

        /// <summary>
        /// Creates a new timer with a specific duration, direction, and automatic restart setting.
        /// </summary>
        /// <param name="timeLimit">How long the timer should run (in seconds).</param>
        /// <param name="countDown">True to count down to 0, false to count up from 0.</param>
        /// <param name="autoReset">Should the timer instantly start over when it finishes?</param>
        public Timer(float timeLimit, bool countDown = true, bool autoReset = false)
        {
            m_CountDown = countDown;
            SetTimeLimit(timeLimit);
            m_AutoReset = autoReset;
            Reset();
        }

        #region Properties

        public float CurrentTime => m_CurrentTime;
        public float TimeLimit => m_TimeLimit;

        public float TimeElapsed => m_CountDown ? Mathf.Max(m_kMinValue, m_TimeLimit - m_CurrentTime) : m_CurrentTime;
        public float TimeRemaining => m_CountDown ? m_CurrentTime : Mathf.Max(m_kMinValue, m_TimeLimit - m_CurrentTime);

        public float FractionOfTimeElapsed => m_TimeLimit > m_kMinValue ? Mathf.Clamp01(TimeElapsed / m_TimeLimit) : 1f;
        public float FractionOfTimeRemaining => 1f - FractionOfTimeElapsed;

        public bool IsRunning => m_IsRunning;
        public bool OutOfTime => m_CountDown ? m_CurrentTime <= m_kMinValue : m_CurrentTime >= m_TimeLimit;

        public int NumberOfLaps => m_TimeLimit > m_kMinValue ? (int)(TimeElapsed / m_TimeLimit) : 0;

        #endregion

        #region Timer Control API

        /// <summary>
        /// Starts or resumes the timer.
        /// </summary>
        public void StartTimer()
        {
            if (m_TimeLimit <= m_kMinValue)
            {
                Debug.LogError("Timer: Cannot start timer with a limit of 0 or less!");
                return;
            }

            m_IsRunning = true;
        }

        /// <summary>
        /// Pauses the timer without losing your progress.
        /// </summary>
        public void PauseTimer()
        {
            m_IsRunning = false;
        }

        /// <summary>
        /// Changes the target duration of the timer.
        /// </summary>
        /// <param name="timeLimit">The new length of the timer in seconds.</param>
        public void SetTimeLimit(float timeLimit)
        {
            if (timeLimit <= m_kMinValue)
            {
                throw new ArgumentException("Timer@SetTimeLimit: Time limit must be greater than zero!");
            }

            m_TimeLimit = timeLimit;
        }

        /// <summary>
        /// Updates the timer progression. Call this inside an Update loop.
        /// </summary>
        /// <param name="deltaTime">How much time has passed since the last frame.</param>
        public void Tick(float deltaTime)
        {
            if (!m_IsRunning) return;

            if (m_CountDown)
            {
                _CountDown(deltaTime);
            }
            else
            {
                _CountUp(deltaTime);
            }
        }

        /// <summary>
        /// Stops the timer and resets current time to its starting point (0 if counting up, m_TimeLimit if counting down).
        /// </summary>
        public void Reset()
        {
            m_IsRunning = false;
            m_CurrentTime = m_CountDown ? m_TimeLimit : m_kMinValue;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Deducts time and checks if a countdown timer has hit zero.
        /// </summary>
        private void _CountDown(float deltaTime)
        {
            m_CurrentTime -= deltaTime;

            if (OutOfTime)
            {
                _HandleExpiry(clampValue: m_kMinValue);
            }
        }

        /// <summary>
        /// Adds time and checks if a countup timer has reached its limit.
        /// </summary>
        private void _CountUp(float deltaTime)
        {
            m_CurrentTime += deltaTime;

            if (OutOfTime)
            {
                _HandleExpiry(clampValue: m_TimeLimit);
            }
        }

        /// <summary>
        /// Triggers the timer end event and determines if it should restart or pause.
        /// </summary>
        private void _HandleExpiry(float clampValue)
        {
            OnTimerExpired?.Invoke();

            if (m_AutoReset)
            {
                Reset();
            }
            else
            {
                _StopAndClamp(clampValue);
            }
        }

        /// <summary>
        /// Locks the timer exactly at its end value and turns it off.
        /// </summary>
        private void _StopAndClamp(float clampValue)
        {
            m_CurrentTime = clampValue;
            m_IsRunning = false;
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Converts the remaining time into a styled text string.
        /// </summary>
        public string FormatRemaining(eFormatType format, char delimiter) => Format(TimeRemaining, delimiter, format);

        /// <summary>
        /// Converts the elapsed time into a styled text string.
        /// </summary>
        public string FormatElapsed(eFormatType format, char delimiter) => Format(TimeElapsed, delimiter, format);

        /// <summary>
        /// Formats any raw number of seconds into a specific clock style.
        /// </summary>
        /// <param name="rawSeconds">The amount of seconds to convert.</param>
        /// <param name="format">The style of clock to return.</param>
        /// <param name="delimiter">The delimiter char to insert between numbers.</param>
        public static string Format(float rawSeconds, char delimiter, eFormatType format)
        {
            // Shade: Clamp to ensure rawSeconds never drops below 0
            rawSeconds = Mathf.Max(0f, rawSeconds);

            return format switch
            {
                eFormatType.SecondsOnly => Mathf.CeilToInt(rawSeconds).ToString(),
                eFormatType.SecondsDecimal => rawSeconds.ToString("F1", CultureInfo.InvariantCulture),
                eFormatType.MinSec => $"{(int)rawSeconds / 60:00}{delimiter}{(int)rawSeconds % 60:00}",
                eFormatType.MinSecMs =>
                    $"{(int)rawSeconds / 60:00}{delimiter}{(int)rawSeconds % 60:00}{delimiter}{(int)((rawSeconds - (int)rawSeconds) * 100f):00}",
                eFormatType.HourMinSec =>
                    $"{(int)rawSeconds / 3600:00}{delimiter}{((int)rawSeconds / 60) % 60:00}{delimiter}{(int)rawSeconds % 60:00}",

                // Shade: Formats the truncated integer seconds to at least 2 digits default (e.g., 5.4 -> "05")
                _ => ((int)rawSeconds).ToString("00"),
            };
        }

        #endregion
    }

    public enum eFormatType
    {
        SecondsOnly = 0,
        SecondsDecimal = 1,
        MinSec = 2,
        MinSecMs = 3,
        HourMinSec = 4,
    }
}
