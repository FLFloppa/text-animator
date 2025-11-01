# Дизайн системы текстовой анимации

## 1. Общее видение

Система предназначена для обработки и анимации текста с использованием XML-подобной разметки в фигурных скобках. Архитектура построена на принципах абстракции, что позволяет ей быть независимой от конкретных реализаций компонентов вывода текста (например, TextMeshPro) и легко расширяемой для поддержки новых тегов и эффектов.

Процесс работы системы делится на два основных этапа:
1.  **Парсинг**: Входная строка разбирается на абстрактное синтаксическое дерево (AST), представляющее собой иерархию текста и тегов.
2.  **Воспроизведение**: Специальный контроллер обходит AST и последовательно, символ за символом, выводит текст в целевой компонент, применяя эффекты, заданные тегами.

## 2. Ключевые компоненты и абстракции

### 2.1. `IPropertySet` и `CharacterPropertyBag` - Модель данных

Основа OCP-совместимости. `CharacterPropertyBag` — это универсальный контейнер, который может хранить любые наборы свойств (`IPropertySet`), не зная их конкретного типа.

```csharp
// Маркерный интерфейс для набора свойств
public interface IPropertySet { }

// Пример: набор свойств для базовых трансформаций
public class TransformPropertySet : IPropertySet
{
    public Vector3 PositionOffset;
    public Quaternion Rotation;
    public Vector3 Scale;
}

// Контейнер, хранящий в себе другие контейнеры (наборы свойств)
public class CharacterPropertyBag
{
    private readonly Dictionary<Type, IPropertySet> _propertySets = new Dictionary<Type, IPropertySet>();

    public TSet GetOrCreatePropertySet<TSet>() where TSet : IPropertySet, new()
    {
        if (!_propertySets.TryGetValue(typeof(TSet), out var set))
        {
            set = new TSet();
            _propertySets[typeof(TSet)] = set;
        }
        return (TSet)set;
    }
    
    public IEnumerable<IPropertySet> GetAllPropertySets() => _propertySets.Values;
}
```

### 2.2. `ICharacterModifier` - Продюсер данных

Модификатор отвечает **только** за наплнение `CharacterPropertyBag` данными. Он не знает, как эти данные будут применены.

```csharp
public interface ICharacterModifier
{
    // Метод наполняет `propertyBag` нужными наборами свойств
    void Modify(CharacterPropertyBag propertyBag, float time);
}
```

### 2.3. `IPropertyApplicator` и `ITextOutput` - Потребители и Диспетчер

Это финальный элемент, обеспечивающий полное разделение.

- **`IPropertyApplicator`**: Знает, как применить **один** тип `IPropertySet` к **одному** типу `ITextOutput`.
- **`ITextOutput`**: Является "слепым" диспетчером. Он хранит реестр аппликаторов и вызывает нужный, не зная деталей их реализации.

```csharp
// Базовый интерфейс аппликатора
public interface IPropertyApplicator
{
    // Сообщает, какой тип данных он обрабатывает
    Type PropertySetType { get; }

    // Применяет данные к цели. Внутри себя приводит типы.
    void Apply(ITextOutput target, IPropertySet propertySet, int charIndex);
}

// Интерфейс вывода текста - теперь это диспетчер
public interface ITextOutput
{
    void SetText(string text);

    // Принимает любой набор свойств и делегирует его аппликатору
    void Dispatch(IPropertySet propertySet, int charIndex);
    
    // Метод для финализации изменений за кадр
    void FinalizeUpdate();
}
```

#### Соответствие правилам проектирования

*   **O(Open/Closed Principle):** Архитектура теперь полностью соответствует OCP. Для добавления нового типа эффекта (например, `ClockShade`) нужно:
    1.  Создать новый `IPropertySet` (`MaterialPropertySet`).
    2.  Создать новый `ICharacterModifier` (`ClockShadeModifier`).
    3.  Создать новый `IPropertyApplicator` (`MaterialToTMProApplicator`).
    Ни один из существующих или базовых классов (`CharacterPropertyBag`, `ITextOutput`, `PlaybackEngine`) **не требует модификации**. Новая функциональность добавляется путем написания нового, изолированного кода и его регистрации в DI-контейнере или композиционном корне приложения.
