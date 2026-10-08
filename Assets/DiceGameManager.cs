using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DiceGameManager : MonoBehaviour
{
    
    [Header("Кубики")]
    public List<Dice> diceList = new List<Dice>();
    public Transform[] spawnPoints;

    [Header("UI элементы")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI statusText;
    public Button rebindButton;
    public TextMeshProUGUI rebindButtonText;

    // Действие New Input System
    private InputAction rollAction;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private bool isEvaluating = false;

    private void Awake()
    {
        // 1. Создаем действие ввода через New Input System с привязкой по умолчанию на Space
        rollAction = new InputAction(name: "RollDice", binding: "<Keyboard>/space");
        rollAction.performed += ctx => OnRollInput();
    }

    private void OnEnable()
    {
        rollAction.Enable();
    }

    private void OnDisable()
    {
        rollAction.Disable();
        rebindingOperation?.Dispose();
    }

    private void Start()
    {
        UpdateStatusText();

        if (rebindButton != null)
        {
            rebindButton.onClick.AddListener(StartRebinding);
        }

        if (scoreText != null)
        {
            scoreText.text = "Сумма очков: 0";
        }
    }

    private void UpdateStatusText()
    {
        // Получаем читаемое имя текущей привязанной клавиши
        string currentKey = rollAction.GetBindingDisplayString();
        if (statusText != null)
        {
            statusText.text = $"Бросок: нажмите [{currentKey}]";
        }
    }

    private void OnRollInput()
    {
        // Если кубики уже катятся или идет ребиндинг, новый бросок не начинаем
        if (isEvaluating) return;

        StartCoroutine(RollAndScoreRoutine());
    }

    private IEnumerator RollAndScoreRoutine()
    {
        isEvaluating = true;
        if (statusText != null) statusText.text = "Кубики брошены... ожидание остановки";
        if (scoreText != null) scoreText.text = "Сумма: ...";

        // Бросаем каждый кубик
        for (int i = 0; i < diceList.Count; i++)
        {
            Vector3 spawnPos = (spawnPoints != null && spawnPoints.Length > i && spawnPoints[i] != null) 
                ? spawnPoints[i].position 
                : new Vector3((i - 0.5f) * 2f, 4f, 0f);

            diceList[i].Roll(spawnPos);
        }

        // Даем небольшую паузу (0.5 сек), чтобы кубики гарантированно взлетели
        yield return new WaitForSeconds(0.5f);

        // Ждем, пока ВСЕ кубики полностью упадут и остановятся
        bool allStopped = false;
        while (!allStopped)
        {
            allStopped = true;
            foreach (var die in diceList)
            {
                if (!die.HasStopped)
                {
                    allStopped = false;
                    break;
                }
            }
            yield return null;
        }

        // Подсчет очков после падения
        int totalSum = 0;
        string breakdown = "";

        for (int i = 0; i < diceList.Count; i++)
        {
            int val = diceList[i].GetUpFacingValue();
            totalSum += val;
            breakdown += $"Кубик {i + 1}: {val}" + (i < diceList.Count - 1 ? " | " : "");
        }

        if (scoreText != null)
        {
            scoreText.text = $"Сумма: {totalSum} ({breakdown})";
        }

        UpdateStatusText();
        isEvaluating = false;
    }

    // --- Задание со звёздочкой (*): Интерактивная смена клавиши броска ---
    public void StartRebinding()
    {
        if (isEvaluating) return;

        rollAction.Disable();
        if (statusText != null) statusText.text = "Нажмите ЛЮБУЮ клавишу на клавиатуре...";
        if (rebindButtonText != null) rebindButtonText.text = "Слушаю клавишу...";
        rebindButton.interactable = false;

        rebindingOperation?.Cancel();
        rebindingOperation = rollAction.PerformInteractiveRebinding()
            // Исключаем мышь, чтобы реагировать именно на кнопки/клавиши
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                rebindingOperation.Dispose();
                rebindingOperation = null;

                rollAction.Enable();
                rebindButton.interactable = true;
                if (rebindButtonText != null) rebindButtonText.text = "Сменить клавишу";
                UpdateStatusText();
            })
            .Start();
    }
    
}
 