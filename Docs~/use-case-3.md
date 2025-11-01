## Юз-кейс 3: Анимация шейдера (ClockShade) - Финальная OCP-версия

### Цель

Продемонстрировать, как финальная архитектура с "Аппликаторами" позволяет добавлять кастомные эффекты, управляющие шейдерами, без какой-либо модификации ядра системы или существующих компонентов вывода.

### Предусловия

*   Архитектура системы соответствует финальной версии в `design.md` (с `IPropertySet` и `IPropertyApplicator`).
*   В проекте существует кастомный шейдер для TextMeshPro со свойством `_ClockAngle`.

### 1. Шаги реализации (Пользователь)

Для добавления нового эффекта пользователь создает 3 новых, полностью изолированных класса.

#### 1.1. Создание Набора Свойств (Код)

Определяется "контракт" данных для нового эффекта.

```csharp
// Файл: MaterialPropertySet.cs
public class MaterialPropertySet : IPropertySet
{
    public Dictionary<int, float> Properties { get; } = new Dictionary<int, float>();
}
```

#### 1.2. Создание Модификатора (Код)

Создается логика, которая будет *производить* эти данные.

```csharp
// Файл: ClockShadeModifier.cs
public class ClockShadeModifier : ICharacterModifier
{
    private static readonly int ClockAngleID = Shader.PropertyToID("_ClockAngle");
    private readonly float _speed;

    public ClockShadeModifier(float speed) { _speed = speed; }

    public void Modify(CharacterPropertyBag propertyBag, float time)
    {
        var materialSet = propertyBag.GetOrCreatePropertySet<MaterialPropertySet>();
        float angle = (time * _speed) % 360;
        materialSet.Properties[ClockAngleID] = angle;
    }
}
```

#### 1.3. Создание Аппликатора (Код)

Вся логика применения эффекта к TextMeshPro инкапсулируется в этом классе.

```csharp
// Файл: MaterialToTMProApplicator.cs
public class MaterialToTMProApplicator : IPropertyApplicator
{
    public Type PropertySetType => typeof(MaterialPropertySet);
    private readonly MaterialPropertyBlock _propBlock = new MaterialPropertyBlock();

    public void Apply(ITextOutput target, IPropertySet propertySet, int charIndex)
    {
        if (!(target is TextMeshProOutput tmproOutput) || !(propertySet is MaterialPropertySet matSet)) return;

        // Аппликатор получает прямой доступ к компоненту
        var textComponent = tmproOutput.TextComponent;
        if (charIndex >= textComponent.textInfo.characterCount) return;

        var charInfo = textComponent.textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        // Вся логика работы с MaterialPropertyBlock теперь здесь
        textComponent.renderer.GetPropertyBlock(_propBlock, charInfo.materialReferenceIndex);
        foreach(var prop in matSet.Properties)
        {
            _propBlock.SetFloat(prop.Key, prop.Value);
        }
        textComponent.renderer.SetPropertyBlock(_propBlock, charInfo.materialReferenceIndex);
    }
}
```

#### 1.4. Реализация `TextMeshProOutput` (Код)

`TextMeshProOutput` становится максимально "тонким" диспетчером, который просто предоставляет доступ к своему компоненту.

```csharp
// Файл: TextMeshProOutput.cs
public class TextMeshProOutput : ITextOutput
{
    // Публичное свойство для доступа аппликаторов
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
    
    public void SetText(string text)
    {
        TextComponent.text = text;
        TextComponent.ForceMeshUpdate();
    }

    public void FinalizeUpdate()
    {
        // Этот метод может быть использован для пакетного обновления
        // вершин в конце кадра, если это потребуется для оптимизации.
        TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32 | TMP_VertexDataUpdateFlags.Vertices);
    }
}
```

#### 1.5. Композиция и Настройка

В главном классе `TextAnimator` происходит "сборка" всех частей:

```csharp
// Файл: TextAnimator.cs
void Start()
{
    // ... создание фабрики и парсера ...

    // 1. Создаем список нужных нам аппликаторов
    var applicators = new List<IPropertyApplicator>
    {
        new TransformToTMProApplicator(), // Для базовых эффектов (волна, дрожание)
        new MaterialToTMProApplicator()   // Наш новый аппликатор
    };

    // 2. Создаем ITextOutput, внедряя в него зависимости
    ITextOutput textOutput = new TextMeshProOutput(_textComponent, applicators);

    // ... компиляция и запуск движка ...
}
```

### 2. Как это работает внутри системы

1.  **Инициализация:** `TextAnimator` создает `TextMeshProOutput` и передает ему список всех аппликаторов, которые должны поддерживаться. `TextMeshProOutput` сохраняет их в словарь для быстрого доступа.
2.  **Исполнение (для одного символа):**
    1.  `PlaybackEngine` вызывает `ClockShadeModifier.Modify(bag, time)`.
    2.  Модификатор наполняет `bag` объектом `MaterialPropertySet` с данными об угле.
    3.  Движок вызывает `textOutput.Dispatch(materialPropertySet, charIndex)`.
    4.  `TextMeshProOutput` ищет в своем словаре аппликатор по ключу `typeof(MaterialPropertySet)`.
    5.  Он находит `MaterialToTMProApplicator` и вызывает его метод `Apply`, передавая ему самого себя (`this`) и `materialPropertySet`.
    6.  `MaterialToTMProApplicator` внутри `Apply` привозит типы к `TextMeshProOutput` и `MaterialPropertySet` и выполняет специфичную логику с `MaterialPropertyBlock`.

### Заключение

Эта архитектура полностью соответствует принципу Открытости/Закрытости. Для добавления нового эффекта мы создали 3 новых класса (`MaterialPropertySet`, `ClockShadeModifier`, `MaterialToTMProApplicator`) и добавили одну строчку в корень композиции (`new MaterialToTMProApplicator()`). **Ни один существующий класс ядра или вывода не был изменен.** Это доказывает, что система является по-настоящему гибкой и расширяемой.