*   **S(Single Responsibility Principle):**
    *   `ICharacterModifier`: Единственная ответственность — вычисление данных и заполнение `PropertyBag`.
    *   `CharacterPropertyBag`: Единственная ответственность — хранение `IPropertySet`.
    *   `IPropertyApplicator`: Единственная ответственность — применение одного типа данных к одному типу вывода.
    *   `ITextOutput`: Единственная ответственность — диспетчеризация `IPropertySet` и базовые операции с текстом.
*   **D(Dependency Inversion Principle):** Все компоненты зависят от абстракций (`IPropertySet`, `ICharacterModifier`, `IPropertyApplicator`, `ITextOutput`).

### 2.3. `ITagParser` - Абстракция парсера тегов

Отделяет логику разбора строки от остальной системы.

```csharp
public interface ITagParser
{
    /// <summary>
    /// Парсит входную строку и строит дерево узлов.
    /// </summary>
    /// <param name="text">Текст с тегами.</param>
    /// <returns>Корневой узел дерева документа.</returns>
    TagNode Parse(string text);
}
```

#### Соответствие правилам проектирования

*   **Типизация:** Строгая типизация входных и выходных параметров.
*   **Паттерны:** Абстрагирует **Builder** или **Interpreter**, который будет строить дерево `TagNode`.
*   **SOLID:**
    *   **S (SRP):** Единственная ответственность — парсинг строки в дерево.
    *   **O (Open/Closed):** Можно добавить новые парсеры (например, для синтаксиса XML или BBCode), не изменяя систему.
    *   **D (DIP):** Основной контроллер анимации зависит от этого интерфейса, а не от конкретного парсера.

### 2.4. Модель документа (Паттерн Composite)

Для представления текста и тегов используется паттерн **Composite**. Это позволяет работать с отдельными узлами (текст, тег) и их совокупностью единообразно.

```csharp
// Базовый компонент
public interface INode
{
    /// <summary>
    /// Родительский узел.
    /// </summary>
    TagNode Parent { get; set; }
}

// Лист - узел с текстом
public class TextNode : INode
{
    public TagNode Parent { get; set; }
    public string Text { get; private set; }

    public TextNode(string text)
    {
        Text = text;
    }
}

// Контейнер - узел с тегом, может содержать другие узлы
public class TagNode : INode
{
    public TagNode Parent { get; set; }
    public string TagName { get; private set; }
    public Dictionary<string, string> Attributes { get; private set; }
    public List<INode> Children { get; private set; }

    public TagNode(string tagName, Dictionary<string, string> attributes)
    {
        TagName = tagName;
        Attributes = attributes;
        Children = new List<INode>();
    }

    public void AddChild(INode node)
    {
        node.Parent = this;
        Children.Add(node);
    }
}
```

#### Соответствие правилам проектирования

*   **Типизация:** Все поля и коллекции строго типизированы (`string`, `List<INode>`, `Dictionary<string, string>`).
*   **Паттерны:** Явная реализация паттерна **Composite**. Это позволяет единообразно обходить дерево, состоящее из простых (текст) и составных (теги) объектов.
*   **SOLID:**
    *   **S (SRP):** `TextNode` отвечает только за хранение текста. `TagNode` отвечает за хранение данных тега и дочерних узлов. `INode` определяет общий контракт.
    *   **O (Open/Closed):** Можно добавить новые типы узлов (например, `CommentNode`), реализовав `INode`, не изменяя код обхода дерева.
    *   **L (LSP):** `TextNode` и `TagNode` взаимозаменяемы там, где ожидается `INode`.

---

## 3. Воспроизведение и Таймлайн

Для управления ходом анимации вводится концепция **Таймлайна**. Движок воспроизведения (`PlaybackEngine`) сперва компилирует дерево `TagNode` в линейный список инструкций (`PlaybackTimeline`), а затем исполняет его.

### 3.1. `IPlaybackInstruction` - Абстрактная инструкция (Паттерн Command)

Каждое действие в таймлайне — это команда.

```csharp
public interface IPlaybackInstruction
{
    /// <summary>
    /// Длительность выполнения инструкции в секундах.
    /// </summary>
    float Duration { get; }
}

// Конкретные инструкции
public class RevealCharacterInstruction : IPlaybackInstruction
{
    public float Duration { get; }
    public char Character { get; }
    public int Index { get; }

    public RevealCharacterInstruction(char character, int index, float duration)
    {
        Character = character;
        Index = index;
        Duration = duration;
    }
}

public class WaitInstruction : IPlaybackInstruction
{
    public float Duration { get; }
    public WaitInstruction(float duration) { Duration = duration; }
}
```

