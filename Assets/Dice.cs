using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Dice : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 initialPosition;

    [Header("Настройки броска")]
    public float minForce = 7f;
    public float maxForce = 12f;
    public float minTorque = 15f;
    public float maxTorque = 35f;

    public bool HasStopped { get; private set; } = true;
    public bool IsRolling { get; private set; } = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
    }

    // Метод броска кубика
    public void Roll(Vector3 spawnPosition)
    {
        HasStopped = false;
        IsRolling = true;

        // Возвращаем кубик на исходную позицию над столом со случайным начальным разворотом
        transform.position = spawnPosition;
        transform.rotation = Random.rotation;

        // Сбрасываем текущие скорости
        rb.linearVelocity = Vector3.zero; // Если версия Unity старая (до Unity 6), напишите rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 1. Случайная сила: импульс вверх с небольшим разбросом в стороны
        float forceAmount = Random.Range(minForce, maxForce);
        Vector3 forceDirection = new Vector3(
            Random.Range(-0.3f, 0.3f),
            1f,
            Random.Range(-0.3f, 0.3f)
        ).normalized;

        rb.AddForce(forceDirection * forceAmount, ForceMode.Impulse);

        // 2. Случайный вращающий момент (кручение по всем осям)
        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized * Random.Range(minTorque, maxTorque);

        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }

    private void FixedUpdate()
{
    if (!IsRolling) return;

    // Проверяем, уснула ли физика или скорость стала пренебрежимо малой
    if (rb.IsSleeping() || (rb.linearVelocity.sqrMagnitude < 0.05f && rb.angularVelocity.sqrMagnitude < 0.05f))
    {
        HasStopped = true;
        IsRolling = false;
    }
}

    // Определение значения грани, которая направлена вверх
    public int GetUpFacingValue()
    {
        // Векторы направлений каждой из 6 граней в локальном пространстве куба
        var faces = new (Vector3 direction, int value)[]
        {
            (transform.up, 6),
            (-transform.up, 1),
            (transform.right, 5),
            (-transform.right, 2),
            (transform.forward, 4),
            (-transform.forward, 3)
        };

        float bestDot = -Mathf.Infinity;
        int resultValue = 1;

        // Ищем грань, чья нормаль максимально сонаправлена с мировым вектором вверх (Vector3.up)
        foreach (var face in faces)
        {
            float dot = Vector3.Dot(face.direction, Vector3.up);
            if (dot > bestDot)
            {
                bestDot = dot;
                resultValue = face.value;
            }
        }

        return resultValue;
    }
}
