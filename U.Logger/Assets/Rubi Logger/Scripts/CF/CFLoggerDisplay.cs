using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rubickanov.Logger
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class CFLoggerDisplay : LoggerDisplay
    {
        [Header("Settings")]

        private List<CFRubiLogger> loggersCF = new List<CFRubiLogger>();


        private void OnEnable()
        {
            SubscribeToLoggers();
        }

        private void OnDisable()
        {
            UnsubscribeFromLoggers();
        }

        private void UpdateConsole(string message)
        {
            RectTransform logLine = CreateLogLine(message);
            AddLogLineToQueue(logLine);
            RemoveOldLogLines();
            StartRemoveLogAfterDelayCoroutine(logLine);
        }

        private IEnumerator RemoveLogAfterDelay(float delay, RectTransform logLine)
        {
            yield return new WaitForSeconds(delay);
            RemoveLogLineIfItExists(logLine);
        }

        private void SubscribeToLoggers()
        {
            foreach (var logger in FindObjectsOfType<CFRubiLogger>())
            {
                logger.LogAdded += UpdateConsole;
                loggersCF.Add(logger);
            }
        }

        private void UnsubscribeFromLoggers()
        {
            foreach (var logger in loggersCF)
            {
                logger.LogAdded -= UpdateConsole;
            }

            loggersCF.Clear();
        }

        private RectTransform CreateLogLine(string message)
        {
            TextMeshProUGUI logLine =
                new GameObject("LogLine", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            logLine.transform.SetParent(transform);
            logLine.fontSize = fontSize;
            logLine.enableWordWrapping = false;
            logLine.text = message;
            return logLine.rectTransform;
        }

        private void AddLogLineToQueue(RectTransform logLine)
        {
            logLines.Enqueue(logLine);
        }

        private void RemoveOldLogLines()
        {
            if (logLines.Count > maxLines)
            {
                Destroy(logLines.Dequeue().gameObject);
            }
        }

        private void StartRemoveLogAfterDelayCoroutine(RectTransform logLine)
        {
            if (logLifetime > 0)
            {
                StartCoroutine(RemoveLogAfterDelay(logLifetime, logLine));
            }
        }

        private void RemoveLogLineIfItExists(RectTransform logLine)
        {
            if (logLine != null && logLines.Contains(logLine))
            {
                logLines.Dequeue();
                Destroy(logLine.gameObject);
            }
        }
    }
}