### 3.2. `PlaybackTimeline` - Контейнер инструкций

Простой контейнер, представляющий собой абстрактный таймлайн.

```csharp
public class PlaybackTimeline
{
    public List<IPlaybackInstruction> Instructions { get; } = new List<IPlaybackInstruction>();
    public float TotalDuration { get; private set; }

    public void AddInstruction(IPlaybackInstruction instruction)
    {
        Instructions.Add(instruction);
        TotalDuration += instruction.Duration;
    }
}
```

### 3.3. `PlaybackEngine` - Движок воспроизведения

Центральный класс, который компилирует и исполняет таймлайн.

```csharp
public class PlaybackEngine
{
    private readonly TagNode _rootNode;
    private readonly ITagHandlerFactory _handlerFactory;

    public PlaybackEngine(TagNode rootNode, ITagHandlerFactory handlerFactory)
    {
        _rootNode = rootNode;
        _handlerFactory = handlerFactory;
    }

    public PlaybackTimeline Compile()
    {
        // Логика обхода _rootNode и построения таймлайна
        // с использованием _handlerFactory для обработки тегов.
        // Теги типа "wait" и "charbychar" будут влиять на генерацию инструкций.
    }

    public void Execute(PlaybackTimeline timeline, ITextOutput textOutput)
    {
        // Логика исполнения инструкций из таймлайна с течением времени,
        // используя textOutput для отображения и анимации.
    }
}
```

#### Соответствие правилам проектирования
*   **Типизация:** Все компоненты строго типизированы.
*   **Паттерны:** `IPlaybackInstruction` — это **Command**. `PlaybackEngine` использует **Interpreter** для обхода дерева и **Builder** для создания `PlaybackTimeline`.
*   **SOLID:**
    *   **S (SRP):** `PlaybackTimeline` хранит инструкции. `PlaybackEngine` их создает и исполняет. Каждая инструкция отвечает за одно действие.
    *   **O (Open/Closed):** Можно добавить новые инструкции (`IPlaybackInstruction`), не меняя движок.
    *   **D (DIP):** `PlaybackEngine` зависит от абстракций `ITagHandlerFactory` и `ITextOutput`.

## 4. Обработка тегов

### 4.1. `ITagHandler` - Стратегия обработки тега

Вводится иерархия интерфейсов для разделения ответственности обработчиков тегов.

```csharp
// Базовый маркерный интерфейс
public interface ITagHandler { }

// Для тегов, влияющих на компиляцию таймлайна (wait, charbychar)
public interface IPlaybackControlHandler : ITagHandler
{
    void Apply(TagNode node, PlaybackTimelineBuilder timelineBuilder);
}

// Для тегов, создающих визуальные эффекты (wave, color)
public interface ICharacterModifierProvider : ITagHandler
{
    ICharacterModifier CreateModifier(TagNode node);
}
```

### 4.2. `ITagHandlerFactory` - Фабрика обработчиков

```csharp
public interface ITagHandlerFactory
{
    ITagHandler CreateHandler(string tagName);
}
```

### 4.3. Логика исполнения PlaybackEngine

Логика исполнения в движке (`Update`-цикл) отвечает за применение всех эффектов и передачу данных в `ITextOutput`.

1.  **Подготовка:** В каждом кадре движок итерирует по всем видимым символам.
2.  **Сборка данных:** Для каждого символа `c` с индексом `i`:
    a. Создается **новый, пустой** `CharacterPropertyBag`.
    b. **Применяются постоянные модификаторы:** Движок находит `TextNode` символа, движется вверх по дереву и для каждого `TagNode` получает `ICharacterModifier`, который наполняет `CharacterPropertyBag` соответствующими наборами свойств (`IPropertySet`). Это обеспечивает аддитивность (FILO).
    c. **Применяются анимации появления:** Движок проверяет список активных одноразовых анимаций для символа `c` и соответствующий `IAppearanceModifier` также наполняет `CharacterPropertyBag` своими данными.
