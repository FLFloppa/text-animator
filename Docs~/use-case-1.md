## Юз-кейс 1: Базовая анимация волны (Финальная архитектура)

### Цель

Продемонстрировать процесс подключения системы к `TextMeshPro` и анимировать строку `{wave}Hello!{/wave}` с использованием финальной OCP-архитектуры.

### Предусловия

*   Проект Unity с TextMeshPro.
*   Архитектура системы соответствует финальной версии в `design.md`.

### 1. Шаги реализации (Пользователь)

#### 1.1. Настройка ассетов (Редактор)

Этот шаг **не меняется**. Пользователь, как и раньше, настраивает ассеты `P_Wave_Amplitude`, `P_Wave_Frequency`, `TH_Wave` и регистрирует их в `DefaultTagRegistry`.

#### 1.2. Обновление Модификатора (Код)

`WaveModifier` теперь должен работать с `TransformPropertySet`, определенным в `design.md`.

```csharp
// Файл: WaveModifier.cs
public class WaveModifier : ICharacterModifier
{
    private readonly float _amplitude;
    private readonly float _frequency;

    public WaveModifier(float amplitude, float frequency) { /* ... */ }

    public void Modify(CharacterPropertyBag propertyBag, float time)
    {
        // Получаем или создаем нужный набор свойств
        var transformSet = propertyBag.GetOrCreatePropertySet<TransformPropertySet>();
        
        // Модифицируем только нужные нам данные, не трогая остальное
        float yOffset = _amplitude * Mathf.Sin(time * _frequency);
        transformSet.PositionOffset += new Vector3(0, yOffset, 0);
    }
}
```

#### 1.3. Создание Аппликатора (Код)

Создается новый класс, который будет применять `TransformPropertySet` к `TextMeshPro`.

```csharp
// Файл: TransformToTMProApplicator.cs
public class TransformToTMProApplicator : IPropertyApplicator
{
    public Type PropertySetType => typeof(TransformPropertySet);

    public void Apply(ITextOutput target, IPropertySet propertySet, int charIndex)
    {
        if (!(target is TextMeshProOutput tmproOutput) || !(propertySet is TransformPropertySet transformSet)) return;

        var textComponent = tmproOutput.TextComponent;
        // ... (логика получения доступа к вершинам символа charIndex)

        // Пример логики (упрощенно):
        // var vertices = ...;
        // var matrix = Matrix4x4.TRS(transformSet.PositionOffset, transformSet.Rotation, transformSet.Scale);
        // for (v in vertices) { v = matrix.MultiplyPoint3x4(v); }
        // ... (записать вершины обратно)
    }
}
```

#### 1.4. Финальная реализация `TextAnimator` и `TextMeshProOutput`

```csharp
// Файл: TextMeshProOutput.cs - финальная версия
public class TextMeshProOutput : ITextOutput
{
    public TMP_Text TextComponent { get; }
    private readonly Dictionary<Type, IPropertyApplicator> _applicators;

    public TextMeshProOutput(TMP_Text textMesh, IEnumerable<IPropertyApplicator> applicators)
    {
        TextComponent = textMesh;
        _applicators = applicators.ToDictionary(a => a.PropertySetType);
    }

    public void Dispatch(IPropertySet propertySet, int charIndex)
    {
        if (_applicators.TryGetValue(propertySet.GetType(), out var applicator))
        {
            applicator.Apply(this, propertySet, charIndex);
        }
    }
    // ... SetText, FinalizeUpdate ...
}

// Файл: TextAnimator.cs - финальная версия
public class TextAnimator : MonoBehaviour
{
    // ... поля ...
    void Start()
    {
        // ... создание фабрики, парсера ...

        var applicators = new List<IPropertyApplicator>
        {
            new TransformToTMProApplicator() // Внедряем зависимость
        };

        ITextOutput textOutput = new TextMeshProOutput(_textComponent, applicators);

        // ... компиляция и запуск ...
    }
}
```

### 2. Как это работает внутри системы (Обновленная логика)

1.  **Инициализация:** `TextAnimator` создает `TransformToTMProApplicator` и "внедряет" его в конструктор `TextMeshProOutput`.
2.  **Исполнение (для символа 'H'):**
    1.  `PlaybackEngine` создает пустой `CharacterPropertyBag`.
    2.  Вызывается `WaveModifier.Modify(bag, time)`.
    3.  Модификатор запрашивает у `bag` объект `TransformPropertySet`, вычисляет `yOffset` и записывает его в `transformSet.PositionOffset`.
    4.  Движок вызывает `textOutput.Dispatch(transformSet, charIndex)`.
    5.  `TextMeshProOutput` находит в своем словаре `TransformToTMProApplicator` (по ключу `typeof(TransformPropertySet)`).
    6.  Вызывается `applicator.Apply(textOutput, transformSet, charIndex)`.
    7.  Аппликатор выполняет свою работу: получает доступ к `tmproOutput.TextComponent`, читает `transformSet.PositionOffset` и изменяет вершины символа 'H'.
    8.  В конце кадра движок вызывает `textOutput.FinalizeUpdate()`, чтобы обновить меш на GPU.

Этот процесс полностью OCP-совместим и соответствует финальной архитектуре.