3.  **Диспетчеризация:** После того как `CharacterPropertyBag` полностью наполнен данными от всех активных эффектов для символа `c`:
    a. Движок итерирует по всем наборам свойств внутри `bag` (`bag.GetAllPropertySets()`).
    b. Для каждого `IPropertySet` он вызывает `textOutput.Dispatch(propertySet, i)`.
4.  **Финализация:** После обработки всех видимых символов за кадр, движок вызывает `textOutput.FinalizeUpdate()`, чтобы компонент вывода мог применить все накопленные за кадр изменения (например, обновить меш текста).

Эта архитектура полностью разделяет обязанности: `PlaybackEngine` оркестрирует процесс, `ICharacterModifier` производит данные, а `ITextOutput` через `IPropertyApplicator`'ы их потребляет.

## 5. Примерные реализации обработчиков

Логика рантайм-обработчиков теперь упрощается, так как парсинг атрибутов делегируется "Определениям параметров".

### 5.1. `WaitTagHandler`
```csharp
public class WaitTagHandler : IPlaybackControlHandler
{
    private readonly ParameterDefinition<float> _durationParam;

    public WaitTagHandler(ParameterDefinition<float> durationParam)
    {
        _durationParam = durationParam;
    }

    public void Apply(TagNode node, PlaybackTimelineBuilder timelineBuilder)
    {
        float duration = _durationParam.Parse(node.Attributes);
        timelineBuilder.AddInstruction(new WaitInstruction(duration));
    }
}
```

### 5.2. `CharByCharTagHandler`
```csharp
public class CharByCharTagHandler : IPlaybackControlHandler
{
    private readonly ParameterDefinition<float> _speedParam;

    public CharByCharTagHandler(ParameterDefinition<float> speedParam)
    {
        _speedParam = speedParam;
    }

    public void Apply(TagNode node, PlaybackTimelineBuilder timelineBuilder)
    {
        float charDuration = _speedParam.Parse(node.Attributes);
        timelineBuilder.ProcessChildren(node, charDuration);
    }
}
```

### 5.3. `WaveTagHandler`
```csharp
// Рантайм-обработчик, который использует определения параметров
public class WaveTagHandler : ICharacterModifierProvider
{
    private readonly ParameterDefinition<float> _ampParam;
    private readonly ParameterDefinition<float> _freqParam;

    public WaveTagHandler(ParameterDefinition<float> ampParam, ParameterDefinition<float> freqParam)
    {
        _ampParam = ampParam;
        _freqParam = freqParam;
    }

    public ICharacterModifier CreateModifier(TagNode node)
    {
        // Парсинг атрибутов делегируется объектам определений
        float amplitude = _ampParam.Parse(node.Attributes);
        float frequency = _freqParam.Parse(node.Attributes);
        
        return new WaveModifier(amplitude, frequency);
    }
}

public class WaveModifier : ICharacterModifier
{
    // ...реализация с использованием Math.Sin для изменения Y-позиции...
    public void Modify(CharacterPropertyBag propertyBag, float time)
    {
        // Получаем текущую позицию, изменяем, кладем обратно
    }
}
```

## 6. Интеграция с редактором и Вариативность (Псевдонимы)

Для удобства и гибкости система поддерживает псевдонимы (aliases) для тегов и их параметров через `ScriptableObject`.

### 6.1. Определения параметров

Создается абстракция "Определения параметра" (`ParameterDefinitionAsset`), которая является дженериком для поддержки разных типов.

```csharp
// Нетипизированный базовый класс для хранения в списке
public abstract class BaseParameterDefinitionAsset : ScriptableObject
{
    public abstract IParameterDefinition Build();
}

// Generic-ассет для определения параметра
public abstract class ParameterDefinitionAsset<T> : BaseParameterDefinitionAsset
{
    [SerializeField] private List<string> _identifiers; // e.g., ["amp", "amplitude"]
    [SerializeField] private T _defaultValue;

    public override IParameterDefinition Build()
    {
        return new ParameterDefinition<T>(_identifiers, _defaultValue);
    }
}

// Пример конкретной реализации для float
[CreateAssetMenu(fileName = "FloatParameter", menuName = "TextAnimation/Float Parameter")]
public class FloatParameterDefinitionAsset : ParameterDefinitionAsset<float> {}

// --- Рантайм (POCO) объекты ---

public interface IParameterDefinition {}

public class ParameterDefinition<T> : IParameterDefinition
{
    public IReadOnlyList<string> Identifiers { get; }
    public T DefaultValue { get; }

    public ParameterDefinition(IReadOnlyList<string> identifiers, T defaultValue)
    {
        Identifiers = identifiers;
        DefaultValue = defaultValue;
    }

    // Логика поиска и парсинга атрибута по всем псевдонимам
    public T Parse(IReadOnlyDictionary<string, string> attributes)
    {
        foreach (var id in Identifiers)
        {
            if (attributes.TryGetValue(id, out var valueStr))
            {
                // Здесь должна быть более надежная логика конвертации типов
                return (T)System.Convert.ChangeType(valueStr, typeof(T));
            }
        }
        return DefaultValue;
    }
}
```

### 6.2. `TagHandlerAsset` - Ассет обработчика тега

Базовый класс для ассетов-обработчиков теперь определяет список псевдонимов для тега.

```csharp
public abstract class TagHandlerAsset : ScriptableObject
{
    [SerializeField] private List<string> _tagIdentifiers; // e.g., ["wave", "w", "волна"]
    public IReadOnlyList<string> TagIdentifiers => _tagIdentifiers;

    public abstract ITagHandler Build();
}
```

### 6.3. Пример: `WaveTagHandlerAsset`

Ассет для `{wave}` теперь содержит не конкретные поля, а ссылки на ассеты определений параметров.

```csharp
[CreateAssetMenu(...)]
public class WaveTagHandlerAsset : TagHandlerAsset
{
    [SerializeField] private FloatParameterDefinitionAsset _amplitudeParam;
    [SerializeField] private FloatParameterDefinitionAsset _frequencyParam;

    public override ITagHandler Build()
    {
        // Строим рантайм-определения параметров
        var amp = (ParameterDefinition<float>)_amplitudeParam.Build();
        var freq = (ParameterDefinition<float>)_frequencyParam.Build();

        // Создаем и возвращаем POCO обработчик с определениями
        return new WaveTagHandler(amp, freq);
    }
}
```

### 6.4. `TagHandlerRegistryAsset` - Обновленный реестр

Реестр теперь работает с `TagHandlerAsset` и регистрирует все псевдонимы тега.

```csharp
[CreateAssetMenu(...)]
public class TagHandlerRegistryAsset : ScriptableObject
{
    [SerializeField] private List<TagHandlerAsset> _tagHandlerAssets;

    public ITagHandlerFactory BuildFactory()
    {
        var runtimeHandlers = new Dictionary<string, ITagHandler>();
        foreach (var handlerAsset in _tagHandlerAssets)
        {
            if (handlerAsset == null) continue;

            var handler = handlerAsset.Build();
            foreach(var identifier in handlerAsset.TagIdentifiers)
            {
                if (string.IsNullOrEmpty(identifier)) continue;
                // Регистрируем обработчик на каждый псевдоним
                runtimeHandlers[identifier] = handler;
            }
        }
        return new RuntimeTagHandlerFactory(runtimeHandlers);
    }
}

// Рантайм-фабрика, использующая построенный словарь
public class RuntimeTagHandlerFactory : ITagHandlerFactory
{
    private readonly IReadOnlyDictionary<string, ITagHandler> _handlers;

    public RuntimeTagHandlerFactory(IReadOnlyDictionary<string, ITagHandler> handlers)
    {
        _handlers = handlers;
    }

    public ITagHandler CreateHandler(string tagName)
    {
        _handlers.TryGetValue(tagName, out var handler);
        return handler; // Вернет null, если не найден
    }
}
```

#### Соответствие правилам проектирования

*   **O (Open/Closed):** Система стала еще более открытой к расширению. Для изменения псевдонимов тега или параметра достаточно изменить данные в `ScriptableObject`, не трогая код.
*   **D (DIP):** Принцип инверсии зависимостей усилился. Рантайм-логика (`WaveTagHandler`) теперь зависит от абстракции `ParameterDefinition<T>`, а не от деталей парсинга или имен атрибутов.
*   **S (SRP):** `ParameterDefinition` отвечает только за парсинг одного конкретного параметра. `TagHandlerAsset` отвечает за конфигурацию одного тега.
